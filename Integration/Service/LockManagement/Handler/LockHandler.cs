using Integration.Service.LockManagement.Abstractions;
using Integration.Service.LockManagement.Exceptions;
using Integration.Service.LockManagement.Watch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration.Service.LockManagement.Handler;
/// <summary>
/// Implementation of ILockHandler that manages locking and retries for executing functions within a lock context.
/// </summary>
public sealed class LockHandler : ILockHandler
{
    private readonly ILockService _lockService;
    private static int _funcInvokeCounter; // for logging

    /// <summary>
    /// Initializes a new instance of the LockHandler class.
    /// </summary>
    /// <param name="lockService">The lock service to be used for acquiring and releasing locks.</param>
    public LockHandler(ILockService lockService)
    {
        _lockService = lockService;
    }

    /// <inheritdoc />
    public T Execute<T>(Func<T> func, string key, int expirySeconds, int maxRetryAttempts = 3, int initialDelayMilliseconds = 500)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new LockAcquisitionException("Key can not be null or empty");
        }

        Log($"Trying to acquire lock for key: {key}");

        int retryCount = 0;

        int currentDelay = initialDelayMilliseconds;

        var lockedValue = Guid.NewGuid().ToString(); // To check which locked values ​​threads have when releasing locks

        while (retryCount < maxRetryAttempts)
        {
            if (_lockService.Lock(key, lockedValue, expirySeconds))
            {
                Log($"Lock acquired for key: {key}");
                try
                {
                    return ExecuteWithLock(func, key, lockedValue, expirySeconds);
                }
                finally
                {
                    _lockService.Release(key, lockedValue);
                }
            }
            Log($"Can not taken lock: {key}");
            retryCount++;
            Thread.Sleep(currentDelay); // waiting time 
            currentDelay *= 2; // Exponential backoff
        }
        Log($"Failed to execute action after {maxRetryAttempts} attempts for key: {key}");
        return default;

    }

    /// <summary>
    /// Executes the provided function while holding the lock, with a watchdog for timeout management.
    /// </summary>
    private T ExecuteWithLock<T>(Func<T> func, string key, string lockedValue, int expirySeconds)
    {
        using var watchdog = new Watchdog(expirySeconds * 1000 * 2, () =>
        {
            Log($"Timeout occurred for content: {key}, releasing lock.");
            _lockService.Release(key, lockedValue);
        });
        watchdog.Start();

        var result = func.Invoke();

        watchdog.Complete();

        Interlocked.Increment(ref _funcInvokeCounter);

        Console.WriteLine($"[FUNC] FUNC INVOKE {_funcInvokeCounter}");

        Log($"Action executed successfully for key: {key}");

        return result;
    }
    /// <summary>
    /// Logs messages to the console.
    /// </summary>
    private static void Log(string message)
    {
        Console.WriteLine($"[LOG] {message}");
    }
}


