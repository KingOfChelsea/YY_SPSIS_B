using FenjiuCapstone.Models.Product;
using MySql.Data.MySqlClient;
using sales_managers.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace FenjiuCapstone.Controllers
{
    public class ProductsController : ApiController
    {
        /// <summary>
        /// 数据库操作对象
        /// </summary>
        DbAccess db = new DbAccess();

        #region 1.产品数据上传接口 create by Zane Xu 2025-1-10
        /// <summary>
        /// 产品数据上传接口
        /// </summary>
        /// <param name="request">上传参数</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/products/add")]
        public HttpResponseMessage CreateProduct([FromBody] ProductRequest request)
        {
            try
            {
                // 检查请求数据是否完整
                if (string.IsNullOrEmpty(request.ProductName) || string.IsNullOrEmpty(request.Category) || request.UnitPrice <= 0)
                {
                    return JsonResponseHelper.CreateJsonResponse(new { success = false, message = "输入数据不完整或无效" });
                }

                // 插入产品数据，StockQuantity 默认为 0
                string sql = @"
                INSERT INTO Products (ProductName, Category, Unit, UnitPrice, StockQuantity,SerialNumber)
                VALUES (@ProductName, @Category, @Unit, @UnitPrice, 0, @SerialNumber)";

                using (var connection = DbAccess.connectingMySql())
                {
                    using (var cmd = new MySqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@ProductName", request.ProductName);
                        cmd.Parameters.AddWithValue("@Category", request.Category);
                        cmd.Parameters.AddWithValue("@Unit", request.Unit);
                        cmd.Parameters.AddWithValue("@UnitPrice", request.UnitPrice);
                        cmd.Parameters.AddWithValue("@SerialNumber", request.SerialNumber);
                        cmd.ExecuteNonQuery();
                    }
                }

                // 返回成功信息
                return JsonResponseHelper.CreateJsonResponse(new { success = true, message = "产品插入成功" });
            }
            catch (Exception ex)
            {
                // 返回错误信息
                return JsonResponseHelper.CreateJsonResponse(new { success = false, message = $"发生错误：{ex.Message}" });
            }
        }
        #endregion

        #region 2.查询产品信息接口 create by Zane Xu 2025-1-10
        /// <summary>
        /// 查询产品信息接口
        /// </summary>
        /// <param name="request">查询参数</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/products/query")]
        public HttpResponseMessage QueryProducts([FromBody] ProductQueryRequest request)
        {
            try
            {
                // 创建查询 SQL 和参数集合
                StringBuilder sqlBuilder = new StringBuilder();
                    sqlBuilder.Append(@"
            SELECT ProductID, ProductName, Category, Unit, UnitPrice, StockQuantity, CreatedAt, SerialNumber
            FROM Products
            WHERE 1 = 1");  // 始终返回 true，方便后续拼接过滤条件

                List<MySqlParameter> parameters = new List<MySqlParameter>();

                // 根据请求的条件动态拼接查询条件
                if (!string.IsNullOrEmpty(request.ProductName))
                {
                    sqlBuilder.Append(" AND ProductName LIKE @ProductName");
                    parameters.Add(new MySqlParameter("@ProductName", $"%{request.ProductName}%"));
                }

                if (!string.IsNullOrEmpty(request.Category))
                {
                    sqlBuilder.Append(" AND Category LIKE @Category");
                    parameters.Add(new MySqlParameter("@Category", $"%{request.Category}%"));
                }

                if (request.UnitPrice.HasValue)
                {
                    sqlBuilder.Append(" AND UnitPrice = @UnitPrice");
                    parameters.Add(new MySqlParameter("@UnitPrice", request.UnitPrice.Value));
                }

                if (request.CreatedAtStart.HasValue)
                {
                    sqlBuilder.Append(" AND CreatedAt >= @CreatedAtStart");
                    parameters.Add(new MySqlParameter("@CreatedAtStart", request.CreatedAtStart.Value));
                }

                // 执行查询
                var connection = DbAccess.connectingMySql();
                var command = new MySqlCommand(sqlBuilder.ToString(), connection);

                foreach (var param in parameters)
                {
                    command.Parameters.Add(param);
                }

                var reader = command.ExecuteReader();

                // 将结果转化为列表
                List<Product> products = new List<Product>();
                while (reader.Read())
                {
                    products.Add(new Product
                    {
                        ProductID = reader.GetInt32("ProductID"),
                        ProductName = reader.IsDBNull(reader.GetOrdinal("ProductName")) ? string.Empty : reader.GetString("ProductName"),
                        Category = reader.IsDBNull(reader.GetOrdinal("Category")) ? string.Empty : reader.GetString("Category"),
                        Unit = reader.IsDBNull(reader.GetOrdinal("Unit")) ? string.Empty : reader.GetString("Unit"),
                        UnitPrice = reader.IsDBNull(reader.GetOrdinal("UnitPrice")) ? 0 : reader.GetDecimal("UnitPrice"),
                        StockQuantity = reader.IsDBNull(reader.GetOrdinal("StockQuantity")) ? 0 : reader.GetInt32("StockQuantity"),
                        CreatedAt = reader.IsDBNull(reader.GetOrdinal("CreatedAt")) ? string.Empty : reader.GetDateTime("CreatedAt").ToString("yyyy-MM-dd"),  // 格式化日期为 yyyy-MM-dd
                        SerialNumber = reader.IsDBNull(reader.GetOrdinal("SerialNumber")) ? string.Empty : reader.GetString("SerialNumber")
                    });
                }

                reader.Close();

                // 返回查询结果
                return JsonResponseHelper.CreateJsonResponse(new { success = true, data = products });
            }
            catch (Exception ex)
            {
                // 返回错误信息
                return JsonResponseHelper.CreateJsonResponse(new { success = false, message = $"发生错误：{ex.Message}" });
            }
        }
        #endregion

    }
}
