using FenjiuCapstone.Models.Sales;
using MySql.Data.MySqlClient;
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
        /// <summary>
        /// 获取指定订单，指定查询
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/salesorders/search")]
        public HttpResponseMessage SearchSalesOrders([FromBody] SalesOrderSearchCriteria criteria)
        {
            List<SalesOrder> orders = new List<SalesOrder>();
            string sql = @"
                        SELECT so.*, c.CustomerName
                        FROM SalesOrders so
                        JOIN Customers c ON so.CustomerID = c.CustomerID
                        WHERE 1=1";

            List<MySqlParameter> parameters = new List<MySqlParameter>();

            if (criteria.CustomerID != null)
            {
                sql += " AND so.CustomerID = @CustomerID";
                parameters.Add(new MySqlParameter("@CustomerID", criteria.CustomerID));
            }
            else if (!string.IsNullOrEmpty(criteria.CustomerName))
            {
                sql += " AND c.CustomerName LIKE @CustomerName";
                parameters.Add(new MySqlParameter("@CustomerName", $"%{criteria.CustomerName}%"));
            }

            if (!string.IsNullOrEmpty(criteria.Status))
            {
                sql += " AND so.Status = @Status";
                parameters.Add(new MySqlParameter("@Status", criteria.Status));
            }

            if (criteria.MinTotalAmount.HasValue)
            {
                sql += " AND so.TotalAmount >= @MinTotalAmount";
                parameters.Add(new MySqlParameter("@MinTotalAmount", criteria.MinTotalAmount.Value));
            }

            if (criteria.MaxTotalAmount.HasValue)
            {
                sql += " AND so.TotalAmount <= @MaxTotalAmount";
                parameters.Add(new MySqlParameter("@MaxTotalAmount", criteria.MaxTotalAmount.Value));
            }

            if (criteria.OrderDateRange != null && criteria.OrderDateRange.Length == 2)
            {
                sql += " AND so.OrderDate BETWEEN @StartDate AND @EndDate";
                parameters.Add(new MySqlParameter("@StartDate", criteria.OrderDateRange[0]));
                parameters.Add(new MySqlParameter("@EndDate", criteria.OrderDateRange[1]));
            }

            // 查询订单
            using (var connection = DbAccess.connectingMySql())
            {
                using (var cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddRange(parameters.ToArray());

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            orders.Add(new SalesOrder
                            {
                                OrderID = reader.GetInt32("OrderID"),
                                CustomerID = reader.GetInt32("CustomerID"),
                                CustomerName = reader.GetString("CustomerName"), // 获取客户名称
                                OrderDate = reader.GetDateTime("OrderDate"),
                                TotalAmount = reader.GetDecimal("TotalAmount"),
                                Status = reader.GetString("Status"),
                                OrderDetails = new List<SalesOrderDetail>() // 预初始化订单详情
                            });
                        }
                    }
                }
            }

            if (orders.Count == 0)
                return JsonResponseHelper.CreateJsonResponse(new { success = false, message = "未找到符合条件的订单" });

            // 查询订单明细（包含产品名称）
            foreach (var order in orders)
            {
                string detailSql = @"
                                    SELECT sod.*, p.ProductName 
                                    FROM SalesOrderDetails sod
                                    JOIN Products p ON sod.ProductID = p.ProductID
                                    WHERE sod.OrderID = @OrderID";

                using (var connection = DbAccess.connectingMySql())
                {
                    using (var cmd = new MySqlCommand(detailSql, connection))
                    {
                        cmd.Parameters.AddWithValue("@OrderID", order.OrderID);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                order.OrderDetails.Add(new SalesOrderDetail
                                {
                                    DetailID = reader.GetInt32("DetailID"),
                                    OrderID = reader.GetInt32("OrderID"),
                                    ProductID = reader.GetInt32("ProductID"),
                                    ProductName = reader.GetString("ProductName"), // 获取产品名称
                                    Quantity = reader.GetInt32("Quantity"),
                                    Price = reader.GetDecimal("Price"),
                                    SubTotal = reader.GetDecimal("SubTotal")
                                });
                            }
                        }
                    }
                }
           
            }

            return JsonResponseHelper.CreateJsonResponse(new { success = true, data = orders });
        }
        #endregion

        #region 3.创建订单（包含订单明细）Created By Zane Xu 2025-3-11
        /// <summary>
        /// 创建订单（包含订单明细）
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/salesorders")]
        public HttpResponseMessage CreateSalesOrder([FromBody] SalesOrder order)
        {
            if (order == null || order.OrderDetails == null || order.OrderDetails.Count == 0)
                return JsonResponseHelper.CreateJsonResponse(new { success = false, message = "订单信息不完整" });
            using (var connection = DbAccess.connectingMySql())
            {
                using (var transaction = DbAccess.sqltransaction(connection))
                {
                    try
                    {
                        // 1. 插入一条订单
                        string orderSql = $"INSERT INTO SalesOrders (CustomerID, OrderDate, TotalAmount, Status) " +
                                          $"VALUES ({order.CustomerID}, NOW(), {order.TotalAmount}, '{order.Status}')";
                        var cmdOrder = DbAccess.orderCmd(orderSql, transaction);
                        cmdOrder.ExecuteNonQuery();
                        int orderId = (int)cmdOrder.LastInsertedId; //最新插入orderId
                                                                    // 2. 插入订单明细
                        foreach (var detail in order.OrderDetails)
                        {
                            string detailSql = $"INSERT INTO SalesOrderDetails (OrderID, ProductID, Quantity, Price, SubTotal) " +
                                               $"VALUES ({orderId}, {detail.ProductID}, {detail.Quantity}, {detail.Price}, {detail.SubTotal})";
                            var cmdDetail = DbAccess.orderCmd(detailSql, transaction);
                            cmdDetail.ExecuteNonQuery();
                        }
                        transaction.Commit();
                        return JsonResponseHelper.CreateJsonResponse(new { success = true, message = "订单创建成功", orderId });

                    }
                    catch (System.Exception ex)
                    {
                        transaction.Rollback();
                        return JsonResponseHelper.CreateJsonResponse(new { success = false, message = ex.Message });
                    }
                }
            }
        }
    #endregion

        #region 4. 更新订单状态 Created By Zane Xu 2025-3-11 
        [HttpPut]
        [Route("api/salesorders/{id}/status")]
        public HttpResponseMessage UpdateOrderStatus(int id, string status)
        {

        string sql = $"UPDATE SalesOrders SET Status = '{status}' WHERE OrderID = {id}";
        int rowsAffected = new DbAccess().Execute(sql);

        if (rowsAffected > 0)
            return JsonResponseHelper.CreateJsonResponse(new { success = true, message = "订单状态更新成功" });

        return JsonResponseHelper.CreateJsonResponse(new { success = false, message = "订单未找到或更新失败" });
        }
        #endregion

        #region 5.删除订单 Created By Zane Xu 2025-3-11
        [HttpDelete]
        [Route("api/salesorders/{id}")]
        public HttpResponseMessage DeleteSalesOrder(int id)
        {
            string sql = $"DELETE FROM SalesOrders WHERE OrderID = {id}";
            int rowsAffected = new DbAccess().Execute(sql);

            if (rowsAffected > 0)
                return JsonResponseHelper.CreateJsonResponse(new { success = true, message = "订单删除成功" });

            return JsonResponseHelper.CreateJsonResponse(new { success = false, message = "订单未找到" });
        }
        #endregion

        #region Created By Zane Xu 2025-3-27

        #endregion

    }

}
