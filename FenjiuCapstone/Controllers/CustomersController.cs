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
            // customers返回未空的判断改成或
            if (customers == null || customers.Count == 0 )
            {
                return JsonResponseHelper.CreateJsonResponse(new { success = true, message =  "未能找到数据！" });
            }
            
            return JsonResponseHelper.CreateJsonResponse(new { success = true, data = customers });
        }
        #endregion

        #region 3.添加新买家信息 Created By Zane Xu 2025-3-7
        /// <summary>
        /// 添加新买家信息
        /// </summary>
        /// <param name="customer">客户对象</param>
        /// <returns>添加成功</returns>
        [HttpPost]
        [Route("api/customers/create-customer")]
        public HttpResponseMessage CreateCustomer([FromBody] Customer customer)
        {
            try
            {
                if (customer == null)
                {
                    return JsonResponseHelper.CreateJsonResponse(new { success = false, message = $"对象为定义{nameof(customer)}" });
                }
                // 插入Customers表 SQL语句
                string sql = $@"INSERT INTO Customers (CustomerName, ContactNumber, Email, Address)
                            VALUES ({(string.IsNullOrEmpty(customer.CustomerName) ? "未命名" : $"'{customer.CustomerName}'")}, 
                                    {(string.IsNullOrEmpty(customer.ContactNumber) ? "未填写电话码" : $"'{customer.ContactNumber}'")},
                                    {(string.IsNullOrEmpty(customer.Email) ? "未填写邮箱" : $"'{customer.Email}'")},                       
                                    {(string.IsNullOrEmpty(customer.Address) ? "未填写地址" : $"'{customer.Address}'")})";
                int result = new DbAccess().Execute(sql);
                if (result > 0)
                {
                    return JsonResponseHelper.CreateJsonResponse(new { success = true, message = "添加成功" });
                }
                return JsonResponseHelper.CreateJsonResponse(new { success = false, message = "添加失败" });
            }
            catch (Exception ex)
            {
                return JsonResponseHelper.CreateJsonResponse(new { succes = false, message = ex.Message, help = ex.HelpLink });
            }   
        }
        #endregion

        #region 4.更新买家信息 Created By Zane Xu 2025-3-7
        /// <summary>
        /// 更新买家信息（需要角色权限）
        /// </summary>
        /// <param name="RoleID">角色权限</param>
        /// <param name="customerInfo">修改信息对象</param>
        /// <returns></returns>
        public HttpResponseMessage UpdateCustomer(int ? RoleID,Customer customerInfo)
        {
            try
            {
                // 判断是否为空
                if (!RoleID.HasValue || customerInfo == null  )
                {
                    return JsonResponseHelper.CreateJsonResponse(new
                    {
                        success = false,
                        error =  RoleID.HasValue? nameof(customerInfo) + "未上传": nameof(RoleID) +" AND"+ nameof(customerInfo)+"未上传"
                    });
                }
                // 判断权限是否足够
                //string QuerySql = $@"Select * Form ROLES WHERE RoleID  = {RoleID} ";
                string UpdateSql = $@"UPDATE Customers SET CustomerName ={customerInfo.CustomerName},ContactNumber ={customerInfo.ContactNumber},Email ={customerInfo.Email},Address={customerInfo.Address} WHERE CustomerID ={customerInfo.CustomerID}";
                //if (RoleID==Utils.EnumAccessModCustomer.ModCustomer.SaleManagers)
                //{

                //}
                int rowAffect = new DbAccess().Execute(UpdateSql);
                if (rowAffect>0)
                {
                    return JsonResponseHelper.CreateJsonResponse(new { success = true, message = "修改成功" });
                }
                return JsonResponseHelper.CreateJsonResponse(new { success = false, message = "插入失败" });
            }
            catch (Exception ex)
            {

                return JsonResponseHelper.CreateJsonResponse(new { success = false, message = ex.Message });
            }
        }
        #endregion
    }
}
