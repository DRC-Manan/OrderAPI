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
	public class RefreshTokenHandler : ICommandHandler<RefreshTokenCommand, AuthResponse>
	{
		private readonly ITokenService _tokenService;
		private readonly IRefreshTokenRepository _refreshTokenRepository;

		public RefreshTokenHandler(ITokenService tokenService
			, IRefreshTokenRepository refreshTokenRepository)
		{
			_tokenService = tokenService;
			_refreshTokenRepository = refreshTokenRepository;
		}

		public async Task<AuthResponse> Handle(RefreshTokenCommand command)
		{
			var response = new AuthResponse { IsAuthenticated = false };

			// In a real application, you would validate the refresh token and check if it's still valid
			var hash = _tokenService.HashToken(command.RefreshToken);
			if (string.IsNullOrEmpty(hash))
				return response;

			// Check if the refresh token exists in the database and is valid
			var storedRefreshToken = await _refreshTokenRepository.GetByHashAsync(hash);
			if (storedRefreshToken == null || storedRefreshToken.IsRevoked || storedRefreshToken.ExpiryDate < DateTime.UtcNow)
				return response;

			// If the refresh token is valid, generate a new access token and a new refresh token
			var newRefreshToken = _tokenService.GenerateRefreshToken();
			var newHash = _tokenService.HashToken(newRefreshToken);
			var newRefreshTokenEntity = new RefreshToken(newHash, storedRefreshToken.UserEmail);

			// 🔥 TOKEN ROTATION - Revoke the old refresh token and save the new one
			storedRefreshToken.Revoke(newHash);
			await _refreshTokenRepository.UpdateAsync(storedRefreshToken);

			// Save the new refresh token in the database
			await _refreshTokenRepository.AddAsync(newRefreshTokenEntity);

			// Generate a new access token
			var newAccessToken = "";

			if (storedRefreshToken.UserEmail == "admin@test.com")
			{
				newAccessToken = _tokenService.GenerateToken(storedRefreshToken.UserEmail, "Admin");
				response.IsAuthenticated = true;
			}
			if (storedRefreshToken.UserEmail == "user@test.com")
			{
				newAccessToken = _tokenService.GenerateToken(storedRefreshToken.UserEmail, "User");
				response.IsAuthenticated = true;
			}

			response.AccessToken = newAccessToken;
			response.RefreshToken = newRefreshToken;

			return response;
		}
	}
}
