using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FenjiuCapstone.Models.Product
{
    /// <summary>
    /// 上传产品对象的参数包含 产品名称，种类，单位，单价，序列号
    /// </summary>
    public class ProductRequest
    {
        public string ProductName { get; set; }
        public string Category { get; set; }
        public string Unit { get; set; }
        public decimal UnitPrice { get; set; }
        public string SerialNumber { get; set; }
    }
}