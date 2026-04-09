using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OAPI.Application.Comman;
using OAPI.Application.Commands;
using OAPI.Application.Commands.CreateOrder;
using OAPI.Application.Commands.Login;

namespace OrderAPI.Controllers
{
	[ApiVersion("1.0")]
	[Route("api/v{version:apiVersion}/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly ICommandHandler<LoginCommand, AuthResponse> _loginHandler;
		private readonly ICommandHandler<RefreshTokenCommand, AuthResponse> _refreshTokenHandler;
		private readonly ICommandHandler<LogoutCommand, Result<string>> _logoutHandler;

		public AuthController(ICommandHandler<LoginCommand, AuthResponse> loginHandler
			, ICommandHandler<RefreshTokenCommand, AuthResponse> refreshTokenHandler
			, ICommandHandler<LogoutCommand, Result<string>> logoutHandler)
		{
			_loginHandler = loginHandler;
			_refreshTokenHandler = refreshTokenHandler;
			_logoutHandler = logoutHandler;
		}

		[MapToApiVersion("1.0")]
		[HttpPost("[action]")]
		public async Task<IActionResult> Login(LoginCommand command)
		{
			var result = await _loginHandler.Handle(command);
			return Ok(result);
		}

		[MapToApiVersion("1.0")]
		[HttpPost("[action]")]
		public async Task<IActionResult> RefreshToken(RefreshTokenCommand command)
		{
			var result = await _refreshTokenHandler.Handle(command);
			return Ok(result);
		}

		[MapToApiVersion("1.0")]
		[HttpPost("[action]")]
		public async Task<IActionResult> Logout(LogoutCommand command)
		{
			var result = await _logoutHandler.Handle(command);
			return Ok(result);
		}
	}
}
