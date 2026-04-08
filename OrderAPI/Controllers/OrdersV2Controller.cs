using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Security.Claims;

namespace OrderAPI.Controllers
{
	[ApiVersion("2.0")]
	[Route("api/v{version:apiVersion}/orders")]
	[ApiController]
	public class OrdersV2Controller : ControllerBase
	{
		[MapToApiVersion("2.0")]
		[HttpGet]
		[Authorize(Roles = "User")]
		public IActionResult Get()
		{
			return Ok("This is version 2 of the Orders API.");
		}
	}
}
