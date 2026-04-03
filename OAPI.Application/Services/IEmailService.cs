using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAPI.Application.Services
{
	public interface IEmailService
	{
		Task SendOrderConfirmationAsync(string email, Guid orderId);
	}
}
