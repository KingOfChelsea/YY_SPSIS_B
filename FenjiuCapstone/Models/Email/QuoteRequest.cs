using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FenjiuCapstone.Models.Email
{
    /// <summary>
    /// 邮箱请求类型
    /// </summary>
    public class QuoteRequest
    {
        /// <summary>
        /// 客户邮箱
        /// </summary>
        public string CustomerEmail { get; set; }  // 客户邮箱

        /// <summary>
        /// 订单信息
        /// </summary>
        public int OrderId { get; set; }
    }
}