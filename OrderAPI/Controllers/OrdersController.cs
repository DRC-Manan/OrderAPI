using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OAPI.Application.Repository;
using OAPI.Domain.Entity;

namespace OrderAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class OrdersController : ControllerBase
	{
		private readonly IOrderRepository _orderRepository;
		
		public OrdersController(IOrderRepository orderRepository)
		{
			_orderRepository = orderRepository;
		}

		[HttpPost]
		public async Task<IActionResult> CreateOrder(Order order)
		{
			if (order == null)
			{
				return BadRequest();
			}

			await _orderRepository.AddAsync(order);
			return Ok(order.OrderId);
		}
	}
}
