using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ParallelDemo.Demo
{
    /// <summary>
    /// ThreadPool Demo
    /// <para>注： ThreadPool 为静态类，其提供的方法都为静态方法。</para>
    /// </summary>
    public class ThreadPoolClass : AbstractClass
    {
        private AutoResetEvent waitObject;


        public ThreadPoolClass(IView view) : base(view)
        {
            this.waitObject = new AutoResetEvent(false);
        }

        /// <summary>
        /// ThreadPool 的第一种应用场景
        /// <para>将需要异步执行的方法、排入队列，当有可用线程时执行被排入队列的方法</para>
        /// </summary>
        public void Demo1()
        {
            // 该应用场景下 ThreadPool 提供如下两种形式的重载方法
            // public static bool QueueUserWorkItem(WaitCallback callBack);
            // public static bool QueueUserWorkItem(WaitCallback callBack, object state);

            // WaitCallback 为 delegate, 一个object类型的入参，没有返回值。
            // public delegate void WaitCallback(object state);


            base.PrintInfo(nameof(ThreadPool.QueueUserWorkItem));

            ThreadPool.QueueUserWorkItem((state) =>
            {
                this.PrintInfo("等待一秒");
                Thread.Sleep(1000);
                this.PrintInfo("任务执行完毕");
            });
        }

        /// <summary>
        /// ThreadPool 的第二种应用场景
        /// <para>注册等待信号对象(WaitHandle)、并在其收到信号时触发回调函数</para>
        /// </summary>
        public void Demo2()
        {
            // 该应用场景下 ThreadPool 提供了四种形式的重载方法, 下面的重载形式在我看来是最具有直观意义的
            // public static RegisteredWaitHandle RegisterWaitForSingleObject(
            //      WaitHandle waitObject, WaitOrTimerCallback callBack, object state, long millisecondsTimeOutInterval, bool executeOnlyOnce);

            // 其中 WaitHandle 为需要等待信号的类型 的 抽象基类，在后续随笔中会做详细介绍，在此不再多言。

            // 其中 WaitOrTimerCallback 为 回调函数委托
            // public delegate void WaitOrTimerCallback(object state, bool timedOut);

            // state 参数为 回调函数的传入参数

            // millisecondsTimeOutInterval 参数为 计时器超时周期， -1 为永不超时、即一直等待。
            // 对于此参数、第一次接触到的人可能有疑问、怎么还有周期？ 
            // 因为 信号可以重复接到多次、所以当每次接到信号后、或者超时后计时器都会重新计时, 所以有了周期的含义。

            // executeOnlyOnce， True 表示只执行一次, False 会一直等到该信号对象被取消注册，否则 只要接到信号或者超时就会触发回调函数。

            base.PrintInfo(nameof(ThreadPool.RegisterWaitForSingleObject));

            ThreadPool.RegisterWaitForSingleObject(this.waitObject, (state, timeout) =>
            {
                this.PrintInfo("++++++等待对象收到信号++++++");

            }, null, -1, false);


            ThreadPool.QueueUserWorkItem((state) =>
            {
                this.PrintInfo("等待一秒");
                Thread.Sleep(1000);

                this.PrintInfo("等待对象发出信号");
                this.waitObject.Set();

                this.PrintInfo("等待5秒");
                Thread.Sleep(5000);

                this.PrintInfo("等待对象发出信号");
                this.waitObject.Set();

            });

        }


    }
}
