using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Media;

namespace DropDownCustomColorPicker
{
    /// <summary>
    /// 反射预定义颜色
    /// </summary>
    public class CustomColors
    {
        List<Color> selectableColors = null;

        public List<Color> SelectableColors
        {
            get { return selectableColors; }
            set { selectableColors = value; }
        }

        public CustomColors()
        {
            var list = new List<Color>();

            Type ColorsType = typeof(Colors);
            PropertyInfo[] ColorsProperty = ColorsType.GetProperties();

            foreach (PropertyInfo property in ColorsProperty)
            {
                list.Add((Color)ColorConverter.ConvertFromString(property.Name));
            }

            list.Sort(new Comparison<Color>((Color x, Color y) =>
            {
                var xtotal = x.R + x.G + x.B;

                var ytotal = y.R + y.G + y.B;

                return xtotal.CompareTo(ytotal); // 升序排列
            }));

            selectableColors = list;
        }

    }
}
