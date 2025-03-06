using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FenjiuCapstone.Models.Login
{
    /// <summary>
    /// 登录后返回状态
    /// </summary>
    public class LoginResponse
    {
        public int UserID { get; set; }  // 用户ID
        public string Username { get; set; }  // 用户名
        public string Role { get; set; }  // 用户角色
        public string Message { get; set; }  // 错误信息/成功消息
        public bool Success { get; set; }  // 登录是否成功
    }
}