using Integration.Service.LockManagement.Abstractions;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration.Service.LockManagement.Service;
/// <summary>
/// Implements ILock using the Redlock algorithm for distributed locks across multiple Redis servers.
/// </summary>
public sealed class RedisRedLockService : Lock, IRedisRedLockService
{
    private readonly List<IDatabase> _redisServers = new();

    /// <summary>
    /// Initializes a new instance of the RedisRedLockService class with multiple Redis connection strings for Redlock.
    /// </summary>
    /// <param name="connectionStrings">A list of Redis connection strings.</param>
    public RedisRedLockService(params string[] connectionStrings)
    {
        foreach (var connection in connectionStrings)
        {
            _redisServers.Add(ConnectionMultiplexer.Connect(connection).GetDatabase());
        }
    }

    /// <inheritdoc />
    public bool Lock(string key, string lockedValue, int expirySeconds)
    {
        var lockTTL = TimeSpan.FromSeconds(expirySeconds);
        var lockTimeout = TimeSpan.FromMilliseconds(expirySeconds * 1000 / 2); // The locking process should take half the time. This implementation can be different according to use case

        var lockStartTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        int lockedCount = TryLock(key, lockedValue, lockTTL);

        if (IsLockTimeout(lockStartTime, lockTimeout))
        {
            Release(key, lockedValue);
            return false;
        }

        if (!CanBeLocked(lockedCount))
        {
            Release(key, lockedValue);
            return false;
        }

        return true;
    }

    /// <inheritdoc />
    public void Release(string key, string lockedValue)
    {
        foreach (var server in _redisServers)
        {
            var savedValue = server.StringGet(key);
            if (string.Equals(savedValue, lockedValue))
            {
                server.KeyDelete(key);
            }
        }
    }


    private int TryLock(string key, string lockedValue, TimeSpan lockTTL)
    {
        // Attempts to acquire locks on multiple Redis servers.

        int lockedCount = 0;
        foreach (var server in _redisServers)
        {
            if (server.StringSet(key, lockedValue, lockTTL, When.NotExists))
            {
                lockedCount++;
            }
        }
        return lockedCount;
    }

    private bool CanBeLocked(int lockedCount)
    {
        // Determines if the lock is acquired on the majority(required locked servers) of servers.

        int requiredLockedServer = _redisServers.Count == 1 ? 1 : (_redisServers.Count / 2) + 1;
        return lockedCount >= requiredLockedServer;
    }

    private static bool IsLockTimeout(long lockStartTime, TimeSpan lockTimeout)
    {
        // Checks if the lock acquisition process has timed out.

        var totalElapsedTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - lockStartTime;
        return totalElapsedTime >= lockTimeout.TotalMilliseconds;
    }
}

