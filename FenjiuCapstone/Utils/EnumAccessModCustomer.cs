using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FenjiuCapstone.Utils
{
    public class EnumAccessModCustomer
    {
        /// <summary>
        /// 枚举类用于判断删除销售人员权限
        /// </summary>
        public enum ModCustomer
        {
            /// <summary>
            /// 销售经理
            /// </summary>
             SaleManagers = 4,

             /// <summary>
             /// 系统管理员
             /// </summary>
             System = 1

        }
    }
}