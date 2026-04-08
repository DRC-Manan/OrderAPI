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
		private readonly ICommandHandler<LoginCommand, Result<string>> _loginHandler;

		public AuthController(ICommandHandler<LoginCommand, Result<string>> loginHandler)
		{
			_loginHandler = loginHandler;
		}

		[MapToApiVersion("1.0")]
		[HttpPost("[action]")]
		public async Task<IActionResult> Login(LoginCommand command)
		{
			var result = await _loginHandler.Handle(command);
			return Ok(result);
		}
	}
}
