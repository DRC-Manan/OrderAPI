using Microsoft.Extensions.Logging;
using OAPI.Application.Comman;
using OAPI.Application.DTO;
using OAPI.Application.Repository;
using OAPI.Application.Services;
using OAPI.Domain.Entity;
using Serilog;
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
		private readonly ILogger<GetOrderQueryHandler> _logger;
		private readonly ICacheService _cacheService;

		public GetOrderQueryHandler(IOrderRepository orderRepository, ILogger<GetOrderQueryHandler> logger, ICacheService cacheService)
		{
			_orderRepository = orderRepository;
			_logger = logger;
			_cacheService = cacheService;
		}

		public async Task<PageResult<OrderDto>> Handle(GetOrdersQuery query)
		{
			var cacheKey = $"orders:all:{query.Email}:{query.Page}:{query.PageSize}";
			var cachedResult = await _cacheService.GetAsync<PageResult<OrderDto>>(cacheKey);

			if (cachedResult != null)
			{
				_logger.LogInformation("Cache hit for {CacheKey}", cacheKey);
				return cachedResult;
			}

			_logger.LogInformation("Fetching orders for {Email}", query.Email);
			_logger.LogInformation("Fetching orders for {Email} Page {Page} Size {Size}", query.Email, query.Page, query.PageSize);

			var (orders, totalCount) = await _orderRepository.GetOrdersAsync(query.Email, query.Page, query.PageSize);

			var orderDtos = orders.Select(o => new OrderDto
			{
				OrderId = o.OrderId,
				Email = o.CustomerEmail,
				Amount = o.TotalAmount
			}).ToList();

			var result = new PageResult<OrderDto>
			{
				Items = orderDtos,
				PageIndex = query.Page,
				PageSize = query.PageSize,
				TotalCount = totalCount
			};

			await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));

			return result; 
		}
	}
}
