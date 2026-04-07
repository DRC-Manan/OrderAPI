using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAPI.Application.Event
{
	public interface IEventDispatcher
	{
		Task DispatchAsync(string type, string content);
	}
}
