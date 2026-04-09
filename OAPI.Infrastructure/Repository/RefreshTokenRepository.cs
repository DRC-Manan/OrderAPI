using Microsoft.EntityFrameworkCore;
using OAPI.Application.Repository;
using OAPI.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAPI.Infrastructure.Repository
{
	public class RefreshTokenRepository: IRefreshTokenRepository
	{
		private readonly AppDbContext _dbContext;

		public RefreshTokenRepository(AppDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task AddAsync(RefreshToken refreshToken)
		{
			await _dbContext.RefreshTokens.AddAsync(refreshToken);
			await _dbContext.SaveChangesAsync();
		}

		public async Task<RefreshToken?> GetByHashAsync(string hash)
		{
			return await _dbContext.RefreshTokens
				.FirstOrDefaultAsync(rt => 
					rt.TokenHash == hash
					&& !rt.IsRevoked);
		}

		public async Task UpdateAsync(RefreshToken refreshToken)
		{
			_dbContext.RefreshTokens.Update(refreshToken);
			await _dbContext.SaveChangesAsync();
		}

		public async Task RevokeAsync(string hash)
		{
			var token = await GetByHashAsync(hash);
			if (token != null)
			{
				token.Revoke(hash); // Revoke without replacement
				await UpdateAsync(token);
			}
		}
	}
}
