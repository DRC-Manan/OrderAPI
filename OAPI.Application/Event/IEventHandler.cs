using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAPI.Application.Event
{
	public interface IEventHandler<TEvent>
	{
		Task HandleAsync(TEvent @event);
	}
}
