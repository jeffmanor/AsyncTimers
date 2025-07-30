# Async Workers Monitor

A WPF application that demonstrates 16 asynchronous worker threads using the **IHostedService** pattern with dependency injection. Each worker represents a specific business process with custom timing, automatic startup, and real-time monitoring UI.

## Features

- **16 Named Worker Threads**: Each worker represents a specific business process with custom timing
- **IHostedService Architecture**: Modern .NET hosting pattern with dependency injection
- **Automatic Startup**: All workers start automatically when the application launches via HostedService
- **Two-Phase Timing**: Workers run with initial intervals (1-60 seconds) then switch to regular intervals
- **Manual Execution**: Click any worker button to manually trigger its execution immediately
- **Visual Status Indicators**: Buttons turn green when their corresponding worker is executing
- **Real-time Logging**: Activity log shows all worker events with descriptive names and timestamps
- **Control Panel**: Start/Stop all workers and clear the log
- **Asynchronous Execution**: Each worker simulates work for 0.5-3.5 seconds
- **Graceful Shutdown**: Proper cancellation token support for clean shutdown

## Architecture

The application uses modern .NET hosting patterns:

- **IHostedService**: `WorkerHostedService` manages all worker lifecycles
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection for service registration
- **Task-based Workers**: Each worker runs as a managed Task with cancellation support
- **IWorkerLogger Interface**: Abstracted logging with dependency injection
- **Host Builder**: Standard .NET Host.CreateDefaultBuilder() setup

## Key Components

- **App.xaml.cs**: Sets up the hosting environment and dependency injection container
- **WorkerHostedService**: IHostedService implementation that manages all workers
- **WorkerThread**: Individual worker class using Task-based execution loops
- **IWorkerLogger**: Interface for logging with dependency injection support
- **MainWindow**: WPF UI for monitoring and manual control

## How to Run

1. Build the project:
   ```
   dotnet build AsyncWorkersApp.csproj
   ```

2. Run the application:
   ```
   dotnet run --project AsyncWorkersApp.csproj
   ```

## UI Components

### Worker Buttons Panel (Left Side)
- 16 buttons representing workers with their timer intervals in a two-column layout
- Buttons turn **green** when the worker is executing
- Buttons are **gray** when the worker is idle
- **Click any worker button** to manually trigger its execution immediately
- Control buttons to start/stop all workers and clear the log

### Activity Log Panel (Right Side)
- Real-time logging of all worker activities
- Console-style display with timestamps
- Auto-scrolling to show latest messages
- Shows start/stop events and execution duration

## Worker Behavior

Each worker follows a two-phase timing pattern:

### Phase 1: Initial Intervals (First execution only)
- **Address Lookup**: 6 seconds → then 1 minute
- **Send Email**: 5 seconds → then 12 seconds
- **Send Text**: 4 seconds → then 12 seconds
- **Email Status**: 4.5 seconds → then 10 minutes
- **Text Status**: 1 second → then 2 minutes
- **Auto Incomplete Visit Roll**: 6.5 seconds → then 10 minutes
- **Create Reminder Instances**: 7.5 seconds → then 30 seconds
- **Reminders**: 8 seconds → then 30 minutes
- **Data Import**: 5.5 seconds → then 10 seconds
- **Data Cleanup**: 60 seconds → then 12 hours
- **Payment Transaction Check**: 9 seconds → then 12 hours
- **Sandbox Purge**: 8.5 seconds → then 10 hours
- **Company Backup**: 20 seconds → then 10 seconds
- **Company Restore**: 21 seconds → then 10 seconds
- **Six Hour**: 7 seconds → then 6 hours
- **TenDlc**: 8.5 seconds → then 15 days

### Phase 2: Regular Intervals (All subsequent executions)
After the first execution, each worker switches to its regular interval and continues indefinitely.

Each worker:
- **Starts automatically** when the application launches
- Runs with an **initial interval** for the first execution
- **Switches to regular interval** after the first execution completes
- **Can be manually triggered** by clicking its button (immediate execution)
- Simulates work for 0.5-3.5 seconds (random duration)
- Logs start and completion times with interval transitions
- Prevents overlapping executions (manual execution skipped if already running)
- Can be manually started and stopped through the control buttons

## Technical Details

- **Framework**: .NET 9.0 WPF Application
- **Hosting**: Uses `Microsoft.Extensions.Hosting` with IHostedService pattern
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Threading**: Task-based execution loops with CancellationToken support
- **UI Updates**: Uses `DispatcherTimer` for smooth UI refresh (100ms)
- **Async Work**: Simulates work using `Task.Delay`
- **Thread Safety**: Volatile fields and proper async synchronization

## Architecture Details

- `App.xaml.cs`: Host builder setup and dependency injection configuration
- `WorkerHostedService`: IHostedService managing all worker lifecycles
- `WorkerThread`: Individual worker with Task-based execution loops
- `IWorkerLogger`: Abstracted logging interface for dependency injection
- `MainWindow`: WPF UI with dependency injection support
