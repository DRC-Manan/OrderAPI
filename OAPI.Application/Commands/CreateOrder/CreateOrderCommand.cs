using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAPI.Application.Commands.CreateOrder
{
	public record CreateOrderCommand(string Email, decimal Amount);
}
