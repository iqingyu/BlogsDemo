using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using ParallelDemo;
using System.Threading;

namespace ParallelDemo.Demo
{
    public abstract class AbstractClass
    {
        protected static string TXT_NAME = "txtTip";
        protected IView view;

        public AbstractClass(IView view)
        {
            this.view = view;
        }

        protected void PrintInfo(string tip)
        {
            this.view.DispatcherAction(() =>
            {
                TextBox txtTip = this.view.GetObjectByName(TXT_NAME) as TextBox;
                txtTip.PrintInfo(tip);
            });
        }

        protected void PrintExInfo(Exception ex)
        {
            this.view.DispatcherAction(() =>
            {
                TextBox txtTip = this.view.GetObjectByName(TXT_NAME) as TextBox;
                txtTip.PrintInfo($"Exception.Message: {ex.Message}");
            });
        }

        public void PrintThreadInfo(string tag, string args)
        {
            PrintInfo($"Tag:{tag},  Args:{args},  ManagedThreadId:{Thread.CurrentThread.ManagedThreadId}");
        }
    }
}
