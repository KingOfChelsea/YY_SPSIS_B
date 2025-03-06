using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FenjiuCapstone.Models.Order
{
    /// <summary>
    /// 创建采购订单类
    /// </summary>
    public class PurchaseOrder
    {
        public int SupplierID { get; set; }   // 供应商ID
        public int EmployeeID { get; set; }   // 采购员工ID
        public List<PurchaseOrderDetail> Details { get; set; }  // 采购明细
    }
}