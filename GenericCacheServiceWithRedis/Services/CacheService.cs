using StackExchange.Redis;
using System.Text.Json;

namespace GenericCacheServiceWithRedis.Services;

public class CacheService : ICacheService
{
	private IDatabase _database;
	public CacheService(IConnectionMultiplexer redis)
	{
		_database = redis.GetDatabase();
	}

	public async Task SetToHash<T>(T data, string hash, string key)
	{
		var exist = await _database.KeyExistsAsync(hash);
		if (!exist)
			return;

		await _database.HashSetAsync(hash,
		[
			new HashEntry(key, JsonSerializer.Serialize(data)),
		]);
	}

	public async Task<IEnumerable<T>> GetHash<T>(string hash)
	{
		var completeHash = await _database.HashGetAllAsync(hash);
		if (completeHash.Length == 0)
			return default;

		return completeHash.Where(e => e.Value.HasValue)
			.Select(e => JsonSerializer.Deserialize<T>(e.Value)).ToList()!;
	}

	public async Task<bool> RemoveFromHash(string hash, string key)
	{
		var exist = await _database.HashExistsAsync(hash, key);
		if (exist)
			return await _database.HashDeleteAsync(hash, key);

		return exist;
	}

	public async Task<bool> RemoveHash(string hash)
	{
		var exist = await _database.KeyExistsAsync(hash);
		if (exist)
			return await _database.KeyDeleteAsync(hash);
		return exist;
	}

	public async Task<bool> SetHash<T>(IEnumerable<(string key, T value)> data, string hash, TimeSpan expireTime)
	{
		await _database.HashSetAsync(hash, data.Select(val => new HashEntry(val.key, JsonSerializer.Serialize(val.value))).ToArray());
		return _database.KeyExpire(hash, expireTime);
	}

	public async Task<T> GetFromHash<T>(string hash, string key)
	{
		var value = await _database.HashGetAsync(hash, key);
		if (value.IsNull)
			return default;

		return JsonSerializer.Deserialize<T>(value);

	}
}
