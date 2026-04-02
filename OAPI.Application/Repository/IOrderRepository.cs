using OAPI.Domain.Entity;

namespace OAPI.Application.Repository
{
	public interface IOrderRepository
	{
		Task AddAsync(Order order);
		Task<Order?> GetByIdAsync(int orderId);
		Task<(IEnumerable<Order>, int)> GetOrdersAsync(string email, int page, int pageSize);
		Task<int> GetOrdersCountAsync(string email);
	}
}
