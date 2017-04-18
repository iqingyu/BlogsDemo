using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ParallelDemo
{
    public static class IViewExtension
    {
        public static Dispatcher GetDispatcher(this IView view)
        {
            return view.Control.Dispatcher;
        }

        public static void DispatcherAction(this IView view, Action action)
        {
            view.Control.Dispatcher.Invoke(action);
        }

        public static object GetObjectByName(this IView view, string name)
        {
            return view.Control.FindName(name);
        }
    }
}
