using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FenjiuCapstone.Models.Sales
{
    /// <summary>
    /// 买家客户类
    /// </summary>
    public class Customer
    {
        /// <summary>
        /// 客户ID
        /// </summary>
        public int CustomerID { get; set; }

        /// <summary>
        /// 客户姓名
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// 客户电话
        /// </summary>
        public string ContactNumber { get; set; }

        /// <summary>
        /// 客户邮箱
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 客户地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 客户创建时间 
        /// 使用 UTC 时间以避免时区问题，默认值为当前 UTC 时间。
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        
    }
}