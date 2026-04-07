using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAPI.Application.Event
{
	public class EventDispatcher: IEventDispatcher
	{
		private readonly IServiceProvider _serviceProvider;

		public EventDispatcher(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public async Task DispatchAsync(string type, string content)
		{
			var eventType = Type.GetType(type);

			if (eventType == null)
				throw new Exception($"Event type {type} not found");

			var @event = JsonConvert.DeserializeObject(content, eventType);

			var handlerType = typeof(IEventHandler<>).MakeGenericType(eventType);

			var handlers = _serviceProvider.GetServices(handlerType);

			if (handlers == null || !handlers.Any())
				throw new Exception($"Handler for {type} not found");

			var method = handlerType.GetMethod("HandleAsync");

			foreach (var handler in handlers)
			{
				await (Task)method.Invoke(handler, new[] { @event });
			}
		}
	}
}
