using sales_managers.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace FenjiuCapstone.Tools
{
    /// <summary>
    /// 用户密码类 1.保存验证码及生成时间 Create by Zane Xu 2025-2-28
    /// </summary>
    public class UserVerificationService
    {
        /// <summary>
        /// 1.保存验证码及生成时间 Create by Zane Xu 2025-2-28
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="verificationCode">验证码</param>
        /// <returns>返回执行结果</returns>
        public static async Task<int> SaveVerificationCodeAsync(int userId, string verificationCode)
        {
            // 获取当前时间
            var createdAt = DateTime.Now;

            // SQL 插入语句
            string sql = @"
                INSERT INTO UserVerificationCodes (UserId, VerificationCode, CreatedAt)
                VALUES (@UserId, @VerificationCode, @CreatedAt)
                ON DUPLICATE KEY UPDATE 
                    VerificationCode = @VerificationCode, 
                    CreatedAt = @CreatedAt;";

            // 使用 DbAccess 类的方法连接到数据库
            using (var connection = DbAccess.connectingMySql())
            {
                using (var command = DbAccess.command(sql, connection))
                {
                    // 添加参数防止SQL注入
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@VerificationCode", verificationCode);
                    command.Parameters.AddWithValue("@CreatedAt", createdAt);

                    try
                    {
                        // 执行插入/更新操作
                        return await command.ExecuteNonQueryAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                        return -1;  // 表示发生了错误
                    }
                }
            }
        }
    }
}