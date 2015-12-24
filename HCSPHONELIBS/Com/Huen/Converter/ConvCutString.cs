using Com.Huen.Libs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Com.Huen.Converter
{
    public class ConvCutString : IValueConverter
    {
        public object Convert(object value,
                        Type targetType,
                        object parameter,
                        System.Globalization.CultureInfo culture)
        {
            string strout = string.Empty;

            if (value == null)
                return strout;

            string str = util.encStr(value.ToString());

            if (!string.IsNullOrEmpty(str))
            {
                strout = util.cutStr(str, 15);
            }
            return strout;
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
