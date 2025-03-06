using FenjiuCapstone.Models.Feishu;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace FenjiuCapstone.Tools
{    /// <summary>
     /// 验证码操作关联飞书
     /// </summary>
    public class FeishuApiService
    {
        /// <summary>
        /// 生成6位验证码
        /// </summary>
        /// <returns></returns>
        public static string GenerateVerificationCode()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString(); // 生成 6 位数字验证码
        }

        /// <summary>
        /// 获取临时的用户访问密钥
        /// </summary>
        /// <returns></returns>
        public static async Task<string> Tenant_access_token()
        {
            string access_url = "https://open.feishu.cn/open-apis/auth/v3/tenant_access_token/internal";
            var payload = new
            {
                app_id = "cli_a7356f26ade9d00b",  // 应用 ID
                app_secret = "4F0XTCZ7akKpBcTUwAy5wbIGFIyH4vOf"  // 应用密钥
            };

            var jsonPayload = JsonConvert.SerializeObject(payload);

            using (var client = new HttpClient()) // 使用 HttpClient 进行请求
            {
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                try
                {
                    // 请求飞书 API
                    var response = await client.PostAsync(access_url, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseBody = await response.Content.ReadAsStringAsync();
                        // 反序列化响应，获取 tenant_access_token
                        var tokenResponse = JsonConvert.DeserializeObject<FeishuTokenResponse>(responseBody);

                        // 如果请求成功，返回 tenant_access_token
                        if (tokenResponse.code == 0)
                        {
                            return tokenResponse.tenant_access_token;
                        }
                        else
                        {
                            // 错误处理
                            return $"Error: {tokenResponse.msg}";
                        }
                    }
                    else
                    {
                        return $"Error: {response.StatusCode}";
                    }
                }
                catch (Exception ex)
                {
                    return $"Error: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// 根据手机号异步获取飞书用户ID 
        /// </summary>
        /// <param name="bearerToken">飞书API访问令牌（Bearer Token）</param>
        /// <param name="mobile">待查询的手机号码</param>
        /// <returns>用户ID字符串</returns>
        /// <exception cref="Exception">包含具体错误信息的异常</exception>
        public static async Task<string> GetUserIdByPhoneNumberAsync(string bearerToken, string mobile)
        {
            // ==================== 1. 构建请求体 ====================
            // 创建符合飞书API要求的请求对象 
            // API文档参考：https://open.feishu.cn/document/server-docs/contact-v3/user/batch_get_id  
            var requestData = new
            {
                mobiles = new[] { mobile }, // 手机号数组（支持批量查询）
                include_resigned = false // 是否包含离职人员 
            };

            // 序列化为JSON字符串（使用Newtonsoft.Json库）
            var jsonPayload = JsonConvert.SerializeObject(requestData);

            // ==================== 2. 创建HTTP客户端 ====================
            // 注意：实际生产环境建议通过IHttpClientFactory管理生命周期 
            // 参考微软文档：https://learn.microsoft.com/zh-cn/dotnet/core/extensions/httpclient-factory  
            using (var httpClient = new HttpClient())
            {
                // 设置授权头（Bearer Token认证）
                // 规范文档：https://datatracker.ietf.org/doc/html/rfc6750  
                httpClient.DefaultRequestHeaders.Authorization
                    = new AuthenticationHeaderValue("Bearer", bearerToken);

                // ==================== 3. 构建请求内容 ====================
                // 创建JSON格式的请求体 
                // 编码使用UTF-8，内容类型为application/json 
                using (var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json"))
                {
                    try
                    {
                        // ==================== 4. 发送POST请求 ====================
                        // 目标API地址（飞书批量获取用户ID接口）
                        const string apiUrl = "https://open.feishu.cn/open-apis/contact/v3/users/batch_get_id?user_id_type=open_id";

                        // 异步发送POST请求 
                        var response = await httpClient.PostAsync(apiUrl, content);

                        // ==================== 5. 处理响应 ====================
                        // 检查HTTP状态码（200-299范围）
                        if (response.IsSuccessStatusCode)
                        {
                            // 读取响应内容 
                            var responseBody = await response.Content.ReadAsStringAsync();

                            // 反序列化响应数据 
                            var userResponse = JsonConvert.DeserializeObject<FeishuUserResponse>(responseBody);

                            // 检查业务逻辑状态码和数据有效性 
                            if (userResponse?.Data?.UserList?.Count > 0)
                            {
                                // 返回第一个匹配用户的ID 
                                return userResponse.Data.UserList[0].UserId;
                            }
                            else
                            {
                                // 处理空数据情况 
                                throw new Exception("用户未找到或响应数据格式异常");
                            }
                        }
                        else
                        {
                            // 记录详细错误信息 
                            var errorContent = await response.Content.ReadAsStringAsync();
                            throw new Exception($"API请求失败，状态码: {response.StatusCode}, 错误信息: {errorContent}");
                        }
                    }
                    catch (HttpRequestException ex)
                    {
                        // 处理网络层异常（如超时、DNS解析失败等）
                        throw new Exception($"网络请求异常: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        // 处理其他未知异常 
                        throw new Exception($"系统异常: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// 向指定用户发送消息
        /// </summary>
        /// <param name="userId">用户Id ou_c7baef7a1768fd4fe47ad0ea91fe9434</param>
        /// <param name="messageContent">内容</param>
        /// <returns></returns>
        public static async Task<string> SendMessageToUser(int id,string userId, string messageContent,string verificationCode)
        {
            // 获取 tenant_access_token
            string tenantAccessToken = await Tenant_access_token();

            // 飞书消息发送接口
            string sendMessageUrl = "https://open.feishu.cn/open-apis/im/v1/messages?receive_id_type=open_id";

            // 构建 messagePayload
            var messagePayload = new
            {
                receive_id = userId,  // 接收消息的用户 open_id
                msg_type = "text", // 消息类型
                content = JsonConvert.SerializeObject(new { text = messageContent }) // 将 content 进行 JSON 字符串化处理
            };

            var jsonPayload = JsonConvert.SerializeObject(messagePayload);

            using (var client = new HttpClient())
            {
                // 设置请求头，Bearer token
                client.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer", tenantAccessToken);

                using (var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")) {
                    try
                    {
                        // 请求飞书 API 发送消息
                        var response = await client.PostAsync(sendMessageUrl, content);

                        if (response.IsSuccessStatusCode)
                        {
                            var responseBody = await response.Content.ReadAsStringAsync();
                            await UserVerificationService.SaveVerificationCodeAsync(id, verificationCode);
                            return $"Message sent successfully: {responseBody}";
                        }
                        else
                        {
                            var responseBody = await response.Content.ReadAsStringAsync();
                            return $"Error: {response.StatusCode}, Response: {responseBody}";
                        }
                    }
                    catch (Exception ex)
                    {
                        return $"Error: {ex.Message}";
                    }
                };
            }
        }
    }
}
