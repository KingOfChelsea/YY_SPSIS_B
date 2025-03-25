using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FenjiuCapstone.Models.Sales
{
    /// <summary>
    /// 销售订单(唯一订单)
    /// </summary>
    public class SalesOrder
    {
        public int OrderID { get; set; }
        public int CustomerID { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public string CustomerName { get; set; }
        public List<SalesOrderDetail> OrderDetails { get; set; } = new List<SalesOrderDetail>();
    }
}