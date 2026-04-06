using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OAPI.Application.Event;
using OAPI.Application.Repository;
using OAPI.Application.Services;
using OAPI.Domain.Entity;

namespace OAPI.Infrastructure.Hangfire
{
	public class OutboxProcessor
	{
		private readonly AppDbContext _context;
		private readonly IEmailService _emailService;
		private readonly ILogger<OutboxProcessor> _logger;

		public OutboxProcessor(
			AppDbContext context,
			IEmailService emailService,
			ILogger<OutboxProcessor> logger)
		{
			_context = context;
			_emailService = emailService;
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

					// Process event
					if (message.Type == nameof(OrderCreatedEvent))
					{
						var evt = JsonConvert.DeserializeObject<OrderCreatedEvent>(message.MessageContent);

						await _emailService.SendOrderConfirmationAsync(evt.Email, evt.OrderId);
					}

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
