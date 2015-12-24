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
    public class InnertelStatus2BakColor : IValueConverter
    {
        public object Convert(object value,
                        Type targetType,
                        object parameter,
                        System.Globalization.CultureInfo culture)
        {
            Brush _val = Brushes.White;
            int _value = int.Parse(value.ToString());

            switch (_value)
            {
                case 1:
                    _val = Brushes.SkyBlue;
                    break;
                default:
                    _val = Brushes.White;
                    break;
            }

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
