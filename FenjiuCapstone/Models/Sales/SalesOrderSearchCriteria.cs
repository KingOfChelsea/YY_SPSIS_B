using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FenjiuCapstone.Models.Sales
{
    public class SalesOrderSearchCriteria:SalesOrder
    {
        public new int ? CustomerID { get; set; }     // 重写CustomerID属性
        public string CustomerName { get; set; }      // 客户名称
        public decimal? MinTotalAmount { get; set; }  // 最小总金额
        public decimal? MaxTotalAmount { get; set; }  // 最大总金额
        public DateTime[] OrderDateRange { get; set; }  // 订单日期范围
    }
}