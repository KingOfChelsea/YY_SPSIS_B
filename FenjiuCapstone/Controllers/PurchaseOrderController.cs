using FenjiuCapstone.Models.Order;
using MySql.Data.MySqlClient;
using sales_managers.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace FenjiuCapstone.Controllers
{
    /// <summary>
    /// 采购总接口
    /// </summary>
    public class PurchaseOrderController : ApiController
    {

        #region 1.采购订单接口 create by zhenyu xu 2025-1-1
        /// <summary>
        /// POST创建采购订单接口
        /// </summary>
        /// <param name="order">采购参数</param>
        /// <returns>创建采购订单成功</returns>
        [HttpPost]
        [Route("api/purchaseorders/create")]
        public HttpResponseMessage CreatePurchaseOrder([FromBody] PurchaseOrder order)
        {
            if (order == null || order.Details == null || order.Details.Count == 0)
            {
                return JsonResponseHelper.CreateJsonResponse(new { success = false, message = "采购订单数据不能为空" });
            }

            using (var connection = DbAccess.connectingMySql())
            {
                MySqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // 1. 插入采购单
                    string insertOrderSql = @"
                        INSERT INTO PurchaseOrders (SupplierID, EmployeeID, CreatedAt, Status) 
                        VALUES (@SupplierID, @EmployeeID, NOW(), '待审核');
                        SELECT LAST_INSERT_ID();";

                    MySqlCommand cmd = new MySqlCommand(insertOrderSql, connection, transaction);
                    cmd.Parameters.AddWithValue("@SupplierID", order.SupplierID);
                    cmd.Parameters.AddWithValue("@EmployeeID", order.EmployeeID);

                    int purchaseOrderID = Convert.ToInt32(cmd.ExecuteScalar());

                    // 2. 插入采购明细
                    foreach (var detail in order.Details)
                    {
                        string insertDetailSql = @"
                    INSERT INTO PurchaseOrderDetails (PurchaseOrderID, ProductID, Quantity, SubTotal)
                    VALUES (@PurchaseOrderID, @ProductID, @Quantity, @SubTotal);";

                        MySqlCommand detailCmd = new MySqlCommand(insertDetailSql, connection, transaction);
                        detailCmd.Parameters.AddWithValue("@PurchaseOrderID", purchaseOrderID);
                        detailCmd.Parameters.AddWithValue("@ProductID", detail.ProductID);
                        detailCmd.Parameters.AddWithValue("@Quantity", detail.Quantity);
                        detailCmd.Parameters.AddWithValue("@SubTotal", detail.SubTotal);
                        detailCmd.ExecuteNonQuery();
                    }

                    // 提交事务
                    transaction.Commit();

                    return JsonResponseHelper.CreateJsonResponse(new { success = true, message = "采购订单创建成功", PurchaseOrderID = purchaseOrderID });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return JsonResponseHelper.CreateJsonResponse(new { success = false, message = "发生错误：" + ex.Message });
                }
            }
        }
        #endregion

        #region 2.查询采购订单数据 create by zhenyu xu 2025-1-1
        /// <summary>
        /// 查询采购订单数据
        /// </summary>
        /// <param name="query">查询参数</param>
        /// <returns>返回采购订单列表</returns>
        [HttpPost]
        [Route("api/purchaseorders/query")]
        public HttpResponseMessage QueryPurchaseOrders([FromBody] PurchaseOrderQuery query)
        {

            using (var connection = DbAccess.connectingMySql())
            {
                // 构建 SQL 查询
                StringBuilder sql = new StringBuilder(@"
            SELECT 
                po.PurchaseOrderID,
                po.CreatedAt,
                po.Status,
                s.SupplierID,
                s.SupplierName,
                e.EmployeeID,
                e.EmployeeName,
                pod.DetailID,
                pod.ProductID,
                p.ProductName,
                pod.Quantity,
                pod.SubTotal
            FROM PurchaseOrders po
            JOIN Suppliers s ON po.SupplierID = s.SupplierID
            JOIN Employees e ON po.EmployeeID = e.EmployeeID
            LEFT JOIN PurchaseOrderDetails pod ON po.PurchaseOrderID = pod.PurchaseOrderID
            LEFT JOIN Products p ON pod.ProductID = p.ProductID
            WHERE 1=1 ");

                List<MySqlParameter> parameters = new List<MySqlParameter>();

                // 动态添加查询条件
                if (query.PurchaseOrderID.HasValue)
                {
                    sql.Append(" AND po.PurchaseOrderID = @PurchaseOrderID");
                    parameters.Add(new MySqlParameter("@PurchaseOrderID", query.PurchaseOrderID));
                }
                if (query.SupplierID.HasValue)
                {
                    sql.Append(" AND s.SupplierID = @SupplierID");
                    parameters.Add(new MySqlParameter("@SupplierID", query.SupplierID));
                }
                if (!string.IsNullOrEmpty(query.SupplierName))
                {
                    sql.Append(" AND s.SupplierName LIKE @SupplierName");
                    parameters.Add(new MySqlParameter("@SupplierName", "%" + query.SupplierName + "%"));
                }
                if (query.EmployeeID.HasValue)
                {
                    sql.Append(" AND e.EmployeeID = @EmployeeID");
                    parameters.Add(new MySqlParameter("@EmployeeID", query.EmployeeID));
                }
                if (!string.IsNullOrEmpty(query.EmployeeName))
                {
                    sql.Append(" AND e.EmployeeName LIKE @EmployeeName");
                    parameters.Add(new MySqlParameter("@EmployeeName", "%" + query.EmployeeName + "%"));
                }
                if (!string.IsNullOrEmpty(query.Status))
                {
                    sql.Append(" AND po.Status = @Status");
                    parameters.Add(new MySqlParameter("@Status", query.Status));
                }
                if (query.CreateTime != null && query.CreateTime.Count == 2)
                {
                    sql.Append(" AND DATE(po.CreatedAt) BETWEEN @StartDate AND @EndDate");
                    parameters.Add(new MySqlParameter("@StartDate", query.CreateTime[0]));
                    parameters.Add(new MySqlParameter("@EndDate", query.CreateTime[1]));
                }

                sql.Append(" ORDER BY po.CreatedAt DESC");
                // 打印生成的 SQL 语句
                //Console.WriteLine();
                //Debug.WriteLine("Generated SQL: " + sql.ToString() + query.SupplierName);
                // 执行 SQL 查询
                using (var cmd = DbAccess.command(sql.ToString(), connection))
                {
                    cmd.Parameters.AddRange(parameters.ToArray());

                    using (var reader = cmd.ExecuteReader())
                    {
                        var orders = new List<dynamic>();
                        var orderDict = new Dictionary<int, dynamic>();

                        while (reader.Read())
                        {
                            int orderId = reader.GetInt32("PurchaseOrderID");

                            if (!orderDict.ContainsKey(orderId))
                            {
                                var order = new
                                {
                                    PurchaseOrderID = orderId,
                                    CreatedAt = reader.GetDateTime("CreatedAt").ToString("yyyy-MM-dd"),
                                    Status = reader.GetString("Status"),
                                    SupplierID = reader.GetInt32("SupplierID"),
                                    SupplierName = reader.GetString("SupplierName"),
                                    EmployeeID = reader.GetInt32("EmployeeID"),
                                    EmployeeName = reader.GetString("EmployeeName"),
                                    Details = new List<dynamic>()
                                };

                                orderDict[orderId] = order;
                                orders.Add(order);
                            }

                            if (!reader.IsDBNull(reader.GetOrdinal("DetailID")))
                            {
                                var detail = new
                                {
                                    DetailID = reader.GetInt32("DetailID"),
                                    ProductID = reader.GetInt32("ProductID"),
                                    ProductName = reader.GetString("ProductName"),
                                    Quantity = reader.GetInt32("Quantity"),
                                    SubTotal = reader.GetDecimal("SubTotal")
                                };

                                ((List<dynamic>)orderDict[orderId].Details).Add(detail);
                            }
                        }

                        return JsonResponseHelper.CreateJsonResponse(new { success = true, data = orders });
                    }
                }
            }
        }
        #endregion

        #region 3.采购单验收入库（增加库存）
        [HttpPost]
        [Route("api/purchaseorders/addstock")]
        public HttpResponseMessage AddStocks(int PurchaseOrderID)
        {
            using (var connection = DbAccess.connectingMySql())
            {
                using (var transaction = connection.BeginTransaction()) // ✅ 启动事务，保证数据一致性
                {
                    try
                    {
                        // ✅ 1️⃣ 查询采购订单明细（获取 ProductID 和 Quantity）
                        string queryOrderSql = @"
                    SELECT ProductID, SUM(Quantity) AS TotalQuantity
                    FROM purchaseorderdetails
                    WHERE PurchaseOrderID = @PurchaseOrderID
                    GROUP BY ProductID";  // ✅ 直接聚合，减少查询次数

                        Dictionary<int, int> stockUpdates = new Dictionary<int, int>();

                        using (var cmd = new MySqlCommand(queryOrderSql, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@PurchaseOrderID", PurchaseOrderID);

                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    int productId = reader.GetInt32("ProductID");
                                    int quantity = reader.GetInt32("TotalQuantity"); // ✅ 计算总采购数量
                                    stockUpdates[productId] = quantity;
                                }
                            }
                        }

                        if (stockUpdates.Count == 0)
                        {
                            transaction.Rollback();
                            return JsonResponseHelper.CreateJsonResponse(new { success = false, message = "未找到对应采购订单的产品信息" });
                        }

                        // ✅ 2️⃣ 批量更新产品库存
                        string updateStockSql = "UPDATE products SET StockQuantity = StockQuantity + CASE ProductID ";
                        foreach (var item in stockUpdates)
                        {
                            updateStockSql += $"WHEN {item.Key} THEN {item.Value} ";
                        }
                        updateStockSql += "END WHERE ProductID IN (" + string.Join(",", stockUpdates.Keys) + ")";

                        using (var cmd = new MySqlCommand(updateStockSql, connection, transaction))
                        {
                            cmd.ExecuteNonQuery();  // ✅ 一次性执行所有更新
                        }

                        string updateStatusSql = @"Update purchaseorders SET Status = '已入库' WHERE PurchaseOrderID = @PurchaseOrderID ";
                        using (var cmd = new MySqlCommand(updateStatusSql, connection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@PurchaseOrderID", PurchaseOrderID);
                            cmd.ExecuteNonQuery();
                        }

                            // ✅ 3️⃣ 提交事务
                            transaction.Commit();
                        return JsonResponseHelper.CreateJsonResponse(new { success = true, message = "库存已成功更新" });
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return JsonResponseHelper.CreateJsonResponse(new { success = false, message = "发生错误：" + ex.Message });
                    }
                }
            }
        }

        #endregion
    }
}
