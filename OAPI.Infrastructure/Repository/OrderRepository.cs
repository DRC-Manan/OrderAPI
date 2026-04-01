using Microsoft.EntityFrameworkCore;
using OAPI.Application.Repository;
using OAPI.Domain.Entity;

namespace OAPI.Infrastructure.Repository
{
	public class OrderRepository : IOrderRepository
	{
		private readonly AppDbContext _dbContext;
		public OrderRepository(AppDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task AddAsync(Order order)
		{
			var orderR = new Order(order.CustomerEmail, order.TotalAmount);
			await _dbContext.Orders.AddAsync(orderR);
			await _dbContext.SaveChangesAsync();
		}

		public Task<Order?> GetByIdAsync(int orderId)
		{
			throw new NotImplementedException();
		}
	}
}
