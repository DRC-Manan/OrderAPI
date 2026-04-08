using OAPI.Application.Comman;
using OAPI.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAPI.Application.Commands.Login
{
	public class LoginHandler: ICommandHandler<LoginCommand, Result<string>>
	{
		private readonly ITokenService _tokenService;

		public LoginHandler(ITokenService tokenService)
		{
			_tokenService = tokenService;
		}

		public Task<Result<string>> Handle(LoginCommand command)
		{
			// Dummy validation (replace with DB later)
			if (command.Email == "admin@test.com" && command.Password == "1234")
			{
				return Task.FromResult(Result<string>.Success(_tokenService.GenerateToken(command.Email, "Admin")));
			}

			if (command.Email == "user@test.com" && command.Password == "1234")
			{
				return Task.FromResult(Result<string>.Success(_tokenService.GenerateToken(command.Email, "User")));
			}

			//throw new UnauthorizedAccessException("Invalid credentials");
			return Task.FromResult(Result<string>.Failure("Invalid credentials"));
		}
	}
}
