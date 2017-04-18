using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParallelDemo.Demo
{
    /// <summary>
    /// 闭包、变量捕获相关Demo
    /// </summary>
    public class VariableCapturingClass : AbstractClass
    {
        public VariableCapturingClass(IView view) : base(view)
        {

        }

        /// <summary>
        /// 闭包、变量捕获引发的bug
        /// </summary>
        public void Demo1()
        {
            int total = 0;

            List<Task> taskList = new List<Task>();

            for (int i = 0; i < 10; i++)
            {
                var task = Task.Run(() =>
                {
                    System.Threading.Interlocked.Add(ref total, i);
                });

                taskList.Add(task);
            }

            Task.WaitAll(taskList.ToArray());

            PrintInfo(total.ToString());

            if (total > 49)
            {
                PrintInfo("闭包、捕获了变量i,使结果超出预期");
            }
        }

        /// <summary>
        /// 窥探闭包的本质
        /// </summary>
        public void Demo2()
        {
            var func = GetFunc();

            PrintInfo($"result:{func().ToString()}"); // 输出结果 结果为12
        }

        private Func<int> GetFunc()
        {
            int result = 10;

            Func<int> func = () =>
            {
                result++;

                return result;
            };

            result++;

            return func;
        }


    }
}
