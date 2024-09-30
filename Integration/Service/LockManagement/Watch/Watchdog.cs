using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integration.Service.LockManagement.Watch;
public class Watchdog : IDisposable
{
    private readonly Timer _timer;
    private readonly Action _timeoutAction;
    private readonly int _timeoutMilliseconds;
    private bool _isCompleted;
    private bool disposedValue;

    public Watchdog(int timeoutMilliseconds, Action timeoutAction)
    {
        _timeoutMilliseconds = timeoutMilliseconds;
        _timeoutAction = timeoutAction;
        _timer = new Timer(OnTimeout, null, Timeout.Infinite, Timeout.Infinite);
    }

    // İşlem başladığında Watchdog zamanlayıcısını başlatır
    public void Start()
    {
        Console.WriteLine("[LOG] Watchdog started.");
        _timer.Change(_timeoutMilliseconds, Timeout.Infinite);
    }

    // İşlem başarıyla tamamlandığında zamanlayıcıyı durdurur
    public void Complete()
    {
        if (!_isCompleted)
        {
            _isCompleted = true;
            Console.WriteLine("[LOG] Watchdog stopped, process completed.");
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }
    }

    // Watchdog süresi dolarsa bu fonksiyon çağrılır
    private void OnTimeout(object state)
    {
        if (!_isCompleted)
        {
            Console.WriteLine("[LOG] Watchdog timeout occurred.");
            _timeoutAction.Invoke();  // Timeout olduğunda belirtilen aksiyon (örneğin kilidi serbest bırakma) çalıştırılır
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~Watchdog()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
