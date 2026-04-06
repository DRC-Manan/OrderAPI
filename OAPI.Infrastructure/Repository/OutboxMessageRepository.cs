using OAPI.Application.Repository;
using OAPI.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAPI.Infrastructure.Repository
{
	public class OutboxMessageRepository : IOutboxMessageRepository
	{
		private readonly AppDbContext _dbContext;

		public OutboxMessageRepository(AppDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task AddAsync(OutboxMessage outboxMessage)
		{
			await _dbContext.OutboxMessages.AddAsync(outboxMessage);
			await _dbContext.SaveChangesAsync();
		}
		public Task<IEnumerable<OutboxMessage>> GetPendingMessagesAsync()
		{
			throw new NotImplementedException();
		}
		public Task MarkAsProcessedAsync(Guid outboxMessageId)
		{
			throw new NotImplementedException();
		}
	}
}
