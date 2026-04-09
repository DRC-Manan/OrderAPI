using OAPI.Application.Comman;
using OAPI.Application.Repository;
using OAPI.Application.Services;
using OAPI.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAPI.Application.Commands.Login
{
	public class LoginHandler: ICommandHandler<LoginCommand, AuthResponse>
	{
		private readonly ITokenService _tokenService;
		private readonly IRefreshTokenRepository _refreshTokenRepository;

		public LoginHandler(ITokenService tokenService
			, IRefreshTokenRepository refreshTokenRepository)
		{
			_tokenService = tokenService;
			_refreshTokenRepository = refreshTokenRepository;
		}

		//public Task<Result<string>> Handle(LoginCommand command)
		//{
		//	// Dummy validation (replace with DB later)
		//	if (command.Email == "admin@test.com" && command.Password == "1234")
		//	{
		//		return Task.FromResult(Result<string>.Success(_tokenService.GenerateToken(command.Email, "Admin")));
		//	}

		//	if (command.Email == "user@test.com" && command.Password == "1234")
		//	{
		//		return Task.FromResult(Result<string>.Success(_tokenService.GenerateToken(command.Email, "User")));
		//	}

		//	//throw new UnauthorizedAccessException("Invalid credentials");
		//	return Task.FromResult(Result<string>.Failure("Invalid credentials"));
		//}

		public async Task<AuthResponse> Handle(LoginCommand command)
		{
			// Dummy validation (replace with DB later)
			var response = new AuthResponse
			{
				IsAuthenticated = false
			};

			var accessToken = "";

			if (command.Email == "admin@test.com" && command.Password == "1234")
			{
				accessToken = _tokenService.GenerateToken(command.Email, "Admin");
				response.IsAuthenticated = true;
			}
			if (command.Email == "user@test.com" && command.Password == "1234")
			{
				accessToken = _tokenService.GenerateToken(command.Email, "User");
				response.IsAuthenticated = true;
			}

			if (response.IsAuthenticated)
			{
				var refreshToken = _tokenService.GenerateRefreshToken();
				var refreshTokenHash = _tokenService.HashToken(refreshToken);

				var refreshTokenObj = new RefreshToken(refreshTokenHash, command.Email);

				await _refreshTokenRepository.AddAsync(refreshTokenObj);

				response.AccessToken = accessToken;
				response.RefreshToken = refreshToken;
			}
			
			return response;
		}
	}
}
