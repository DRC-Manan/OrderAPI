using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAPI.Application.DTO
{
	public class OrderDto
	{
		public Guid OrderId { get; set; }
		public string Email { get; set; }
		public decimal Amount { get; set; }
	}
}
