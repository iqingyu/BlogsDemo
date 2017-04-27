using System;
using System.Collections.Generic;
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
using System.Windows.Ink;
using System.Text.RegularExpressions;
using System.Reflection;

namespace DropDownCustomColorPicker
{

    public partial class ColorPicker : UserControl
    {
        // 该变量辅助 SHIFT + 3
        private bool shiftPressed = false;
        private bool selectChanged = false;

        private Color customColor = Colors.Black;


        public CustomColorPicker CustomColorPicker
        {
            get { return (CustomColorPicker)GetValue(CustomColorPickerProperty); }
            set { SetValue(CustomColorPickerProperty, value); }
        }

        public static readonly DependencyProperty CustomColorPickerProperty =
            DependencyProperty.Register("CustomColorPicker", typeof(CustomColorPicker), typeof(ColorPicker), new PropertyMetadata(null));



        public Color CustomColor
        {
            get { return customColor; }
            set
            {
                if (customColor != value)
                {
                    customColor = value;

                    UpdatePreview();

                    if (CustomColorPicker != null)
                        CustomColorPicker.RaiseSelectedColorChangedEvent();
                }
            }
        }



        public ColorPicker()
        {
            InitializeComponent();

            InitColors();

            image.Source = loadBitmap(DropDownCustomColorPicker.Properties.Resources.ColorSwatchCircle);
            txtAll.LostFocus += new RoutedEventHandler(txtAll_LostFocus);
            txtAll.KeyDown += new KeyEventHandler(txtAll_KeyDown);

            CanvasColor.MouseLeftButtonDown += new MouseButtonEventHandler(CanColor_MouseLeftButtonDown);
            CanvasColor.MouseMove += CanvasColor_MouseMove;

            // 防止菜单关闭
            this.MouseLeftButtonUp += ColorPicker_MouseLeftButtonUp;

            InitDefaultValues();
        }


        #region Private Method

        
        private void InitDefaultValues()
        {
            this.CustomColor = this.customColor;
            this.epDefaultcolor.IsExpanded = true;
        }

        private void InitColors()
        {
            DefaultPicker.Items.Clear();
            CustomColors customColors = new CustomColors();
            foreach (var item in customColors.SelectableColors)
            {
                DefaultPicker.Items.Add(item);
            }
            DefaultPicker.SelectionChanged += new SelectionChangedEventHandler(DefaultPicker_SelectionChanged);
        }

        private void UpdatePreview()
        {
            lblPreview.Background = new SolidColorBrush(CustomColor);

            string alphaHex = CustomColor.A.ToString("X").PadLeft(2, '0');
            string redHex = CustomColor.R.ToString("X").PadLeft(2, '0');
            string greenHex = CustomColor.G.ToString("X").PadLeft(2, '0');
            string blueHex = CustomColor.B.ToString("X").PadLeft(2, '0');

            txtAll.Text = String.Format("#{0}{1}{2}{3}", alphaHex, redHex, greenHex, blueHex);

            if (SdA.Value != CustomColor.A)
                SdA.Value = CustomColor.A;

            if (SdR.Value != CustomColor.R)
                SdR.Value = CustomColor.R;

            if (SdG.Value != CustomColor.G)
                SdG.Value = CustomColor.G;

            if (SdB.Value != CustomColor.B)
                SdB.Value = CustomColor.B;
        }



        private Color MakeColorFromHex(object sender)
        {
            try
            {
                ColorConverter cc = new ColorConverter();
                return (Color)cc.ConvertFrom(((TextBox)sender).Text);

            }
            catch
            {
                string alphaHex = CustomColor.A.ToString("X").PadLeft(2, '0');
                string redHex = CustomColor.R.ToString("X").PadLeft(2, '0');
                string greenHex = CustomColor.G.ToString("X").PadLeft(2, '0');
                string blueHex = CustomColor.B.ToString("X").PadLeft(2, '0');

                txtAll.Text = String.Format("#{0}{1}{2}{3}", alphaHex, redHex, greenHex, blueHex);
            }

            return CustomColor;
        }

        public static BitmapSource loadBitmap(System.Drawing.Bitmap source)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(source.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
        }


        private void ChangeColor()
        {
            try
            {
                CustomColor = GetColorFromImage((int)Mouse.GetPosition(CanvasColor).X, (int)Mouse.GetPosition(CanvasColor).Y);
                MovePointer();
            }
            catch
            {

            }
        }

        private void Reposition()
        {
            // 效率太低，屏蔽该功能
            return;

            for (int i = 0; i < CanvasColor.ActualWidth; i++)
            {
                bool flag = false;

                for (int j = 0; j < CanvasColor.ActualHeight; j++)
                {

                    try
                    {
                        Color Colorfromimagepoint = GetColorFromImage(i, j);

                        if (SimmilarColor(Colorfromimagepoint, CustomColor))
                        {
                            MovePointerDuringReposition(i, j);
                            flag = true;
                            break;
                        }

                    }
                    catch
                    {
                    }
                }

                if (flag)
                {
                    break;
                }
            }

        }


