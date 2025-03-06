using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FenjiuCapstone.Models.Inventory.Transfer
{
    public class InventoryTransfer
    {
        // 调拨ID，唯一标识每一条调拨记录
        public int TransferID { get; set; }

        // 来源仓库ID，关联Warehouses表
        public int FromWarehouseID { get; set; }

        // 目标仓库ID，关联Warehouses表
        public int ToWarehouseID { get; set; }

        // 商品ID，关联Products表
        public int ProductID { get; set; }

        // 调拨数量
        public int Quantity { get; set; }

        // 调拨日期
        public DateTime TransferDate { get; set; }

        // 调拨状态
        public string Status { get; set; }
    }
}