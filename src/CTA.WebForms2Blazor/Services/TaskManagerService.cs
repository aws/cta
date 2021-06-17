using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CTA.WebForms2Blazor.Services
{
    public class TaskManagerService
    {
        public enum TaskState { Active, Waiting, Canceled }
        public enum TaskStallTimeout { None = 0, Short = 100, Medium = 1000, Long = 10000 }

        private IDictionary<int, ManagedTask> _managedTasks;
        // This is the first managed task which should be
        // canceled if necessary, only updated when some kind
        // of state changes
        private ManagedTask _primaryResolutionTarget;
        private DateTime? _currentStallStart;
        private bool _stalled;
        private int _nextAvailableTaskId;

        public TaskManagerService()
        {
            _managedTasks = new Dictionary<int, ManagedTask>();
            _currentStallStart = null;
        }

        public int RegisterNewTask()
        {
            var newManagedTask = new ManagedTask(_nextAvailableTaskId);
            _nextAvailableTaskId += 1;

            _managedTasks.Add(newManagedTask.TaskId, newManagedTask);
            UpdateStallState();

            return newManagedTask.TaskId;
        }
        
        public CancellationToken SetTaskWaiting(int taskId, TaskStallTimeout stallTimeout = TaskStallTimeout.Short)
        {
            var token = _managedTasks[taskId].SetWaiting(stallTimeout);
            UpdateStallState();
            return token;
        }

        public void SetTaskActive(int taskId)
        {
            _managedTasks[taskId].SetActive();
            UpdateStallState();
        }

        public void RetireTask(int taskId)
        {
            _managedTasks.Remove(taskId);
            UpdateStallState();
        }

        // Updates the current stall state and manages the
        // timestamps for when certain stall states were entered
        private void UpdateStallState()
        {
            if (_managedTasks.Count > 0 && !_managedTasks.Values.Any(managedTask => managedTask.CurrentTaskState == TaskState.Active))
            {
                _stalled = true;
                if (_currentStallStart == null)
                {
                    // We don't need to call try resolve stall unless the stall state
                    // has just changed
                    UpdatePrimaryResolutionTarget();
                    TryResolveStall();
                }
                else
                {
                    // We want to update the resolution target no matter what
                    // as changes have occurred that may affect this value even
                    // though the stall state itself hasn't changed
                    UpdatePrimaryResolutionTarget();
                }
            }
            else
            {
                // If there are no managed tasks or active tasks
                // exist then we want to reset stall state and
                // tracking variables
                _stalled = false;
                _currentStallStart = null;
                _primaryResolutionTarget = null;
            }
        }

        // When the stall state is updated in any way
        // and resolves to a stall, we will update this value
        private void UpdatePrimaryResolutionTarget()
        {
            _primaryResolutionTarget = _managedTasks.Values.Where(managedTask => managedTask.CurrentTaskState == TaskState.Waiting)
                .OrderBy(managedTask => managedTask.StallTimeout).ThenBy(managedTask => managedTask.LastStatusChange).FirstOrDefault();
        }

        private async void TryResolveStall()
        {
            _currentStallStart = DateTime.Now;

            while (_stalled)
            {
                var msSinceStall = (int)DateTime.Now.Subtract((DateTime)_currentStallStart).TotalMilliseconds;

                // If primary resolution target is null then that means that the all remaining tasks
                // are in the canceled state
                if (_primaryResolutionTarget?.CanTimeout(msSinceStall) ?? false)
                {
                    _primaryResolutionTarget.Timeout(msSinceStall);
                    // NOTE: We can't just exit the function and change the stall state
                    // here. After a cancellation, the task enters the canceled state and
                    // must notify the service of it's new status depending on how the task
                    // wants to handle the cancellation. We do, however, reset the stall
                    // start tracker to prevent extra tasks from being immediately canceled
                    _currentStallStart = DateTime.Now;
                    UpdateStallState();
                }

                await Task.Delay(25);
            }
        }

        private class ManagedTask
        {
            private const string InvalidTimeoutRequestError = "Attempted to timeout managed task [id:{0}] but timeout is not possible";
            private const string InvalidStateForOperationError = "Attempted {0} operation, which requires task [id:{1}] to be in state {2} but was {3}";
            private const string SetWaitingOperation = "set waiting";

            private readonly int _taskId;
            private TaskStallTimeout _stallTimeout;
            private TaskState _currentTaskState;
            private DateTime _lastStatusChange;
            private CancellationTokenSource _currentCancellationTokenSource;

            public int TaskId { get { return _taskId; } }
            public TaskStallTimeout StallTimeout { get { return _stallTimeout; } }
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
                _stallTimeout = TaskStallTimeout.None;
                CurrentTaskState = TaskState.Active;
            }

            public CancellationToken SetWaiting(TaskStallTimeout stallTimeout)
            {
                // Checks like this exist to enforce proper use
                // of the service, i.e. keeping the service properly
                // updated with task state changes when they happen
                if (_currentTaskState != TaskState.Active)
                {
                    throw new InvalidOperationException(string.Format(InvalidStateForOperationError, SetWaitingOperation, _taskId, TaskState.Active, _currentTaskState));
                }

                CurrentTaskState = TaskState.Waiting;
                _stallTimeout = stallTimeout;
                _currentCancellationTokenSource = new CancellationTokenSource();

                return _currentCancellationTokenSource.Token;
            }

            public void SetActive()
            {
                _stallTimeout = TaskStallTimeout.None;
                CurrentTaskState = TaskState.Active;
            }
            
            public bool CanTimeout(int msSinceStall)
            {
                var msSinceStateChange = DateTime.Now.Subtract(_lastStatusChange).TotalMilliseconds;

                return _currentTaskState == TaskState.Waiting
                    // We will only ever use msSinceStateChange in rare and special cases
                    // where all tasks were at one point in the canceled state, this just ensures
                    // that at least the given timeout is reached from the last state change which
                    // is guaranteed already in all other cases due to the prerequisite of a state
                    // change to waiting in order to enter the stalled state
                    && Math.Max(msSinceStall, msSinceStateChange) > (int)_stallTimeout;
            }

            public void Timeout(int msSinceStall)
            {
                // Call CanTimeout before using this method unless you want
                // an exception to be called on failure
                if (!CanTimeout(msSinceStall))
                {
                    throw new InvalidOperationException(string.Format(InvalidTimeoutRequestError, _taskId));
                }

                CurrentTaskState = TaskState.Canceled;
                _currentCancellationTokenSource.Cancel();
                _currentCancellationTokenSource = null;
            }
        }
    }
}
