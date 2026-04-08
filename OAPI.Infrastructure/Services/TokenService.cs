using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OAPI.Application.Services;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace OAPI.Infrastructure.Services
{
	public class TokenService: ITokenService
	{
		private readonly IConfiguration _configuration;

		public TokenService(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public string GenerateToken(string email, string role)
		{
			var claims = new[]
			{
				new Claim(ClaimTypes.Email, email),
				new Claim(ClaimTypes.Role, role)
			};

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var token = new JwtSecurityToken(
				issuer: _configuration["Jwt:Issuer"],
				audience: _configuration["Jwt:Audience"],
				claims: claims,
				expires: DateTime.Now.AddHours(1),
				signingCredentials: creds);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}
	}
}
