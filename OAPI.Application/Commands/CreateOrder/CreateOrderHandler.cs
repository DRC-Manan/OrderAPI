using OAPI.Application.Comman;
using OAPI.Application.Repository;
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

		public CreateOrderHandler(IOrderRepository orderRepository)
		{
			_orderRepository = orderRepository;
		}

		public async Task<Result<Guid>> Handle(CreateOrderCommand command)
		{
			var order = new Order(command.Email, command.Amount);
			await _orderRepository.AddAsync(order);
			return Result<Guid>.Success(order.OrderId);
		}
	}
}
