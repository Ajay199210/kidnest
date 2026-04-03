using KidNest.Core.Entities;

namespace KidNest.Core.Interfaces
{
    public interface IOrdersRepository
    {
        Task<int> AddAsync(Order order, List<OrderItem> items);
        Task<IEnumerable<Order>> GetAllAsync();
        Task<IEnumerable<Order>> GetByUserIdAsync(int userId);
        Task<Order?> GetByIdAsync(int id);
        Task<bool> DeleteAsync(int id);

        Task<(IEnumerable<Order> orders, int totalCount)> GetFilteredOrdersAsync(
            int start,
            int length,
            string searchValue,
            string sortColumn,
            string sortDirection);
    }
}
