using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FenjiuCapstone.Models.Inventory.Transfer
{
    public class InventoryTransferRequest
    {
        public int FromWarehouseID { get; set; }
        public int ToWarehouseID { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
    }
}