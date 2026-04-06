using OAPI.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAPI.Application.Repository
{
	public interface IOutboxMessageRepository
	{
		Task AddAsync(OutboxMessage outboxMessage);
		Task<IEnumerable<OutboxMessage>> GetPendingMessagesAsync();
		Task MarkAsProcessedAsync(Guid outboxMessageId);
	}
}
