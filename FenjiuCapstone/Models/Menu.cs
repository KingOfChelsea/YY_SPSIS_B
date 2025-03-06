using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FenjiuCapstone.Models
{
    public class Menu
    {
        public int PermissionID { get; set; }
        public string PermissionName { get; set; }
        public string Path { get; set; }
        public string Icon { get; set; }
        public bool IsEnabled { get; set; }
        public int? ParentID { get; set; }
        public List<Menu> SubMenus { get; set; } // 这里增加一个子节点的列表

        public Menu()
        {
            SubMenus = new List<Menu>(); // 初始化子菜单列表
        }
    }
}