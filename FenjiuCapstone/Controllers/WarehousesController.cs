using FenjiuCapstone.Models.Inventory;
using FenjiuCapstone.Models.Inventory.products_info;
using FenjiuCapstone.Models.Inventory.Transfer;
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

    public class WarehousesController : ApiController
    {
        #region 1.获取所有厂库 create by Zane Xu 2025-1-10
        /// <summary>
        /// 1. 获取所有厂库 GET接口
        /// </summary>
        /// <returns></returns>
        [Route("api/warehouses/getAll")]
        [HttpGet]
        public HttpResponseMessage GetAllWarehouses()
        {
            try
            {
                string sql = "SELECT * FROM Warehouses";
                using (MySqlDataReader reader = DbAccess.read(sql))
                {
                    List<Warehouse> warehouses = new List<Warehouse>();
                    while (reader.Read())
                    {
                        warehouses.Add(new Warehouse
                        {
                            WarehouseID = reader.GetInt32("WarehouseID"),
                            WarehouseName = reader.GetString("WarehouseName"),
                            Location = reader.GetString("Location"),
                            ContactPerson = reader.IsDBNull(reader.GetOrdinal("ContactPerson")) ? string.Empty : reader.GetString("ContactPerson"),
                            ContactNumber = reader.IsDBNull(reader.GetOrdinal("ContactNumber")) ? string.Empty : reader.GetString("ContactNumber"),
                            CreateTime = reader.GetDateTime("CreateTime")
                        });
                    }
                    return JsonResponseHelper.CreateJsonResponse(new { success = true, data = warehouses });
                }
            }
            catch (Exception ex)
            {
                return JsonResponseHelper.CreateJsonResponse(new { success = false, message = ex.Message });
            }
        }
        #endregion

        #region 2.多条件查询仓库 create by Zane Xu 2025-1-10
        /// <summary>
        /// 2.多条件查询仓库
        /// </summary>
        /// <param name="request">查询参数</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/warehouse/search")]
        public HttpResponseMessage SearchWarehouses([FromBody] WarehouseSearchRequest request)
        {
            try
            {
                // 初始查询语句
                StringBuilder sqlBuilder = new StringBuilder("SELECT * FROM Warehouses WHERE 1 = 1");

                // 参数列表
                List<MySqlParameter> parameters = new List<MySqlParameter>();

                // 根据条件动态构建查询语句
                if (request.WarehouseID.HasValue)
                {
                    sqlBuilder.Append(" AND WarehouseID = @WarehouseID");
                    parameters.Add(new MySqlParameter("@WarehouseID", request.WarehouseID.Value));
                }

                if (!string.IsNullOrEmpty(request.WarehouseName))
                {
                    sqlBuilder.Append(" AND WarehouseName LIKE @WarehouseName");
                    parameters.Add(new MySqlParameter("@WarehouseName", "%" + request.WarehouseName + "%"));
                }

                if (!string.IsNullOrEmpty(request.ContactPerson))
                {
                    sqlBuilder.Append(" AND ContactPerson LIKE @ContactPerson");
                    parameters.Add(new MySqlParameter("@ContactPerson", "%" + request.ContactPerson + "%"));
                }

                if (!string.IsNullOrEmpty(request.ContactNumber))
                {
                    sqlBuilder.Append(" AND ContactNumber LIKE @ContactNumber");
                    parameters.Add(new MySqlParameter("@ContactNumber", "%" + request.ContactNumber + "%"));
                }

                // 执行查询
                using (var connection = DbAccess.connectingMySql())
                {
                    using (var cmd = new MySqlCommand(sqlBuilder.ToString(), connection))
                    {
                        // 添加参数
                        foreach (var param in parameters)
                        {
                            cmd.Parameters.Add(param);
                        }

                        using (var reader = cmd.ExecuteReader())
                        {
                            List<Warehouse> warehouses = new List<Warehouse>();
                            while (reader.Read())
                            {
                                warehouses.Add(new Warehouse
                                {
                                    WarehouseID = reader.GetInt32("WarehouseID"),
                                    WarehouseName = reader.GetString("WarehouseName"),
                                    Location = reader.GetString("Location"),
                                    ContactPerson = reader.IsDBNull(reader.GetOrdinal("ContactPerson")) ? string.Empty : reader.GetString("ContactPerson"),
                                    ContactNumber = reader.IsDBNull(reader.GetOrdinal("ContactNumber")) ? string.Empty : reader.GetString("ContactNumber"),
                                    CreateTime = reader.GetDateTime("CreateTime")
                                });
                            }

                            if (warehouses.Count > 0)
                                return JsonResponseHelper.CreateJsonResponse(new { success = true, data = warehouses });

                            return JsonResponseHelper.CreateJsonResponse(new { success = false, message = "未找到匹配的厂库" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return JsonResponseHelper.CreateJsonResponse(new { success = false, message = ex.Message });
            }
        }
        #endregion

        #region  3.创建仓库 create by Zane Xu 2025-1-10

        /// <summary>
        /// 创建仓库
        /// </summary>
        /// <param name="warehouse"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/warehouse/create")]
        public HttpResponseMessage CreateWarehouse([FromBody] Warehouse warehouse)
        {
            try
            {
                string sql = $@"
                INSERT INTO Warehouses (WarehouseName, Location, ContactPerson, ContactNumber) 
                VALUES ('{warehouse.WarehouseName}', '{warehouse.Location}', 
                        {(string.IsNullOrEmpty(warehouse.ContactPerson) ? "NULL" : $"'{warehouse.ContactPerson}'")}, 
                        {(string.IsNullOrEmpty(warehouse.ContactNumber) ? "NULL" : $"'{warehouse.ContactNumber}'")})";

                int result = new DbAccess().Execute(sql);
                if (result > 0)
                    return JsonResponseHelper.CreateJsonResponse(new { success = true, message = "厂库新增成功" });

                return JsonResponseHelper.CreateJsonResponse(new { success = false, message = "新增失败" });
            }
            catch (Exception ex)
            {
                return JsonResponseHelper.CreateJsonResponse(new { success = false, message = ex.Message });
            }
        }
        #endregion


        // 4. 更新厂库
        [HttpPut]
        [Route("api/warehouse/update/{id}")]
        public HttpResponseMessage UpdateWarehouse(int id, [FromBody] Warehouse warehouse)
        {
            try
            {
                string sql = $@"
                UPDATE Warehouses 
                SET WarehouseName = '{warehouse.WarehouseName}', 
                    Location = '{warehouse.Location}', 
                    ContactPerson = {(string.IsNullOrEmpty(warehouse.ContactPerson) ? "NULL" : $"'{warehouse.ContactPerson}'")}, 
                    ContactNumber = {(string.IsNullOrEmpty(warehouse.ContactNumber) ? "NULL" : $"'{warehouse.ContactNumber}'")}
                WHERE WarehouseID = {id}";

                int result = new DbAccess().Execute(sql);
                if (result > 0)
                {
                    return JsonResponseHelper.CreateJsonResponse(new { success = true, message = "厂库更新成功" });
                }

                return JsonResponseHelper.CreateJsonResponse(new { success = false, message = "更新失败" });
            }
            catch (Exception ex)
            {
                return JsonResponseHelper.CreateJsonResponse(new { success = false, message = ex.Message });
            }
        }

        // 5. 删除厂库
        [HttpDelete]
        [Route("api/warehouse/delete/{id}")]
        public HttpResponseMessage DeleteWarehouse(int id)
        {
            try
            {
                string sql = $"DELETE FROM Warehouses WHERE WarehouseID = {id}";
                int result = new DbAccess().Execute(sql);
                if (result > 0)
                    return JsonResponseHelper.CreateJsonResponse(new { success = true, message = "厂库删除成功" });

                return JsonResponseHelper.CreateJsonResponse(new { success = false, message = "删除失败，可能厂库不存在" });
            }
            catch (Exception ex)
            {
                return JsonResponseHelper.CreateJsonResponse(new { success = false, message = ex.Message });
            }
        }

        // 6.库存分配
        [HttpPost]
        [Route("api/warehouse/distribute")]
        public HttpResponseMessage DistributeInventory([FromBody] InventoryDistributionRequest request)
        {
            try
            {
                // 1. 获取当前产品的库存量
                string getStockSql = "SELECT StockQuantity FROM Products WHERE ProductID = @ProductID";
                int currentStock = 0;

                using (var connection = DbAccess.connectingMySql())
                {
                    using (var cmd = new MySqlCommand(getStockSql, connection))
                    {
                        cmd.Parameters.AddWithValue("@ProductID", request.ProductID);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                currentStock = reader.GetInt32("StockQuantity");
                            }
                            else
                            {
                                return JsonResponseHelper.CreateJsonResponse(new { success = false, message = "产品未找到" });
                            }
                        }
                    }
                }

                // 2. 查询该产品在各个仓库的已分配库存量
                string getAllocatedStockSql = @"
            SELECT WarehouseID, SUM(Quantity) AS AllocatedQuantity
            FROM InventoryIn
            WHERE ProductID = @ProductID
            GROUP BY WarehouseID";

                int totalAllocatedStock = 0;
                using (var connection = DbAccess.connectingMySql())
                {
                    using (var cmd = new MySqlCommand(getAllocatedStockSql, connection))
                    {
                        cmd.Parameters.AddWithValue("@ProductID", request.ProductID);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                totalAllocatedStock += reader.GetInt32("AllocatedQuantity");
                            }
                        }
                    }
                }

                // 3. 计算剩余库存量
                int remainingStock = currentStock - totalAllocatedStock;

                // 4. 检查分配数量是否超过剩余库存量
                if (request.Quantity > remainingStock)
                {
                    return JsonResponseHelper.CreateJsonResponse(new { success = false, message = "分配数量不能超过剩余库存量" });
                }

                // 5. 插入库存分配记录到 InventoryIn 表
                string insertInventoryInSql = @"
            INSERT INTO InventoryIn (WarehouseID, ProductID, Quantity, SupplierID)
            VALUES (@WarehouseID, @ProductID, @Quantity, @SupplierID)";

                using (var connection = DbAccess.connectingMySql())
                {
                    using (var cmd = new MySqlCommand(insertInventoryInSql, connection))
                    {
                        cmd.Parameters.AddWithValue("@WarehouseID", request.WarehouseID);
                        cmd.Parameters.AddWithValue("@ProductID", request.ProductID);
                        cmd.Parameters.AddWithValue("@Quantity", request.Quantity);
                        cmd.Parameters.AddWithValue("@SupplierID", request.SupplierID);
                        cmd.ExecuteNonQuery();
                    }
                }

                // 6. 返回成功响应
                return JsonResponseHelper.CreateJsonResponse(new { success = true, message = "库存分配成功" });
            }
            catch (Exception ex)
            {
                return JsonResponseHelper.CreateJsonResponse(new { success = false, message = ex.Message });
            }
        }

        // 7.库存分配前置
        [HttpGet]
        [Route("api/warehouse/products-info")]
        public HttpResponseMessage GetProductsInfo()
        {
            try
            {
                // 1. 获取所有产品信息
                string getProductSql = "SELECT ProductID, ProductName FROM Products";
                List<ProductInfoResponse> products = new List<ProductInfoResponse>();

                using (var connection = DbAccess.connectingMySql())
                {
                    using (var cmd = new MySqlCommand(getProductSql, connection))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                products.Add(new ProductInfoResponse
                                {
                                    ProductID = reader.GetInt32("ProductID"),
                                    ProductName = reader.GetString("ProductName"),
                                    RemainingStock = 0 // 默认设置为 0，稍后会更新
                                });
                            }
                        }
                    }
                }

                // 2. 获取所有仓库信息
                string getWarehouseSql = "SELECT WarehouseID, WarehouseName FROM Warehouses";
                List<WarehouseInfoResponse> warehouses = new List<WarehouseInfoResponse>();

                using (var connection = DbAccess.connectingMySql())
                {
                    using (var cmd = new MySqlCommand(getWarehouseSql, connection))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                warehouses.Add(new WarehouseInfoResponse
                                {
                                    WarehouseID = reader.GetInt32("WarehouseID"),
                                    WarehouseName = reader.GetString("WarehouseName")
                                });
                            }
                        }
                    }
                }

                // 3. 获取所有供应商信息
                string getSupplierSql = "SELECT SupplierID, SupplierName FROM Suppliers";
                List<SupplierInfoResponse> suppliers = new List<SupplierInfoResponse>();

                using (var connection = DbAccess.connectingMySql())
                {
                    using (var cmd = new MySqlCommand(getSupplierSql, connection))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                suppliers.Add(new SupplierInfoResponse
                                {
                                    SupplierID = reader.GetInt32("SupplierID"),
                                    SupplierName = reader.GetString("SupplierName")
                                });
                            }
                        }
                    }
                }

                // 4. 计算每个产品的剩余库存量
                foreach (var product in products)
                {
                    string getStockSql = "SELECT StockQuantity FROM Products WHERE ProductID = @ProductID";
                    int currentStock = 0;

                    using (var connection = DbAccess.connectingMySql())
                    {
                        using (var cmd = new MySqlCommand(getStockSql, connection))
                        {
                            cmd.Parameters.AddWithValue("@ProductID", product.ProductID);
                            using (var reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    currentStock = reader.IsDBNull(reader.GetOrdinal("StockQuantity")) ? 0 : reader.GetInt32("StockQuantity");
                                }
                            }
                        }
                    }

                    // 获取已分配到仓库的库存量
                    string getAllocatedStockSql = @"
                SELECT SUM(Quantity) AS AllocatedQuantity
                FROM InventoryIn
                WHERE ProductID = @ProductID";

                    int allocatedStock = 0;

                    using (var connection = DbAccess.connectingMySql())
                    {
                        using (var cmd = new MySqlCommand(getAllocatedStockSql, connection))
                        {
                            cmd.Parameters.AddWithValue("@ProductID", product.ProductID);
                            using (var reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    allocatedStock = reader.IsDBNull(reader.GetOrdinal("AllocatedQuantity")) ? 0 : reader.GetInt32("AllocatedQuantity");
                                }
                            }
                        }
                    }

                    // 计算剩余可分配库存
                    product.RemainingStock = currentStock - allocatedStock;
                }

                // 5. 过滤剩余库存量大于 0 的产品
                var availableProducts = products.Where(p => p.RemainingStock > 0).ToList();

                // 6. 返回所有数据（产品、仓库、供应商以及剩余库存量）
                var response = new
                {
                    success = true,
                    products = availableProducts, // 只返回剩余库存量大于 0 的产品
                    warehouses = warehouses,
                    suppliers = suppliers
                };

                return JsonResponseHelper.CreateJsonResponse(response);
            }
            catch (Exception ex)
            {
                return JsonResponseHelper.CreateJsonResponse(new { success = false, message = ex.Message });
            }
        }

        // 8.分配情况
        [HttpPost]
        [Route("api/warehouse/inventory-statistics")]
        public HttpResponseMessage GetWarehouseInventoryStatistics([FromBody] WarehouseInventoryRequest request)
        {
            try
            {
                // SQL 查询：查询仓库入库统计信息，按仓库分组
                string getInventorySql = @"
            SELECT 
                i.WarehouseID, 
                w.WarehouseName, 
                p.ProductID, 
                p.ProductName, 
                SUM(i.Quantity) AS TotalQuantity
            FROM InventoryIn i
            JOIN Warehouses w ON i.WarehouseID = w.WarehouseID
            JOIN Products p ON i.ProductID = p.ProductID
            WHERE 
                (@WarehouseID IS NULL OR i.WarehouseID = @WarehouseID)  -- 如果传入的仓库ID为空，返回所有仓库数据
                AND (@StartDate IS NULL OR i.InDate >= @StartDate)   -- 如果传入的起始日期为空，返回所有数据
                AND (@EndDate IS NULL OR i.InDate <= @EndDate)       -- 如果传入的结束日期为空，返回所有数据
            GROUP BY i.WarehouseID, w.WarehouseName, p.ProductID, p.ProductName
            ORDER BY i.WarehouseID ASC, p.ProductName"; // --按WarehouseID升序排列，产品名称按默认顺序

                List<WarehouseInventoryResponse> warehouseStats = new List<WarehouseInventoryResponse>();

                using (var connection = DbAccess.connectingMySql())
                {
                    using (var cmd = new MySqlCommand(getInventorySql, connection))
                    {
                        // 设置查询参数，如果为空则为 NULL
                        cmd.Parameters.AddWithValue("@WarehouseID", request.WarehouseID ?? (object)DBNull.Value);  // 如果为空，查询所有仓库
                        cmd.Parameters.AddWithValue("@StartDate", request.StartDate ?? (object)DBNull.Value);      // 如果为空，查询所有日期
                        cmd.Parameters.AddWithValue("@EndDate", request.EndDate ?? (object)DBNull.Value);          // 如果为空，查询所有日期

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // 为每个仓库检查它是否已经存在于列表中
                                var warehouse = warehouseStats.FirstOrDefault(w => w.WarehouseID == reader.GetInt32("WarehouseID"));
                                if (warehouse == null)
                                {
                                    // 新仓库，初始化数据
                                    warehouse = new WarehouseInventoryResponse
                                    {
                                        WarehouseID = reader.GetInt32("WarehouseID"),
                                        WarehouseName = reader.GetString("WarehouseName"),
                                        Products = new List<ProductInventoryResponse>()
                                    };
                                    warehouseStats.Add(warehouse);
                                }

                                // 将产品数据添加到当前仓库
                                warehouse.Products.Add(new ProductInventoryResponse
                                {
                                    ProductName = reader.GetString("ProductName"),
                                    TotalQuantity = reader.GetInt32("TotalQuantity")
                                });
                            }
                        }
                    }
                }

                // 2. 返回结果
                var response = new
                {
                    success = true,
                    data = warehouseStats
                };

                return JsonResponseHelper.CreateJsonResponse(response);
            }
            catch (Exception ex)
            {
                return JsonResponseHelper.CreateJsonResponse(new { success = false, message = ex.Message });
            }
        }

        // 9.创建调拨记录
        [HttpPost]
        [Route("api/warehouse/inventory-transfer")]
        public HttpResponseMessage CreateInventoryTransfer([FromBody] InventoryTransferRequest request)
        {
            try
            {
                // 1. SQL 查询：检查是否有足够的库存进行调拨 (查询 InventoryIn 表)
                string checkStockSql = @"
            SELECT SUM(Quantity) 
            FROM InventoryIn 
            WHERE WarehouseID = @FromWarehouseID 
            AND ProductID = @ProductID 
            AND Quantity >= @Quantity";

                using (var connection = DbAccess.connectingMySql())
                {
                    var cmdCheckStock = new MySqlCommand(checkStockSql, connection);
                    cmdCheckStock.Parameters.AddWithValue("@FromWarehouseID", request.FromWarehouseID);
                    cmdCheckStock.Parameters.AddWithValue("@ProductID", request.ProductID);
                    cmdCheckStock.Parameters.AddWithValue("@Quantity", request.Quantity);

                    // 获取可用库存
                    var availableStockObj = cmdCheckStock.ExecuteScalar();
                    int availableStock = (availableStockObj == DBNull.Value) ? 0 : Convert.ToInt32(availableStockObj);

                    // 如果库存不足，返回错误
                    if (availableStock < request.Quantity)
                    {
                        return JsonResponseHelper.CreateJsonResponse(new { success = false, message = "库存不足，无法进行调拨" });
                    }

                    // 2. 创建调拨记录
                    string createTransferSql = @"
                                        INSERT INTO InventoryTransfers (FromWarehouseID, ToWarehouseID, ProductID, Quantity, Status)
                                        VALUES (@FromWarehouseID, @ToWarehouseID, @ProductID, @Quantity, 'Pending')";
                    var cmdCreateTransfer = new MySqlCommand(createTransferSql, connection);
                    cmdCreateTransfer.Parameters.AddWithValue("@FromWarehouseID", request.FromWarehouseID);
                    cmdCreateTransfer.Parameters.AddWithValue("@ToWarehouseID", request.ToWarehouseID);
                    cmdCreateTransfer.Parameters.AddWithValue("@ProductID", request.ProductID);
                    cmdCreateTransfer.Parameters.AddWithValue("@Quantity", request.Quantity);

                    cmdCreateTransfer.ExecuteNonQuery();

                    // 返回成功响应
                    return JsonResponseHelper.CreateJsonResponse(new { success = true, message = "调拨记录已创建，等待审核" });
                }
            }
            catch (Exception ex)
            {
                return JsonResponseHelper.CreateJsonResponse(new { success = false, message = $"发生错误：{ex.Message}" });
            }
        }

        #region 10.审核调拨记录时更新库存  zhenyu xu 2025/2/23
        // 10.审核调拨记录时更新库存
        [HttpPost]
        [Route("api/warehouse/approve-transfer")]
        public HttpResponseMessage ApproveInventoryTransfer_List([FromBody] ApproveTransferRequest request)
        {
            try
            {
                // 1. 连接数据库
                using (var connection = DbAccess.connectingMySql())
                {
                    var transaction = connection.BeginTransaction(); // 开启事务

                    try
                    {
                        // 2. 查询调拨记录信息，确保该调拨是 "Pending" 状态
                        string getTransferDetailsSql = @"
                                SELECT Quantity, ProductID, FromWarehouseID, ToWarehouseID
                                FROM InventoryTransfers 
                                WHERE TransferID = @TransferID 
                                AND Status = 'Pending'";

                        int quantityToTransfer, productId, fromWarehouseId, toWarehouseId;

                        using (var cmdGetTransferDetails = new MySqlCommand(getTransferDetailsSql, connection, transaction))
                        {
                            cmdGetTransferDetails.Parameters.AddWithValue("@TransferID", request.TransferID);
                            using (var reader = cmdGetTransferDetails.ExecuteReader())
                            {
                                if (!reader.Read()) // 如果找不到调拨记录或状态不是 Pending，则返回错误
                                    return JsonResponseHelper.CreateJsonResponse(new { success = false, message = "调拨记录不存在或已处理" });

                                // 读取调拨记录的详细信息
                                quantityToTransfer = reader.GetInt32("Quantity");     // 需要调拨的数量
                                productId = reader.GetInt32("ProductID");            // 产品ID
                                fromWarehouseId = reader.GetInt32("FromWarehouseID");// 源仓库ID
                                toWarehouseId = reader.GetInt32("ToWarehouseID");    // 目标仓库ID
                            }
                        }

                        // 3. 查询源仓库的库存信息（按入库时间排序，尽量使用最早的库存）
                        string getStockSql = @"
                                    SELECT InID, Quantity 
                                    FROM InventoryIn 
                                    WHERE WarehouseID = @FromWarehouseID 
                                    AND ProductID = @ProductID 
                                    ORDER BY InDate ASC"; // 按入库时间升序排列，优先扣减最早的库存

                        List<(int InID, int StockQuantity)> stockList = new List<(int, int)>(); // 存储可用库存记录

                        using (var cmdGetStock = new MySqlCommand(getStockSql, connection, transaction))
                        {
                            cmdGetStock.Parameters.AddWithValue("@FromWarehouseID", fromWarehouseId);
                            cmdGetStock.Parameters.AddWithValue("@ProductID", productId);

                            using (var reader = cmdGetStock.ExecuteReader())
                            {
                                while (reader.Read())
                                    stockList.Add((reader.GetInt32("InID"), reader.GetInt32("Quantity"))); // 读取库存ID和库存数量
                            }
                        }

                        // 4. 依次扣减库存
                        int remainingQuantity = quantityToTransfer; // 需要扣减的剩余数量
                        foreach (var stock in stockList)
                        {
                            if (remainingQuantity <= 0) break; // 如果已经扣减完，不再执行后续操作

                            // 如果库存充足，直接减少所需数量
                            string updateStockSql = stock.StockQuantity >= remainingQuantity
                                ? "UPDATE InventoryIn SET Quantity = Quantity - @RemainingQuantity WHERE InID = @InID"
                                : "UPDATE InventoryIn SET Quantity = 0 WHERE InID = @InID"; // 若库存不够，则直接清零

                            using (var cmdUpdateStock = new MySqlCommand(updateStockSql, connection, transaction))
                            {
                                cmdUpdateStock.Parameters.AddWithValue("@RemainingQuantity", Math.Min(remainingQuantity, stock.StockQuantity));
                                cmdUpdateStock.Parameters.AddWithValue("@InID", stock.InID);
                                cmdUpdateStock.ExecuteNonQuery();
                            }

                            remainingQuantity -= stock.StockQuantity; // 计算剩余要扣减的数量
                        }

                        // 5. 检查库存是否足够
                        if (remainingQuantity > 0)
                        {
                            transaction.Rollback(); // 如果库存不足，回滚事务
                            return JsonResponseHelper.CreateJsonResponse(new { success = false, message = "库存不足，无法完成调拨" });
                        }

                        // 6. 更新调拨记录状态，标记为 "Approved"
                        string updateTransferStatusSql = @"
                UPDATE InventoryTransfers 
                SET Status = 'Approved' 
                WHERE TransferID = @TransferID";

                        using (var cmdUpdateTransferStatus = new MySqlCommand(updateTransferStatusSql, connection, transaction))
                        {
                            cmdUpdateTransferStatus.Parameters.AddWithValue("@TransferID", request.TransferID);
                            cmdUpdateTransferStatus.ExecuteNonQuery(); // 执行更新语句
                        }

                        // 7. 目标仓库增加库存  Inventory
                        string updateTargetStockSql = @"
                                INSERT INTO InventoryIn (WarehouseID, ProductID, Quantity)
                                VALUES (@ToWarehouseID, @ProductID, @Quantity)
                                ON DUPLICATE KEY UPDATE Quantity = Quantity + @Quantity"; // 如果已经存在该产品库存，则增加数量

                        using (var cmdUpdateTargetStock = new MySqlCommand(updateTargetStockSql, connection, transaction))
                        {
                            cmdUpdateTargetStock.Parameters.AddWithValue("@ToWarehouseID", toWarehouseId);
                            cmdUpdateTargetStock.Parameters.AddWithValue("@ProductID", productId);
                            cmdUpdateTargetStock.Parameters.AddWithValue("@Quantity", quantityToTransfer);
                            cmdUpdateTargetStock.ExecuteNonQuery(); // 更新目标仓库库存
                        }

                        // 8. 提交事务
                        transaction.Commit();
                        return JsonResponseHelper.CreateJsonResponse(new { success = true, message = "调拨记录已审核，并更新库存" });
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback(); // 发生异常时，回滚事务
                        return JsonResponseHelper.CreateJsonResponse(new { success = false, message = $"发生错误：{ex.Message}" });
                    }
                }
            }
            catch (Exception ex)
            {
                return JsonResponseHelper.CreateJsonResponse(new { success = false, message = $"发生错误：{ex.Message}" });
            }
        }
        #endregion

        #region 11.查询审核调拨记录 zhenyu xu 2025/2/23
        [HttpPost]
        [Route("api/warehouse/query-transfers")]
        public HttpResponseMessage QueryInventoryTransfers([FromBody] TransferQueryRequest request)
        {
            try
            {
                using (var connection = DbAccess.connectingMySql())
                {
                    // 1. 构建 SQL 查询
                    StringBuilder sql = new StringBuilder(@"
                SELECT 
                    t.TransferID, 
                    t.ProductID, 
                    p.ProductName, 
                    t.FromWarehouseID, 
                    w1.WarehouseName AS FromWarehouseName, 
                    t.ToWarehouseID, 
                    w2.WarehouseName AS ToWarehouseName, 
                    t.Quantity, 
                    t.TransferDate, 
                    t.Status
                FROM InventoryTransfers t
                JOIN Products p ON t.ProductID = p.ProductID
                JOIN Warehouses w1 ON t.FromWarehouseID = w1.WarehouseID
                JOIN Warehouses w2 ON t.ToWarehouseID = w2.WarehouseID
                WHERE 1=1");

                    // 2. 添加查询参数
                    List<MySqlParameter> parameters = new List<MySqlParameter>();

                    if (request.TransferID.HasValue)
                    {
                        sql.Append(" AND t.TransferID = @TransferID");
                        parameters.Add(new MySqlParameter("@TransferID", request.TransferID));
                    }

                    if (request.ProductID.HasValue)
                    {
                        sql.Append(" AND t.ProductID = @ProductID");
                        parameters.Add(new MySqlParameter("@ProductID", request.ProductID));
                    }

                    if (request.FromWarehouseID.HasValue)
                    {
                        sql.Append(" AND t.FromWarehouseID = @FromWarehouseID");
                        parameters.Add(new MySqlParameter("@FromWarehouseID", request.FromWarehouseID));
                    }

                    if (request.ToWarehouseID.HasValue)
                    {
                        sql.Append(" AND t.ToWarehouseID = @ToWarehouseID");
                        parameters.Add(new MySqlParameter("@ToWarehouseID", request.ToWarehouseID));
                    }

                    if (!string.IsNullOrEmpty(request.Status))
                    {
                        sql.Append(" AND t.Status = @Status");
                        parameters.Add(new MySqlParameter("@Status", request.Status));
                    }

                    // 处理时间范围（数组格式：["2024-02-01", "2024-02-15"]）
                    if (request.DateRange != null && request.DateRange.Count == 2)
                    {
                        sql.Append(" AND t.TransferDate BETWEEN @StartDate AND @EndDate");
                        parameters.Add(new MySqlParameter("@StartDate", request.DateRange[0]));
                        parameters.Add(new MySqlParameter("@EndDate", request.DateRange[1]));
                    }

                    sql.Append(" ORDER BY t.TransferDate DESC"); // 按调拨时间倒序排列

                    // 3. 执行 SQL 查询
                    var cmd = new MySqlCommand(sql.ToString(), connection);
                    cmd.Parameters.AddRange(parameters.ToArray());

                    List<TransferRecordResponse> transferRecords = new List<TransferRecordResponse>();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            transferRecords.Add(new TransferRecordResponse
                            {
                                TransferID = reader.GetInt32("TransferID"),
                                ProductID = reader.GetInt32("ProductID"),
                                ProductName = reader.GetString("ProductName"),
                                FromWarehouseID = reader.GetInt32("FromWarehouseID"),
                                FromWarehouseName = reader.GetString("FromWarehouseName"),
                                ToWarehouseID = reader.GetInt32("ToWarehouseID"),
                                ToWarehouseName = reader.GetString("ToWarehouseName"),
                                Quantity = reader.GetInt32("Quantity"),
                                TransferDate = reader.GetDateTime("TransferDate").ToString("yyyy-MM-dd HH:mm:ss"),
                                Status = reader.GetString("Status")
                            });
                        }
                    }

                    // 4. 返回 JSON 响应
                    return JsonResponseHelper.CreateJsonResponse(new { success = true, data = transferRecords });
                }
            }
            catch (Exception ex)
            {
                return JsonResponseHelper.CreateJsonResponse(new { success = false, message = $"发生错误：{ex.Message}" });
            }
        }
        #endregion


    }
}
