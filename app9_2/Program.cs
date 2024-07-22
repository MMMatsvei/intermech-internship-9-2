public class MemoryUsageMonitor : IDisposable
{
    private long criticalBytes;
    private bool disposed = false;
    private CancellationTokenSource cancellationTokenSource;

    public MemoryUsageMonitor(long critical)
    {
        if (critical <= 0)
        {
            throw new ArgumentOutOfRangeException("Значение должно быть положительным.");
        }

        criticalBytes = critical;

        cancellationTokenSource = new CancellationTokenSource();
    }

    public void Monitor()
    {
        if (disposed)
        {
            throw new ObjectDisposedException(nameof(MemoryUsageMonitor));
        }

        var token = cancellationTokenSource.Token;

        while (!token.IsCancellationRequested)
        {
            double usedMemory = GC.GetTotalMemory(false);

            Console.WriteLine($"Использовано {(usedMemory / 1024 / 1024):F2} MB");

            if (usedMemory >= 0.5 * criticalBytes)
            {
                Console.WriteLine($"Предупреждение: использовано {(usedMemory / criticalBytes * 100):F2}% памяти.");
            }

            Thread.Sleep(1000);
        }
    }

    public void Stop()
    {
        cancellationTokenSource.Cancel();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing) 
            {
                cancellationTokenSource?.Cancel();
            }
            disposed = true;
        }
    }

    ~MemoryUsageMonitor()
    {
        Dispose(false);
    }
}


class Program
{
    static void Main()
    {
        long critical = 50 * 1024 * 1024; 

        using (MemoryUsageMonitor monitor = new MemoryUsageMonitor(critical))
        {
            Thread monitorThread = new Thread(() => monitor.Monitor());
            monitorThread.Start();

            var largeList = new List<byte[]>();
            for (int i = 0; i < 10; i++) 
            {
                largeList.Add(new byte[10 * 1024 * 1024]);
                Thread.Sleep(1000); 
            }

            monitor.Stop();
            monitorThread.Join();
        }
    }
}



