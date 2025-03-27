using FenjiuCapstone.Models.Login;
using MySql.Data.MySqlClient;
using sales_managers.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace FenjiuCapstone.Controllers
{

    public class UserController : ApiController
    {
        #region 1.用户登录接口（获取验证码，初步确认手机号以及密码是否存在） Create by Zane Xu 2025-2-28
        /// <summary>
        /// 1. 用户登录接口
        /// </summary>
        [HttpPost]
        [Route("api/users/send-verification-code")]
        public async Task<HttpResponseMessage> SendVerificationCode([FromBody] LoginRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Phone))
                {
                    // 返回
                    return JsonResponseHelper.CreateJsonResponse(new { success = false, message = "手机号不能为空" });
                }
                if (string.IsNullOrEmpty(request.Password))
                {
                    return JsonResponseHelper.CreateJsonResponse(new { success = false, message = "密码不能为空" });
                }

                // 1. 查询数据库，检查手机号是否存在
                using (var connection = DbAccess.connectingMySql())
                {
                    string checkUserSql = @"
                            SELECT UserID, Phone
                            FROM Users 
                            WHERE Phone = @Phone and Password = @Password";

                    var cmdCheckUser = new MySqlCommand(checkUserSql, connection);
                    cmdCheckUser.Parameters.AddWithValue("@Phone", request.Phone);
                    cmdCheckUser.Parameters.AddWithValue("@Password", request.Password);
                    using (var reader = cmdCheckUser.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            return JsonResponseHelper.CreateJsonResponse(new { success = false, message = "手机号/密码错误，请正确填写账号密码" });
                        }

                        reader.Read();
                        var userId = reader.GetInt32("UserID");

                        // 2. 生成6位验证码
                        string verificationCode = Tools.FeishuApiService.GenerateVerificationCode();

                        // 3. 获取飞书的临时用户访问令牌
                        string tenantAccessToken = await Tools.FeishuApiService.Tenant_access_token();

                        // 4. 根据手机号获取飞书用户ID
                        string userIdOnFeishu = await Tools.FeishuApiService.GetUserIdByPhoneNumberAsync(tenantAccessToken, request.Phone);

                        // 5. 发送验证码到飞书用户
                        string message = $"您的验证码是：{verificationCode}";
                        string sendMessageResult = await Tools.FeishuApiService.SendMessageToUser(userId, userIdOnFeishu, message, verificationCode);

                        // 6. 返回响应
                        return JsonResponseHelper.CreateJsonResponse(new { success = true, message = "消息已经成功发送", userId= userId });
                    }
                }
            }
            catch (Exception ex)
            {
                return JsonResponseHelper.CreateJsonResponse(new { success = false, message = $"发生错误：{ex.Message}" });
            }
        }
        #endregion

        #region 2.解析验证码并返回有效token值 Create by Zane Xu 2025-2-28
        /// <summary>
        /// 根据UserId和验证码进行校验，并生成Token
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="verificationCode">用户输入的验证码</param>
        /// <returns>返回Token或错误信息</returns>
        [HttpPost]
        [Route("api/users/Generation-Token")]
        public HttpResponseMessage VerifyCodeAndGenerateToken(int userId, string verificationCode)
        {
            try
            {
                // 1. 查询最近10分钟内的验证码
                string query = @"
                SELECT VerificationCode, CreatedAt
                FROM UserVerificationCodes
                WHERE UserId = @UserId
                AND CreatedAt >= NOW() - INTERVAL 10 MINUTE
                ORDER BY CreatedAt DESC
                LIMIT 1";

                string storedVerificationCode = string.Empty;
                DateTime expiryDate = DateTime.MinValue;

                using (var connection = DbAccess.connectingMySql())
                {
                    var cmd = DbAccess.command(query, connection);
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                storedVerificationCode = reader.GetString("VerificationCode");
                                expiryDate = reader.GetDateTime("CreatedAt");
                            }
                        }
                    }
                

                // 2. 校验验证码是否匹配且是否在有效期内
                if (string.IsNullOrEmpty(storedVerificationCode))
                {
                    return JsonResponseHelper.CreateJsonResponse(new { success = false, message = "验证码未找到" });
                }

                if (storedVerificationCode != verificationCode)
                {
                    return JsonResponseHelper.CreateJsonResponse(new { success = false, message = "验证码不匹配" });
                }

                if (expiryDate < DateTime.Now.AddMinutes(-10))
                {
                    return JsonResponseHelper.CreateJsonResponse(new { success = false, message = "验证码已过期" });
                }

                // 3. 校验成功，生成Token（使用JWT生成，或者其他方式）
                string queryUserInfoSql = @" SELECT 
                                    u.EmployeeID, u.RoleID, u.Username, r.RoleName, e.EmployeeName, e.Position, u.Phone 
                                FROM 
                                    users u 
                                JOIN roles r ON r.RoleID = u.RoleID 
                                JOIN employees e ON u.EmployeeID = e.EmployeeID WHERE UserId = @UserId ";
                    var cmdInfo = DbAccess.command(queryUserInfoSql, connection);
                    cmdInfo.Parameters.AddWithValue("@UserId", userId);
                    using (var reader = cmdInfo.ExecuteReader())
                    {
                            if(reader.Read())
                            {
                                // 从数据库中读取用户信息 
                                var userInfo = new UserInfo
                                {
                                    EmployeeID = reader.GetInt32(reader.GetOrdinal("EmployeeID")),
                                    RoleID = reader.GetInt32(reader.GetOrdinal("RoleID")),
                                    Username = reader.GetString(reader.GetOrdinal("Username")),
                                    RoleName = reader.GetString(reader.GetOrdinal("RoleName")),
                                    EmployeeName = reader.GetString(reader.GetOrdinal("EmployeeName")),
                                    Position = reader.GetString(reader.GetOrdinal("Position")),
                                    Phone = reader.GetString(reader.GetOrdinal("Phone"))
                                };

                            // 使用 UserInfo 对象生成 Token 
                            var token = Tools.Token.GenerateToken(userInfo);
                            // 4. 返回生成的token
                            return JsonResponseHelper.CreateJsonResponse(new { success = true, message = "验证码验证成功", token = token });
                        }
                        else
                        {
                            return JsonResponseHelper.CreateJsonResponse(new { success = false, message = "用户信息未找到" });
                        }

                        
                    }
                    
                    //var token = Tools.Token.GenerateToken("my",1,1,"13178822476") ; // 这里使用JWT生成token，可以使用你自己的生成方式
                }
               
               
            }
            catch (Exception ex)
            {
                return JsonResponseHelper.CreateJsonResponse(new { success = false, message = $"发生错误：{ex.Message}" });
            }
        }


        #endregion

        #region 3.用户传入token验证码免登录
        [HttpPost]
        [Route("api/users/parse-token")]
        public HttpResponseMessage ParseToken(string token)
        {
            try
            {
                // 解析token并获取用户信息
                UserInfo userInfo = Tools.Token.GetUserInfoFromToken(token);

                return JsonResponseHelper.CreateJsonResponse(new { success = true,
                    data=userInfo
                });
            }
            catch (Exception ex)
            {
                return JsonResponseHelper.CreateJsonResponse(new { success = false, message = $"发生错误：{ex.Message}" });
            }
        }
    }
    #endregion
}

