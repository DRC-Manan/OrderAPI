using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAPI.Domain.Entity
{
	public class InboxMessage
	{
		public Guid InboxMessageId { get; private set; }
		public Guid MessageId { get; private set; }
		public DateTime ProcessedOnUtc { get; private set; }

		private InboxMessage() { }

		public InboxMessage(Guid messageId)
		{
			InboxMessageId = Guid.NewGuid();
			MessageId = messageId;
			ProcessedOnUtc = DateTime.UtcNow;
		}
	}
}
