using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration.Service.LockManagement.Abstractions;

/// <summary>
/// Represents a service for handling local (in-memory) locking mechanisms.
/// Inherits from ILock interface.
/// </summary>
public interface ILocalLockService : ILock
{
}
