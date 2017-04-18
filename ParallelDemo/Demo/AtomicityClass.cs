using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ParallelDemo.Demo
{
    public class AtomicityClass : AbstractClass
    {
        public AtomicityClass(IView view) : base(view)
        {
        }

        /// <summary>
        /// 测试原子性
        /// </summary>
        public void TestAtomicity()
        {
            long test = 0;

            long breakFlag = 0;
            int index = 0;
            Task.Run(() =>
            {
                base.PrintInfo("开始循环   写数据");
                while (true)
                {
                    test = (index % 2 == 0) ? 0x0 : 0x1234567890abcdef;

                    index++;

                    if (Interlocked.Read(ref breakFlag) > 0)
                    {
                        break;
                    }
                }

                base.PrintInfo("退出循环   写数据");
            });

            Task.Run(() =>
            {
                base.PrintInfo("开始循环   读数据");
                while (true)
                {
                    long temp = test;

                    if (temp != 0 && temp != 0x1234567890abcdef)
                    {
                        Interlocked.Increment(ref breakFlag);
                        base.PrintInfo($"读写撕裂:   { Convert.ToString(temp, 16)}");
                        break;
                    }
                }

                base.PrintInfo("退出循环   读数据");
            });
        }

    }
}
