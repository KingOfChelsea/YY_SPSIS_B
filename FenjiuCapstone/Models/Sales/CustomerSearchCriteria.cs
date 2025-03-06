using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FenjiuCapstone.Models.Sales
{
    public class CustomerSearchCriteria: Customer
    {
        public DateTime[] DateRange { get; set; }  // 用于筛选时间范围
    }
}