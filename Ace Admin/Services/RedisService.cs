using StackExchange.Redis;
using System;
using System.Threading.Tasks;

public class RedisService
{
    private readonly IDatabase _db;

    public RedisService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task SetRefreshTokenAsync(int userId, string refreshToken, int expiryDays = 7)
    {
        // user → token
        await _db.StringSetAsync($"refresh:{userId}", refreshToken, TimeSpan.FromDays(expiryDays));

        // token → user (REVERSE MAPPING)
        await _db.StringSetAsync($"refreshToken:{refreshToken}", userId, TimeSpan.FromDays(expiryDays));
    }
    public async Task<string?> GetRefreshTokenAsync(int userId)
    {
        return await _db.StringGetAsync($"refresh:{userId}");
    }
    public async Task<int?> GetUserIdByRefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
            return null;

        string key = $"refreshToken:{refreshToken}";
        var value = await _db.StringGetAsync(key);

        if (value.IsNullOrEmpty)
            return null;

        if (int.TryParse(value, out int userId))
            return userId;

        return null;
    }

    public async Task DeleteRefreshTokenAsync(int userId)
    {
        await _db.KeyDeleteAsync($"refresh:{userId}");
    }

    public async Task SetMachineIdAsync(int userId, string machineId)
    {
        await _db.StringSetAsync($"machine:{userId}", machineId, TimeSpan.FromDays(7));
    }

    public async Task<string?> GetMachineIdAsync(int userId)
    {
        return await _db.StringGetAsync($"machine:{userId}");
    }
}
