using Microsoft.Extensions.Caching.Memory;
using OAPI.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAPI.Infrastructure.Services
{
	public class MemoryCacheService: ICacheService
	{
		private readonly IMemoryCache _memoryCache;
		private static readonly List<string> _keys = new();

		public MemoryCacheService(IMemoryCache memoryCache)
		{
			_memoryCache = memoryCache;
		}

		public Task<T?> GetAsync<T>(string key)
		{
			if (_memoryCache.TryGetValue(key, out T value))
			{
				return Task.FromResult<T?>(value);
			}
			return Task.FromResult<T?>(default);
		}

		public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
		{
			var cacheEntryOptions = new MemoryCacheEntryOptions();
			if (expiration.HasValue)
			{
				cacheEntryOptions.SetAbsoluteExpiration(expiration.Value);
			}
			_keys.Add(key);
			_memoryCache.Set(key, value, cacheEntryOptions);
			return Task.CompletedTask;
		}

		public Task RemoveAsync(string prefix)
		{
			var keysToRemove = _keys.Where(k => k.StartsWith(prefix)).ToList();
			foreach (var key in keysToRemove)
			{
				_memoryCache.Remove(key);
				_keys.Remove(key);
			}
			return Task.CompletedTask;
		}
	}
}
