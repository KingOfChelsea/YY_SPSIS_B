using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FenjiuCapstone.Models.Order
{
    /// <summary>
    /// 采购订单查询参数
    /// </summary>
    public class PurchaseOrderQuery
    {
        public int? PurchaseOrderID { get; set; }  // 采购单ID（可选）
        public int? SupplierID { get; set; }       // 供应商ID（可选）
        public string SupplierName { get; set; }   // 供应商名称（可选）
        public int? EmployeeID { get; set; }       // 员工ID（可选）
        public string EmployeeName { get; set; }   // 员工名称（可选）
        public string Status { get; set; }         // 采购单状态（可选）
        public List<string> CreateTime { get; set; }  // 时间范围（可选）
    }
}