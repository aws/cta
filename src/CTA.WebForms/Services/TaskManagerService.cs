using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using CTA.Rules.Config;
using Timer = System.Timers.Timer;

namespace CTA.WebForms.Services
{
    public class TaskManagerService
    {
        private const string TaskStatusUpdateLogTemplate = "{0}: Task {1} {2}";
        private const string EnterManagedRunLogAction = "Entering Managed Run";
        private const string ExitManagedRunLogAction = "Exiting Managed Run";
        private const string RetiredLogAction = "Retired";
        private const string StalledLogAction = "Entered Stall";
        private const string UnstalledLogAction = "Exited Stall";

        public enum TaskState { Active, Waiting }
        // Predefined timeout periods (in milliseconds) for when the application enters a stall state
        public enum TaskStallTimeoutMs { None = 0, Short = 100, Medium = 1000, Long = 10000 }

        private readonly ConcurrentDictionary<int, ManagedTask> _managedTasks;
        // When the application enters a stall state, it has to remain in that state for this many milliseconds
        // before we cancel the oldest task
        private readonly TaskStallTimeoutMs _stallTimeoutMs;
        private bool _stalled;
        private int _stallOccurrenceNumber;
        private int _nextAvailableTaskId;

        public TaskManagerService(TaskStallTimeoutMs stallTimeout = TaskStallTimeoutMs.Short)
        {
            _managedTasks = new ConcurrentDictionary<int, ManagedTask>();
            _stallTimeoutMs = stallTimeout;
        }

        public int RegisterNewTask()
        {
            var newManagedTask = new ManagedTask(_nextAvailableTaskId);
            _nextAvailableTaskId += 1;

            // It should be impossible for the key to already exist in the dictionary, but
            // if it does we choose to overwrite it to avoid having a stale managed task that
            // may result in extra stalls
            _managedTasks.AddOrUpdate(newManagedTask.TaskId,
                addValueFactory: (key) => newManagedTask,
                updateValueFactory: (key, managedTask) => newManagedTask);
            UpdateStallState();

            return newManagedTask.TaskId;
        }

        public async Task<TResult> ManagedRun<TResult>(int taskId, Func<CancellationToken, Task<TResult>> func)
        {
            LogHelper.LogInformation(string.Format(TaskStatusUpdateLogTemplate, GetType().Name, taskId, EnterManagedRunLogAction));

            TResult result;
            var managedTask = _managedTasks[taskId];
            var token = managedTask.SetWaiting();
            UpdateStallState();

            try
            {
                result = await func(token);
            }
            finally
            {
                managedTask.SetActive();
                UpdateStallState();
            }

            LogHelper.LogInformation(string.Format(TaskStatusUpdateLogTemplate, GetType().Name, taskId, ExitManagedRunLogAction));

            return result;
        }

        public void RetireTask(int taskId)
        {
            // We don't care to check the result of TryRemove because failure just means that some other
            // thread has already removed the item, which shouldn't ever happen anyway
            _managedTasks.TryRemove(taskId, out var _);
            LogHelper.LogInformation(string.Format(TaskStatusUpdateLogTemplate, GetType().Name, taskId, RetiredLogAction));
            UpdateStallState();
        }
        
        private Timer _watchdogTimer;
        private int _maxWaitBetweenTaskCompletionInMs;
        private int _lastSize;
        private long _lastUpdate;
        public void InitializeWatchdog(int intervalInMs = 1000, int maxWaitBetweenTaskCompletionInMs = 30000)
        {
            LogHelper.LogInformation("Initializing watchdog.");
            _maxWaitBetweenTaskCompletionInMs = maxWaitBetweenTaskCompletionInMs;
            _lastSize = _managedTasks.Count;
            _lastUpdate = DateTime.Now.Ticks;

            // Create a watchdog timer triggers an event every intervalInMs.
            // The event will check if the number of scheduled migration tasks
            // has changed. If the number of tasks has not changed in maxWaitBetweenTaskCompletionInMs,
            // then we assume we have reached a task deadlock and tasks will be cancelled.
            _watchdogTimer = new Timer(intervalInMs);
            _watchdogTimer.Elapsed += ReleaseTasksOnDeadlock;
            _watchdogTimer.AutoReset = true;
            _watchdogTimer.Enabled = true;
        }

