namespace AsyncWorkersApp;

public interface IWorkerLogger
{
    void LogMessage(string message);
}

public class WorkerLogger : IWorkerLogger
{
    private MainWindow? _mainWindow;

    public void SetMainWindow(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
    }

    public void LogMessage(string message)
    {
        _mainWindow?.LogMessage(message);
    }
}
