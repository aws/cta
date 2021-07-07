using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CTA.Rules.Config;

namespace CTA.WebForms2Blazor.Services
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
        public enum TaskStallTimeout { None = 0, Short = 100, Medium = 1000, Long = 10000 }

        private IDictionary<int, ManagedTask> _managedTasks;
        private bool _stalled;
        private int _stallOccurrenceNumber;
        private int _nextAvailableTaskId;

        public TaskManagerService()
        {
            _managedTasks = new Dictionary<int, ManagedTask>();
        }

        public int RegisterNewTask()
        {
            var newManagedTask = new ManagedTask(_nextAvailableTaskId);
            _nextAvailableTaskId += 1;

            _managedTasks.Add(newManagedTask.TaskId, newManagedTask);
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
                managedTask.SetActive();
            }
            finally
            {
                UpdateStallState();
            }

            LogHelper.LogInformation(string.Format(TaskStatusUpdateLogTemplate, GetType().Name, taskId, ExitManagedRunLogAction));

            return result;
        }

        public void RetireTask(int taskId)
        {
            _managedTasks.Remove(taskId);
            LogHelper.LogInformation(string.Format(TaskStatusUpdateLogTemplate, GetType().Name, taskId, RetiredLogAction));
            UpdateStallState();
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
            // NOTE: Commented out for use later, currently
            // configured to immediately cancel oldest task
            // on stall

            // Take the delay value as a constructor parameter?
            // var currentStallOccurrence = _stallOccurrenceNumber;
            // await Task.Delay((int)TaskStallTimeout.Short);
            // Ensure that stall state is still active and same occurrence
            // if (_stalled && _stallOccurrenceNumber == currentStallOccurrence)
            // {
            var oldestTask = _managedTasks.Values.OrderBy(managedTask => managedTask.LastStatusChange).FirstOrDefault();

            if (oldestTask != null)
            {
                oldestTask.CancelTask();
                _stalled = false;
                LogHelper.LogInformation(string.Format(Constants.GenericInformationLogTemplate, GetType().Name, UnstalledLogAction));
            }
            // }
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
