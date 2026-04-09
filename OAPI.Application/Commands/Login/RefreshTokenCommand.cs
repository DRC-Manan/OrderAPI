using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAPI.Application.Commands.Login
{
	public record RefreshTokenCommand(string RefreshToken);
}
