using Hangfire;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OAPI.Application.Comman;
using OAPI.Application.Event;
using OAPI.Application.Repository;
using OAPI.Application.Services;
using OAPI.Domain.Entity;
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
		private readonly ICacheService _cacheService;
		private readonly IBackgroundJobClient _backgroundJobClient;
		private readonly IOutboxMessageRepository _outboxMessageRepository;

		public CreateOrderHandler(IOrderRepository orderRepository
			, ILogger<CreateOrderHandler> logger
			, ICacheService cacheService
			, IBackgroundJobClient backgroundJobClient
			, IOutboxMessageRepository outboxMessageRepository)
		{
			_orderRepository = orderRepository;
			_logger = logger;
			_cacheService = cacheService;
			_backgroundJobClient = backgroundJobClient;
			_outboxMessageRepository = outboxMessageRepository;
		}

		public async Task<Result<Guid>> Handle(CreateOrderCommand command)
		{
			_logger.LogInformation("Creating order for {Email}", command.Email);

			var order = new Order(command.Email, command.Amount);
			await _orderRepository.AddAsync(order);

			var orderCreatedEvent = new OrderCreatedEvent(order.OrderId, command.Email);

			var outboxMessage = new OutboxMessage(typeof(OrderCreatedEvent).AssemblyQualifiedName, JsonConvert.SerializeObject(orderCreatedEvent));

			await _outboxMessageRepository.AddAsync(outboxMessage);

			await _cacheService.RemoveAsync($"orders:all:{command.Email}");

			//_backgroundJobClient.Enqueue<IEmailService>(x => x.SendOrderConfirmationAsync(command.Email, order.OrderId));

			_logger.LogInformation("Order created with ID {OrderId}", order.OrderId);
			return Result<Guid>.Success(order.OrderId);
		}
	}
}
