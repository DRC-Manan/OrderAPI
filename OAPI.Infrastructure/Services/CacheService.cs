using Microsoft.Extensions.Caching.Distributed;
using OAPI.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OAPI.Infrastructure.Services
{
	public class CacheService: ICacheService
	{
		private readonly IDistributedCache _cache;

		public CacheService(IDistributedCache cache)
		{
			_cache = cache;
		}

		public async Task<T?> GetAsync<T>(string key)
		{
			var data = await _cache.GetStringAsync(key);

			if (data == null)
				return default;

			return JsonSerializer.Deserialize<T>(data);
		}

		public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
		{
			var options = new DistributedCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(5)
			};

			var jsonData = JsonSerializer.Serialize(value);
			await _cache.SetStringAsync(key, jsonData, options);
		}

		public async Task RemoveAsync(string key)
		{
			await _cache.RemoveAsync(key);
		}
	}
}
