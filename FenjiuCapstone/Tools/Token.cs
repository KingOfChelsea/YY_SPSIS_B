using FenjiuCapstone.Models.Login;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace FenjiuCapstone.Tools
{
    /// <summary>
    /// Token 类定义各种方法方式
    /// </summary>
    public class Token
    {
        private const string SecretKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAgk/u2lvOCvf8PUZIzFfV6Fc0dOTCllVKvYzdwjv8jmwPR6ZEmMoGbvBXU+trVM/gZNkqxvU7vihQzdLDUmVtlx62RNJlZBS6h5MyspfZaVH7JS97pN4A6KjPVKT46Pxj1VYZnwZx3Nej4f4L/Y6hEVHO7bn/Rl0MEmTEk3dUaP6W5AVDcCWyU15vOMcEAZuJbOUp9jsLMuj/+6LceJKTOd6SI2oe675754TTWjlDf20E2RnWzFRDmID3pBeco2mP53rYYB3mEt2F3J2l67lTWoEmpkmDpb7ehlP3BX/UuwT9tPRKuBt8y9hJvf9x4U3UQlQ/ym72DpUNxBEqkVaGuwIDAQAB";
        /// <summary>
        /// 生成JWT Token
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="roleID">角色ID</param>
        /// <param name="employeeID">工号</param>
        /// <param name="phone">电话</param>
        /// <param name="RoleName">角色姓名</param>
        /// <param name="Position">岗位</param>
        /// <param name="EmployeeName">职工姓名</param>
        /// <returns></returns>
        public static string GenerateToken(UserInfo userInfo)
        {
            var claims = new[]
        {
                new Claim(ClaimTypes.Name, userInfo.Username),
                new Claim(ClaimTypes.Role, userInfo.RoleID.ToString()),
                new Claim("EmployeeID", userInfo.EmployeeID.ToString()),
                new Claim("Phone", userInfo.Phone),
                new Claim("RoleName", userInfo.RoleName),
                new Claim("Position", userInfo.Position),
                new Claim("EmployeeName", userInfo.EmployeeName)
         };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "your-app",   // 设置发行者
                audience: "your-app-users", // 设置接收者
                claims: claims,
                expires: DateTime.Now.AddHours(8), // 设置过期时间
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// 生成一个随机的密钥
        /// </summary>
        /// <param name="length">256-bit 密钥</param>
        /// <returns></returns>
        static string GenerateRandomKey(int length)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var key = new byte[length];
                rng.GetBytes(key);
                return Convert.ToBase64String(key);
            }
        }

        /// <summary>
        /// 解析Token并提取用户信息
        /// </summary>
        /// <param name="token">用户传出的token</param>
        /// <returns></returns>
        public static ClaimsPrincipal ParseToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(SecretKey);

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = "your-app", // 设置合法的发布者
                    ValidateAudience = true,
                    ValidAudience = "your-app-users", // 设置合法的受众
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                }, out var validatedToken);

                return principal;
            }
            catch (Exception ex)
            {
                throw new Exception("Token validation failed", ex);
            }
        }
        public static UserInfo GetUserInfoFromToken(string token)
        {
            var principal = ParseToken(token);
            UserInfo userinfo = new UserInfo
            {
                EmployeeID = int.Parse(principal.FindFirst("EmployeeID")?.Value),
                RoleID = int.Parse(principal.FindFirst(ClaimTypes.Role)?.Value),
                Username = principal.FindFirst(ClaimTypes.Name)?.Value,
                RoleName = principal.FindFirst("RoleName")?.Value,
                EmployeeName = principal.FindFirst("EmployeeName")?.Value,
                Position = principal.FindFirst("Position")?.Value,
                Phone = principal.FindFirst("Phone")?.Value

            };
            return userinfo;
        }
    }
}