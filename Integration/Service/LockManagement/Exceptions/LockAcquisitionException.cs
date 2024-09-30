using Integration.Service.LockManagement.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration.Service.LockManagement.Exceptions;
/// <summary>
/// Custom exception class for handling errors that occur during lock acquisition.
/// </summary>
public class LockAcquisitionException : Exception
{
    /// <summary>
    /// Initializes a new instance of the LockAcquisitionException class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public LockAcquisitionException(string message) : base(message)
    {
    }
}
