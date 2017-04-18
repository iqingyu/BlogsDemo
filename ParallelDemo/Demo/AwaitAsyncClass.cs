using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Text;
using System.Threading.Tasks;

namespace ParallelDemo.Demo
{
    public class AwaitAsyncClass : AbstractClass
    {
        public AwaitAsyncClass(IView view) : base(view)
        {

        }

        /// <summary>
        /// 第一个简单demo
        /// </summary>
        /// <returns></returns>
        public async Task MethodAsync()
        {
            PrintInfo($"ManagedThreadId - 1 - :{Thread.CurrentThread.ManagedThreadId}");

            // 休眠
            await Task.Delay(TimeSpan.FromMilliseconds(100));

            PrintInfo($"ManagedThreadId - 2 - :{Thread.CurrentThread.ManagedThreadId}");
        }

        /// <summary>
        /// 在循环中使用await, 观察使用的线程数量
        /// </summary>
        /// <returns></returns>
        public async Task ForMethodAsync()
        {
            // 休眠
            // await Task.Delay(TimeSpan.FromMilliseconds(100)).ConfigureAwait(false);

            for (int i = 0; i < 5; i++)
            {
                await Task.Run(() =>
                {
                    // 打印线程id
                    PrintThreadInfo("ForMethodAsync", i.ToString());
                });
            }            
        }


        /// <summary>
        /// 死锁 Demo
        /// </summary>
        /// <returns></returns>
        public async Task DeadLockDemoAsync()
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100));// deadlock

            //await Task.Delay(TimeSpan.FromMilliseconds(100)).ConfigureAwait(false);// un-deadlock

            DeadlockDemo.Test();
        }

    }

    /// <summary>
    /// MSDN Demo
    /// </summary>
    public static class DeadlockDemo
    {
        private static async Task DelayAsync()
        {
            await Task.Delay(1000);
        }
        // This method causes a deadlock when called in a GUI or ASP.NET context.
        public static void Test()
        {
            // Start the delay.
            var delayTask = DelayAsync();
            // Wait for the delay to complete.
            delayTask.Wait();
        }
    }
}
