using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace CaptureWindowDemo
{
    /// <summary>
    /// 窗口截图
    /// 参考随笔：http://www.cnblogs.com/08shiyan/p/6843097.html
    /// </summary>
    public static class CaptureWindowHelper
    {

        /// <summary>
        /// 截取指定句柄窗口的图像, 也可也截屏
        /// <para>当然 当前封装是依赖传入参数获取窗口大小，也可以使用API自动获取</para>
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Bitmap CaptureWindow(IntPtr handle, int width, int height)
        {
            try
            {
                // get the hDC of the target window
                IntPtr hdcSrc = User32.GetWindowDC(handle);
                // create a device context we can copy to
                IntPtr hdcDest = GDI32.CreateCompatibleDC(hdcSrc);
                // create a bitmap we can copy it to,
                // using GetDeviceCaps to get the width/height
                IntPtr hBitmap = GDI32.CreateCompatibleBitmap(hdcSrc, width, height);
                // select the bitmap object
                IntPtr hOld = GDI32.SelectObject(hdcDest, hBitmap);
                // bitblt over
                GDI32.BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, GDI32.SRCCOPY);
                // restore selection
                GDI32.SelectObject(hdcDest, hOld);
                // clean up 
                GDI32.DeleteDC(hdcDest);
                User32.ReleaseDC(handle, hdcSrc);

                // get a .NET image object for it
                Bitmap img = Image.FromHbitmap(hBitmap);
                // free up the Bitmap object
                GDI32.DeleteObject(hBitmap);

                return img;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }


        /// <summary>
        /// 截取指定句柄窗口的图像, 也可也截屏
        /// <para>当然 当前封装是依赖传入参数获取窗口大小，也可以使用API自动获取</para>
        /// <para>该方法可以直接生成缩略图， 通过 widthDest 和 heightDest 置顶缩略图宽高</para>
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="widthSrc"></param>
        /// <param name="heightSrc"></param>
        /// <param name="widthDest"></param>
        /// <param name="heightDest"></param>
        /// <returns></returns>
        public static Bitmap CaptureWindow(IntPtr handle, int widthSrc, int heightSrc, int widthDest, int heightDest)
        {
            try
            {
                // get the hDC of the target window
                IntPtr hdcSrc = User32.GetWindowDC(handle);
                // create a device context we can copy to
                IntPtr hdcDest = GDI32.CreateCompatibleDC(hdcSrc);
                // create a bitmap we can copy it to,
                // using GetDeviceCaps to get the width/height
                IntPtr hBitmap = GDI32.CreateCompatibleBitmap(hdcSrc, widthDest, heightDest);
                // select the bitmap object
                IntPtr hOld = GDI32.SelectObject(hdcDest, hBitmap);

                // 设置高质量的缩放模式
                GDI32.SetStretchBltMode(hdcDest, GDI32.STRETCH_HALFTONE);

                // 如果 缩放模式设置为 GDI32.STRETCH_HALFTONE， 则必须调用下面的方法
                GDI32.POINTAPI point;
                GDI32.SetBrushOrgEx(hdcDest, 0, 0, out point);

                // bitblt over
                GDI32.StretchBlt(hdcDest, 0, 0, widthDest, heightDest, hdcSrc, 0, 0, widthSrc, heightSrc, GDI32.SRCCOPY);
                // restore selection
                GDI32.SelectObject(hdcDest, hOld);
                // clean up 
                GDI32.DeleteDC(hdcDest);
                User32.ReleaseDC(handle, hdcSrc);

                // get a .NET image object for it
                Bitmap img = Image.FromHbitmap(hBitmap);
                // free up the Bitmap object
                GDI32.DeleteObject(hBitmap);

                return img;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return null;
        }




        /// <summary>  
        /// Helper class containing Gdi32 API functions  
        /// </summary>  
        public class GDI32
        {
            public const int CAPTUREBLT = 1073741824;
            public const int SRCCOPY = 0x00CC0020; // BitBlt dwRop parameter  
            [DllImport("gdi32.dll")]
            public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest,
                int nWidth, int nHeight, IntPtr hObjectSource,
                int nXSrc, int nYSrc, int dwRop);
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth,
                int nHeight);
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleDC(IntPtr hDC);
            [DllImport("gdi32.dll")]
            public static extern bool DeleteDC(IntPtr hDC);
            [DllImport("gdi32.dll")]
            public static extern bool DeleteObject(IntPtr hObject);
            [DllImport("gdi32.dll")]
            public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);




            /// <summary>
            /// 缩放截图
            /// </summary>
            /// <param name="hdcDest"></param>
            /// <param name="nXOriginDest"></param>
            /// <param name="nYOriginDest"></param>
            /// <param name="nWidthDest"></param>
            /// <param name="nHeightDest"></param>
            /// <param name="hdcSrc"></param>
            /// <param name="nXOriginSrc"></param>
            /// <param name="nYOriginSrc"></param>
            /// <param name="nWidthSrc"></param>
            /// <param name="nHeightSrc"></param>
            /// <param name="dwRop"></param>
            /// <returns></returns>
            [DllImport("gdi32.dll")]
            public static extern bool StretchBlt(IntPtr hdcDest, int nXOriginDest, int nYOriginDest, int nWidthDest, int nHeightDest,
                IntPtr hdcSrc, int nXOriginSrc, int nYOriginSrc, int nWidthSrc, int nHeightSrc, int dwRop);


            public const int STRETCH_ANDSCANS = 0x01;
            public const int STRETCH_ORSCANS = 0x02;
            public const int STRETCH_DELETESCANS = 0x03;
            public const int STRETCH_HALFTONE = 0x04;


            /// <summary>
            /// 设置缩放模式
            /// </summary>
            /// <param name="hdc"></param>
            /// <param name="iStretchMode"></param>
            /// <returns>失败返回0</returns>
            [DllImport("gdi32.dll")]
            public static extern int SetStretchBltMode(IntPtr hdc, int iStretchMode);


            [StructLayout(LayoutKind.Sequential)]
            public struct POINTAPI
            {
                public int x;
                public int y;
            }


            [DllImport("gdi32.dll")]
            public static extern bool SetBrushOrgEx(IntPtr hdc, int nXOrg, int nYOrg, out POINTAPI lppt);





        }


        /// <summary>  
        /// Helper class containing User32 API functions  
        /// </summary>  
        public class User32
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {
                public int left;
                public int top;
                public int right;
                public int bottom;
            }

            [DllImport("user32.dll")]
            public static extern IntPtr GetDesktopWindow();

            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowDC(IntPtr hWnd);

            [DllImport("user32.dll")]
            public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);

            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);


            public const int WM_PAINT = 0x000F;
            [DllImport("user32.dll", EntryPoint = "SendMessageA")]
            public static extern uint SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);


            [DllImport("user32.dll")]
            public static extern bool PrintWindow(
                IntPtr hwnd,                // Window to copy,Handle to the window that will be copied.
                IntPtr hdcBlt,              // HDC to print into,Handle to the device context.
                UInt32 nFlags               // Optional flags,Specifies the drawing options. It can be one of the following values.
                );

            [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
            public static extern int ShowWindow(IntPtr hwnd, int nCmdShow);

            #region  窗口关联
            //            nCmdShow的含义
            //0 关闭窗口
            //1 正常大小显示窗口
            //2 最小化窗口
            //3 最大化窗口
            //使用实例: ShowWindow(myPtr, 0);
            #endregion
        }

    }
}
