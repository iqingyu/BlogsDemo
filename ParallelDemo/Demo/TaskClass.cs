using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ParallelDemo.Demo
{
    public class TaskClass : AbstractClass
    {
        public TaskClass(IView view) : base(view)
        {
        }


        /// <summary>
        /// Task.NET 4.0 中提倡的方式
        /// <para></para>
        /// </summary>
        public void Demo1()
        {
            // Task 对外公开了构造函数、但是微软并不建议直接使用Task构造函数去实例化对象，而是 使用 Task.Factory.StartNew();

            // MSDN 中的备注如下：
            // For performance reasons, TaskFactory's StartNew method should be the preferred mechanism for creating and scheduling computational tasks,
            // but for scenarios where creation and scheduling must be separated, the constructors may be used, 
            // and the task's Start method may then be used to schedule the task for execution at a later time.
            // Task 类还提供了初始化任务但不计划执行任务的构造函数。 出于性能方面的考虑，TaskFactory 的 StartNew 方法应该是创建和计划计算任务的首选机制，
            // 但是对于创建和计划必须分开的情况，可以使用构造函数，然后可以使用任务的 Start 方法计划任务在稍后执行。


            Task.Factory.StartNew(() =>
            {
                base.PrintInfo("Task.Factory.StartNew(一个参数)");
            }).ContinueWith((t) =>
            {
                base.PrintInfo(t.Id.ToString());
                base.PrintInfo(t.CreationOptions.ToString());
            }).ContinueWith((t) =>
            {
                base.PrintInfo(t.Id.ToString());
                base.PrintInfo(t.CreationOptions.ToString());
            });

            // Task.Factory.StartNew 提供了高达16个的重载函数。
            // 其中 Task.Factory.StartNew<TTask> 是创建带有返回值的异步任务。

            // 以最复杂的重载为例、逐一介绍其参数
            // public Task<TResult> StartNew<TResult>(Func<object, TResult> function, object state, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler);

            // function : 回调函数、我想没有必要做解释吧。
            // state : 回调函数的传入参数
            // CancellationToken : 用以取消Task （后续随笔会做详细介绍）
            // TaskCreationOptions : 指定可控制任务的创建和执行的可选行为的标志(后续随笔会做详细介绍)
            // TaskScheduler : 一个实例 TaskScheduler 类表示一个任务计划程序. 该值一般都是使用 TaskScheduler.Default 

            // 也就是说：
            //      Task.Factory.StartNew(()=> { });
            // 和 
            //      Task.Factory.StartNew(()=> { }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
            // 两种方式效果是一致的。

            // 如果你想更精细化的控制任务、可以使用其他重载方法、传递不同参数以达到预想的目的。
        }

        /// <summary>
        /// Task初始化任务但并不计划执行
        /// </summary>
        public void Demo2()
        {
            // 前文说过 Task 提供了构造函数、它提供了初始化任务，但并不去计划执行的方式。
            // 让我们再看一下 Task 得构造函数吧，还是以最复杂的为例：
            // public Task(Func<object, TResult> function, object state, CancellationToken cancellationToken, TaskCreationOptions creationOptions);
            // 其参数和 Task.Factory.StartNew 相比、 少了TaskScheduler。在性能方面MSDN提示后者会更好。

            base.PrintInfo("初始化任务");
            Task task = new Task(() =>
            {
                base.PrintInfo("被计划的任务开始执行");

                base.PrintInfo("任务休眠5秒");
                Thread.Sleep(5000);

                base.PrintInfo("任务执行完毕");
            });

            // 为了保证能实时更新UI、看到代码执行效果、故而将代码异步执行
            Task.Factory.StartNew(() =>
            {
                base.PrintInfo("休眠两秒");
                Thread.Sleep(2000);

                base.PrintInfo("将任务列入执行计划");
                task.Start();


                base.PrintInfo("等待Task执行完毕");
                task.Wait();// Wait方法 会等待Task执行完毕
                base.PrintInfo("Task执行完毕");
            });

            // 另外再强调一点：Task.Start()， 只是将任务列入执行计划，至于任务什么时候去执行则取决于线程池中什么时候有可用线程。
            // Task.Factory.StartNew 也是一样的。
        }

        /// <summary>
        /// Task.NET 4.5 中的简易方式
        /// </summary>
        public void Demo3()
        {
            Task.Run(() =>
            {
                PrintInfo("简洁的代码");
            });


            Task.Run(() =>
            {
                PrintInfo("验证 CreationOptions 属性");
            }).ContinueWith((t) =>
            {
                PrintInfo("CreationOptions:" + t.CreationOptions.ToString());
            });
        }


        /// <summary>
        /// 封装APM
        /// </summary>
        public void Demo4()
        {
            var response = GetResponse("http://www.cnblogs.com/08shiyan");

            // ... 
        }

        private Task<WebResponse> GetResponse(string url)
        {
            var request = WebRequest.CreateHttp(url);
            request.Method = "GET";
            return Task.Factory.FromAsync<WebResponse>(request.BeginGetResponse, request.EndGetResponse, null);
        }


        /// <summary>
        /// 封装EAP
        /// </summary>
        public void Demo5()
        {
            var result = GetResult("http://www.cnblogs.com/08shiyan");

            // ...
        }

        private Task<string> GetResult(string url)
        {
            TaskCompletionSource<string> source = new TaskCompletionSource<string>();

            WebClient webClient = new WebClient();
            webClient.DownloadStringCompleted += (sender, args) =>
            {
                if (args.Cancelled)
                {
                    source.SetCanceled();
                    return;
                }
                if (args.Error != null)
                {
                    source.SetException(args.Error);
                    return;
                }
                source.SetResult(args.Result);
            };
            webClient.DownloadStringAsync(new Uri(url), null);

            return source.Task;
        }

        

    }
}
