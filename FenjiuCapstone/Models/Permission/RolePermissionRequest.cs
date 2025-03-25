using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FenjiuCapstone.Models.Permission
{
    /// <summary>
    /// 用于接收前端传递的角色 ID 及其对应的权限 ID 列表，以便更新角色的权限
    /// </summary>
    public class RolePermissionRequest
    {
        public int RoleID { get; set; }  // 角色 ID
        public List<int> PermissionIDs { get; set; }  // 角色关联的权限 ID 列表
    }
}