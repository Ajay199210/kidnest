using KidNest.Core.Entities;
using KidNest.Core.Enums;
using KidNest.Core.Interfaces;
using KidNest.Core.Shared;
using KidNest.Services.DTOs.Orders;
using KidNest.Services.Interfaces;

namespace KidNest.Services.Services
{
    public class OrdersService : IOrdersService
    {
        private readonly IOrdersRepository _ordersRepo;
        private readonly IUsersRespository _usersRepo;
        private readonly IProductsRepository _productsRepo;

        public OrdersService(IOrdersRepository ordersRepo, IUsersRespository usersRepo,
            IProductsRepository productsRepo)
        {
            _ordersRepo = ordersRepo;
            _usersRepo = usersRepo;
            _productsRepo = productsRepo;
        }

        public async Task<OperationResult> CreateOrderAsync(OrderCreateDTO orderCreateDTO)
        {
            try
            {
                // Validate User
                var user = await _usersRepo.GetByIdAsync(orderCreateDTO.UserId);
                if (user == null) return OperationResult.Fail("User does not exist.");

                // Validate Order Items
                if (orderCreateDTO.OrderItemsDTO == null || orderCreateDTO.OrderItemsDTO.Count == 0)
                    return OperationResult.Fail("Order must contain at least one product.");

                var trustedOrderItems = new List<OrderItem>();

                foreach (var item in orderCreateDTO.OrderItemsDTO)
                {
                    // Validate Product
                    var product = await _productsRepo.GetByIdAsync(item.ProductId);
                    if (product == null) return OperationResult.Fail($"Product {item.ProductId} does not exist.");

                    // Validate Variant (if provided)
                    ProductVariant? variant = null;
                    if (item.ProductVariantId.HasValue)
                    {
                        variant = await _productsRepo.GetVariantByIdAsync(item.ProductVariantId.Value);
                        if (variant == null || variant.ProductId != item.ProductId)
                            return OperationResult.Fail($"Invalid variant for product {product.Name}.");

                        if (variant.Quantity < item.Quantity)
                            return OperationResult.Fail($"Not enough stock for variant {variant.Id}.");
                    }
                    else if (product.Quantity < item.Quantity)
                    {
                        return OperationResult.Fail($"Not enough stock for product {product.Name}.");
                    }

                    // Add server-validated item
                    trustedOrderItems.Add(new OrderItem
                    {
                        ProductId = product.Id,
                        ProductName = product.Name, // From DB, not client
                        ProductPrice = CalculateDiscountedPrice(product.Price ?? 0m, product.Discount), // Server-calculated
                        ProductVariantId = variant?.Id,
                        Quantity = item.Quantity
                    });
                }

                // Create Order
                var order = new Order
                {
                    OrderDate = DateTime.UtcNow,
                    UserId = user.Id,
                    Status = OrderStatus.Pending.ToString(),
                    ModifiedDate = DateTime.UtcNow,
                    ModifiedBy = user.FullName
                };

                // Use trusted order items (not client-provided DTO data)
                var orderId = await _ordersRepo.AddAsync(order, trustedOrderItems);

                if (orderId > 0)
                {
                    // Dont' reduce stock Check by summing all ordered items quantities
                    //await ReduceStockAsync(trustedOrderItems);
                    return OperationResult.Ok();
                }

                return OperationResult.Fail("Failed to create order.");
            }
            catch (Exception ex)
            {
                return OperationResult.Fail($"An unexpected error occurred: {ex.Message}");
            }
        }

