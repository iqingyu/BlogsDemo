using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Controls;

namespace ParallelDemo.Demo
{
    public class ParallelClass : AbstractClass
    {
        public ParallelClass(IView view) : base(view)
        {
        }

        /// <summary>
        /// Parallel.Invoke并行多个独立的Action
        /// </summary>
        public void Demo1()
        {
            Task.Run(() =>
            {
                List<Action> actions = new List<Action>();

                // 生成并行任务
                for (int i = 0; i < 5; i++)
                {
                    // 注意、这里很关键，不可直接使用i变量。 
                    // 原因在稍后的随笔中进行说明
                    int index = i;
                    actions.Add(new Action(() =>
                    {
                        PrintInfo(string.Format("Task{0} 开始", index));

                        PrintInfo(string.Format("Task{0} 休眠1秒", index));
                        Thread.Sleep(1000);


                        PrintInfo(string.Format("Task{0} 休眠5秒", index));
                        Thread.Sleep(5000);
                        
                        PrintInfo(string.Format("Task{0} 结束", index));
                    }));
                }

                // 执行并行任务
                Parallel.Invoke(actions.ToArray());

                // 当上述的5个任务全部执行完毕后，才会执行该代码
                PrintInfo("并行任务执行完毕");
            });
        }


        /// <summary>
        /// Parallel简单的For并行
        /// </summary>
        public void Demo2()
        {
            // 为了实时更新UI、将代码异步执行
            Task.Run(() =>
            {
                Parallel.For(1, 100, (index) =>
                {
                    PrintInfo(string.Format("Index:{0}, 开始执行Task", index));

                    Thread.Sleep(1000);
                    PrintInfo(string.Format("Index:{0}, 开始休眠Action 1秒", index));

                    PrintInfo(string.Format("Index:{0}, Task执行完毕", index));
                });

                PrintInfo("并行任务执行完毕");
            });
        }

        /// <summary>
        /// 中断Parallel.For并行
        /// </summary>
        public void Demo3()
        {
            // 为了实时更新UI、将代码异步执行
            Task.Run(() =>
            {
                int breakIndex = new Random().Next(10, 50);
                PrintInfo(" BreakIndex : -------------------------" + breakIndex);

                Parallel.For(1, 100, (index, state) =>
                {
                    PrintInfo(string.Format("Index:{0}, 开始执行Task", index));

                    if (breakIndex == index)
                    {
                        PrintInfo(string.Format("Index:{0}, ------------------ Break Task", index));
                        state.Break();
                        // Break方法执行后、
                        // 大于 当前索引的并且未被安排执行的迭代将被放弃
                        // 小于 当前索引的的迭代将继续正常执行直至迭代执行完毕
                        return;
                    }

                    Thread.Sleep(1000);
                    PrintInfo(string.Format("Index:{0}, 休眠Action 1秒", index));

                    PrintInfo(string.Format("Index:{0}, Task执行完毕", index));
                });

                PrintInfo("并行任务执行完毕");
            });
        }


        /// <summary>
        /// 终止Parallel.For并行
        /// </summary>
        public void Demo4()
        {
            // 为了实时更新UI、将代码异步执行
            Task.Run(() =>
            {
                int stopIndex = new Random().Next(10, 50);
                PrintInfo(" StopIndex : -------------------------" + stopIndex);

                Parallel.For(1, 100, (index, state) =>
                {
                    PrintInfo(string.Format("Index:{0}, 开始执行Task", index));

                    if (stopIndex == index)
                    {
                        PrintInfo(string.Format("Index:{0}, ------------------ Stop Task", index));
                        state.Stop();
                        // Stop方法执行后
                        // 整个迭代将被放弃
                        return;
                    }

                    Thread.Sleep(1000);
                    PrintInfo(string.Format("Index:{0}, 休眠Action 1秒", index));

                    PrintInfo(string.Format("Index:{0}, Task执行完毕", index));
                });

                PrintInfo("并行任务执行完毕");
            });
        }


        /// <summary>
        /// Parallel.For并行中的数据聚合
        /// </summary>
        public void Demo5()
        {
            Task.Run(() =>
            {
                // 求 1 到 10 的阶乘的 和
                long total = 0;
                Parallel.For<long>(1, 10,
                    () =>
                    {
                        PrintInfo("LocalInit");
                        return 0;
                    },
                    (index, state, local) =>
                    {
                        PrintInfo("Body");
                        int result = 1;
                        for (int i = 2; i < index; i++)
                        {
                            result *= i;
                        }
                        local += result;
                        return local;
                    },
                    (x) =>
                    {
                        PrintInfo("LocalFinally");
                        Interlocked.Add(ref total, x);
                    });

                PrintInfo("Total : " + total);
                PrintInfo("并行任务执行完毕");
            });


            // MSDN备注：
            // 对于参与循环执行的每个线程调用 LocalInit 委托一次，并返回每个线程的初始本地状态。 
            // 这些初始状态传递到每个线程上的第一个 body 调用。 然后，每个后续正文调用返回可能修改过的状态值，传递到下一个正文调用。 
            // 最后，每个线程上的最后正文调用返回传递给 LocalFinally 委托的状态值。 
            // 每个线程调用 localFinally 委托一次，以对每个线程的本地状态执行最终操作。 
            // 此委托可以被多个线程同步调用；因此您必须同步对任何共享变量的访问。

            // 也就是说：
            // 1) 并行中开辟的线程数 决定了 LocalInit、LocalFinally 的调用次数
            // 2) 多个 迭代委托、Body 可能被同一个线程调用。
            // 3) 迭代委托、Body 中的 local值，并不一定是 LocalInit 的初始值，也有可能是被修改的返回值。
            // 4) LocalFinally 可能是被同时调用的，需要注意线程同步问题。            
        }
        

        /// <summary>
        /// Parallel.ForEach并行
        /// </summary>
        public void Demo6()
        {
            Task.Run(() =>
            {
                Parallel.ForEach<int>(Enumerable.Range(1, 10), (num) =>
                {
                    PrintInfo("Task 开始");


                    PrintInfo("Task 休眠" + num + "秒");
                    Thread.Sleep(TimeSpan.FromSeconds(num));


                    PrintInfo("Task 结束");
                });

                PrintInfo("并行任务执行完毕");
            });
        }

        /// <summary>
        /// Parallel.ForEach中的索引，中断、终止操作
        /// </summary>
        public void Demo7()
        {
            Task.Run(() =>
            {
                Parallel.ForEach<int>(Enumerable.Range(0, 10), (num, state, index) =>
                {
                    // num, 并行数据源中的数据项
                    // state, 
                    PrintInfo(" Index : " + index + "         Num: " + num);
                });
                PrintInfo("并行任务执行完毕");
            });
        }



    }
}
