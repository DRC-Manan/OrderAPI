using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAPI.Domain.Entity
{
	public class Order
	{
		public Guid OrderId { get; private set; }
		public string CustomerEmail { get; private set; }
		public decimal TotalAmount { get; private set; }
		public int OrderStatus { get; private set; }
		public DateTime CreatedAt { get; private set; }

		public Order(string customerEmail, decimal totalAmount)
		{
			OrderId = Guid.NewGuid();
			CustomerEmail = customerEmail;
			TotalAmount = totalAmount;
			CreatedAt = DateTime.UtcNow;
			OrderStatus = 0; // Assuming 0 is the default status
		}

		private Order() { }


	}
}
