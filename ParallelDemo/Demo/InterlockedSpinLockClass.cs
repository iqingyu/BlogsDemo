using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ParallelDemo.Demo
{
    /// <summary>
    /// 原子操作和自旋锁Demo
    /// </summary>
    public class InterlockedSpinLockClass : AbstractClass
    {
        public InterlockedSpinLockClass(IView view) : base(view)
        {
        }

        /// <summary>
        /// 原子操作-计数
        /// </summary>
        public void Demo1()
        {
            Task.Run(() =>
            {
                long total = 0;
                long result = 0;

                PrintInfo("正在计数");

                Parallel.For(0, 10, (i) =>
                {
                    for (int j = 0; j < 10000000; j++)
                    {
                        Interlocked.Increment(ref total);
                        result++;
                    }
                });

                PrintInfo($"操作结果应该为\t\t: {10 * 10000000}");
                PrintInfo($"原子操作结果\t\t: {total}");
                PrintInfo($"i++操作结果\t\t: {result}");
            });
        }

        /// <summary>
        /// 原子操作-单例模式
        /// </summary>
        public void Demo2()
        {
            ConcurrentQueue<InterlockedSingleClass> queue = new ConcurrentQueue<Demo.InterlockedSpinLockClass.InterlockedSingleClass>();

            // 虽然这个测试不严谨、但也或多或少的说明了一些问题
            for (int i = 0; i < 10; i++) // 同时分配的线程数过多、调度器反而调度不过来
            {
                Task.Run(() =>
                {
                    var result = InterlockedSingleClass.SingleInstance;

                    queue.Enqueue(result);
                });
            }


            // 1秒钟后显示结果
            Task.Delay(1000).ContinueWith((t) =>
            {
                PrintInfo($"利用原子操作-单例模式、生成的对象总数:{queue.Count}");

                InterlockedSingleClass firstItem = null;
                queue.TryDequeue(out firstItem);

                for (int i = 0; i < queue.Count;)
                {
                    InterlockedSingleClass temp = null;
                    queue.TryDequeue(out temp);

                    if (temp == null || firstItem == null || !object.ReferenceEquals(temp, firstItem))
                    {
                        PrintInfo("单例模式失效(按照预期、该代码不会被运行到)");
                    }
                }

                PrintInfo("原子操作-单例模式-运行完毕");
            });

        }


        /// <summary>
        /// 自旋锁Demo,来源MSDN
        /// </summary>
        public void Demo3()
        {
            SpinLock sl = new SpinLock();

            StringBuilder sb = new StringBuilder();

            // Action taken by each parallel job.
            // Append to the StringBuilder 10000 times, protecting
            // access to sb with a SpinLock.
            Action action = () =>
            {
                bool gotLock = false;
                for (int i = 0; i < 10000; i++)
                {
                    gotLock = false;
                    try
                    {
                        sl.Enter(ref gotLock);

                        sb.Append((i % 10).ToString());
                    }
                    finally
                    {
                        // Only give up the lock if you actually acquired it
                        if (gotLock)
                            sl.Exit();
                    }
                }
            };

            // Invoke 3 concurrent instances of the action above
            Parallel.Invoke(action, action, action);

            // Check/Show the results
            PrintInfo($"sb.Length = {sb.Length} (should be 30000)");

            PrintInfo($"number of occurrences of '5' in sb: {sb.ToString().Where(c => (c == '5')).Count()} (should be 3000)");

        }



        public class InterlockedSingleClass
        {
            private static InterlockedSingleClass single = null;

            public static InterlockedSingleClass SingleInstance
            {
                get
                {
                    // if (single == null) // 为了测试效果，该行代码注释掉
                    {
                        Interlocked.CompareExchange<InterlockedSingleClass>(ref single, new InterlockedSingleClass(), null);
                    }

                    return single;
                }
            }

        }
    }
}
