using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FenjiuCapstone.Models.Product
{
    /// <summary>
    /// 产品信息查询支持输入参数
    /// </summary>
    public class ProductQueryRequest
    {
        public string ProductName { get; set; }
        public string Category { get; set; }
        public decimal? UnitPrice { get; set; }
        public DateTime? CreatedAtStart { get; set; }
    }
}