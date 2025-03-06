using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FenjiuCapstone.Models.Inventory
{
    /// <summary>
    /// 查询仓库属性
    /// </summary>
    public class WarehouseSearchRequest
    {
        public int? WarehouseID { get; set; }
        public string WarehouseName { get; set; }
        public string ContactPerson { get; set; }
        public string ContactNumber { get; set; }
    }
}