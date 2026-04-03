using KidNest.Core.Shared;
using KidNest.Services.DTOs.Orders;

namespace KidNest.Services.Interfaces
{
    public interface IOrdersService
    {
        Task<IEnumerable<OrderDTO>> GetAllOrdersAsync();
        Task<IEnumerable<OrderDTO>> GetOrdersByUserId(int userId);
        Task<OrderDTO?> GetOrderByIdAsync(int orderId);
        Task<OperationResult> CreateOrderAsync(OrderCreateDTO orderCreateDTO);
        //Task<OperationResult> UpdateOrderAsync(OrderDTO orderDTO);
        Task<OperationResult> DeleteOrderAsync(int orderId);

        Task<DataTableResponse<OrderDTO>> GetPaginatedOrdersAsync(DataTableRequest request);
    }
}
