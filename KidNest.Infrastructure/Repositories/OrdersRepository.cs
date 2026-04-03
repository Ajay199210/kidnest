using KidNest.Core.Entities;
using KidNest.Core.Interfaces;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Reflection.PortableExecutable;

namespace KidNest.Infrastructure.Repositories
{
    public class OrdersRepository : IOrdersRepository
    {
        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            var orders = new List<Order>();

            string query = @"
                SELECT 
                    o.OrderId, 
                    o.OrderDate, 
                    o.UserId, 
                    o.OrderStatus,
                    o.ModifiedDate,
                    o.ModifiedBy,
                    oi.ProductId,
                    oi.Quantity,
                    p.ProductName, 
                    p.ProductPrice,
                    u.AppUserEmail
                FROM Orders o
                INNER JOIN OrderItems oi ON o.OrderId = oi.OrderId
                INNER JOIN Products p ON p.ProductId = oi.ProductId
                INNER JOIN AppUsers u ON u.AppUserId = o.UserId
                ORDER BY o.OrderDate DESC";

            using (var connection = DbConnectionFactory.CreateConnection())
            using (var command = new SqlCommand(query, connection))
            {
                try
                {
                    await connection.OpenAsync();
                    await using var reader = await command.ExecuteReaderAsync();

                    // Get all column indexes once
                    var orderIdIndex = reader.GetOrdinal("OrderId");
                    var orderDateIndex = reader.GetOrdinal("OrderDate");
                    var userIdIndex = reader.GetOrdinal("UserId");
                    var orderStatusIndex = reader.GetOrdinal("OrderStatus");
                    var modifiedDateIndex = reader.GetOrdinal("ModifiedDate");
                    var modifiedByIndex = reader.GetOrdinal("ModifiedBy");
                    var productIdIndex = reader.GetOrdinal("ProductId");
                    var quantityIndex = reader.GetOrdinal("Quantity");
                    var productNameIndex = reader.GetOrdinal("ProductName");
                    var productPriceIndex = reader.GetOrdinal("ProductPrice");
                    var appUserEmailIndex = reader.GetOrdinal("AppUserEmail");

                    while (await reader.ReadAsync())
                    {
                        var orderId = reader.GetInt32(orderIdIndex);

                        var existingOrder = orders.FirstOrDefault(o => o.Id == orderId);
                        if (existingOrder == null)
                        {
                            existingOrder = new Order
                            {
                                Id = orderId,
                                OrderDate = reader.IsDBNull(orderDateIndex) ? null : reader.GetDateTime(orderDateIndex),
                                UserId = reader.IsDBNull(userIdIndex) ? null : reader.GetInt32(userIdIndex),
                                Status = reader.IsDBNull(orderStatusIndex) ? null : reader.GetString(orderStatusIndex),
                                ModifiedDate = reader.IsDBNull(modifiedDateIndex) ? null : reader.GetDateTime(modifiedDateIndex),
                                ModifiedBy = reader.IsDBNull(modifiedByIndex) ? null : reader.GetString(modifiedByIndex),
                                User = new AppUser
                                {
                                    Email = reader.IsDBNull(appUserEmailIndex) ? null : reader.GetString(appUserEmailIndex),
                                },
                            };

                            orders.Add(existingOrder);
                        }

                        var orderItem = new OrderItem
                        {
                            ProductId = reader.IsDBNull(productIdIndex) ? 0 : reader.GetInt32(productIdIndex),
                            Quantity = reader.IsDBNull(quantityIndex) ? 0 : reader.GetInt32(quantityIndex),
                            ProductName = reader.IsDBNull(productNameIndex) ? null : reader.GetString(productNameIndex),
                            ProductPrice = reader.IsDBNull(productPriceIndex) ? null : reader.GetDecimal(productPriceIndex),
                        };

                        existingOrder.OrderItems!.Add(orderItem);
                    }
                }
                catch (SqlException)
                {
                    throw new Exception("Error while loading orders");
                }
                finally
                {
                    await connection.CloseAsync();
                }
            }

