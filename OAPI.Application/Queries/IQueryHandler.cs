using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAPI.Application.Queries
{
	public interface IQueryHandler<TQuery, TResult>
	{
		Task<TResult> Handle(TQuery query);
	}
}
