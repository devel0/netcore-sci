using System;
using System.Drawing;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Brush = Avalonia.Media.Brush;
using Color = Avalonia.Media.Color;

namespace SearchAThing
{

    public class SmartConverter : IValueConverter
    {

        static readonly Type typeofBoolean = typeof(Boolean);
        static readonly Type typeofThickness = typeof(Thickness);
        static readonly Type typeofBrush = typeof(Brush);
        //static readonly Type typeofVisibility = typeof(Visibility);
        static readonly Type typeofDouble = typeof(double);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = (string)(value.ToString());
            var pstr = (string)(parameter.ToString());
            var ss = pstr.Split(' ');

            if (targetType == typeofBoolean) str = str.ToLower();            

            var matches = str == ss[0];
            if (!matches && ss.Length <= 2) return null;

            object res = null;

            var i = matches ? 1 : 2;

            if (targetType == typeofBoolean)
            {
                res = bool.Parse(ss[i]);
            }
            else if (targetType == typeofThickness)
            {
                res = new Thickness(int.Parse(ss[i]));
            }
            // else if (targetType == typeofBrush)
            // {
            //     var cc = new ColorConverter();
            //     res = new SolidColorBrush((Color)cc.ConvertFromString(ss[i]));
            // }
            else if (targetType == typeofDouble)
            {
                res = double.Parse(ss[i]);
            }
            else
                res = System.Convert.ChangeType(ss[i], targetType);

            return res;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}