        /// <summary>
        /// 1*1 pixel copy is based on an article by Lee Brimelow    
        /// http://thewpfblog.com/?p=62
        /// </summary>
        private Color GetColorFromImage(int i, int j)
        {
            CroppedBitmap cb = new CroppedBitmap(image.Source as BitmapSource,
                new Int32Rect(i,
                    j, 1, 1));
            byte[] color = new byte[4];
            cb.CopyPixels(color, 4, 0);
            Color Colorfromimagepoint = Color.FromArgb((byte)SdA.Value, color[2], color[1], color[0]);
            return Colorfromimagepoint;
        }

        private void MovePointerDuringReposition(int i, int j)
        {
            EpPointer.SetValue(Canvas.LeftProperty, (double)(i - 3));
            EpPointer.SetValue(Canvas.TopProperty, (double)(j - 3));
            EpPointer.InvalidateVisual();
            CanvasColor.InvalidateVisual();
        }

        private void MovePointer()
        {
            EpPointer.SetValue(Canvas.LeftProperty, (double)(Mouse.GetPosition(CanvasColor).X - 5));
            EpPointer.SetValue(Canvas.TopProperty, (double)(Mouse.GetPosition(CanvasColor).Y - 5));
            CanvasColor.InvalidateVisual();
        }

        private bool SimmilarColor(Color pointColor, Color selectedColor)
        {
            int diff = Math.Abs(pointColor.R - selectedColor.R) + Math.Abs(pointColor.G - selectedColor.G) + Math.Abs(pointColor.B - selectedColor.B);
            if (diff < 20) return true;
            else
                return false;
        }

        #endregion
        

        #region Events


        private void DefaultPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DefaultPicker.SelectedValue != null)
            {
                CustomColor = (Color)DefaultPicker.SelectedValue;
                selectChanged = true;
            }

            // Tip 其实可以在这里直接关闭Picker的，之所以绕远，在 MouseUp事件里去关闭， 
            // 是为了兼容该控件在 Popup 上使用的情况（方便控制焦点转移 和 Popup自由控件开关）

            //if (this.CustomColorPicker != null)
            //{
            //    this.CustomColorPicker.ClosePicker();
            //}            
        }


        private void DefaultPicker_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.selectChanged = false;
        }

        private void DefaultPicker_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.selectChanged)
            {
                if (this.CustomColorPicker != null)
                {
                    this.CustomColorPicker.ClosePicker();
                }
            }
        }

        private void ColorPicker_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // 防止菜单关闭
            e.Handled = true;
        }

        private void CanvasColor_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                ChangeColor();
                e.Handled = true;
            }
        }

        private void CanColor_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ChangeColor();
            e.Handled = true;
        }

        private void txtAll_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    if (string.IsNullOrEmpty(((TextBox)sender).Text))
                        return;

                    CustomColor = MakeColorFromHex(sender);
                    Reposition();
                }
                catch
                {
                }
            }

            string input = e.Key.ToString().Substring(1);
            if (string.IsNullOrEmpty(input))
            {
                input = e.Key.ToString();
            }
            if (input == "3" && shiftPressed == true)
            {
                input = "#";
            }

            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                shiftPressed = true;
            }
            else
            {
                shiftPressed = false;
            }

            if (!(input == "#" || (input[0] >= 'A' && input[0] <= 'F') || (input[0] >= 'a' && input[0] <= 'F') || (input[0] >= '0' && input[0] <= '9')))
                e.Handled = true;
            if (input.Length > 1)
                e.Handled = true;
        }

        private void txtAll_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(((TextBox)sender).Text))
                    return;
                CustomColor = MakeColorFromHex(sender);
                Reposition();
            }
            catch
            {
            }

        }


        private void SdA_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CustomColor.A != (byte)SdA.Value)
                CustomColor = Color.FromArgb((byte)SdA.Value, CustomColor.R, CustomColor.G, CustomColor.B);
        }

        private void SdR_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CustomColor.R != (byte)SdR.Value)
                CustomColor = Color.FromArgb(CustomColor.A, (byte)SdR.Value, CustomColor.G, CustomColor.B);
        }

        private void SdG_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CustomColor.G != (byte)SdG.Value)
                CustomColor = Color.FromArgb(CustomColor.A, CustomColor.R, (byte)SdG.Value, CustomColor.B);
        }

        private void SdB_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CustomColor.B != (byte)SdB.Value)
                CustomColor = Color.FromArgb(CustomColor.A, CustomColor.R, CustomColor.G, (byte)SdB.Value);
        }


        private void epDefaultcolor_Collapsed(object sender, RoutedEventArgs e)
        {
            epCustomcolor.IsExpanded = true;
        }

        private void epDefaultcolor_Expanded(object sender, RoutedEventArgs e)
        {
            epCustomcolor.IsExpanded = false;
        }

        private void epCustomcolor_Expanded(object sender, RoutedEventArgs e)
        {
            epDefaultcolor.IsExpanded = false;
        }

        private void epCustomcolor_Collapsed(object sender, RoutedEventArgs e)
        {
            epDefaultcolor.IsExpanded = true;
        }


        #endregion

    }





}
