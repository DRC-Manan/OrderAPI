using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAPI.Application.Services
{
	public interface ITokenService
	{
		string GenerateToken(string email, string role);
		string GenerateRefreshToken();
		public string HashToken(string token);
	}
}
