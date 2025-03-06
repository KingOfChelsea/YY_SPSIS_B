using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FenjiuCapstone.Models.Inventory
{
    public class InventoryDistributionRequest
    {
        public int ProductID { get; set; }       // 产品ID
        public int WarehouseID { get; set; }     // 仓库ID
        public int Quantity { get; set; }        // 分配数量
        public int SupplierID { get; set; }      // 供应商ID
    }
}