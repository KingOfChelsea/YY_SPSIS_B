using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FenjiuCapstone.Models.Product
{
    public class Product
    {
        public int ProductID { get; set; }         // 产品ID
        public string ProductName { get; set; }    // 产品名称
        public string Category { get; set; }       // 产品类别
        public string Unit { get; set; }           // 单位
        public decimal UnitPrice { get; set; }     // 单价
        public int StockQuantity { get; set; }     // 库存数量
        public string CreatedAt { get; set; }      // 创建时间 (格式为 yyyy-MM-dd)
        public string SerialNumber { get; set; }   // 货号/条形码
    }
}