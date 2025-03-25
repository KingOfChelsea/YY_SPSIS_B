using FenjiuCapstone.Models.Sales;
using sales_managers.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using System.Web;

namespace FenjiuCapstone.Tools
{
    /// <summary>
    /// 邮箱类
    /// </summary>
    public class UseEmail
    {
        /// <summary>
        /// 生成 HTML 格式的报价单 Created By Zane Xu 2025-3-13
        /// </summary>
        public string GenerateQuoteHtml(List<SalesOrder> orders)
        {
            StringBuilder html = new StringBuilder();

            // 添加 HTML 头部和内联 CSS 样式 
            html.Append(@"
    <html>
    <head>
        <style>
            body { font-family: Arial, sans-serif; margin: 20px; background-color: #f4f4f4; }
            h2 { color: #e74c3c; text-align: center; font-size: 28px; margin-bottom: 20px; }
            .company-info { background-color: #3498db; color: white; padding: 15px; text-align: center; border-radius: 10px; margin-bottom: 20px; }
            .company-info h3 { margin: 0; font-size: 24px; }
            .company-info p { margin: 5px 0; font-size: 16px; }
            table { width: 100%; border-collapse: collapse; margin-bottom: 20px; background-color: white; border-radius: 10px; overflow: hidden; }
            th, td { padding: 12px; text-align: left; border: 1px solid #ddd; }
            th { background-color: #e74c3c; color: white; font-weight: bold; }
            tr:nth-child(even) { background-color: #f9f9f9; }
            tr:hover { background-color: #f1f1f1; }
            .detail-table { width: 100%; margin: 0; background-color: #fff; }
            .detail-table th { background-color: #3498db; color: white; }
            .detail-table tr:hover { background-color: #e2e6ea; }
            .footer { text-align: center; margin-top: 20px; color: #666; font-size: 14px; }
        </style>
    </head>
    <body>
    ");

            // 公司信息 
            html.Append(@"
    <div class='company-info'>
        <h3>广州翊宇电子商务有限公司</h3>
        <p>地址：广州市天河区珠江新城CBD大厦18楼</p>
        <p>电话：020-12345678 | 邮箱：info@yiyu.com</p> 
        <p>网址：www.yiyu.com</p> 
    </div>
    ");

            // 报价单标题 
            html.Append("<h2>报价单</h2>");

            // 主表格 
            html.Append("<table>");
            html.Append("<tr><th>订单ID</th><th>订单日期</th><th>总金额</th><th>状态</th></tr>");

            foreach (var order in orders)
            {
                // 将SalesOrder对象序列化为JSON字符串 
                string jsonOrder = JsonSerializer.Serialize(order);

                // 对JSON字符串进行URL编码，确保在URL中正确传递 
                string encodedOrder = HttpUtility.UrlEncode(jsonOrder);

                // 主订单行 
                html.Append($"<tr><td>{order.OrderID}</td><td>{order.OrderDate:yyyy-MM-dd}</td><td>{order.TotalAmount:C}</td><td>{order.Status}</td></tr>");

                // 订单明细表格 
                html.Append("<tr><td colspan='4'>");
                html.Append("<table class='detail-table'>");
                html.Append("<tr><th>产品ID</th><th>数量</th><th>单价</th><th>小计</th></tr>");

                foreach (var detail in order.OrderDetails)
                {
                    html.Append($"<tr><td>{detail.ProductID}</td><td>{detail.Quantity}</td><td>{detail.Price:C}</td><td>{detail.SubTotal:C}</td></tr>");
                }

                html.Append("</table></td></tr>");

                html.Append($"<a href='http://localhost:5173/H5?order={encodedOrder}' style='text-decoration: none; font-size: larger;'>签约报价单</a>");
            }

            html.Append("</table>");

            // 页脚 
            html.Append("<div class='footer'>");
            html.Append("感谢您选择广州翊宇电子商务有限公司！如有任何问题，请随时联系我们。");
            html.Append("</div>");

            // 结束 HTML 
            html.Append("</body></html>");

            return html.ToString();
        }

        /// <summary>
        /// 发送邮件 Created By Zane Xu 2025-3-13
        /// </summary>
        public bool SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                var smtpClient = new SmtpClient("smtp.163.com")
                {
                    Port = 25,
                    Credentials = new NetworkCredential("globalbay_ms@163.com", "STfemGJxZzFJnW7H"),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("globalbay_ms@163.com"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(toEmail);

                smtpClient.Send(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                JsonResponseHelper.CreateJsonResponse(new { success = false, message = ex.Message });
                return false;
            }
        }
    }
}