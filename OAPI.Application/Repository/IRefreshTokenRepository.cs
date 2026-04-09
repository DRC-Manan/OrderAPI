using OAPI.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAPI.Application.Repository
{
	public interface IRefreshTokenRepository
	{
		Task AddAsync(RefreshToken token);
		Task<RefreshToken?> GetByHashAsync(string hash);
		Task UpdateAsync(RefreshToken token);
	}
}
