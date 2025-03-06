using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FenjiuCapstone.Models.Inventory
{
    public class WarehouseInventoryRequest
    {
        public int? WarehouseID { get; set; } // 可选，仓库ID
        public DateTime? StartDate { get; set; } // 可选，起始日期
        public DateTime? EndDate { get; set; } // 可选，结束日期
    }

}