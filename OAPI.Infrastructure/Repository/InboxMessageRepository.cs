using Microsoft.EntityFrameworkCore;
using OAPI.Application.Repository;
using OAPI.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAPI.Infrastructure.Repository
{
	public class InboxMessageRepository : IInboxMessageRepository
	{
		private readonly AppDbContext _context;

		public InboxMessageRepository(AppDbContext context)
		{
			_context = context;
		}

		public async Task AddAsync(InboxMessage inboxMessage)
		{
			await _context.InboxMessages.AddAsync(inboxMessage);
			await _context.SaveChangesAsync();
		}
		public async Task<InboxMessage?> GetByMessageIdAsync(Guid messageId)
		{
			return await _context.InboxMessages
				.FirstOrDefaultAsync(m => m.MessageId == messageId);
		}
	}
}
