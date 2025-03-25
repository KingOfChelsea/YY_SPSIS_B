using FenjiuCapstone.Models.Permission;
using MySql.Data.MySqlClient;
using sales_managers.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FenjiuCapstone.Controllers
{
    /// <summary>
    /// 用户权限控制器
    /// </summary>
    public class PermissionsController : ApiController
    {
        #region 1.获取所有权限（树形结构） Created By Zane Xu 2025-3-14
        [HttpGet]
        [Route("api/permissions/getall-permissions")]
        public HttpResponseMessage GetAllPermissions()
        {
            List<Permission> permissions = new List<Permission>();
            string sql = "SELECT * FROM Permissions WHERE IsEnabled = 1 ORDER BY ParentID, PermissionID";
            using (var reader = DbAccess.read(sql))
            {
                Dictionary<int, Permission> permissionMap = new Dictionary<int, Permission>();
                while (reader.Read())
                {
                    Permission permission = new Permission
                    {
                        PermissionID = reader.GetInt32("PermissionID"),
                        PermissionName = reader.GetString("PermissionName"),
                        ParentID = reader.IsDBNull(reader.GetOrdinal("ParentID")) ? null : (int?)reader.GetInt32("ParentID"),
                        Icon = reader.IsDBNull(reader.GetOrdinal("Icon")) ? null : reader.GetString("Icon"),
                        Path = reader.IsDBNull(reader.GetOrdinal("Path")) ? null : reader.GetString("Path"),
                        IsEnabled = reader.GetBoolean("IsEnabled"),
                        CreatedAt = reader.GetDateTime("CreatedAt").ToString("yyyy-MM-dd")
                    };

                    permissionMap[permission.PermissionID] = permission;
                }
                // 构建层级关系
                List<Permission> rootPermissions = new List<Permission>();
                foreach (var permission in permissionMap.Values)
                {
                    if (permission.ParentID.HasValue && permissionMap.ContainsKey(permission.ParentID.Value))
                    {
                        permissionMap[permission.ParentID.Value].SubPermissions.Add(permission);
                    }
                    else
                    {
                        rootPermissions.Add(permission);
                    }
                }

                return JsonResponseHelper.CreateJsonResponse(new { success = true, data = rootPermissions });
            }
         }
        #endregion

        #region 2.角色分配权限(分配接口) Created By Zane Xu 2025-3-14
        [HttpPost]
        [Route("api/permissions/assign-role-permissions")]
        public HttpResponseMessage AssignRolePermissions([FromBody] RolePermissionRequest request)
        {
            if (request == null || request.RoleID == 0 || request.PermissionIDs == null)
                return JsonResponseHelper.CreateJsonResponse(new { success = false, message = "请求数据不完整" });

            using (var connection = DbAccess.connectingMySql())
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 获取当前角色已有的权限
                        string selectSql = "SELECT PermissionID FROM RolePermissions WHERE RoleID = @RoleID";
                        HashSet<int> existingPermissions = new HashSet<int>();

                        using (var cmdSelect = new MySqlCommand(selectSql, connection, transaction))
                        {
                            cmdSelect.Parameters.AddWithValue("@RoleID", request.RoleID);
                            using (var reader = cmdSelect.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    existingPermissions.Add(reader.GetInt32("PermissionID"));
                                }
                            }
                        }

                        // 计算需要新增的权限（在 request.PermissionIDs 里但不在 existingPermissions 里）
                        var newPermissions = request.PermissionIDs.Except(existingPermissions).ToList();

                        // 计算需要删除的权限（在 existingPermissions 里但不在 request.PermissionIDs 里）
                        var removedPermissions = existingPermissions.Except(request.PermissionIDs).ToList();

                        // 1. 删除不需要的权限
                        if (removedPermissions.Count > 0)
                        {
                            string deleteSql = "DELETE FROM RolePermissions WHERE RoleID = @RoleID AND PermissionID IN (" +
                                               string.Join(",", removedPermissions) + ")";
                            var cmdDelete = new MySqlCommand(deleteSql, connection, transaction);
                            cmdDelete.Parameters.AddWithValue("@RoleID", request.RoleID);
                            cmdDelete.ExecuteNonQuery();
                        }

                        // 2. 插入新权限
                        if (newPermissions.Count > 0)
                        {
                            string insertSql = "INSERT INTO RolePermissions (RoleID, PermissionID) VALUES (@RoleID, @PermissionID)";
                            foreach (var permissionId in newPermissions)
                            {
                                var cmdInsert = new MySqlCommand(insertSql, connection, transaction);
                                cmdInsert.Parameters.AddWithValue("@RoleID", request.RoleID);
                                cmdInsert.Parameters.AddWithValue("@PermissionID", permissionId);
                                cmdInsert.ExecuteNonQuery();
                            }
                        }

                        // 提交事务
                        transaction.Commit();
                        return JsonResponseHelper.CreateJsonResponse(new { success = true, message = "角色权限更新成功" });
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return JsonResponseHelper.CreateJsonResponse(new { success = false, message = ex.Message });
                    }
                }
            }
        }
        #endregion

        #region 3.获取角色的权限 Created By Zane Xu 2025-3-14
        [HttpGet]
        [Route("api/permissions/role/{roleId}")]
        public HttpResponseMessage GetRolePermissions(int roleId)
        {
            List<int> permissionIds = new List<int>();
            string sql = "SELECT PermissionID FROM RolePermissions WHERE RoleID = @RoleID";

            using (var connection = DbAccess.connectingMySql())
            {
                using (var cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@RoleID", roleId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            permissionIds.Add(reader.GetInt32("PermissionID"));
                        }
                    }
                }
            }

            return JsonResponseHelper.CreateJsonResponse(new { success = true, data = permissionIds });
        }
        #endregion

        #region 4.创建新权限  Created By Zane Xu 2025-3-14
        [HttpPost]
        [Route("api/permissions/create-permission")]
        public HttpResponseMessage CreatePermission([FromBody] Permission request)
        {
            using (var connection = DbAccess.connectingMySql())
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1. 创建主权限
                        string sql = @"INSERT INTO Permissions (PermissionName, ParentID,Icon ,Path, IsEnabled) 
                               VALUES (@PermissionName, @ParentID,@Icon, @Path, @IsEnabled)";
                        var cmd = new MySqlCommand(sql, connection, transaction);
                        cmd.Parameters.AddWithValue("@PermissionName", request.PermissionName);
                        cmd.Parameters.AddWithValue("@ParentID", (object)request.ParentID ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@PermissionName", request.PermissionName);
                        cmd.Parameters.AddWithValue("@Icon", "Icon");
                        cmd.Parameters.AddWithValue("@IsEnabled", request.IsEnabled);
                        cmd.ExecuteNonQuery();

                        int parentPermissionID = (int)cmd.LastInsertedId;

                        // 2. 插入子权限（如果有）
                        if (request.SubPermissions != null && request.SubPermissions.Count > 0)
                        {
                            foreach (var subPerm in request.SubPermissions)
                            {
                                string subSql = @"INSERT INTO Permissions (PermissionName, ParentID,Icon, Path, IsEnabled) 
                                          VALUES (@PermissionName, @ParentID,@Icon, @Path, @IsEnabled)";
                                var cmdSub = new MySqlCommand(subSql, connection, transaction);
                                cmdSub.Parameters.AddWithValue("@PermissionName", subPerm.PermissionName);
                                cmdSub.Parameters.AddWithValue("@ParentID", parentPermissionID);
                                cmdSub.Parameters.AddWithValue("@Icon", "Icon");
                                cmdSub.Parameters.AddWithValue("@Path", subPerm.Path);
                                cmdSub.Parameters.AddWithValue("@IsEnabled", true);
                                cmdSub.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        return JsonResponseHelper.CreateJsonResponse(new { success = true, message = "权限添加成功" });
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return JsonResponseHelper.CreateJsonResponse(new { success = false, message = ex.Message });
                    }
                }
            }
        }
        #endregion
    }
}
