
namespace GenericCacheServiceWithRedis.Services;

public interface ICacheService
{
	Task<T> GetFromHash<T>(string hash, string key);
	Task<IEnumerable<T>> GetHash<T>(string hash);
	Task<bool> RemoveFromHash(string hash, string key);
	Task<bool> RemoveHash(string hash);
	Task<bool> SetHash<T>(IEnumerable<(string key, T value)> data, string hash, TimeSpan expireTime);
	Task SetToHash<T>(T data, string hash, string key);
}