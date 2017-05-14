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

namespace CaptureWindowDemo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private IntPtr screenIntptr;
        private System.Drawing.Size screenSize;

        public MainWindow()
        {
            InitializeComponent();

            this.screenIntptr = CaptureWindowHelper.User32.GetDesktopWindow();


            CaptureWindowHelper.User32.RECT rect = new CaptureWindowHelper.User32.RECT();

            CaptureWindowHelper.User32.GetWindowRect(this.screenIntptr, ref rect);

            this.screenSize = new System.Drawing.Size(rect.right - rect.left, rect.bottom - rect.top);
        }

        private void captureFull_Click(object sender, RoutedEventArgs e)
        {
            var bitmap = CaptureWindowHelper.CaptureWindow(this.screenIntptr, this.screenSize.Width, this.screenSize.Height);

            if (bitmap == null)
                return;

            this.img.Source = this.BitmapToBitmapImage(bitmap);
        }

        private void captureThumbnail_Click(object sender, RoutedEventArgs e)
        {
            var bitmap = CaptureWindowHelper.CaptureWindow(this.screenIntptr, this.screenSize.Width, this.screenSize.Height, this.screenSize.Width / 2, this.screenSize.Height / 2);

            if (bitmap == null)
                return;

            this.img.Source = this.BitmapToBitmapImage(bitmap);
        }


        private BitmapImage BitmapToBitmapImage(System.Drawing.Bitmap bitmap)
        {
            BitmapImage bitmapImage = new BitmapImage();

            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);

                bitmapImage.BeginInit();
                bitmapImage.StreamSource = ms;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
            }

            // 释放资源
            bitmap.Dispose();

            return bitmapImage;
        }

    }
}
