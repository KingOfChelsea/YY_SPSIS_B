using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FenjiuCapstone.Models.Inventory.products_info
{
    public class ProductInfoResponse
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public int RemainingStock { get; set; } // 剩余可分配量
    }
}