using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FenjiuCapstone.Models
{
    /// <summary>
    /// 供应商插入对象属性
    /// </summary>
    public class SupplierRequest
    {
        public int SupplierID { get; set; } //供应商ID
        public string SupplierName { get; set; } //供应商名称
        public string ContactPerson { get; set; } //联系人
        public string ContactNumber { get; set; } // 联系电话
        public string Address { get; set; } //地址
        public string CreatedAt { get; set; } //创建时间
    }
}