using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FenjiuCapstone.Models.Login
{
    /// <summary>
    /// 用户返回前端使用的用户信息，存放在token中
    /// </summary>
    public class UserInfo
    {
        public string Username { get; set; }
        public int RoleID { get; set; }
        public int EmployeeID { get; set; }
        public string Phone { get; set; }
        public string RoleName { get; set; }
        public string Position { get; set; }
        public string EmployeeName { get; set; }
    }
}