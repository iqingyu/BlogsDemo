using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ParallelDemo.Demo
{
    public class ConcurrentCollectionClass : AbstractClass
    {
        public ConcurrentCollectionClass(IView view) : base(view)
        {
        }


        /// <summary>
        /// MSDN Demo
        /// BlockingCollection<T>.Add()
        /// BlockingCollection<T>.CompleteAdding()
        /// BlockingCollection<T>.TryTake()
        /// BlockingCollection<T>.IsCompleted
        /// </summary>
        public void Demo1()
        {
            // Construct and fill our BlockingCollection
            using (BlockingCollection<int> blocking = new BlockingCollection<int>())
            {
                int NUMITEMS = 10000;

                for (int i = 0; i < NUMITEMS; i++)
                {
                    blocking.Add(i);
                }
                blocking.CompleteAdding();


                int outerSum = 0;

                // Delegate for consuming the BlockingCollection and adding up all items
                Action action = () =>
                {
                    int localItem;
                    int localSum = 0;

                    while (blocking.TryTake(out localItem))
                    {
                        localSum += localItem;
                    }
                    Interlocked.Add(ref outerSum, localSum);
                };

                // Launch three parallel actions to consume the BlockingCollection
                Parallel.Invoke(action, action, action);

                base.PrintInfo(string.Format("Sum[0..{0}) = {1}, should be {2}", NUMITEMS, outerSum, ((NUMITEMS * (NUMITEMS - 1)) / 2)));
                base.PrintInfo(string.Format("bc.IsCompleted = {0} (should be true)", blocking.IsCompleted));

            }
        }


        /// <summary>
        /// 限制容量
        /// </summary>
        public void Demo2()
        {
            BlockingCollection<int> blocking = new BlockingCollection<int>(5);

            Task.Run(() =>
            {
                for (int i = 0; i < 20; i++)
                {
                    blocking.Add(i);
                    PrintInfo($"add:({i})");
                }

                blocking.CompleteAdding();
                PrintInfo("CompleteAdding");
            });

            // 等待先生产数据
            Task.Delay(500).ContinueWith((t) =>
            {
                while (!blocking.IsCompleted)
                {
                    var n = 0;
                    if (blocking.TryTake(out n))
                    {
                        PrintInfo($"TryTake:({n})");
                    }
                }

                PrintInfo("IsCompleted = true");
            });

        }


        /// <summary>
        /// 在 BlockingCollection  中使用Stack
        /// </summary>
        public void Demo3()
        {
            BlockingCollection<int> blocking = new BlockingCollection<int>(new ConcurrentStack<int>(), 5);

            Task.Run(() =>
            {
                for (int i = 0; i < 20; i++)
                {
                    blocking.Add(i);
                    PrintInfo($"add:({i})");
                }

                blocking.CompleteAdding();
                PrintInfo("CompleteAdding");
            });

            // 等待先生产数据
            Task.Delay(500).ContinueWith((t) =>
            {
                while (!blocking.IsCompleted)
                {
                    var n = 0;
                    if (blocking.TryTake(out n))
                    {
                        PrintInfo($"TryTake:({n})");
                    }
                }

                PrintInfo("IsCompleted = true");
            });



        }


    }
}
