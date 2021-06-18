using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CTA.WebForms2Blazor.Services
{
    public class TaskManagerService
    {
        public enum TaskState { Active, Waiting }
        public enum TaskStallTimeout { None = 0, Short = 100, Medium = 1000, Long = 10000 }

        private IDictionary<int, ManagedTask> _managedTasks;
        private bool _stalled;
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

            return result;
        }

        public void RetireTask(int taskId)
        {
            _managedTasks.Remove(taskId);
            UpdateStallState();
        }

        private void UpdateStallState()
        {
            if (_managedTasks.Count > 0 && !_managedTasks.Values.Any(managedTask => managedTask.CurrentTaskState == TaskState.Active))
            {
                if (!_stalled)
                {
                    _stalled = true;
                    TryResolveStall();
                }
            }
            else
            {
                _stalled = false;
            }
        }

        private async void TryResolveStall()
        {
            // Take the delay value as a constructor parameter?
            // await Task.Delay((int)TaskStallTimeout.Short);
            if (_stalled)
            {
                var oldestTask = _managedTasks.Values.OrderBy(managedTask => managedTask.LastStatusChange).FirstOrDefault();

                if (oldestTask != null)
                {
                     oldestTask.CancelTask();
                    _stalled = false;
                }
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
