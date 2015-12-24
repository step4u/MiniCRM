using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Media;
using Com.Huen.Libs;

namespace Com.Huen.Converter
{
    public class DSec2HMS : IValueConverter
    {
        public object Convert(object value,
                        Type targetType,
                        object parameter,
                        System.Globalization.CultureInfo culture)
        {
            int _sec = (int)(string.IsNullOrEmpty(value.ToString()) == true ? 0 : double.Parse(value.ToString()));
            TimeSpan _ts = new TimeSpan(0, 0, _sec);
            string _val = string.Format("{0:00}:{1:00}:{2:00}", _ts.Hours, _ts.Minutes, _ts.Seconds);

            return _val;
        }

        public object ConvertBack(object value,
                                    Type targetType,
                                    object parameter,
                                    System.Globalization.CultureInfo culture)
        {
            return new NotSupportedException("ConvertBack is not supported");
        }
    }
}
