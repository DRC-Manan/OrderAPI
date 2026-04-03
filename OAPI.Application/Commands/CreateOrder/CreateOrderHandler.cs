using OAPI.Application.Comman;
using OAPI.Application.Repository;
using OAPI.Domain.Entity;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAPI.Application.Commands.CreateOrder
{
	public class CreateOrderHandler: ICommandHandler<CreateOrderCommand, Result<Guid>>
	{
		private readonly IOrderRepository _orderRepository;
		private readonly ILogger<CreateOrderHandler> _logger;

		public CreateOrderHandler(IOrderRepository orderRepository, ILogger<CreateOrderHandler> logger)
		{
			_orderRepository = orderRepository;
			_logger = logger;
		}

		public async Task<Result<Guid>> Handle(CreateOrderCommand command)
		{
			_logger.LogInformation("Creating order for {Email}", command.Email);

			var order = new Order(command.Email, command.Amount);
			await _orderRepository.AddAsync(order);

			_logger.LogInformation("Order created with ID {OrderId}", order.OrderId);
			return Result<Guid>.Success(order.OrderId);
		}
	}
}
