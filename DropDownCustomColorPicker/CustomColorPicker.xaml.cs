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
using System.Windows.Controls.Primitives;

namespace DropDownCustomColorPicker
{
    /// <summary>
    /// Interaction logic for CustomColorPicker.xaml
    /// </summary>
    public partial class CustomColorPicker : UserControl
    {

        #region TBEvents

        /// <summary>
        /// 自定义颜色改变事件
        /// </summary>
        public static readonly RoutedEvent SelectedColorChangedEvent =
            EventManager.RegisterRoutedEvent("SelectedColorChanged", RoutingStrategy.Direct, typeof(RoutedPropertyChangedEventHandler<Color>), typeof(CustomColorPicker));

        /// <summary>
        /// 自定义颜色改变事件 CLR包装器
        /// </summary>
        public event RoutedPropertyChangedEventHandler<Color> SelectedColorChanged
        {
            add { this.AddHandler(SelectedColorChangedEvent, value); }
            remove { this.RemoveHandler(SelectedColorChangedEvent, value); }
        }


        /// <summary>
        /// 自定义DropDown状态改变事件
        /// </summary>
        public static readonly RoutedEvent DropDownStateChangedEvent =
            EventManager.RegisterRoutedEvent("DropDownStateChanged", RoutingStrategy.Direct, typeof(RoutedPropertyChangedEventHandler<bool>), typeof(CustomColorPicker));

        /// <summary>
        /// 自定义DropDown状态改变事件 CLR包装器
        /// </summary>
        public event RoutedPropertyChangedEventHandler<bool> DropDownStateChanged
        {
            add { this.AddHandler(DropDownStateChangedEvent, value); }
            remove { this.RemoveHandler(DropDownStateChangedEvent, value); }
        }

        #endregion


        #region DPProperties

        public Size ColorRectSize
        {
            get { return (Size)GetValue(ColorRectSizeProperty); }
            set { SetValue(ColorRectSizeProperty, value); }
        }

        public static readonly DependencyProperty ColorRectSizeProperty =
            DependencyProperty.Register("ColorRectSize", typeof(Size), typeof(CustomColorPicker), new PropertyMetadata(new Size(12, 12)));


        public PlacementMode Mode
        {
            get { return (PlacementMode)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }

        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register("Mode", typeof(PlacementMode), typeof(CustomColorPicker), new PropertyMetadata(PlacementMode.Bottom));


        #endregion


        public Color SelectedColor
        {
            get
            {
                return cp.CustomColor;
            }
            set
            {
                cp.CustomColor = value;
                recContent.Fill = new SolidColorBrush(value);
            }
        }


        public CustomColorPicker()
        {
            InitializeComponent();

            b.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(b_PreviewMouseLeftButtonUp);
        }


        /// <summary>
        /// 打开Picker
        /// </summary>
        public void OpenPicker()
        {
            // 为快捷菜单设置焦点
            this.b.Focus();

            b_PreviewMouseLeftButtonUp(null, null);
        }

        /// <summary>
        /// 关闭 Picker
        /// </summary>
        public void ClosePicker()
        {
            if (b.ContextMenu != null)
            {
                b.ContextMenu.IsOpen = false;
            }

            // 转移焦点
            this.Focus();
            this.b.Focus();
        }


        #region Events

        internal void RaiseSelectedColorChangedEvent()
        {
            RoutedPropertyChangedEventArgs<Color> args = new RoutedPropertyChangedEventArgs<Color>(Colors.Transparent, cp.CustomColor, SelectedColorChangedEvent)
            {
                Source = this
            };

            recContent.Fill = new SolidColorBrush(cp.CustomColor);
            this.RaiseEvent(args);
        }


        protected void RaiseDropDownStateChangedEvent(bool opened)
        {
            RoutedPropertyChangedEventArgs<bool> args = new RoutedPropertyChangedEventArgs<bool>(false, opened, DropDownStateChangedEvent)
            {
                Source = this
            };

            this.RaiseEvent(args);
        }

        private void b_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (b.ContextMenu != null)
            {
                this.cp.CustomColorPicker = this;
                b.ContextMenu.PlacementTarget = b;
                b.ContextMenu.Placement = this.Mode;
                ContextMenuService.SetPlacement(b, this.Mode);
                b.ContextMenu.IsOpen = true;
            }
        }

        private void ContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            RaiseDropDownStateChangedEvent(false);
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            RaiseDropDownStateChangedEvent(true);
        }

        #endregion

    }
}
