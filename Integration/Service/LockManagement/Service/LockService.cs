using Integration.Service.LockManagement.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration.Service.LockManagement.Service;
/// <summary>
/// Provides a thread-safe lock service that wraps an existing ILock implementation and ensures lock management within a synchronized context.
/// </summary>
public sealed class LockService : ILockService
{
    private readonly ILock _lock;
    private readonly object _lockObject = new();

    /// <summary>
    /// Initializes a new instance of the LockService class.
    /// </summary>
    /// <param name="lock">The underlying lock mechanism to use.</param>
    public LockService(ILock @lock)
    {
        _lock = @lock ?? throw new ArgumentNullException(nameof(@lock));
    }

    /// <inheritdoc />
    public bool Lock(string key, string lockedValue, int expriySeconds)
    {
        lock (_lockObject)
        {
            return _lock.Lock(key, lockedValue, expriySeconds);
        }
    }

    /// <inheritdoc />
    public void Release(string key, string lockedValue)
    {
        lock (_lockObject)
        {
            _lock.Release(key, lockedValue);
        }
    }
}