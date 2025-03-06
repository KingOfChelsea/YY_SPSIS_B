using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FenjiuCapstone.Models.Inventory
{
    public class ProductInventoryResponse
    {
        public string ProductName { get; set; } // 产品名称
        public int TotalQuantity { get; set; } // 入库总数量
    }
}