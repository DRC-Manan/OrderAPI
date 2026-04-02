using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAPI.Application.Commands
{
	public interface ICommandHandler<TCommand, TResult>
	{
		Task<TResult> Handle(TCommand command);
	}
}
