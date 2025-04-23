using FenjiuCapstone.Models.Users.Request;
using FenjiuCapstone.Tools;
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
    public class UsersController : ApiController
    {
        [HttpPost]
        [Route("api/users/register")]
        public HttpResponseMessage RegisterUser([FromBody] User request)
        {
            try
            {
                using (var conn = DbAccess.connectingMySql())
                {
                    // 1. 检查用户名是否重复
                    string checkUserSql = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
                    using (var checkCmd = new MySqlCommand(checkUserSql, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@Username", request.Username);
                        int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                        if (count > 0)
                        {
                            return JsonResponseHelper.CreateJsonResponse(new { success = false, message = "用户名已存在" });
                        }
                    }   
                string sha256 = Password.GenerateAes256Key();
                string hashedPassword = Password.Encrypt(request.Password, sha256);
                // 3. 插入用户
                string insertSql = @"
                INSERT INTO Users (Username, Password, Phone, RoleID, EmployeeID, CreatedAt, KeyValue)
                VALUES (@Username, @Password, @Phone, @RoleID, @EmployeeID, NOW(), @Key)";
                using (var insertCmd = new MySqlCommand(insertSql, conn))
                {
                    insertCmd.Parameters.AddWithValue("@Username", request.Username);
                    insertCmd.Parameters.AddWithValue("@Password", hashedPassword);
                    insertCmd.Parameters.AddWithValue("@Phone", request.Phone);
                    insertCmd.Parameters.AddWithValue("@RoleID", request.RoleID);
                    insertCmd.Parameters.AddWithValue("@EmployeeID", request.EmployeeID);
                    insertCmd.Parameters.AddWithValue("@Key", sha256);

                    int result = insertCmd.ExecuteNonQuery();
                    if (result > 0)
                    {
                        return JsonResponseHelper.CreateJsonResponse(new { success = true, message = "注册成功" });
                    }
                    else
                    {
                        return JsonResponseHelper.CreateJsonResponse(new { success = false, message = "注册失败，请重试" });
                    }
                }
               }
            }
            catch (Exception ex)
            {
                return JsonResponseHelper.CreateJsonResponse(new { success = false, message = "错误"+ex });
            }
        }
    }
}
