using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FenjiuCapstone.Models.Inventory.Transfer
{
    public class TransferRecordResponse
    {
        public int TransferID { get; set; }           // 调拨记录 ID
        public int ProductID { get; set; }            // 产品 ID
        public string ProductName { get; set; }       // 产品名称
        public int FromWarehouseID { get; set; }      // 来源仓库 ID
        public string FromWarehouseName { get; set; } // 来源仓库名称
        public int ToWarehouseID { get; set; }        // 目标仓库 ID
        public string ToWarehouseName { get; set; }   // 目标仓库名称
        public int Quantity { get; set; }             // 调拨数量
        public string TransferDate { get; set; }      // 调拨日期 (格式化为 yyyy-MM-dd HH:mm:ss)
        public string Status { get; set; }            // 调拨状态
    }
}