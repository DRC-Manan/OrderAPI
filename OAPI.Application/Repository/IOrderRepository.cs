using OAPI.Domain.Entity;

namespace OAPI.Application.Repository
{
	public interface IOrderRepository
	{
		Task AddAsync(Order order);
		Task<Order?> GetByIdAsync(int orderId);
	}
}
