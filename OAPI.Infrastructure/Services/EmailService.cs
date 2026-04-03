using Hangfire;
using Microsoft.Extensions.Logging;
using OAPI.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAPI.Infrastructure.Services
{
	public class EmailService : IEmailService
	{
		public EmailService()
		{

		}

		[AutomaticRetry(Attempts = 3)]
		public async Task SendOrderConfirmationAsync(string email, Guid orderId)
		{
			await Task.Delay(2000); // simulate delay
		}
	}
}
