using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FenjiuCapstone.Models.Inventory.Transfer
{
    public class TransferQueryRequest
    {
        public int? TransferID { get; set; }          // 调拨记录 ID（可选）
        public int? ProductID { get; set; }           // 产品 ID（可选）
        public int? FromWarehouseID { get; set; }     // 来源仓库 ID（可选）
        public int? ToWarehouseID { get; set; }       // 目标仓库 ID（可选）
        public string Status { get; set; }            // 调拨状态（可选，如 "Pending", "Approved", "Completed"）
        public List<string> DateRange { get; set; }   // 时间范围数组（格式：["2024-02-01", "2024-02-15"]）
    }

}