using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration.Service.LockManagement.Handler;
/// <summary>
/// Defines a handler interface for managing execution of operations within a locking mechanism.
/// </summary>
public interface ILockHandler
{
    /// <summary>
    /// Executes the provided function within the context of a lock with retry attempts and exponential backoff.
    /// </summary>
    /// <typeparam name="T">The type of the result produced by the function.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <param name="key">The key for which the lock will be acquired.</param>
    /// <param name="expirySeconds">The time in seconds for which the lock will be held.</param>
    /// <param name="maxRetryAttempts">The maximum number of retry attempts to acquire the lock.</param>
    /// <param name="initialDelayMilliseconds">The initial delay in milliseconds between retry attempts, with exponential backoff.</param>
    /// <returns>The result of the executed function if successful, or the default value if the lock could not be acquired.</returns>
    T Execute<T>(Func<T> func, string key, int expirySeconds, int maxRetryAttempts = 3, int initialDelayMilliseconds = 500);
}

