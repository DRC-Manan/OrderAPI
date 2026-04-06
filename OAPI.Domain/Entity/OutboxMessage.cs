using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAPI.Domain.Entity
{
	public class OutboxMessage
	{
		public Guid OutboxMessageId { get; private set; }
		public string Type { get; private set; } = default!;
		public string MessageContent { get; private set; } = default!;
		public DateTime OccurredAt { get; private set; }
		public DateTime? ProcessedAt { get; private set; }
		public string? Error { get; private set; }

		public OutboxMessage(string type, string messageContent)
		{
			OutboxMessageId = Guid.NewGuid();
			Type = type;
			MessageContent = messageContent;
			OccurredAt = DateTime.UtcNow;
		}

		private OutboxMessage() { }

		public void MarkAsProcessed()
		{
			ProcessedAt = DateTime.UtcNow;
			Error = null;
		}

		public void MarkAsFailed(string error)
		{
			Error = error;
			ProcessedAt = null;
		}
	}
}
