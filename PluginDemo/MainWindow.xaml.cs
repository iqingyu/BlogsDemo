using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

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
            this.txtBlock.AppendText($"GetEnum():{ this.remoteIPlugin.GetEnum().ToString()}\r\n");


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

            if (firstAction == null)
                return;


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
        }
    }
}
