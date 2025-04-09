using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FenjiuCapstone.Models.Users.Request
{
    public class User
    {
        public int UserID { get; set; }              // 用户ID（主键）
        public string Username { get; set; }         // 用户名
        public string Password { get; set; }         // 密码（建议前端/后端加密处理）
        public string Phone { get; set; }            // 手机号
        public int RoleID { get; set; }              // 角色ID（外键）
        public int EmployeeID { get; set; }          // 员工ID（外键）
        public DateTime CreatedAt { get; set; }      // 创建时间
        public string Key { get; set; }              // 解密Key 
    }

}