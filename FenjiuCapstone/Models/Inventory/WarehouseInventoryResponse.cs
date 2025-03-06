using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FenjiuCapstone.Models.Inventory
{
    public class WarehouseInventoryResponse
    {
        public int WarehouseID { get; set; } // 仓库ID
        public string WarehouseName { get; set; } // 仓库名称
        public List<ProductInventoryResponse> Products { get; set; } // 产品列表
    }
}