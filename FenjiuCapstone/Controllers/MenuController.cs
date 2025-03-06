using FenjiuCapstone.Models;
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
    public class MenuController : ApiController
    {
        DbAccess db = new DbAccess();

        /// <summary>
        /// 菜单请求（参数，用户ID来请求）
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/menus")]
        public HttpResponseMessage GetMenus([FromBody] MenuRequest request)
        {
            DbAccess db = new DbAccess();
            List<Menu> menus = new List<Menu>();

            // 获取 UserID
            int userId = request.UserID;

            // 根据 UserID 获取数据的 SQL 查询，使用参数化查询来避免 SQL 注入
            string sql = @"
                        SELECT
                            CASE
                                WHEN p.ParentID IS NULL THEN p.PermissionID
                                ELSE p.ParentID
                            END AS GroupID,
                            p.PermissionID,
                            p.PermissionName,
                            p.Path,
                            p.Icon,
                            p.IsEnabled,
                            p.ParentID
                        FROM 
                            Users u
                        JOIN 
                            roles ur ON u.UserID = ur.RoleID
                        JOIN 
                            RolePermissions rp ON ur.RoleID = rp.RoleID
                        JOIN 
                            Permissions p ON rp.PermissionID = p.PermissionID
                        WHERE 
                            u.UserID = @UserID
                        AND 
                            p.IsEnabled = TRUE
                        ORDER BY 
                            GroupID ASC, 
                            p.ParentID ASC, 
                            p.PermissionID ASC;";

            try
            {
                // 使用参数化查询，避免 SQL 注入
                var connection = DbAccess.connectingMySql();
                using (var cmd = new MySqlCommand(sql, connection))
                {
                    // 添加 UserID 参数
                    cmd.Parameters.AddWithValue("@UserID", userId);

                    // 执行查询
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var menu = new Menu
                        {
                            PermissionID = reader.GetInt32("PermissionID"),
                            PermissionName = reader.GetString("PermissionName"),
                            Path = reader.GetString("Path"),
                            Icon = reader.GetString("Icon"),
                            IsEnabled = reader.GetBoolean("IsEnabled"),
                            ParentID = reader.IsDBNull(reader.GetOrdinal("ParentID")) ? (int?)null : reader.GetInt32("ParentID")
                        };

                        menus.Add(menu);
                    }
                }
            }
            catch (Exception ex)
            {
                // 异常处理
                Console.WriteLine($"Error: {ex.Message}");
                return JsonResponseHelper.CreateJsonResponse(new { error = ex.Message });
            }

            // 根据 ParentID 构建树形结构
            var menuDictionary = menus.ToDictionary(menu => menu.PermissionID);
            List<Menu> rootMenus = new List<Menu>();

            foreach (var menu in menus)
            {
                if (menu.ParentID.HasValue)
                {
                    // 如果有父节点，找到父节点并将当前菜单加入到父节点的 SubMenus 中
                    if (menuDictionary.ContainsKey(menu.ParentID.Value))
                    {
                        var parentMenu = menuDictionary[menu.ParentID.Value];
                        parentMenu.SubMenus.Add(menu);
                    }
                }
                else
                {
                    // 如果没有父节点，说明是根节点
                    rootMenus.Add(menu);
                }
            }

            // 返回构建好的菜单树
            return JsonResponseHelper.CreateJsonResponse(rootMenus);
        }

    }
}
