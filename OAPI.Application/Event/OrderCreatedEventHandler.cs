using OAPI.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAPI.Application.Event
{
	public class OrderCreatedEventHandler : IEventHandler<OrderCreatedEvent>
	{
		private readonly IEmailService _emailService;

		public OrderCreatedEventHandler(IEmailService emailService)
		{
			_emailService = emailService;
		}

		public async Task HandleAsync(OrderCreatedEvent @event)
		{
			// Handle the event
			await _emailService.SendOrderConfirmationAsync(@event.Email, @event.OrderId);
		}
	}
}
