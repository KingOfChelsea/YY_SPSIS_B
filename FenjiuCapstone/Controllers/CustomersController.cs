using FenjiuCapstone.Models.Sales;
using sales_managers.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FenjiuCapstone.Controllers
{
    public class CustomersController : ApiController
    {
        #region 1.获取所有买家信息 Created By Zane Xu 2025-3-6 
        /// <summary>
        /// 获取所有买家信息
        /// </summary>
        /// <returns>返回信息</returns>
        [HttpGet]
        [Route("api/customers/get-customers")]
        public HttpResponseMessage GetCustomers()
        {
            List<Customer> customers = new List<Customer>();
            string sql = "SELECT * FROM Customers";
            using (var reader = DbAccess.read(sql))
            {
                while (reader.Read())
                {
                    customers.Add(new Customer
                    {
                        CustomerID = reader.GetInt32("CustomerID"),
                        CustomerName = reader.GetString("CustomerName"),
                        ContactNumber = reader.GetString("ContactNumber"),
                        Email = reader.GetString("Email"),
                        Address = reader.GetString("Address"),
                        CreatedAt = reader.GetDateTime("CreatedAt")
                    });
                };
            }
            return JsonResponseHelper.CreateJsonResponse(new { success = false, data = customers });
        }
        #endregion

        #region 2.获取指定买家信息 Created By Zane Xu 2025-3-6
        /// <summary>
        /// 获取指定买家信息（模糊查询）
        /// </summary>
        /// <param name="criteria">继承了Customer对象，且添加了DateRange参数</param>
        /// <returns>买家信息</returns>
        [HttpPost]
        [Route("api/customers/get-criteria")]
        public HttpResponseMessage GetCustomer([FromBody] CustomerSearchCriteria criteria)
        {
            List<Customer> customers = new List<Customer>();
            string sql = "SELECT * FROM Customers WHERE 1=1";
            if (criteria.DateRange != null && criteria.DateRange.Length == 2)
            {
                DateTime startDate = criteria.DateRange[0];
                DateTime endDate = criteria.DateRange[1];
                sql += $" AND CreatedAt BETWEEN '{startDate:yyyy-MM-dd}' AND '{endDate:yyyy-MM-dd}'";
            }
            // 如果传入了其他筛选条件，继续添加(模糊查询)
            if (!string.IsNullOrEmpty(criteria.CustomerName))
            {
                sql += $" AND CustomerName LIKE '%{criteria.CustomerName}%'";
            }

            if (!string.IsNullOrEmpty(criteria.ContactNumber))
            {
                sql += $" AND ContactNumber LIKE '%{criteria.ContactNumber}%'";
            }

            if (!string.IsNullOrEmpty(criteria.Email))
            {
                sql += $" AND Email LIKE '%{criteria.Email}%'";
            }
            using (var reader = DbAccess.read(sql))
            {
                while (reader.Read())
                {
                    customers.Add(new Customer
                    {
                        CustomerID = reader.GetInt32("CustomerID"),
                        CustomerName = reader.GetString("CustomerName"),
                        ContactNumber = reader.GetString("ContactNumber"),
                        Email = reader.GetString("Email"),
                        Address = reader.GetString("Address"),
                        CreatedAt = reader.GetDateTime("CreatedAt")
                    });
                }
            }
            if (customers == null || customers.Count == 0 )
            {
                return JsonResponseHelper.CreateJsonResponse(new { success = true, message =  "未能找到数据！" });
            }

            return JsonResponseHelper.CreateJsonResponse(new { success = true, data = customers });
        }
        #endregion


    }
}
