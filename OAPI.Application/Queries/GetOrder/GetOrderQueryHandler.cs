using OAPI.Application.Comman;
using OAPI.Application.DTO;
using OAPI.Application.Repository;
using OAPI.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAPI.Application.Queries.GetOrder
{
	public class GetOrderQueryHandler : IQueryHandler<GetOrdersQuery, PageResult<OrderDto>>
	{
		private readonly IOrderRepository _orderRepository;

		public GetOrderQueryHandler(IOrderRepository orderRepository)
		{
			_orderRepository = orderRepository;
		}

		public async Task<PageResult<OrderDto>> Handle(GetOrdersQuery query)
		{
			var (orders, totalCount) = await _orderRepository.GetOrdersAsync(query.Email, query.Page, query.PageSize);

			var orderDtos = orders.Select(o => new OrderDto
			{
				OrderId = o.OrderId,
				Email = o.CustomerEmail,
				Amount = o.TotalAmount
			}).ToList();

			return new PageResult<OrderDto>
			{
				Items = orderDtos,
				PageIndex = query.Page,
				PageSize = query.PageSize,
				TotalCount = totalCount
			}; 
		}
	}
}
