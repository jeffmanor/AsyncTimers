using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace AsyncWorkersApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly List<WorkerThread> _workers = new();
    private readonly Button[] _workerButtons = new Button[16];
    private readonly DispatcherTimer _uiUpdateTimer;
    private readonly IWorkerLogger _logger;

    public MainWindow(IWorkerLogger logger)
    {
        _logger = logger;
        InitializeComponent();
        
        // Set up the logger reference to this window
        if (_logger is WorkerLogger workerLogger)
        {
            workerLogger.SetMainWindow(this);
        }
        
        InitializeWorkers();
        InitializeUI();
        
        // Timer to update UI colors
        _uiUpdateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _uiUpdateTimer.Tick += UpdateUI;
        _uiUpdateTimer.Start();
    }

    private void InitializeWorkers()
    {
        // Worker names
        var workerNames = new string[]
        {
            "Address Lookup",               // Worker 1
            "Send Email",                   // Worker 2
            "Send Text",                    // Worker 3
            "Email Status",                 // Worker 4
            "Text Status",                  // Worker 5
            "Auto Incomplete Visit Roll",   // Worker 6
            "Create Reminder Instances",    // Worker 7
            "Reminders",                    // Worker 8
            "Data Import",                  // Worker 9
            "Data Cleanup",                 // Worker 10
            "Payment Transaction Check",    // Worker 11
            "Sandbox Purge",                // Worker 12
            "Company Backup",               // Worker 13
            "Company Restore",              // Worker 14
            "Six Hour",                     // Worker 15
            "TenDlc"                        // Worker 16
        };

        // Create 16 workers with custom intervals
        var regularIntervals = new TimeSpan[]
        {
            TimeSpan.FromMinutes(1),      // Worker 1: 1 minute
            TimeSpan.FromSeconds(12),     // Worker 2: 12 seconds
            TimeSpan.FromSeconds(12),     // Worker 3: 12 seconds
            TimeSpan.FromMinutes(10),     // Worker 4: 10 minutes
            TimeSpan.FromMinutes(2),      // Worker 5: 2 minutes
            TimeSpan.FromMinutes(10),     // Worker 6: 10 minutes
            TimeSpan.FromSeconds(30),     // Worker 7: 30 seconds
            TimeSpan.FromMinutes(30),     // Worker 8: 30 minutes
            TimeSpan.FromSeconds(10),     // Worker 9: 10 seconds
            TimeSpan.FromHours(12),       // Worker 10: 12 hours
            TimeSpan.FromHours(12),       // Worker 11: 12 hours
            TimeSpan.FromHours(10),       // Worker 12: 10 hours
            TimeSpan.FromSeconds(10),     // Worker 13: 10 seconds
            TimeSpan.FromSeconds(10),     // Worker 14: 10 seconds
            TimeSpan.FromHours(6),        // Worker 15: 6 hours
            TimeSpan.FromDays(15)         // Worker 16: 15 days
        };

        var initialIntervals = new TimeSpan[]
        {
            TimeSpan.FromSeconds(6),      // Worker 1: 6 seconds initially
            TimeSpan.FromSeconds(5),      // Worker 2: 5 seconds initially
            TimeSpan.FromSeconds(4),      // Worker 3: 4 seconds initially
            TimeSpan.FromSeconds(4.5),    // Worker 4: 4.5 seconds initially
            TimeSpan.FromSeconds(1),      // Worker 5: 1 second initially
            TimeSpan.FromSeconds(6.5),    // Worker 6: 6.5 seconds initially
            TimeSpan.FromSeconds(7.5),    // Worker 7: 7.5 seconds initially
            TimeSpan.FromSeconds(8),      // Worker 8: 8 seconds initially
            TimeSpan.FromSeconds(5.5),    // Worker 9: 5.5 seconds initially
            TimeSpan.FromSeconds(60),     // Worker 10: 60 seconds initially
            TimeSpan.FromSeconds(9),      // Worker 11: 9 seconds initially
            TimeSpan.FromSeconds(8.5),    // Worker 12: 8.5 seconds initially
            TimeSpan.FromSeconds(20),     // Worker 13: 20 seconds initially
            TimeSpan.FromSeconds(21),     // Worker 14: 21 seconds initially
            TimeSpan.FromSeconds(7),      // Worker 15: 7 seconds initially
            TimeSpan.FromSeconds(8.5)     // Worker 16: 8.5 seconds initially
        };

        for (int i = 0; i < regularIntervals.Length; i++)
        {
            WorkerThread worker = i switch
            {
                0 => new AddressLookupWorker(i + 1, workerNames[i], regularIntervals[i], initialIntervals[i], _logger),
                1 => new SendEmailWorker(i + 1, workerNames[i], regularIntervals[i], initialIntervals[i], _logger),
                2 => new SendTextWorker(i + 1, workerNames[i], regularIntervals[i], initialIntervals[i], _logger),
                3 => new EmailStatusWorker(i + 1, workerNames[i], regularIntervals[i], initialIntervals[i], _logger),
                4 => new TextStatusWorker(i + 1, workerNames[i], regularIntervals[i], initialIntervals[i], _logger),
                5 => new AutoIncompleteVisitRollWorker(i + 1, workerNames[i], regularIntervals[i], initialIntervals[i], _logger),
                6 => new CreateReminderInstancesWorker(i + 1, workerNames[i], regularIntervals[i], initialIntervals[i], _logger),
                7 => new RemindersWorker(i + 1, workerNames[i], regularIntervals[i], initialIntervals[i], _logger),
                8 => new DataImportWorker(i + 1, workerNames[i], regularIntervals[i], initialIntervals[i], _logger),
                9 => new DataCleanupWorker(i + 1, workerNames[i], regularIntervals[i], initialIntervals[i], _logger),
                10 => new PaymentTransactionCheckWorker(i + 1, workerNames[i], regularIntervals[i], initialIntervals[i], _logger),
                11 => new SandboxPurgeWorker(i + 1, workerNames[i], regularIntervals[i], initialIntervals[i], _logger),
                12 => new CompanyBackupWorker(i + 1, workerNames[i], regularIntervals[i], initialIntervals[i], _logger),
                13 => new CompanyRestoreWorker(i + 1, workerNames[i], regularIntervals[i], initialIntervals[i], _logger),
                14 => new SixHourWorker(i + 1, workerNames[i], regularIntervals[i], initialIntervals[i], _logger),
                15 => new TenDlcWorker(i + 1, workerNames[i], regularIntervals[i], initialIntervals[i], _logger),
                _ => throw new ArgumentOutOfRangeException(nameof(i), "Invalid worker index")
            };
            _workers.Add(worker);
        }
    }

    private void InitializeUI()
    {
        // Store button references for easy access
        _workerButtons[0] = Worker1Button;
        _workerButtons[1] = Worker2Button;
        _workerButtons[2] = Worker3Button;
        _workerButtons[3] = Worker4Button;
        _workerButtons[4] = Worker5Button;
        _workerButtons[5] = Worker6Button;
        _workerButtons[6] = Worker7Button;
        _workerButtons[7] = Worker8Button;
        _workerButtons[8] = Worker9Button;
        _workerButtons[9] = Worker10Button;
        _workerButtons[10] = Worker11Button;
        _workerButtons[11] = Worker12Button;
        _workerButtons[12] = Worker13Button;
        _workerButtons[13] = Worker14Button;
        _workerButtons[14] = Worker15Button;
        _workerButtons[15] = Worker16Button;

        // Wire up event handlers
        StartAllButton.Click += (s, e) => StartAllWorkers();
        StopAllButton.Click += (s, e) => StopAllWorkers();
        ClearLogButton.Click += (s, e) => ClearLog();

        // Wire up worker button click handlers to manually execute work
        Worker1Button.Click += (s, e) => ExecuteWorkerManually(0);
        Worker2Button.Click += (s, e) => ExecuteWorkerManually(1);
        Worker3Button.Click += (s, e) => ExecuteWorkerManually(2);
        Worker4Button.Click += (s, e) => ExecuteWorkerManually(3);
        Worker5Button.Click += (s, e) => ExecuteWorkerManually(4);
        Worker6Button.Click += (s, e) => ExecuteWorkerManually(5);
        Worker7Button.Click += (s, e) => ExecuteWorkerManually(6);
        Worker8Button.Click += (s, e) => ExecuteWorkerManually(7);
        Worker9Button.Click += (s, e) => ExecuteWorkerManually(8);
        Worker10Button.Click += (s, e) => ExecuteWorkerManually(9);
        Worker11Button.Click += (s, e) => ExecuteWorkerManually(10);
        Worker12Button.Click += (s, e) => ExecuteWorkerManually(11);
        Worker13Button.Click += (s, e) => ExecuteWorkerManually(12);
        Worker14Button.Click += (s, e) => ExecuteWorkerManually(13);
        Worker15Button.Click += (s, e) => ExecuteWorkerManually(14);
        Worker16Button.Click += (s, e) => ExecuteWorkerManually(15);

        LogMessage("Application initialized. Use 'Start All' to begin workers.");
    }

    public IEnumerable<WorkerThread> GetWorkers() => _workers;

    private void StartAllWorkers()
    {
        LogMessage("Starting all worker threads...");
        foreach (var worker in _workers)
        {
            worker.Start();
        }
        LogMessage("All workers started successfully.");
    }

    private void StopAllWorkers()
    {
        LogMessage("Stopping all worker threads...");
        foreach (var worker in _workers)
        {
            worker.Stop();
        }
        LogMessage("All workers stopped.");
    }

    private void ClearLog()
    {
        LogTextBox.Clear();
        LogMessage("Log cleared.");
    }

    private void ExecuteWorkerManually(int workerIndex)
    {
        if (workerIndex >= 0 && workerIndex < _workers.Count)
        {
            var worker = _workers[workerIndex];
            if (worker.IsExecuting)
            {
                LogMessage($"{worker.WorkerName} is already executing. Manual execution skipped.");
            }
            else
            {
                LogMessage($"Manual execution triggered for {worker.WorkerName}");
                worker.ExecuteManually();
            }
        }
    }

    public void LogMessage(string message)
    {
        Dispatcher.Invoke(() =>
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            LogTextBox.AppendText($"[{timestamp}] {message}\n");
            LogTextBox.ScrollToEnd();
        });
    }

    private void UpdateUI(object? sender, EventArgs e)
    {
        for (int i = 0; i < _workers.Count; i++)
        {
            var isExecuting = _workers[i].IsExecuting;
            _workerButtons[i].Background = isExecuting ? Brushes.LightGreen : Brushes.LightGray;
        }
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        _uiUpdateTimer.Stop();
        StopAllWorkers();
        base.OnClosing(e);
    }
}

public class WorkerHostedService : IHostedService
{
    private readonly MainWindow _mainWindow;
    private readonly IWorkerLogger _logger;

    public WorkerHostedService(MainWindow mainWindow, IWorkerLogger logger)
    {
        _mainWindow = mainWindow;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogMessage("WorkerHostedService starting...");
        
        // Start all workers automatically
        foreach (var worker in _mainWindow.GetWorkers())
        {
            worker.Start();
        }
        
        _logger.LogMessage("All workers started successfully via HostedService.");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogMessage("WorkerHostedService stopping...");
        
        // Stop all workers
        foreach (var worker in _mainWindow.GetWorkers())
        {
            worker.Stop();
        }
        
        _logger.LogMessage("All workers stopped via HostedService.");
        return Task.CompletedTask;
    }
}