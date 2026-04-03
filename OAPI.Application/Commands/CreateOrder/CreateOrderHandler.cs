using OAPI.Application.Comman;
using OAPI.Application.Repository;
using OAPI.Domain.Entity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OAPI.Application.Services;
using Hangfire;

namespace OAPI.Application.Commands.CreateOrder
{
	public class CreateOrderHandler: ICommandHandler<CreateOrderCommand, Result<Guid>>
	{
		private readonly IOrderRepository _orderRepository;
		private readonly ILogger<CreateOrderHandler> _logger;
		private readonly ICacheService _cacheService;
		private readonly IBackgroundJobClient _backgroundJobClient;

		public CreateOrderHandler(IOrderRepository orderRepository
			, ILogger<CreateOrderHandler> logger
			, ICacheService cacheService
			, IBackgroundJobClient backgroundJobClient)
		{
			_orderRepository = orderRepository;
			_logger = logger;
			_cacheService = cacheService;
			_backgroundJobClient = backgroundJobClient;
		}

		public async Task<Result<Guid>> Handle(CreateOrderCommand command)
		{
			_logger.LogInformation("Creating order for {Email}", command.Email);

			var order = new Order(command.Email, command.Amount);
			await _orderRepository.AddAsync(order);

			await _cacheService.RemoveAsync($"orders:all:{command.Email}");

			_backgroundJobClient.Enqueue<IEmailService>(x => x.SendOrderConfirmationAsync(command.Email, order.OrderId));

			_logger.LogInformation("Order created with ID {OrderId}", order.OrderId);
			return Result<Guid>.Success(order.OrderId);
		}
	}
}
