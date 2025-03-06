using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FenjiuCapstone.Models.Login
{
    public class UserVerificationCode
    {
        public int UserId { get; set; }  // 用户ID
        public string VerificationCode { get; set; }  // 验证码
        public DateTime CreatedAt { get; set; }  // 验证码生成时间
    }
}