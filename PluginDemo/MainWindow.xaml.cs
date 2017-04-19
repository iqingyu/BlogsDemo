using System;
using System.Windows;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;

namespace PluginDemo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private AppDomain domain;
        private IPlugin remoteIPlugin;



        public MainWindow()
        {
            InitializeComponent();            
        }

        private void loadBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                unLoadBtn_Click(sender, e);

                this.txtBlock.Text = string.Empty;

                // 在新的AppDomain中加载 RemoteCamera 类型
                AppDomainSetup objSetup = new AppDomainSetup();
                objSetup.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;
                objSetup.ShadowCopyFiles = "true";

                // 虽然此方法已经被标记为过时方法， msdn备注也提倡不使用该方法，
                // 但是 以.net 4.0 + win10环境测试，还必须调用该方法 否则，即便卸载了应用程序域 dll 还是未被解除锁定
                AppDomain.CurrentDomain.SetShadowCopyFiles();

                this.domain = AppDomain.CreateDomain("RemoteAppDomain", null, objSetup);
                this.remoteIPlugin = this.domain.CreateInstance("PluginDemo.NewDomain", "PluginDemo.NewDomain.Plugin").Unwrap() as IPlugin;

                this.txtBlock.AppendText("创建AppDomain成功\r\n\r\n");
            }
            catch (Exception ex)
            {
                this.txtBlock.AppendText(ex.Message);
                this.txtBlock.AppendText("\r\n\r\n");
            }
        }

        private void unLoadBtn_Click(object sender, RoutedEventArgs e)
        {
            if (this.remoteIPlugin != null)
            {
                this.remoteIPlugin = null;
            }

            if (this.domain != null)
            {
                AppDomain.Unload(this.domain);
                this.domain = null;
                this.txtBlock.AppendText("卸载AppDomain成功\r\n\r\n");
            }
        }



        private void invokeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (this.remoteIPlugin == null)
                return;

            this.txtBlock.AppendText($"GetInt():{ this.remoteIPlugin.GetInt().ToString()}\r\n");
            this.txtBlock.AppendText($"GetString():{ this.remoteIPlugin.GetString().ToString()}\r\n");


            try
            {
                this.remoteIPlugin.GetNonMarshalByRefObject();
            }
            catch (Exception ex)
            {
                this.txtBlock.AppendText($"GetNonMarshalByRefObject():{ ex.Message}\r\n");
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }

            this.txtBlock.AppendText("\r\n\r\n\r\n");

            Action firstAction = null;

            try
            {
                firstAction = this.remoteIPlugin.GetAction();

                firstAction();
            }
            catch (Exception ex)
            {
                this.txtBlock.AppendText($"GetAction(), firstAction:  { ex.Message}\r\n");
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }

            try
            {
                this.txtBlock.AppendText("\r\n\r\n\r\n");
                var secondAction = this.remoteIPlugin.GetAction();

                secondAction();
            }
            catch (Exception ex)
            {
                this.txtBlock.AppendText($"GetAction(), secondAction:  { ex.Message}\r\n");
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }


            try
            {
                this.txtBlock.AppendText("\r\n\r\n\r\n");
                if (firstAction != null)
                    firstAction();
            }
            catch (Exception ex)
            {
                this.txtBlock.AppendText($"firstAction:  { ex.Message}\r\n");
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }

            this.txtBlock.AppendText("\r\n\r\n\r\n");
            
            try
            {
                var list1 = this.remoteIPlugin.GetList();
                this.txtBlock.AppendText($"list1.Count: {list1.Count}\r\n");
                var list2 = this.remoteIPlugin.GetList();
                this.txtBlock.AppendText($"list2.Count: {list2.Count}\r\n");


                this.txtBlock.AppendText("\r\n");
                list1.Add("C");
                this.txtBlock.AppendText("list1.Add(\"C\")\r\n");


                this.txtBlock.AppendText($"list1.Count: {list1.Count}\r\n");
                this.txtBlock.AppendText($"list2.Count: {list2.Count}\r\n");

                this.txtBlock.AppendText("！！！此例子，也间接证明支持序列化的类型，在跨AppDomain通信时，传递的是对象副本");
            }
            catch (Exception ex)
            {
                this.txtBlock.AppendText($"GetList():  { ex.Message}\r\n");
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
            }
           
        }
    }
}
