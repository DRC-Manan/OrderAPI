using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAPI.Application.Queries.GetOrder
{
	public record GetOrdersQuery(string? Email, int Page = 1, int PageSize = 10);
}
