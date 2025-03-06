using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FenjiuCapstone.Models.Login
{
    public class LoginRequest
    {
        /// <summary>
        /// 登录接口
        /// </summary>
        public string Phone { get; set; }  // 登录手机号
        public string Password { get; set; }  // 用户密码
    }
}