using FenjiuCapstone.Models.Role;
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
    /// 角色管理控制器
    /// </summary>
    [RoutePrefix("api/roles")]
    public class RolesController : ApiController
    {
        #region 1.获取所有角色 Created By Zane Xu 2025-3-14
        /// <summary>
        /// 获取所有角色
        /// </summary>
        [HttpGet]
        [Route("get-all")]
        public HttpResponseMessage GetAllRoles()
        {
            List<Role> roles = new List<Role>();
            string sql = "SELECT * FROM Roles";

            using (var reader = DbAccess.read(sql))
            {
                while (reader.Read())
                {
                    roles.Add(new Role
                    {
                        RoleID = reader.GetInt32("RoleID"),
                        RoleName = reader.GetString("RoleName")
                    });
                }
            }
            return JsonResponseHelper.CreateJsonResponse(new { success = true, data = roles });
        }
        #endregion

        #region 2.创建角色 Created By Znae Xu 2025-3-14
        /// <summary>
        /// 创建角色
        /// </summary>
        /// <param name="role">角色对象</param>
        /// <returns></returns>
        [HttpPost]
        [Route("create")]
        public HttpResponseMessage CreateRole([FromBody] Role role)
        {
            if (string.IsNullOrEmpty(role.RoleName))
                return JsonResponseHelper.CreateJsonResponse(new { success = false, message = "角色姓名不能为空！" });

            string sql = "INSERT INTO Roles (RoleName) VALUES (@RoleName)";

            using (var connection = DbAccess.connectingMySql())
            {
                var cmd = new MySqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@RoleName", role.RoleName);
                int result = cmd.ExecuteNonQuery();
                return JsonResponseHelper.CreateJsonResponse(new { success = result > 0, message = result > 0 ? "角色添加成功" : "添加失败" });
            }
        }///
        #endregion

    }
}
