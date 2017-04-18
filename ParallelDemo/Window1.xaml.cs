using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ParallelDemo.Demo;
using System.Net;

namespace ParallelDemo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class Window1 : Window, IView
    {
        #region 字段

        private ThreadPoolClass threadPoolClass;

        private TaskClass taskClass;

        private ParallelClass parallelClass;

        private PLinqClass plinqClass;

        private AwaitAsyncClass awaitClass;

        private AtomicityClass atomicityClass;

        private ReaderWriterLockSlimClass readerWriterClass;

        private InterlockedSpinLockClass spinLockClass;

        private VariableCapturingClass variableClass;

        private ConcurrentCollectionClass concurrentClass;

        #endregion

        public ButtonClickCommand ButtonClickCommand
        {
            get
            {
                return new ButtonClickCommand(ButtonClick);
            }
        }

        public Control Control
        {
            get
            {
                return this;
            }
        }

        public Window1()
        {
            InitializeComponent();

            this.DataContext = this;

            this.threadPoolClass = new ThreadPoolClass(this);
            this.taskClass = new TaskClass(this);
            this.parallelClass = new Demo.ParallelClass(this);
            this.plinqClass = new PLinqClass(this);
            this.awaitClass = new Demo.AwaitAsyncClass(this);
            this.atomicityClass = new Demo.AtomicityClass(this);
            this.readerWriterClass = new Demo.ReaderWriterLockSlimClass(this);
            this.spinLockClass = new Demo.InterlockedSpinLockClass(this);
            this.variableClass = new Demo.VariableCapturingClass(this);
            this.concurrentClass = new Demo.ConcurrentCollectionClass(this);


            this.Init();
        }


        #region ThreadPool

        [Tag("ThreadPool第一种应用场景")]
        private void Demo1()
        {
            this.threadPoolClass.Demo1();
        }


        [Tag("ThreadPool第二种应用场景")]
        private void Demo2()
        {
            this.threadPoolClass.Demo2();
        }

        #endregion


        #region Delegate 即 APM系列

        [Tag("Delegate.BeginInvoke")]
        private void Demo3()
        {
            this.txtTip.PrintInfo("UI, Id:" + Thread.CurrentThread.ManagedThreadId);

            Action action = new Action(this.DelegateTest);
            action.BeginInvoke(null, null);
            action.BeginInvoke(null, null);
        }

        private void DelegateTest()
        {
            int id = Thread.CurrentThread.ManagedThreadId;

            this.Dispatcher.Invoke(() =>
            {
                this.txtTip.PrintInfo("BeginInvoke, Id:" + id);
            });
        }


        [Tag("Delegate.BeginInvoke带有参数和返回值")]
        private void Demo4()
        {
            Func<string, string> func = new Func<string, string>(this.DelegateTest2);

            this.txtTip.PrintInfo(" 输入参数 123 ");

            func.BeginInvoke(" 123 ", new AsyncCallback((System.IAsyncResult result) =>
            {
                Func<string, string> tmp = result.AsyncState as Func<string, string>;
                if (tmp != null)
                {
                    string returnResult = tmp.EndInvoke(result);

                    this.Dispatcher.Invoke(() =>
                    {
                        this.txtTip.PrintInfo(" 函数回调 结果 : " + returnResult);
                    });
                }
            }), func);

        }
        private string DelegateTest2(string args)
        {
            return args + " : Return args";
        }

        #endregion


        #region Task

        [Tag("Task.NET 4.0 中提倡的方式")]
        private void Demo5()
        {
            taskClass.Demo1();
        }



        [Tag("Task初始化任务但并不计划执行")]
        private void Demo6()
        {
            this.taskClass.Demo2();
        }


        [Tag("Task.NET 4.5 中的简易方式")]
        private void Demo7()
        {
            this.taskClass.Demo3();
        }

        #endregion


        #region Parallel

        [Tag("Parallel.Invoke并行多个独立的Action")]
        private void Demo8()
        {
            this.parallelClass.Demo1();
        }

        [Tag("Parallel简单的For并行")]
        private void Demo9()
        {
            this.parallelClass.Demo2();
        }

        [Tag("中断Parallel.For并行")]
        private void Demo10()
        {
            this.parallelClass.Demo3();
        }

        [Tag("终止Parallel.For并行")]
        private void Demo11()
        {
            this.parallelClass.Demo4();
        }

        [Tag("Parallel.For并行中的数据聚合")]
        private void Demo12()
        {
            this.parallelClass.Demo5();
        }


        [Tag("Parallel.ForEach并行")]
        private void Demo13()
        {
            this.parallelClass.Demo6();
        }

        [Tag("Parallel.ForEach中的索引，中断、终止操作")]
        private void Demo14()
        {
            this.parallelClass.Demo7();
        }

        #endregion


        #region EPM系列

        [Tag("EPM系列")]
        private void Demo15()
        {
            var address = "http://www.cnblogs.com/08shiyan/";

            WebClient client = new WebClient();
            Uri uri = new Uri(address);

            client.DownloadStringCompleted += new DownloadStringCompletedEventHandler((object sender, DownloadStringCompletedEventArgs e) =>
            {
                this.txtTip.PrintInfo("下载完成");
            });

            client.DownloadStringAsync(uri);

            this.txtTip.PrintInfo("开始异步下载数据");
        }

        #endregion


        #region PLinq

        [Tag("PLinq:Linq的并行版本")]
        private void Demo16()
        {
            this.plinqClass.Demo1();
        }

        [Tag("PLinq:AsOrdered")]
        private void Demo17()
        {
            this.plinqClass.Demo2();
        }

        #endregion


        #region await async

        [Tag("await async:第一个demo")]
        private async void Demo20()
        {
            var t = this.awaitClass.MethodAsync();

            this.txtTip.PrintInfo($"ManagedThreadId - 3 - :{Thread.CurrentThread.ManagedThreadId}");

            await t;
        }


        [Tag("For await")]
        private async void Demo21()
        {
            this.awaitClass.PrintThreadInfo("For await-1", "");

            await this.awaitClass.ForMethodAsync();

            this.awaitClass.PrintThreadInfo("For await-2", "");

            new List<string>();
        }

        [Tag("DeadLock")]
        private async void Demo22()
        {
            this.awaitClass.PrintThreadInfo("DeadLock-1", "");

            await this.awaitClass.DeadLockDemoAsync();

            this.awaitClass.PrintThreadInfo("DeadLock-2", "");
        }


        #endregion


        #region 封装 APM  和  EAP

        /// <summary>
        /// APM
        /// </summary>
        private void Demo23()
        {
            this.taskClass.Demo4();
        }

        /// <summary>
        /// EAP
        /// </summary>
        private void Demo24()
        {
            this.taskClass.Demo5();
        }

        #endregion


        #region 原子性操作

        /// <summary>
        /// 看似安全的操作实际不安全
        /// </summary>
        [Tag("Long-原子操作测试")]
        private void Demo25()
        {
            this.atomicityClass.TestAtomicity();
        }

        #endregion


        #region 读写锁

        /// <summary>
        /// 读写锁
        /// </summary>
        [Tag("读写锁-ReaderWriterLockSlim")]
        private void Demo26()
        {
            this.readerWriterClass.Demo1();
        }

        /// <summary>
        /// 读写锁线程关联性
        /// </summary>
        [Tag("读写锁-测试重复进入不同锁模式")]
        private void Demo27()
        {
            this.readerWriterClass.Demo2();
        }

        #endregion


        #region 原子操作和自旋锁

        [Tag("原子操作-计数")]
        private void Demo28()
        {
            this.spinLockClass.Demo1();
        }

        [Tag("原子操作-单例模式")]
        private void Demo29()
        {
            this.spinLockClass.Demo2();
        }


        [Tag("自旋锁")]
        private void Demo30()
        {
            this.spinLockClass.Demo3();
        }
        #endregion


        #region 闭包

        [Tag("闭包-1")]
        private void Demo31()
        {
            this.variableClass.Demo1();
        }

        [Tag("闭包-2-窥探闭包的本质")]
        private void Demo32()
        {
            this.variableClass.Demo2();
        }


        #endregion


        #region 线程安全的集合

        [Tag("线程安全的集合-MSDN")]
        private void Demo33()
        {
            this.concurrentClass.Demo1();
        }

        [Tag("线程安全的集合-限制容量")]
        private void Demo34()
        {
            this.concurrentClass.Demo2();
        }

        [Tag("线程安全的集合-在BlockingCollection中使用Stack")]
        private void Demo35()
        {
            this.concurrentClass.Demo3();
        }
        #endregion










        private void Init()
        {
            var tags = this.GetType()
                .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                /*.AsParallel()*/.Select((m) =>
                {
                    object[] attribute = m.GetCustomAttributes(typeof(TagAttribute), false);
                    if (attribute.Length == 1)
                    {
                        var args = m.GetParameters();

                        if (args.Length == 0 && m.ReturnType == typeof(void))
                            return (attribute[0] as TagAttribute).Tag;
                    }

                    return string.Empty;
                }).Where(tag => !string.IsNullOrEmpty(tag));


            foreach (var item in tags)
            {
                Button btn = new Button() { Tag = item, Content = item, Margin = new Thickness(2) };

                btn.SetBinding(Button.CommandProperty, new Binding(nameof(this.ButtonClickCommand)) { Source = this });

                btn.SetBinding(Button.CommandParameterProperty, new Binding(nameof(Button.Tag)) { Source = btn });

                this.panel.Children.Add(btn);
            }

        }

        private void ButtonClick(object tag)
        {
            string tagString = tag as string;
            if (string.IsNullOrEmpty(tagString))
                return;

            var methods = this.GetType().GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.Instance).AsParallel().Where((p) =>
             {
                 object[] attribute = p.GetCustomAttributes(typeof(TagAttribute), false);
                 if (attribute.Length == 1)
                 {
                     if ((attribute[0] as TagAttribute).Tag == tagString)
                     {
                         var args = p.GetParameters();

                         if (args.Length == 0 && p.ReturnType == typeof(void))
                             return true;
                     }
                 }

                 return false;
             });

            this.txtTip.PrintInfo("");
            this.txtTip.PrintInfo("========" + tagString + "========");
            foreach (var item in methods)
            {
                item.Invoke(this, null);
            }
        }

    }

    public static class TextBoxExtension
    {
        public static void PrintInfo(this TextBox txtTip, string tip)
        {
            if (txtTip != null)
            {
                if (txtTip.LineCount > 30 && string.IsNullOrEmpty(tip))
                    txtTip.Text = string.Empty;

                txtTip.AppendText(string.Format("{0} {1}\r\n", DateTime.Now.ToString("HH:mm:ss"), tip));
            }
        }
    }

    public class ButtonClickCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private Action<object> action;

        public ButtonClickCommand(Action<object> action)
        {
            this.action = action;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (this.action != null)
            {
                this.action(parameter);
            }
        }
    }


    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    sealed class TagAttribute : Attribute
    {
        private readonly string tag;

        public TagAttribute(string tag)
        {
            this.tag = tag;
        }

        public string Tag
        {
            get { return tag; }
        }
    }

}
