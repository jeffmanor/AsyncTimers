using Microsoft.Extensions.Hosting;

namespace AsyncWorkersApp;

public abstract class WorkerThread
{
    private readonly int _workerId;
    private readonly string _workerName;
    private readonly TimeSpan _regularInterval;
    private readonly TimeSpan _initialInterval;
    protected readonly IWorkerLogger _logger;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _workerTask;
    private volatile bool _isExecuting = false;
    private volatile bool _isRunning = false;
    private volatile bool _hasRunInitial = false;

    public bool IsExecuting => _isExecuting;
    public bool IsRunning => _isRunning;
    public string WorkerName => _workerName;
    public int WorkerId => _workerId;
    public TimeSpan RegularInterval => _regularInterval;
    public TimeSpan InitialInterval => _initialInterval;

    public WorkerThread(int workerId, string workerName, TimeSpan regularInterval, TimeSpan initialInterval, IWorkerLogger logger)
    {
        _workerId = workerId;
        _workerName = workerName;
        _regularInterval = regularInterval;
        _initialInterval = initialInterval;
        _logger = logger;
    }

    public void Start()
    {
        if (_isRunning) return;
        
        _isRunning = true;
        _hasRunInitial = false;
        _cancellationTokenSource = new CancellationTokenSource();
        
        string initialDescription = GetIntervalDescription(_initialInterval);
        string regularDescription = GetIntervalDescription(_regularInterval);
        _logger.LogMessage($"{_workerName} started with {initialDescription} initial interval, then {regularDescription} regular interval");

        // Start the worker task
        _workerTask = Task.Run(async () => await ExecuteWorkerLoop(_cancellationTokenSource.Token));
    }

    public void Stop()
    {
        if (!_isRunning) return;
        
        _isRunning = false;
        _cancellationTokenSource?.Cancel();
        _workerTask?.Wait(5000); // Wait up to 5 seconds for graceful shutdown
        _cancellationTokenSource?.Dispose();
        _logger.LogMessage($"{_workerName} stopped");
    }