        public async Task<OperationResult> DeleteOrderAsync(int orderId)
        {
            try
            {
                // Add business logic (e.g., check if the order exists, or is related to an active process)
                Order? order = await _ordersRepo.GetByIdAsync(orderId);
                if (order == null)
                {
                    return OperationResult.Fail("Order does not exist.");
                }

                // Optionally, check if the order is related to an active process (e.g., payment, shipment)
                // bool isOrderProcessing = await _ordersRepo.IsOrderProcessingAsync(orderId);
                // if (isOrderProcessing)
                // {
                //     return OperationResult.Fail("Order is currently being processed and cannot be deleted.");
                // }

                // Delete the order
                bool isDeleted = await _ordersRepo.DeleteAsync(orderId);

                if (isDeleted)
                {
                    await RestoreStockAsync(order.OrderItems);

                    return OperationResult.Ok();
                }

                return OperationResult.Fail("Order deletion failed. The order may not exist or may be locked.");
            }
            catch (Exception ex)
            {
                // Log the exception (e.g., using Serilog or NLog)
                return OperationResult.Fail($"An unexpected error occurred while deleting the order: {ex.Message}");
            }
        }

        public async Task<IEnumerable<OrderDTO>> GetAllOrdersAsync()
        {
            // Retrieve all orders from the repository
            var orders = await _ordersRepo.GetAllAsync();

            // Map each order to an OrderDTO
            return orders.Select(o => new OrderDTO
            {
                Id = o.Id,
                UserId = o.UserId,
                UserEmail = o.User?.Email,
                Status = o.Status,
                OrderDate = o.OrderDate,
                ModifiedDate = o.ModifiedDate,
                ModifiedBy = o.ModifiedBy,
                OrderItemsDTO = o.OrderItems!.Select(item => new OrderItemDTO
                {
                    ProductId = item.ProductId,
                    Name = item.ProductName,
                    Quantity = item.Quantity,

                }).ToList()
            }).ToList();
        }

        public async Task<OrderDTO?> GetOrderByIdAsync(int orderId)
        {
            // Fetch domain model from repository
            var order = await _ordersRepo.GetByIdAsync(orderId);

            if (order == null)
                return null;  // Or throw NotFoundException if preferred

            // Map domain model to DTO
            return new OrderDTO
            {
                Id = order.Id,
                UserId = order.UserId,
                Status = order.Status,
                OrderDate = order.OrderDate,
                ModifiedDate = order.ModifiedDate,
                ModifiedBy = order.ModifiedBy,
                UserEmail = order.User?.Email,
                UserAddress = order.User?.Address,
                OrderItemsDTO = order.OrderItems!.Select(oi => new OrderItemDTO
                {
                    ProductId = oi.ProductId,
                    Name = oi.ProductName ?? "N/A",
                    Price = oi.ProductPrice ?? 0m,
                    Discount = oi.ProductDiscount ?? 0m,
                    Quantity = oi.Quantity,
                    ColorName = oi.MdColor?.Name,
                    ColorHex = oi.MdColor?.HexValue,
                    SizeCode = oi.MdSize?.SizeCode
                }).ToList() ?? []
            };
        }

        public async Task<IEnumerable<OrderDTO>> GetOrdersByUserId(int userId)
        {
            var orders = await _ordersRepo.GetByUserIdAsync(userId);

            return orders.Select(o => new OrderDTO
            {
                Id = o.Id,
                UserId = userId,
                UserEmail = o.User?.Email,
                Status = o.Status,
                OrderDate = o.OrderDate,
                ModifiedDate = o.ModifiedDate,
                ModifiedBy = o.ModifiedBy,
                OrderItemsDTO = o.OrderItems!.Select(item => new OrderItemDTO
                {
                    ProductId = item.ProductId,
                    Name = item.ProductName,
                    Quantity = item.Quantity,
                    ColorName = item.MdColor?.Name,
                    ColorHex = item.MdColor?.HexValue,
                    SizeCode = item.MdSize?.SizeCode,
                    Price = item.ProductPrice ?? 0m,
                    Discount = item.ProductDiscount ?? 0m
                }).ToList()
            }).ToList();
        }

        //public async Task<OperationResult> UpdateOrderAsync(OrderDTO orderDTO)
        //{
        //    try
        //    {
        //        // Possible validation checks here (e.g. validate product existence, status, etc.)
        //        if (orderDTO.OrderDate == default)
        //        {
        //            return OperationResult.Fail("Order date is required.");
        //        }