            return orders;
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            string query = @"
                SELECT o.*, oi.*, u.*, p.ProductName, p.ProductPrice, p.ProductDiscount,
                    pv.*, mc.*, ms.*
                FROM [dbo].[Orders] o
                INNER JOIN [dbo].[OrderItems] oi ON o.OrderId = oi.OrderId
                INNER JOIN [dbo].[AppUsers] u ON o.UserId = u.AppUserId
                INNER JOIN [dbo].[Products] p ON p.ProductId = oi.ProductId
                LEFT JOIN [dbo].[ProductVariants] pv ON pv.ProductVariantId = oi.ProductVariantId
                LEFT JOIN [dbo].[MdColors] mc ON mc.MdColorId = pv.ProductVariantColorId
                LEFT JOIN [dbo].[MdSizes] ms ON ms.MdSizeId = pv.ProductVariantSizeId
                WHERE o.OrderId = @OrderId
            ";

            using var connection = DbConnectionFactory.CreateConnection();
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@OrderId", id);

            try
            {
                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                Order? order = null;

                // Column indexes
                int orderIdIndex = reader.GetOrdinal("OrderId");
                int orderDateIndex = reader.GetOrdinal("OrderDate");
                int userIdIndex = reader.GetOrdinal("UserId");
                int statusIndex = reader.GetOrdinal("OrderStatus");
                int modifiedDateIndex = reader.GetOrdinal("ModifiedDate");
                int modifiedByIndex = reader.GetOrdinal("ModifiedBy");

                int orderItemIdIndex = reader.GetOrdinal("OrderItemId");
                int productIdIndex = reader.GetOrdinal("ProductId");
                int quantityIndex = reader.GetOrdinal("Quantity");

                int appUserIdIndex = reader.GetOrdinal("AppUserId");
                int fullNameIndex = reader.GetOrdinal("AppUserFullName");
                int phoneIndex = reader.GetOrdinal("AppUserPhoneNumber");
                int emailIndex = reader.GetOrdinal("AppUserEmail");
                int addressIndex = reader.GetOrdinal("AppUserAddress");
                int dobIndex = reader.GetOrdinal("AppUserDOB");
                int codeIndex = reader.GetOrdinal("AppUserCode");
                int passwordIndex = reader.GetOrdinal("AppUserPassword");
                int lastLoginDateIndex = reader.GetOrdinal("AppUserLastLoginDate");
                int pcNameIndex = reader.GetOrdinal("AppUserLastLogInPCName");
                int isActiveIndex = reader.GetOrdinal("AppUserIsActive");
                int rowVersionIndex = reader.GetOrdinal("AppUserRowVersion");
                int updatedByIndex = reader.GetOrdinal("AppUserUserUpdatedBy");
                int lastUpdatedIndex = reader.GetOrdinal("AppUserLastUpdated");
                int createdByIndex = reader.GetOrdinal("AppUserUserCreatedBy");
                int createdDateIndex = reader.GetOrdinal("AppUserCreatedDate");
                int timeStampIndex = reader.GetOrdinal("tTimeStamp");

                int productNameIndex = reader.GetOrdinal("ProductName");
                int productPriceIndex = reader.GetOrdinal("ProductPrice");
                int productDiscountIndex = reader.GetOrdinal("ProductDiscount");

                int productVariantIdIndex = reader.GetOrdinal("ProductVariantId");
                int productVariantQuantityIndex = reader.GetOrdinal("ProductVariantQuantity");
                int colorIdIndex = reader.GetOrdinal("MdColorId");
                int colorNameIndex = reader.GetOrdinal("MdColorName");
                int colorHexIndex = reader.GetOrdinal("MdColorHexValue");
                int sizeIdIndex = reader.GetOrdinal("MdSizeId");
                int sizeCodeIndex = reader.GetOrdinal("MdSizeCode");

                while (await reader.ReadAsync())
                {
                    if (order == null)
                    {
                        order = new Order
                        {
                            Id = reader.GetInt32(orderIdIndex),
                            OrderDate = reader.IsDBNull(orderDateIndex) ? null : reader.GetDateTime(orderDateIndex),
                            UserId = reader.IsDBNull(userIdIndex) ? null : reader.GetInt32(userIdIndex),
                            Status = reader.IsDBNull(statusIndex) ? null : reader.GetString(statusIndex),
                            ModifiedDate = reader.IsDBNull(modifiedDateIndex) ? null : reader.GetDateTime(modifiedDateIndex),
                            ModifiedBy = reader.IsDBNull(modifiedByIndex) ? null : reader.GetString(modifiedByIndex),
                            OrderItems = new List<OrderItem>(),
                            User = new AppUser
                            {
                                Id = reader.GetInt32(appUserIdIndex),
                                FullName = reader.IsDBNull(fullNameIndex) ? null : reader.GetString(fullNameIndex),
                                PhoneNumber = reader.IsDBNull(phoneIndex) ? null : reader.GetString(phoneIndex),
                                Email = reader.IsDBNull(emailIndex) ? null : reader.GetString(emailIndex),
                                Address = reader.IsDBNull(addressIndex) ? null: reader.GetString(addressIndex),
                                DOB = reader.IsDBNull(dobIndex) ? null : reader.GetDateTime(dobIndex),
                                Code = reader.IsDBNull(codeIndex) ? null : reader.GetString(codeIndex),
                                Password = reader.IsDBNull(passwordIndex) ? null : reader.GetString(passwordIndex),
                                LastLoginDate = reader.IsDBNull(lastLoginDateIndex) ? null : reader.GetDateTime(lastLoginDateIndex),
                                LastLogInPCName = reader.IsDBNull(pcNameIndex) ? null : reader.GetString(pcNameIndex),
                                IsActive = reader.IsDBNull(isActiveIndex) ? null : reader.GetBoolean(isActiveIndex),
                                RowVersion = reader.IsDBNull(rowVersionIndex) ? 0 : reader.GetInt32(rowVersionIndex),
                                UserUpdatedBy = reader.IsDBNull(updatedByIndex) ? null : reader.GetString(updatedByIndex),
                                LastUpdated = reader.IsDBNull(lastUpdatedIndex) ? null : reader.GetDateTime(lastUpdatedIndex),
                                UserCreatedBy = reader.IsDBNull(createdByIndex) ? null : reader.GetString(createdByIndex),
                                CreatedDate = reader.IsDBNull(createdDateIndex) ? null : reader.GetDateTime(createdDateIndex),
                                //TimeStamp = reader.IsDBNull(timeStampIndex) ? null : reader.GetDateTime(timeStampIndex)
                            }
                        };
                    }

                    // Add order item
                    order.OrderItems?.Add(new OrderItem
                    {
                        Id = reader.GetInt32(orderItemIdIndex),
                        OrderId = reader.GetInt32(orderIdIndex),
                        ProductId = reader.GetInt32(productIdIndex),
                        Quantity = reader.GetInt32(quantityIndex),
                        ProductName = reader.IsDBNull(productNameIndex) ? null : reader.GetString(productNameIndex),
                        ProductPrice = reader.IsDBNull(productPriceIndex) ? null : reader.GetDecimal(productPriceIndex),
                        ProductDiscount = reader.IsDBNull(productDiscountIndex) ? null : reader.GetDecimal(productDiscountIndex),
                        ProductVariantId = reader.IsDBNull(productVariantIdIndex) ? null : reader.GetInt32(productVariantIdIndex),

                        MdColor = reader.IsDBNull(colorIdIndex) ? null : new MdColor
                        {
                            Name = reader.IsDBNull(colorNameIndex) ? null : reader.GetString(colorNameIndex),
                            HexValue = reader.IsDBNull(colorHexIndex) ? null : reader.GetString(colorHexIndex),
                            //IsActive = reader.IsDBNull(colorIsActiveIndex) ? null : reader.GetBoolean(colorIsActiveIndex),
                        },

                        MdSize = reader.IsDBNull(sizeIdIndex) ? null : new MdSize
                        {
                            //Id = reader.GetInt32(sizeIdIndex),
                            //Description = reader.IsDBNull(sizeDescriptionIndex) ? null : reader.GetString(sizeDescriptionIndex),
                            SizeCode = reader.IsDBNull(sizeCodeIndex) ? null : reader.GetString(sizeCodeIndex),
                            //IsActive = reader.IsDBNull(sizeIsActiveIndex) ? null : reader.GetBoolean(sizeIsActiveIndex),
                        }
                    });
                }

                return order;
            }
            catch (SqlException)
            {
                throw new Exception("Error loading order with user and items");
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        public async Task<int> AddAsync(Order order, List<OrderItem> items)
        {
            string query = @"INSERT INTO [dbo].[Orders] (
                OrderDate, UserId, OrderStatus, ModifiedDate, ModifiedBy)
                OUTPUT INSERTED.OrderId
                VALUES (@OrderDate, @UserId, @OrderStatus, @ModifiedDate, @ModifiedBy);";

            using (var connection = DbConnectionFactory.CreateConnection())
            {
                await connection.OpenAsync();
                var transaction = (SqlTransaction)await connection.BeginTransactionAsync();
                try
                {
                    int orderId;

                    // Insert the order first
                    using (var orderCommand = new SqlCommand(query, connection, transaction))
                    {
                        orderCommand.Parameters.AddWithValue("@OrderDate",
                            order.OrderDate.HasValue ? (object)order.OrderDate.Value : DBNull.Value);

                        orderCommand.Parameters.AddWithValue("@UserId",
                            order.UserId.HasValue ? (object)order.UserId.Value : DBNull.Value);

                        orderCommand.Parameters.AddWithValue("@OrderStatus",
                            order.Status ?? (object)DBNull.Value);

                        orderCommand.Parameters.AddWithValue("@ModifiedDate",
                            order.ModifiedDate.HasValue ? (object)order.ModifiedDate.Value : DBNull.Value);

                        orderCommand.Parameters.AddWithValue("@ModifiedBy",
                            order.ModifiedBy ?? (object)DBNull.Value);

                        // Execute order insert and get the inserted order ID
                        orderId = (int)await orderCommand.ExecuteScalarAsync();

                        // Insert order items
                        foreach (var orderItem in items)
                        {
                            string itemQuery = @"INSERT INTO [dbo].[OrderItems] (
                                OrderId, ProductId, ProductVariantId, Quantity)
                                VALUES (@OrderId, @ProductId, @ProductVariantId, @Quantity);";

                            using (var itemCommand = new SqlCommand(itemQuery, connection, transaction))
                            {
                                itemCommand.Parameters.AddWithValue("@OrderId", orderId);
                                itemCommand.Parameters.AddWithValue("@ProductId", orderItem.ProductId);
                                itemCommand.Parameters.AddWithValue("@ProductVariantId", 
                                    orderItem.ProductVariantId ?? (object)DBNull.Value);
                                itemCommand.Parameters.AddWithValue("@Quantity", orderItem.Quantity);

                                await itemCommand.ExecuteNonQueryAsync();
                            }
                        }
                    }

                    // Commit the transaction
                    await transaction.CommitAsync();

                    return orderId;
                }
                catch (SqlException)
                {
                    // Rollback in case of an error
                    await transaction.RollbackAsync();
                    throw new Exception($"An error occurred while placing the order");
                }
                finally
                {
                    await connection.CloseAsync();
                }
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            string query = @"DELETE FROM [dbo].[Orders] WHERE [OrderId] = @id;";

            using (var connection = DbConnectionFactory.CreateConnection())
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@id", id);

                try
                {
                    await connection.OpenAsync();
                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    return rowsAffected > 0;
                }
                catch (SqlException)
                {
                    throw;
                }
                finally
                {
                    await connection.CloseAsync();
                }
            }
        }