    private async Task ExecuteWorkerLoop(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested && _isRunning)
            {
                // Wait for the appropriate interval
                TimeSpan waitTime = _hasRunInitial ? _regularInterval : _initialInterval;
                
                try
                {
                    await Task.Delay(waitTime, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break; // Exit the loop if cancelled
                }

                if (cancellationToken.IsCancellationRequested || !_isRunning)
                    break;

                await ExecuteWork(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogMessage($"{_workerName} worker loop encountered error: {ex.Message}");
        }
    }

    public void ExecuteManually()
    {
        // Execute work immediately on a background thread
        Task.Run(async () =>
        {
            if (_isExecuting) return;

            _isExecuting = true;
            _logger.LogMessage($"{_workerName} starting manual execution...");

            try
            {
                // Execute work using the virtual method
                var workDuration = await DoWorkAsync();
                
                _logger.LogMessage($"{_workerName} completed manual execution (took {workDuration.TotalSeconds:F1}s)");
            }
            catch (Exception ex)
            {
                _logger.LogMessage($"{_workerName} encountered error during manual execution: {ex.Message}");
            }
            finally
            {
                _isExecuting = false;
            }
        });
    }

    private string GetIntervalDescription(TimeSpan interval)
    {
        if (interval.TotalDays >= 1)
            return $"{interval.TotalDays} day(s)";
        else if (interval.TotalHours >= 1)
            return $"{interval.TotalHours} hour(s)";
        else if (interval.TotalMinutes >= 1)
            return $"{interval.TotalMinutes} minute(s)";
        else
            return $"{interval.TotalSeconds} second(s)";
    }

    protected abstract Task<TimeSpan> DoWorkAsync(CancellationToken cancellationToken = default);

    private async Task ExecuteWork(CancellationToken cancellationToken)
    {
        if (_isExecuting || !_isRunning) return;

        _isExecuting = true;
        
        bool wasInitialRun = !_hasRunInitial;
        if (!_hasRunInitial)
        {
            _logger.LogMessage($"{_workerName} starting initial execution...");
            _hasRunInitial = true;
        }
        else
        {
            _logger.LogMessage($"{_workerName} starting execution...");
        }

        try
        {
            // Execute work using the virtual method
            var workDuration = await DoWorkAsync(cancellationToken);
            
            _logger.LogMessage($"{_workerName} completed execution (took {workDuration.TotalSeconds:F1}s)");
        }
        catch (OperationCanceledException)
        {
            _logger.LogMessage($"{_workerName} execution was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogMessage($"{_workerName} encountered error: {ex.Message}");
        }
        finally
        {
            _isExecuting = false;
            
            // After first execution, log the interval switch
            if (wasInitialRun && _isRunning)
            {
                string regularDescription = GetIntervalDescription(_regularInterval);
                _logger.LogMessage($"{_workerName} switching to regular {regularDescription} interval");
            }
        }
    }
}

// Specialized Worker Classes
public class AddressLookupWorker : WorkerThread
{
    public AddressLookupWorker(int workerId, string workerName, TimeSpan regularInterval, TimeSpan initialInterval, IWorkerLogger logger)
        : base(workerId, workerName, regularInterval, initialInterval, logger) { }

    protected override async Task<TimeSpan> DoWorkAsync(CancellationToken cancellationToken = default)
    {
        // Address lookup: Quick database query simulation
        var startTime = DateTime.Now;
        var random = new Random();
        var workDuration = TimeSpan.FromSeconds(random.NextDouble() * 1.0 + 0.5); // 0.5-1.5 seconds
        await Task.Delay(workDuration, cancellationToken);
        var endTime = DateTime.Now;
        return endTime - startTime;
    }
}

public class SendEmailWorker : WorkerThread
{
    public SendEmailWorker(int workerId, string workerName, TimeSpan regularInterval, TimeSpan initialInterval, IWorkerLogger logger)
        : base(workerId, workerName, regularInterval, initialInterval, logger) { }

    protected override async Task<TimeSpan> DoWorkAsync(CancellationToken cancellationToken = default)
    {
        // Email sending: Network operation simulation
        var startTime = DateTime.Now;
        var random = new Random();
        var workDuration = TimeSpan.FromSeconds(random.NextDouble() * 2.0 + 1.0); // 1.0-3.0 seconds
        await Task.Delay(workDuration, cancellationToken);
        var endTime = DateTime.Now;
        return endTime - startTime;
    }
}

public class SendTextWorker : WorkerThread
{
    public SendTextWorker(int workerId, string workerName, TimeSpan regularInterval, TimeSpan initialInterval, IWorkerLogger logger)
        : base(workerId, workerName, regularInterval, initialInterval, logger) { }

    protected override async Task<TimeSpan> DoWorkAsync(CancellationToken cancellationToken = default)
    {
        // SMS sending: Quick API call simulation
        var startTime = DateTime.Now;
        var random = new Random();
        var workDuration = TimeSpan.FromSeconds(random.NextDouble() * 1.5 + 0.5); // 0.5-2.0 seconds
        await Task.Delay(workDuration, cancellationToken);
        var endTime = DateTime.Now;
        return endTime - startTime;
    }
}

public class EmailStatusWorker : WorkerThread
{
    public EmailStatusWorker(int workerId, string workerName, TimeSpan regularInterval, TimeSpan initialInterval, IWorkerLogger logger)
        : base(workerId, workerName, regularInterval, initialInterval, logger) { }

    protected override async Task<TimeSpan> DoWorkAsync(CancellationToken cancellationToken = default)
    {
        // Email status checking: Batch processing simulation
        var startTime = DateTime.Now;
        var random = new Random();
        var workDuration = TimeSpan.FromSeconds(random.NextDouble() * 2.5 + 1.5); // 1.5-4.0 seconds
        await Task.Delay(workDuration, cancellationToken);
        var endTime = DateTime.Now;
        return endTime - startTime;
    }
}

public class TextStatusWorker : WorkerThread
{
    public TextStatusWorker(int workerId, string workerName, TimeSpan regularInterval, TimeSpan initialInterval, IWorkerLogger logger)
        : base(workerId, workerName, regularInterval, initialInterval, logger) { }

    protected override async Task<TimeSpan> DoWorkAsync(CancellationToken cancellationToken = default)
    {
        // Text status checking: Quick status verification
        var startTime = DateTime.Now;
        var random = new Random();
        var workDuration = TimeSpan.FromSeconds(random.NextDouble() * 1.0 + 0.8); // 0.8-1.8 seconds
        await Task.Delay(workDuration, cancellationToken);
        var endTime = DateTime.Now;
        return endTime - startTime;
    }
}

public class AutoIncompleteVisitRollWorker : WorkerThread
{
    public AutoIncompleteVisitRollWorker(int workerId, string workerName, TimeSpan regularInterval, TimeSpan initialInterval, IWorkerLogger logger)
        : base(workerId, workerName, regularInterval, initialInterval, logger) { }

    protected override async Task<TimeSpan> DoWorkAsync(CancellationToken cancellationToken = default)
    {
        // Visit roll processing: Complex business logic simulation
        var startTime = DateTime.Now;
        var random = new Random();
        var workDuration = TimeSpan.FromSeconds(random.NextDouble() * 3.0 + 2.0); // 2.0-5.0 seconds
        await Task.Delay(workDuration, cancellationToken);
        var endTime = DateTime.Now;
        return endTime - startTime;
    }
}

public class CreateReminderInstancesWorker : WorkerThread
{
    public CreateReminderInstancesWorker(int workerId, string workerName, TimeSpan regularInterval, TimeSpan initialInterval, IWorkerLogger logger)
        : base(workerId, workerName, regularInterval, initialInterval, logger) { }

    protected override async Task<TimeSpan> DoWorkAsync(CancellationToken cancellationToken = default)
    {
        // Reminder creation: Database insertions simulation
        var startTime = DateTime.Now;
        var random = new Random();
        var workDuration = TimeSpan.FromSeconds(random.NextDouble() * 1.5 + 1.0); // 1.0-2.5 seconds
        await Task.Delay(workDuration, cancellationToken);
        var endTime = DateTime.Now;
        return endTime - startTime;
    }
}

public class RemindersWorker : WorkerThread
{
    public RemindersWorker(int workerId, string workerName, TimeSpan regularInterval, TimeSpan initialInterval, IWorkerLogger logger)
        : base(workerId, workerName, regularInterval, initialInterval, logger) { }

    protected override async Task<TimeSpan> DoWorkAsync(CancellationToken cancellationToken = default)
    {
        // Reminder processing: Notification sending simulation
        var startTime = DateTime.Now;
        var random = new Random();
        var workDuration = TimeSpan.FromSeconds(random.NextDouble() * 2.0 + 1.5); // 1.5-3.5 seconds
        await Task.Delay(workDuration, cancellationToken);
        var endTime = DateTime.Now;
        return endTime - startTime;
    }
}

public class DataImportWorker : WorkerThread
{
    public DataImportWorker(int workerId, string workerName, TimeSpan regularInterval, TimeSpan initialInterval, IWorkerLogger logger)
        : base(workerId, workerName, regularInterval, initialInterval, logger) { }

    protected override async Task<TimeSpan> DoWorkAsync(CancellationToken cancellationToken = default)
    {
        // Data import: Fast incremental import simulation
        var startTime = DateTime.Now;
        var random = new Random();
        var workDuration = TimeSpan.FromSeconds(random.NextDouble() * 0.8 + 0.3); // 0.3-1.1 seconds
        await Task.Delay(workDuration, cancellationToken);
        var endTime = DateTime.Now;
        return endTime - startTime;
    }
}

public class DataCleanupWorker : WorkerThread
{
    public DataCleanupWorker(int workerId, string workerName, TimeSpan regularInterval, TimeSpan initialInterval, IWorkerLogger logger)
        : base(workerId, workerName, regularInterval, initialInterval, logger) { }

    protected override async Task<TimeSpan> DoWorkAsync(CancellationToken cancellationToken = default)
    {
        // Data cleanup: Heavy processing simulation
        var startTime = DateTime.Now;
        var random = new Random();
        var workDuration = TimeSpan.FromSeconds(random.NextDouble() * 4.0 + 3.0); // 3.0-7.0 seconds
        await Task.Delay(workDuration, cancellationToken);
        var endTime = DateTime.Now;
        return endTime - startTime;
    }
}

public class PaymentTransactionCheckWorker : WorkerThread
{
    public PaymentTransactionCheckWorker(int workerId, string workerName, TimeSpan regularInterval, TimeSpan initialInterval, IWorkerLogger logger)
        : base(workerId, workerName, regularInterval, initialInterval, logger) { }

    protected override async Task<TimeSpan> DoWorkAsync(CancellationToken cancellationToken = default)
    {
        // Payment verification: Financial processing simulation
        var startTime = DateTime.Now;
        var random = new Random();
        var workDuration = TimeSpan.FromSeconds(random.NextDouble() * 2.5 + 2.0); // 2.0-4.5 seconds
        await Task.Delay(workDuration, cancellationToken);
        var endTime = DateTime.Now;
        return endTime - startTime;
    }
}

public class SandboxPurgeWorker : WorkerThread
{
    public SandboxPurgeWorker(int workerId, string workerName, TimeSpan regularInterval, TimeSpan initialInterval, IWorkerLogger logger)
        : base(workerId, workerName, regularInterval, initialInterval, logger) { }

    protected override async Task<TimeSpan> DoWorkAsync(CancellationToken cancellationToken = default)
    {
        // Sandbox purge: File system operations simulation
        var startTime = DateTime.Now;
        var random = new Random();
        var workDuration = TimeSpan.FromSeconds(random.NextDouble() * 3.5 + 2.5); // 2.5-6.0 seconds
        await Task.Delay(workDuration, cancellationToken);
        var endTime = DateTime.Now;
        return endTime - startTime;
    }
}

public class CompanyBackupWorker : WorkerThread
{
    public CompanyBackupWorker(int workerId, string workerName, TimeSpan regularInterval, TimeSpan initialInterval, IWorkerLogger logger)
        : base(workerId, workerName, regularInterval, initialInterval, logger) { }

    protected override async Task<TimeSpan> DoWorkAsync(CancellationToken cancellationToken = default)
    {
        // Company backup: Fast incremental backup simulation
        var startTime = DateTime.Now;
        var random = new Random();
        var workDuration = TimeSpan.FromSeconds(random.NextDouble() * 1.2 + 0.8); // 0.8-2.0 seconds
        await Task.Delay(workDuration, cancellationToken);
        var endTime = DateTime.Now;
        return endTime - startTime;
    }
}

public class CompanyRestoreWorker : WorkerThread
{
    public CompanyRestoreWorker(int workerId, string workerName, TimeSpan regularInterval, TimeSpan initialInterval, IWorkerLogger logger)
        : base(workerId, workerName, regularInterval, initialInterval, logger) { }

    protected override async Task<TimeSpan> DoWorkAsync(CancellationToken cancellationToken = default)
    {
        // Company restore: Verification and validation simulation
        var startTime = DateTime.Now;
        var random = new Random();
        var workDuration = TimeSpan.FromSeconds(random.NextDouble() * 1.5 + 1.0); // 1.0-2.5 seconds
        await Task.Delay(workDuration, cancellationToken);
        var endTime = DateTime.Now;
        return endTime - startTime;
    }
}

public class SixHourWorker : WorkerThread
{
    public SixHourWorker(int workerId, string workerName, TimeSpan regularInterval, TimeSpan initialInterval, IWorkerLogger logger)
        : base(workerId, workerName, regularInterval, initialInterval, logger) { }

    protected override async Task<TimeSpan> DoWorkAsync(CancellationToken cancellationToken = default)
    {
        // Six hour maintenance: Comprehensive system check simulation
        var startTime = DateTime.Now;
        var random = new Random();
        var workDuration = TimeSpan.FromSeconds(random.NextDouble() * 5.0 + 4.0); // 4.0-9.0 seconds
        await Task.Delay(workDuration, cancellationToken);
        var endTime = DateTime.Now;
        return endTime - startTime;
    }
}

public class TenDlcWorker : WorkerThread
{
    public TenDlcWorker(int workerId, string workerName, TimeSpan regularInterval, TimeSpan initialInterval, IWorkerLogger logger)
        : base(workerId, workerName, regularInterval, initialInterval, logger) { }

    protected override async Task<TimeSpan> DoWorkAsync(CancellationToken cancellationToken = default)
    {
        // 10DLC compliance: Regulatory processing simulation
        var startTime = DateTime.Now;
        var random = new Random();
        var workDuration = TimeSpan.FromSeconds(random.NextDouble() * 6.0 + 5.0); // 5.0-11.0 seconds
        await Task.Delay(workDuration, cancellationToken);
        var endTime = DateTime.Now;
        return endTime - startTime;
    }
}
