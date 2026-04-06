using OAPI.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAPI.Application.Repository
{
	public interface IInboxMessageRepository
	{
		Task AddAsync(InboxMessage inboxMessage);
		Task<InboxMessage?> GetByMessageIdAsync(Guid messageId);
	}
}
