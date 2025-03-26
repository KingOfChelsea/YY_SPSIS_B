using FenjiuCapstone.Models.Email;
using FenjiuCapstone.Models.Sales;
using FenjiuCapstone.Tools;
using MySql.Data.MySqlClient;
using sales_managers.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FenjiuCapstone.Controllers
{
    /// <summary>
    /// 客户端发送邮箱
    /// </summary>
    public class QuotesController : ApiController
    {
        [HttpPost]
        [Route("api/quotes/send-quote")]
        public HttpResponseMessage SendQuote([FromBody] QuoteRequest request)
        {
            if (string.IsNullOrEmpty(request.CustomerEmail))
                return JsonResponseHelper.CreateJsonResponse(new { success = false, message = "客户邮箱不能为空" });
            // 获取客户订单
            string sql = @"
            SELECT so.OrderID, so.OrderDate, so.TotalAmount, so.Status, c.CustomerName
            FROM SalesOrders so
            JOIN Customers c ON so.CustomerID = c.CustomerID
            WHERE c.Email = @CustomerEmail AND so.OrderID = @OrderID";

            List<SalesOrder> orders = new List<SalesOrder>();
            using (var connection = DbAccess.connectingMySql())
            {
                using (var cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@CustomerEmail", request.CustomerEmail);
                    cmd.Parameters.AddWithValue("@OrderID", request.OrderId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            orders.Add(new SalesOrder
                            {
                                OrderID = reader.GetInt32("OrderID"),
                                OrderDate = reader.GetDateTime("OrderDate"),
                                TotalAmount = reader.GetDecimal("TotalAmount"),
                                Status = reader.GetString("Status"),
                                CustomerID = 0 // 这里不需要客户ID
                            });
                        }
                    }
                }
            }

            if (orders.Count == 0)
                return JsonResponseHelper.CreateJsonResponse(new { success = false, message = "未找到该客户的订单" });
            
            // 获取订单明细
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

            // 生成 HTML 格式报价单
            string emailBody = new UseEmail().GenerateQuoteHtml(orders);

            // 发送邮件
            bool emailSent = new UseEmail().SendEmail(request.CustomerEmail, "您的报价单", emailBody);

            if (emailSent)
                return JsonResponseHelper.CreateJsonResponse(new { success = true, message = "报价单已发送到客户邮箱" });
            else
                return JsonResponseHelper.CreateJsonResponse(new { success = false, message = "邮件发送失败" });
        }
    }
}
