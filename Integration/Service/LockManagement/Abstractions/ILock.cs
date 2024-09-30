using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration.Service.LockManagement.Abstractions;
/// <summary>
/// Represents a generic lock interface to handle locking mechanisms.
/// </summary>
public interface ILock
{
    /// <summary>
    /// Acquires a lock for the specified key and locked value with a defined expiry time.
    /// </summary>
    /// <param name="key">The key to lock.</param>
    /// <param name="lockedValue">The value associated with the lock.</param>
    /// <param name="expirySeconds">The time in seconds for which the lock will remain active.</param>
    /// <returns>True if the lock is acquired, false otherwise.</returns>
    bool Lock(string key, string lockedValue, int expriySeconds);

    /// <summary>
    /// Releases the lock for the specified key and locked value.
    /// </summary>
    /// <param name="key">The key for which the lock is being released.</param>
    /// <param name="lockedValue">The value associated with the lock.</param>
    void Release(string key, string lockedValue);
}