        //        // Create a new Order object from the DTO
        //        var orderToUpdate = new Order
        //        {
        //            Id = orderDTO.Id,
        //            UserId = orderDTO.UserId,
        //            Status = orderDTO.Status,
        //            OrderDate = orderDTO.OrderDate,
        //            ModifiedDate = DateTime.UtcNow, // Set the modified date
        //            ModifiedBy = orderDTO.ModifiedBy
        //        };

        //        // Update the order in the repository
        //        bool isOrderUpdated = await _ordersRepo.UpdateAsync(orderToUpdate);

        //        if (isOrderUpdated)
        //        {
        //            return OperationResult.Ok();
        //        }

        //        return OperationResult.Fail("Order update failed. The order might not exist or no changes were detected.");
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception here (e.g., using Serilog, NLog, etc.)
        //        return OperationResult.Fail($"An unexpected error occurred" +
        //            $" while updating the order: {ex.Message}");
        //    }
        //}

        private static decimal CalculateDiscountedPrice(decimal price, decimal? discountPercentage)
        {
            if (discountPercentage == null || discountPercentage == 0)
                return price;

            return price * (1 - discountPercentage.Value / 100);
        }

        private async Task ReduceStockAsync(IEnumerable<OrderItem> items)
        {
            foreach (var item in items)
            {
                Product? product = await _productsRepo.GetByIdAsync(item.ProductId);
                if (product == null)
                {
                    throw new InvalidOperationException($"Product with ID {item.ProductId} " +
                        $"was not found during stock update.");
                }

                if (product.Quantity < item.Quantity)
                {
                    throw new InvalidOperationException(
                        $"Insufficient stock for product {product.Name} during stock update. " +
                        $"You ordered: {item.Quantity} units, " +
                        $"Available: {product.Quantity} units"
                    );
                }

                product.Quantity -= item.Quantity;

                await _productsRepo.UpdateAsync(product);
            }
        }

        private async Task RestoreStockAsync(IEnumerable<OrderItem> items)
        {
            foreach (var item in items)
            {
                Product? product = await _productsRepo.GetByIdAsync(item.ProductId);
                if (product == null)
                {
                    throw new InvalidOperationException($"Product with ID {item.ProductId} " +
                        $"was not found during stock restoration.");
                }

                product.Quantity += item.Quantity;

                await _productsRepo.UpdateAsync(product);
            }
        }

        // Filtering, Sorting & Pagination
        public async Task<DataTableResponse<OrderDTO>> GetPaginatedOrdersAsync(DataTableRequest request)
        {
            // Get sorting info from first column
            var sortColumn = request.Order.FirstOrDefault();
            var columnName = sortColumn != null ? request.Columns[sortColumn.Column].Data : "Id";

            var (orders, totalCount) = await _ordersRepo.GetFilteredOrdersAsync(
                request.Start,
                request.Length,
                request.Search.Value,
                columnName,
                sortColumn?.Dir ?? "asc"
            );

            var orderDTOs = orders.Select(o => new OrderDTO
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                UserId = o.UserId,
                UserEmail = o.User?.Email,
                Status = o.Status,
                //OrderStatus = o.Status,
                ModifiedDate = o.ModifiedDate,
                ModifiedBy = o.ModifiedBy,
                OrderItemsDTO = o.OrderItems!.Select(oi => new OrderItemDTO
                {
                    ProductId = oi.ProductId,
                    Name = oi.ProductName,
                    Quantity = oi.Quantity,
                    Price = oi.ProductPrice ?? 0m,
                    Discount = oi.ProductDiscount ?? 0m
                }).ToList()
            }).ToList();

            return new DataTableResponse<OrderDTO>
            {
                Items = orderDTOs,
                TotalCount = totalCount
            };
        }
    }
}