        public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId)
        {
            string query = @"
                SELECT o.OrderId, o.OrderDate, o.OrderStatus,
                        oi.OrderItemId, oi.ProductId, oi.Quantity,
                        p.ProductName, p.ProductPrice, p.ProductDiscount,
                        pv.*, mc.*, ms.*
                FROM [dbo].[Orders] o
                INNER JOIN [dbo].[OrderItems] oi ON o.OrderId = oi.OrderId
                INNER JOIN [dbo].[Products] p ON p.ProductId = oi.ProductId
                LEFT JOIN [dbo].[ProductVariants] pv ON pv.ProductVariantId = oi.ProductVariantId
                LEFT JOIN [dbo].[MdColors] mc ON mc.MdColorId = pv.ProductVariantColorId
                LEFT JOIN [dbo].[MdSizes] ms ON ms.MdSizeId = pv.ProductVariantSizeId
                WHERE o.UserId = @UserId
                ORDER BY o.OrderDate DESC";

            using var connection = DbConnectionFactory.CreateConnection();
            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", userId);

            var orders = new List<Order>();

            try
            {
                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                int orderIdIndex = reader.GetOrdinal("OrderId");
                int orderDateIndex = reader.GetOrdinal("OrderDate");
                int statusIndex = reader.GetOrdinal("OrderStatus");

                int itemIdIndex = reader.GetOrdinal("OrderItemId");
                int productIdIndex = reader.GetOrdinal("ProductId");
                int countIndex = reader.GetOrdinal("Quantity");

                int productNameIndex = reader.GetOrdinal("ProductName");
                int productPriceIndex = reader.GetOrdinal("ProductPrice");
                int productDiscountIndex = reader.GetOrdinal("ProductDiscount");

                int productVariantIdIndex = reader.GetOrdinal("ProductVariantId");
                int productVariantQuantityIndex = reader.GetOrdinal("ProductVariantQuantity");
                int colorIdIndex = reader.GetOrdinal("MdColorId");
                int colorNameIndex = reader.GetOrdinal("MdColorName");
                int colorHexIndex = reader.GetOrdinal("MdColorHexValue");
                int sizeIdIndex = reader.GetOrdinal("MdSizeId");
                int sizeCodeIndex = reader.GetOrdinal("MdSizeCode");

                var orderLookup = new Dictionary<int, Order>();

                while (await reader.ReadAsync())
                {
                    int orderId = reader.GetInt32(orderIdIndex);

                    if (!orderLookup.TryGetValue(orderId, out var order))
                    {
                        order = new Order
                        {
                            Id = orderId,
                            OrderDate = reader.IsDBNull(orderDateIndex) ? null : reader.GetDateTime(orderDateIndex),
                            Status = reader.IsDBNull(statusIndex) ? null : reader.GetString(statusIndex),
                            OrderItems = new List<OrderItem>()
                        };

                        orderLookup.Add(orderId, order);
                        orders.Add(order);
                    }

                    order.OrderItems!.Add(new OrderItem
                    {
                        Id = reader.GetInt32(itemIdIndex),
                        ProductId = reader.GetInt32(productIdIndex),
                        Quantity = reader.GetInt32(countIndex),
                        ProductName = reader.IsDBNull(productNameIndex) ? null : reader.GetString(productNameIndex),
                        ProductPrice = reader.IsDBNull(productPriceIndex) ? null : reader.GetDecimal(productPriceIndex),
                        ProductDiscount = reader.IsDBNull(productDiscountIndex) ? null : reader.GetDecimal(productDiscountIndex),
                        ProductVariantId = reader.IsDBNull(productVariantIdIndex) ? null : reader.GetInt32(productVariantIdIndex),

                        MdColor = reader.IsDBNull(colorIdIndex) ? null : new MdColor
                        {
                            Name = reader.IsDBNull(colorNameIndex) ? null : reader.GetString(colorNameIndex),
                            HexValue = reader.IsDBNull(colorHexIndex) ? null : reader.GetString(colorHexIndex),
                            //IsActive = reader.IsDBNull(colorIsActiveIndex) ? null : reader.GetBoolean(colorIsActiveIndex),
                        },

                        MdSize = reader.IsDBNull(sizeIdIndex) ? null : new MdSize
                        {
                            //Id = reader.GetInt32(sizeIdIndex),
                            //Description = reader.IsDBNull(sizeDescriptionIndex) ? null : reader.GetString(sizeDescriptionIndex),
                            SizeCode = reader.IsDBNull(sizeCodeIndex) ? null : reader.GetString(sizeCodeIndex),
                            //IsActive = reader.IsDBNull(sizeIsActiveIndex) ? null : reader.GetBoolean(sizeIsActiveIndex),
                        }
                    });
                }

                return orders;
            }
            catch (SqlException)
            {
                throw new Exception("Failed to load orders for transaction history.");
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        // Filter orders
        public async Task<(IEnumerable<Order> orders, int totalCount)> GetFilteredOrdersAsync(
            int start,
            int length,
            string searchValue,
            string sortColumn,
            string sortDirection)
        {
            var ordersDict = new Dictionary<int, Order>();
            int totalCount = 0;

            // Base query to get order headers
            string orderHeadersQuery = @"
                SELECT 
                    o.OrderId,
                    o.UserId,
                    o.OrderDate, 
                    o.OrderStatus,
                    u.AppUserEmail
                FROM [dbo].[Orders] AS o
                INNER JOIN [dbo].[AppUsers] AS u ON o.UserId = u.AppUserId";

            // Query to get order items for the filtered orders
            string orderItemsQuery = @"
                SELECT 
                    oi.OrderId,
                    oi.Quantity,
                    p.ProductName,
                    p.ProductPrice AS ProductPrice,
                    p.ProductDiscount
                FROM [dbo].[OrderItems] AS oi
                INNER JOIN [dbo].[Products] AS p ON oi.ProductId = p.ProductId
                WHERE oi.OrderId IN (SELECT OrderId FROM #PaginatedOrderIDs)";

            // Count query
            string countQuery = @"
                SELECT COUNT(DISTINCT o.OrderId)
                FROM [dbo].[Orders] AS o
                LEFT JOIN [dbo].[AppUsers] AS u ON o.UserId = u.AppUserId
                LEFT JOIN [dbo].[OrderItems] AS oi ON o.OrderId = oi.OrderId
                LEFT JOIN [dbo].[Products] AS p ON oi.ProductId = p.ProductId";

            // Build WHERE clause for search
            string whereClause = string.Empty;
            if (!string.IsNullOrEmpty(searchValue))
            {
                whereClause = @"
                    WHERE 
                        o.OrderId LIKE @searchValue OR
                        u.AppUserEmail LIKE @searchValue OR
                        p.ProductName LIKE @searchValue OR 
                        o.OrderStatus LIKE @searchValue";
            }

            // Build ORDER BY clause for ROW_NUMBER()
            string rowNumberOrderBy = "ORDER BY o.OrderDate DESC"; // Default sort
            if (!string.IsNullOrEmpty(sortColumn))
            {
                rowNumberOrderBy = sortColumn switch
                {
                    "date" => "ORDER BY o.OrderDate",
                    "user" => "ORDER BY u.AppUserEmail",
                    "status" => "ORDER BY o.OrderStatus",
                    _ => "ORDER BY o.OrderId"
                } + (sortDirection == "desc" ? " DESC" : " ASC");
            }

            // Final query using temp table for proper pagination
            string finalQuery = $@"
                -- Create temp table for paginated order IDs
                CREATE TABLE #PaginatedOrderIDs (OrderId INT PRIMARY KEY);

                -- Insert paginated order IDs
                INSERT INTO #PaginatedOrderIDs
                SELECT OrderId FROM (
                    SELECT 
                        o.OrderId,
                        ROW_NUMBER() OVER ({rowNumberOrderBy}) AS RowNum
                    FROM [dbo].[Orders] AS o
                    LEFT JOIN [dbo].[AppUsers] AS u ON o.UserId = u.AppUserId
                    LEFT JOIN [dbo].[OrderItems] AS oi ON o.OrderId = oi.OrderId
                    LEFT JOIN [dbo].[Products] AS p ON oi.ProductId = p.ProductId
                    {whereClause}
                    GROUP BY o.OrderId, o.OrderDate, u.AppUserEmail, o.OrderStatus
                ) AS FilteredOrders
                WHERE RowNum > @start AND RowNum <= @start + @length;

                -- Get order headers
                {orderHeadersQuery}
                    WHERE o.OrderId IN (SELECT OrderId FROM #PaginatedOrderIDs)
                {rowNumberOrderBy};

                -- Get order items
                {orderItemsQuery};

                -- Get total count
                {countQuery} {whereClause};

                -- Clean up
                DROP TABLE #PaginatedOrderIDs;";

            using var connection = DbConnectionFactory.CreateConnection();
            using var command = new SqlCommand(finalQuery, connection);

            if (!string.IsNullOrEmpty(searchValue))
            {
                command.Parameters.AddWithValue("@searchValue", $"%{searchValue}%");
            }
            command.Parameters.AddWithValue("@start", start);
            command.Parameters.AddWithValue("@length", length);

            try
            {
                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();

                // Read first result set for order headers
                if (reader.HasRows)
                {
                    // Get ordinals first
                    int orderIdOrdinal = reader.GetOrdinal("OrderId");
                    int orderDateOrdinal = reader.GetOrdinal("OrderDate");
                    int orderStatusOrdinal = reader.GetOrdinal("OrderStatus");
                    int userIdOrdinal = reader.GetOrdinal("UserId");
                    int emailOrdinal = reader.GetOrdinal("AppUserEmail");

                    while (await reader.ReadAsync())
                    {
                        int orderId = reader.GetInt32(orderIdOrdinal);

                        if (!ordersDict.ContainsKey(orderId))
                        {
                            var order = new Order
                            {
                                Id = orderId,
                                OrderDate = reader.IsDBNull(orderDateOrdinal) ? null : reader.GetDateTime(orderDateOrdinal),
                                Status = reader.IsDBNull(orderStatusOrdinal) ? null : reader.GetString(orderStatusOrdinal),
                                UserId = reader.IsDBNull(userIdOrdinal) ? null : reader.GetInt32(userIdOrdinal),
                                User = new AppUser
                                {
                                    Email = reader.IsDBNull(emailOrdinal) ? null : reader.GetString(emailOrdinal)
                                },
                                OrderItems = new List<OrderItem>()
                            };
                            ordersDict[orderId] = order;
                        }
                    }
                }

                // Read second result set for order items
                if (await reader.NextResultAsync() && reader.HasRows)
                {
                    int orderIdOrdinal = reader.GetOrdinal("OrderId");
                    int productNameOrdinal = reader.GetOrdinal("ProductName");
                    int quantityOrdinal = reader.GetOrdinal("Quantity");
                    int productPriceOrdinal = reader.GetOrdinal("ProductPrice");
                    int productDiscountOrdinal = reader.GetOrdinal("ProductDiscount");

                    while (await reader.ReadAsync())
                    {
                        int orderId = reader.GetInt32(orderIdOrdinal);

                        if (ordersDict.TryGetValue(orderId, out var order))
                        {
                            var orderItem = new OrderItem
                            {
                                OrderId = orderId,
                                ProductName = reader.GetString(productNameOrdinal),
                                Quantity = reader.GetInt32(quantityOrdinal),
                                ProductPrice = reader.IsDBNull(productPriceOrdinal) ? null : reader.GetDecimal(productPriceOrdinal),
                                ProductDiscount = reader.IsDBNull(productDiscountOrdinal) ? null : reader.GetDecimal(productDiscountOrdinal)
                            };
                            order.OrderItems.Add(orderItem);
                        }
                    }
                }

                // Read third result set for total count
                if (await reader.NextResultAsync() && await reader.ReadAsync())
                {
                    totalCount = reader.GetInt32(0);
                }

            }
            catch (SqlException)
            {
                throw new Exception("Error while fetching orders");
            }

            var orders = ordersDict.Select(kvp => kvp.Value);

            return (orders, totalCount);
        }
    }
}
