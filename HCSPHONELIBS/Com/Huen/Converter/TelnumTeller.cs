using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Media;
using Com.Huen.Libs;
using Com.Huen.DataModel;


namespace Com.Huen.Converter
{
    public class TelnumTeller : IMultiValueConverter
    {
        public object Convert(object[] value,
                        Type targetType,
                        object parameter,
                        System.Globalization.CultureInfo culture)
        {
            string rtn = string.Empty;

            rtn = string.Format("{0} ({1})", value[0].ToString(), value[1].ToString().Trim());

            return rtn;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
