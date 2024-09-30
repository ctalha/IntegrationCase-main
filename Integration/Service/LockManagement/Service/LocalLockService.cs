using Integration.Service.LockManagement.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace Integration.Service.LockManagement.Service;

/// <summary>
/// Implementation of ILock that provides a local (in-memory) locking mechanism using MemoryCache.
/// </summary>
public sealed class LocalLockService : Lock, ILocalLockService
{
    public static readonly MemoryCache cache = new("in_memory_cache");

    /// <inheritdoc />
    public bool Lock(string key, string lockedValue, int expriySeconds)
    {
        bool isLocked = false;

        if (cache.Get(key) is null)
        {
            isLocked = true;
            cache.Add(key, lockedValue, DateTimeOffset.UtcNow.AddSeconds(expriySeconds));
        }
        return isLocked;
    }

    /// <inheritdoc />
    public void Release(string key, string lockedValue)
    {
        var savedValue = cache.Get(key).ToString();
        if (string.Equals(savedValue, lockedValue))
        {
            cache.Remove(key);
        }
    }
}

