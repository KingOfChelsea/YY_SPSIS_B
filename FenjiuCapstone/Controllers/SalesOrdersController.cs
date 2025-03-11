using FenjiuCapstone.Models.Sales;
using sales_managers.Common;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;

namespace FenjiuCapstone.Controllers
{
    /// <summary>
    ///  订单管理 API
    /// </summary>
    public class SalesOrdersController : ApiController
    {
        #region 1.获取所有订单 Created By Zane Xu 2025-3-7
        /// <summary>
        /// 获取所有订单
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/salesorders")]
        public HttpResponseMessage GetSalesOrders()
        {
            List<SalesOrder> orders = new List<SalesOrder>();
            string sql = "SELECT * FROM SalesOrders";

            using (var reader = DbAccess.read(sql))
            {
                while (reader.Read())
                {
                    orders.Add(new SalesOrder
                    {
                        OrderID = reader.GetInt32("OrderID"),
                        CustomerID = reader.GetInt32("CustomerID"),
                        OrderDate = reader.GetDateTime("OrderDate"),
                        TotalAmount = reader.GetDecimal("TotalAmount"),
                        Status = reader.GetString("Status")
                    });
                }
            }

            return JsonResponseHelper.CreateJsonResponse(new { success = true, data = orders });
        }
        #endregion

        #region 2.获取指定订单（包含明细）Created By Zane Xu 2025-3-7
        [HttpPost]
        [Route("api/salesorders/search")]
        public HttpResponseMessage SearchSalesOrders([FromBody] SalesOrderSearchCriteria criteria)
        {
            List<SalesOrder> orders = new List<SalesOrder>();
            string sql = "SELECT so.* FROM SalesOrders so JOIN Customers c ON so.CustomerID = c.CustomerID WHERE 1=1";
            string CustomerName = null;
            if (criteria.CustomerID != null)
            {
                string Sql_Select_CustomerName = $@"select CustomerName FROM customers c where c.CustomerID = '{criteria.CustomerID}' ";
                using (var reader = DbAccess.read(Sql_Select_CustomerName))
                {
                    if (reader.Read())
                    {
                        // 如果查询到结果，返回客户名称
                        CustomerName = reader.GetString("CustomerName");
                        
                    }
                    else
                    {
                        // 如果查询结果为空，返回未找到客户信息
                        return JsonResponseHelper.CreateJsonResponse(new { success = false, message = "客户ID不存在或输入有误，请联系管理员确认" });
                    }
                    sql += $" AND so.CustomerID = '{criteria.CustomerID}'";
                }
            }
            if (!string.IsNullOrEmpty(criteria.Status))
                sql += $" AND so.Status = '{criteria.Status}'";

            if (criteria.MinTotalAmount.HasValue)
                sql += $" AND so.TotalAmount >= {criteria.MinTotalAmount.Value}";

            if (criteria.MaxTotalAmount.HasValue)
                sql += $" AND so.TotalAmount <= {criteria.MaxTotalAmount.Value}";

            if (criteria.OrderDateRange != null && criteria.OrderDateRange.Length == 2)
                sql += $" AND so.OrderDate BETWEEN '{criteria.OrderDateRange[0]:yyyy-MM-dd HH:mm:ss}' " +
                       $"AND '{criteria.OrderDateRange[1]:yyyy-MM-dd HH:mm:ss}'";

            using (var reader = DbAccess.read(sql))
            {
                while (reader.Read())
                {
                    orders.Add(new SalesOrder
                    {
                        OrderID = reader.GetInt32("OrderID"),
                        CustomerID = reader.GetInt32("CustomerID"),
                        OrderDate = reader.GetDateTime("OrderDate"),
                        TotalAmount = reader.GetDecimal("TotalAmount"),
                        Status = reader.GetString("Status")
                    });
                }
            }
            if (orders.Count == 0)
                return JsonResponseHelper.CreateJsonResponse(new { success = false, message = "未找到符合条件的订单" });
            // 查询订单明细
            foreach (var order in orders)
            {
                string detailSql = $"SELECT * FROM SalesOrderDetails WHERE OrderID = {order.OrderID}";
                order.OrderDetails = new List<SalesOrderDetail>();

                using (var reader = DbAccess.read(detailSql))
                {
                    while (reader.Read())
                    {
                        order.OrderDetails.Add(new SalesOrderDetail
                        {
                            DetailID = reader.GetInt32("DetailID"),
                            OrderID = reader.GetInt32("OrderID"),
                            ProductID = reader.GetInt32("ProductID"),
                            Quantity = reader.GetInt32("Quantity"),
                            Price = reader.GetDecimal("Price"),
                            SubTotal = reader.GetDecimal("SubTotal")
                        });
                    }
                }
            }
            return JsonResponseHelper.CreateJsonResponse(new { success = true, data = orders });

        }
        #endregion
    }
}
