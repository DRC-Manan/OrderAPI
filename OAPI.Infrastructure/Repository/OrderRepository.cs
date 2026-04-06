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
			await _dbContext.Orders.AddAsync(order);
			await _dbContext.SaveChangesAsync();
		}

		public Task<Order?> GetByIdAsync(int orderId)
		{
			throw new NotImplementedException();
		}

		public async Task<(IEnumerable<Order>, int)> GetOrdersAsync(string email, int page, int pageSize)
		{
			var query = _dbContext.Orders.Where(o => o.CustomerEmail == email);
			var totalCount = await query.CountAsync();
			var orders = await query
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();
			return (orders, totalCount);
		}

		public async Task<int> GetOrdersCountAsync(string email)
		{
			return await _dbContext.Orders.CountAsync(o => o.CustomerEmail == email);
		}
	}
}
