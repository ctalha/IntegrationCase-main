using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration.Service.LockManagement.Abstractions;
/// <summary>
/// Represents a Redis-based Redlock service for distributed environments using multiple Redis instances.
/// Inherits from ILock interface.
/// </summary>
public interface IRedisRedLockService : ILock
{
}