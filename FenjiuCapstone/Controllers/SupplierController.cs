using FenjiuCapstone.Models;
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
    public class SupplierController : ApiController
    {
        #region 1.插入供应商接口 create by zhenyu xu 2025-1-3
        /// <summary>
        /// 插入供应商信息（无需填写创建时间，默认为现在时间）
        /// </summary>
        /// <param name="request">插入供应商信息参数</param>
        /// <returns></returns>
        [Route("api/suppliers")]
        [HttpPost]
        public HttpResponseMessage AddSupplier([FromBody] SupplierRequest request)
        {
            // 检查请求是否有效
            if (string.IsNullOrEmpty(request.SupplierName))
            {
                return JsonResponseHelper.CreateJsonResponse(new { success = false, message = "厂家名称不能为空。" });
            }

            // SQL 插入语句
            string sql = @"
            INSERT INTO Suppliers (SupplierName, ContactPerson, ContactNumber, Address) 
            VALUES (@SupplierName, @ContactPerson, @ContactNumber, @Address);";

            try
            {
                // 使用 DbAccess 连接和执行
                using (var connection = DbAccess.connectingMySql())
                {
                    using (var cmd = new MySqlCommand(sql, connection))
                    {
                        // 添加参数
                        cmd.Parameters.AddWithValue("@SupplierName", request.SupplierName);
                        cmd.Parameters.AddWithValue("@ContactPerson", request.ContactPerson ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@ContactNumber", request.ContactNumber ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Address", request.Address ?? (object)DBNull.Value);

                        // 执行插入操作
                        int rowsAffected = cmd.ExecuteNonQuery();

                        // 返回成功信息
                        if (rowsAffected > 0)
                        {
                            return JsonResponseHelper.CreateJsonResponse(new { success = true, message = "厂家添加成功。" });
                        }
                        else
                        {
                            return JsonResponseHelper.CreateJsonResponse(new { success = false, message = "厂家添加失败。" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 异常处理
                return JsonResponseHelper.CreateJsonResponse(new { success = false, message = $"发生错误：{ex.Message}" });
            }
        }
        #endregion

        #region 2.查询全部供应商数据返回前端使用 create by zhenyu xu 2025-1-3
        /// <summary>
        ///  GET接口查询供应商数据接口，返回给前端使用
        ///  <说明></说明>
        /// </summary>
        /// <returns></returns>
        [Route("api/suppliers/data")]
        [HttpGet]
        public HttpResponseMessage GetSuppliers()
        {
            // SQL 查询语句
            string sql = "SELECT SupplierID, SupplierName, ContactPerson, ContactNumber, Address, CreatedAt FROM Suppliers";

            try
            {
                List<SupplierRequest> suppliers = new List<SupplierRequest>();

                // 使用 DbAccess 执行查询
                using (var reader = DbAccess.read(sql))
                {
                    while (reader.Read())
                    {
                        SupplierRequest supplier = new SupplierRequest
                        {
                            SupplierID = reader.GetInt32("SupplierID"),
                            SupplierName = reader.GetString("SupplierName"),
                            ContactPerson = reader.IsDBNull(reader.GetOrdinal("ContactPerson")) ? null : reader.GetString("ContactPerson"),
                            ContactNumber = reader.IsDBNull(reader.GetOrdinal("ContactNumber")) ? null : reader.GetString("ContactNumber"),
                            Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString("Address"),
                            CreatedAt = reader.GetDateTime("CreatedAt").ToString("yyyy-MM-dd")
                        };

                        suppliers.Add(supplier);
                    }
                }

                // 返回 JSON 响应
                return JsonResponseHelper.CreateJsonResponse(new { success = true, data = suppliers });
            }
            catch (Exception ex)
            {
                // 异常处理
                return JsonResponseHelper.CreateJsonResponse(new { success = false, message = $"发生错误：{ex.Message}" });
            }
        }
        #endregion

        #region 3.根据参数查询供应商数据 create by zhenyu xu 2025-1-3
        /// <summary>
        /// 根据参数查询供应商数据
        /// </summary>
        /// <param name="request">查询参数</param>
        /// <returns></returns>
        [Route("api/suppliers/search")]
        [HttpPost]
        public HttpResponseMessage SearchSuppliers([FromBody] SupplierSearchRequest request)
        {
            StringBuilder sql = new StringBuilder(@"
            SELECT 
                SupplierID, 
                SupplierName, 
                ContactPerson, 
                ContactNumber, 
                Address, 
                CreatedAt
            FROM 
                Suppliers
            WHERE 
                1 = 1");  // 默认总是返回 true，便于后续拼接其他条件

            List<MySqlParameter> parameters = new List<MySqlParameter>();

            // 判断是否至少有一个查询条件
            bool hasCondition = false;

            if (!string.IsNullOrEmpty(request.SupplierName))
            {
                sql.Append(" AND SupplierName LIKE @SupplierName");
                parameters.Add(new MySqlParameter("@SupplierName", "%" + request.SupplierName + "%"));
                hasCondition = true;
            }

            if (!string.IsNullOrEmpty(request.ContactPerson))
            {
                sql.Append(" AND ContactPerson LIKE @ContactPerson");
                parameters.Add(new MySqlParameter("@ContactPerson", "%" + request.ContactPerson + "%"));
                hasCondition = true;
            }

            if (!string.IsNullOrEmpty(request.ContactNumber))
            {
                sql.Append(" AND ContactNumber LIKE @ContactNumber");
                parameters.Add(new MySqlParameter("@ContactNumber", "%" + request.ContactNumber + "%"));
                hasCondition = true;
            }

            if (request.CreatedAt.HasValue)
            {
                sql.Append(" AND DATE(CreatedAt) = @CreatedAt");
                parameters.Add(new MySqlParameter("@CreatedAt", request.CreatedAt.Value.ToString("yyyy-MM-dd")));
                hasCondition = true;
            }

            // 如果没有条件，则返回一个提示或空结果
            if (!hasCondition)
            {
                return JsonResponseHelper.CreateJsonResponse(new { success = false, message = "至少提供一个查询条件" });
            }

            sql.Append(" ORDER BY CreatedAt DESC");  // 排序

            try
            {
                List<SupplierRequest> suppliers = new List<SupplierRequest>();

                using (var connection = DbAccess.connectingMySql())
                {
                    using (var cmd = DbAccess.command(sql.ToString(), connection))
                    {
                        // 添加参数
                        foreach (var param in parameters)
                        {
                            cmd.Parameters.Add(param);
                        }

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                SupplierRequest supplier = new SupplierRequest
                                {
                                    SupplierID = reader.GetInt32("SupplierID"),
                                    SupplierName = reader.GetString("SupplierName"),
                                    ContactPerson = reader.IsDBNull(reader.GetOrdinal("ContactPerson")) ? null : reader.GetString("ContactPerson"),
                                    ContactNumber = reader.IsDBNull(reader.GetOrdinal("ContactNumber")) ? null : reader.GetString("ContactNumber"),
                                    Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString("Address"),
                                    CreatedAt = reader.GetDateTime("CreatedAt").ToString("yyyy-MM-dd")
                                };

                                suppliers.Add(supplier);
                            }
                        }
                    }
                }

                // 返回查询结果
                return JsonResponseHelper.CreateJsonResponse(new
                {
                    success = true,
                    data = suppliers
                });
            }
            catch (Exception ex)
            {
                return JsonResponseHelper.CreateJsonResponse(new { success = false, message = $"发生错误：{ex.Message}" });
            }
        }
        #endregion
    }
}
