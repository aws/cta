using NUnit.Framework;
using CTA.WebForms.Services;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace CTA.WebForms.Tests.Services
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
        public async Task ManagedRun_Returns_Result_On_Completion()
        {
            var taskId1 = _tmService.RegisterNewTask();
            var taskId2 = _tmService.RegisterNewTask();

            Func<CancellationToken, Task<int>> mockServiceTask1 = (CancellationToken token) => Task.Run(() => 1);
            var taskResult1 = await _tmService.ManagedRun(taskId1, mockServiceTask1);

            Func<CancellationToken, Task<int>> mockServiceTask2 = (CancellationToken token) => Task.Run(() => 2);
            var taskResult2 = await _tmService.ManagedRun(taskId2, mockServiceTask2);

            Assert.AreEqual(1, taskResult1);
            Assert.AreEqual(2, taskResult2);
        }

        [Test]
        public void ManagedRun_Propagates_Exception_On_Cancellation()
        {
            var taskId1 = _tmService.RegisterNewTask();
            Func<CancellationToken, Task<int>> mockServiceTask1 = (CancellationToken token) => Task.Run(async () => {
                var counter = 0;

                while (counter < 10)
                {
                    if (token.IsCancellationRequested)
                    {
                        throw new OperationCanceledException();
                    }
                    counter += 1;
                    await Task.Delay(25);
                }

                return 1;
            });

            Assert.ThrowsAsync(typeof(OperationCanceledException), async () => await _tmService.ManagedRun(taskId1, mockServiceTask1));
        }

        [Test]
        public void RetireTask_Triggers_Cancellation_If_Other_Tasks_Waiting()
        {
            var taskId1 = _tmService.RegisterNewTask();
            var taskId2 = _tmService.RegisterNewTask();

            Func<CancellationToken, Task<int>> mockServiceTask = (CancellationToken token) => Task.Run(async () => {
                var counter = 0;

                while (counter < 10)
                {
                    if (token.IsCancellationRequested)
                    {
                        throw new OperationCanceledException();
                    }
                    counter += 1;
                    await Task.Delay(25);
                }

                return 1;
            });
            var task1 = _tmService.ManagedRun(taskId1, mockServiceTask);

            _tmService.RetireTask(taskId2);

            Assert.ThrowsAsync(typeof(OperationCanceledException), async () => await task1);
        }
    }
}
