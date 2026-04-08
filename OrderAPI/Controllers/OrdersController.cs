using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OAPI.Application.Comman;
using OAPI.Application.Commands;
using OAPI.Application.Commands.CreateOrder;
using OAPI.Application.DTO;
using OAPI.Application.Queries;
using OAPI.Application.Queries.GetOrder;
using OAPI.Application.Repository;
using OAPI.Domain.Entity;

namespace OrderAPI.Controllers
{
	[ApiVersion("1.0")]
	[Route("api/v{version:apiVersion}/orders")]
	//// Option 2: Apply Per Controller / Endpoint (Recommended)
	//[EnableRateLimiting("FixedWindowPolicy")]
	[ApiController]
	public class OrdersController : ControllerBase
	{
		private readonly ICommandHandler<CreateOrderCommand, Result<Guid>> _handler;
		private readonly IQueryHandler<GetOrdersQuery, PageResult<OrderDto>> _queryHandler;

		public OrdersController(ICommandHandler<CreateOrderCommand, Result<Guid>> handler
			, IQueryHandler<GetOrdersQuery, PageResult<OrderDto>> queryHandler)
		{
			_handler = handler;
			_queryHandler = queryHandler;
		}

		[HttpPost]
		[EnableRateLimiting("strict")]
		public async Task<IActionResult> CreateOrder(CreateOrderCommand command)
		{
			if (command == null)
			{
				return BadRequest();
			}
			var orderId = await _handler.Handle(command);
			return Ok(orderId);
		}

		[MapToApiVersion("1.0")]
		[HttpGet]
		[EnableRateLimiting("loose")]
		public async Task<IActionResult> GetOrders([FromQuery] GetOrdersQuery query)
		{
			if (query == null)
			{
				return BadRequest();
			}
			var result = await _queryHandler.Handle(query);
			return Ok(result);
		}
	}
}