        public void DisableWatchdog()
        {
            if (_watchdogTimer == null)
            {
                return;
            }

            LogHelper.LogWarning("Disabling watchdog.");
            _watchdogTimer.Enabled = false;
            _watchdogTimer = null;
        }

        private void ReleaseTasksOnDeadlock(object source, ElapsedEventArgs e)
        {
            LogHelper.LogDebug("Watchdog: Scanning...");
            var now = DateTime.Now.Ticks;
            var currentSize = _managedTasks.Count;
            if (currentSize != _lastSize)
            {
                LogHelper.LogDebug("Watchdog: Tasks are being processed. Continuing.");
                _lastSize = currentSize;
                _lastUpdate = now;
            }
            else
            {
                var timeElapsed = (now - _lastUpdate) / TimeSpan.TicksPerMillisecond;
                LogHelper.LogDebug($"Watchdog: No tasks completed in {timeElapsed}ms");
                if (timeElapsed > _maxWaitBetweenTaskCompletionInMs)
                {
                    CancelRemainingManagedTasks();
                    DisableWatchdog();
                }
            }
        }

        private void CancelRemainingManagedTasks()
        {
            LogHelper.LogWarning($"Watchdog timer exceeded. All {_managedTasks.Count} remaining tasks will be cancelled.");
            foreach (var (taskId, managedTask) in _managedTasks)
            {
                managedTask.CancelTask();
                LogHelper.LogWarning($"Watchdog cancelling Task {taskId} (check logs for a description of this task).");
            }
        }

        private void UpdateStallState()
        {
            var shouldBeStalled = _managedTasks.Count > 0 && _managedTasks.Values.All(managedTask => managedTask.CurrentTaskState == TaskState.Waiting);

            // Previously the service was unstalled and now we
            // want to initiate an attempt to resolve the stall
            if (shouldBeStalled && !_stalled)
            {
                _stalled = true;
                LogHelper.LogInformation(string.Format(Constants.GenericInformationLogTemplate, GetType().Name, StalledLogAction));
                _stallOccurrenceNumber += 1;
                TryResolveStall();
            }
            else if (!shouldBeStalled && _stalled)
            {
                _stalled = false;
                LogHelper.LogInformation(string.Format(Constants.GenericInformationLogTemplate, GetType().Name, UnstalledLogAction));
            }
            // If service is already stalled and still should be
            // then we do nothing
        }

        private async void TryResolveStall()
        {
            var currentStallOccurrence = _stallOccurrenceNumber;
            await Task.Delay((int)_stallTimeoutMs);

            // Ensure that stall state is still active and same occurrence
            if (_stalled && _stallOccurrenceNumber == currentStallOccurrence)
            {
                var oldestTask = _managedTasks.Values.OrderBy(managedTask => managedTask.LastStatusChange).FirstOrDefault();

                if (oldestTask != null)
                {
                    oldestTask.CancelTask();
                    LogHelper.LogInformation(string.Format(Constants.GenericInformationLogTemplate, GetType().Name, UnstalledLogAction));
                }

                _stalled = false;
            }
        }

        private class ManagedTask
        {
            private readonly int _taskId;
            private TaskState _currentTaskState;
            private DateTime _lastStatusChange;
            private CancellationTokenSource _currentCancellationTokenSource;

            public int TaskId { get { return _taskId; } }
            public TaskState CurrentTaskState {
                get
                {
                    return _currentTaskState;
                }
                private set
                {
                    _lastStatusChange = DateTime.Now;
                    _currentTaskState = value;
                }
            }
            public DateTime LastStatusChange { get { return _lastStatusChange; } }
            
            public ManagedTask(int taskId)
            {
                _taskId = taskId;
                CurrentTaskState = TaskState.Active;
            }

            public CancellationToken SetWaiting()
            {
                CurrentTaskState = TaskState.Waiting;
                _currentCancellationTokenSource = new CancellationTokenSource();

                return _currentCancellationTokenSource.Token;
            }

            public void SetActive()
            {
                CurrentTaskState = TaskState.Active;
            }

            public void CancelTask()
            {
                if (_currentTaskState == TaskState.Waiting && _currentCancellationTokenSource != null)
                {
                    CurrentTaskState = TaskState.Active;
                    _currentCancellationTokenSource.Cancel();
                    _currentCancellationTokenSource = null;
                }
            }
        }
    }
}
