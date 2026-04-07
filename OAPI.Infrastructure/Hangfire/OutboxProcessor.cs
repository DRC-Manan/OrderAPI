using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OAPI.Application.Event;
using OAPI.Application.Repository;
using OAPI.Application.Services;
using OAPI.Domain.Entity;

namespace OAPI.Infrastructure.Hangfire
{
	public class OutboxProcessor
	{
		private readonly AppDbContext _context;
		//private readonly IEmailService _emailService;
		private readonly IEventDispatcher _dispatcher;
		private readonly ILogger<OutboxProcessor> _logger;

		public OutboxProcessor(
			AppDbContext context,
			//IEmailService emailService,
			IEventDispatcher dispatcher,
			ILogger<OutboxProcessor> logger)
		{
			_context = context;
			//_emailService = emailService;
			_dispatcher = dispatcher;
			_logger = logger;
		}

		public async Task ProcessAsync()
		{
			var messages = await _context.OutboxMessages
				.Where(x => x.ProcessedAt == null)
				.Take(20)
				.ToListAsync();

			foreach (var message in messages)
			{
				try
				{
					// 🔥 Check Inbox (Idempotency)
					var alreadyProcessed = await _context.InboxMessages
						.AnyAsync(x => x.MessageId == message.OutboxMessageId);

					if (alreadyProcessed)
					{
						message.MarkAsProcessed();
						continue;
					}

					// Dispatch the event. DispatchAsync is responsible for resolving the type (Type.GetType).
					await _dispatcher.DispatchAsync(message.Type, message.MessageContent);

					// ✅ Save Inbox entry
					_context.InboxMessages.Add(new InboxMessage(message.OutboxMessageId));

					message.MarkAsProcessed();
				}
				catch (Exception ex)
				{
					message.MarkAsFailed(ex.Message);
					_logger.LogError(ex, "Error processing outbox message {Id}", message.OutboxMessageId);
				}
			}

			await _context.SaveChangesAsync();
		}
	}
}
