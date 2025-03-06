using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FenjiuCapstone.Models.Order
{
    /// <summary>
    /// 创建采购详细列表，数量以及金额
    /// </summary>
    public class PurchaseOrderDetail
    {
        public int ProductID { get; set; }   // 产品ID
        public int Quantity { get; set; }   // 采购数量
        public decimal SubTotal { get; set; }  // 小计金额
    }
}