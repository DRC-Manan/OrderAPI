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
	public class LogoutHandler : ICommandHandler<LogoutCommand, Result<string>>
	{
		private readonly ITokenService _tokenService;
		private readonly IRefreshTokenRepository _refreshTokenRepository;

		public LogoutHandler(ITokenService tokenService
			, IRefreshTokenRepository refreshTokenRepository)
		{
			_tokenService = tokenService;
			_refreshTokenRepository = refreshTokenRepository;
		}

		public async Task<Result<string>> Handle(LogoutCommand command)
		{
			var hash = _tokenService.HashToken(command.RefreshToken);
			var storedToken = await _refreshTokenRepository.GetByHashAsync(hash);

			if (storedToken == null)
			{
				return Result<string>.Failure("Invalid refresh token");
			}

			storedToken.Revoke(hash);
			await _refreshTokenRepository.UpdateAsync(storedToken);

			return Result<string>.Success("Logged out successfully");
		}
	}
}
