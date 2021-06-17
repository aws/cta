using NUnit.Framework;
using CTA.WebForms2Blazor.Services;
using System.Threading.Tasks;

namespace CTA.WebForms2Blazor.Tests.Services
{
    public class TaskManagerServiceTests
    {
        private TaskManagerService _tmService;

        [SetUp]
        public void SetUp()
        {
            _tmService = new TaskManagerService();
        }

        [Test]
        public async Task SetTaskWaiting_Cancels_Single_Task_After_Timeout()
        {
            var taskId = _tmService.RegisterNewTask();
            var token = _tmService.SetTaskWaiting(taskId);

            // Add 100 ms to account for any processing
            // delays before we check the task
            await Task.Delay((int)TaskManagerService.TaskStallTimeout.Short + 100);
            Assert.True(token.IsCancellationRequested);
        }

        [Test]
        public async Task SetTaskWaiting_Does_Not_Cancel_If_Active_Tasks_Exist()
        {
            var taskId = _tmService.RegisterNewTask();
            var token = _tmService.SetTaskWaiting(taskId);

            _tmService.RegisterNewTask();

            // Add 100 ms to account for any processing
            // delays before we check the task
            await Task.Delay((int)TaskManagerService.TaskStallTimeout.Short + 100);
            Assert.False(token.IsCancellationRequested);
        }

        [Test]
        public async Task SetTaskWaiting_Cancels_Tasks_With_Shorter_Timeouts_First()
        {
            var taskId1 = _tmService.RegisterNewTask();
            var taskId2 = _tmService.RegisterNewTask();

            var token1 = _tmService.SetTaskWaiting(taskId1, TaskManagerService.TaskStallTimeout.Medium);
            await Task.Delay(100);
            var token2 = _tmService.SetTaskWaiting(taskId2, TaskManagerService.TaskStallTimeout.Short);

            var task1 = Task.Run(async () =>
            {
                while (!token1.IsCancellationRequested)
                {
                    await Task.Delay(5);
                }
            }, token1);

            var task2 = Task.Run(async () =>
            {
                while (!token2.IsCancellationRequested)
                {
                    await Task.Delay(5);
                }
            }, token2);

            var first = await Task.WhenAny(task1, task2);

            Assert.AreEqual(task2, first);
        }

        [Test]
        public async Task SetTaskWaiting_Cancels_Oldest_Of_Shortest_Timeout_Tasks()
        {
            var taskId1 = _tmService.RegisterNewTask();
            var taskId2 = _tmService.RegisterNewTask();

            var token1 = _tmService.SetTaskWaiting(taskId1);
            await Task.Delay(100);
            var token2 = _tmService.SetTaskWaiting(taskId2);

            var task1 = Task.Run(async () =>
            {
                while (!token1.IsCancellationRequested)
                {
                    await Task.Delay(5);
                }
            }, token1);

            var task2 = Task.Run(async () =>
            {
                while (!token2.IsCancellationRequested)
                {
                    await Task.Delay(5);
                }
            }, token2);

            var first = await Task.WhenAny(task1, task2);
            Assert.AreEqual(task1, first);
        }

        [Test]
        public async Task SetTaskWaiting_Resets_Stall_Timeout_Reference_Point_After_Cancellation()
        {
            var taskId1 = _tmService.RegisterNewTask();
            var taskId2 = _tmService.RegisterNewTask();

            var token1 = _tmService.SetTaskWaiting(taskId1);
            await Task.Delay(100);
            var token2 = _tmService.SetTaskWaiting(taskId2);

            var task1 = Task.Run(async () =>
            {
                while (!token1.IsCancellationRequested)
                {
                    await Task.Delay(5);
                }
            }, token1);

            var task2 = Task.Run(async () =>
            {
                while (!token2.IsCancellationRequested)
                {
                    await Task.Delay(5);
                }
            }, token2);

            var first = await Task.WhenAny(task1, task2);
            Assert.AreEqual(task1, first);

            // We use half the timeout to be sure that
            // we didn't check too quick but also not
            // to check too slow and accidentally pass
            // the timeout point
            await Task.Delay((int)TaskManagerService.TaskStallTimeout.Short / 2);
            Assert.False(token2.IsCancellationRequested);

            // Add 100 ms to account for any processing
            // delays before we check the task
            await Task.Delay((int)TaskManagerService.TaskStallTimeout.Short / 2 + 100);
            Assert.True(token2.IsCancellationRequested);
        }

        [Test]
        public async Task SetTaskActive_Prevents_Timeout_Cancellations()
        {
            var taskId1 = _tmService.RegisterNewTask();
            var taskId2 = _tmService.RegisterNewTask();
            var token1 = _tmService.SetTaskWaiting(taskId1);
            var token2 = _tmService.SetTaskWaiting(taskId2);

            _tmService.SetTaskActive(taskId1);

            // Add 100 ms to account for any processing
            // delays before we check the task
            await Task.Delay((int)TaskManagerService.TaskStallTimeout.Short + 100);
            Assert.False(token1.IsCancellationRequested);
            Assert.False(token2.IsCancellationRequested);
        }

        [Test]
        public async Task RetireTask_Triggers_Cancellation_Timeout_If_Other_Tasks_Waiting()
        {
            var taskId1 = _tmService.RegisterNewTask();
            var taskId2 = _tmService.RegisterNewTask();
            var token1 = _tmService.SetTaskWaiting(taskId1);

            _tmService.RetireTask(taskId2);

            // Add 100 ms to account for any processing
            // delays before we check the task
            await Task.Delay((int)TaskManagerService.TaskStallTimeout.Short + 100);
            Assert.True(token1.IsCancellationRequested);
        }
    }
}
