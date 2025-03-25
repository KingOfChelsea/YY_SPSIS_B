using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FenjiuCapstone.Models.Permission
{
    /// <summary>
    /// 用户权限类
    /// </summary>
    public class Permission
    {
        public int PermissionID { get; set; } //ID
        public string PermissionName { get; set; }
        public int? ParentID { get; set; }  // 父级权限ID
        public string Icon { get; set; }
        public string Path { get; set; }
        public bool IsEnabled { get; set; }
        public string CreatedAt { get; set; }
        public List<Permission> SubPermissions { get; set; } = new List<Permission>(); // 子权限
    }
}