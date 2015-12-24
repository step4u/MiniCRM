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
    public class ConvDateDiff : IMultiValueConverter
    {
        public object Convert(object[] value,
                        Type targetType,
                        object parameter,
                        System.Globalization.CultureInfo culture)
        {
            string strout = string.Empty;

            DateTime sdate = DateTime.Parse(value[0].ToString());
            DateTime edate = DateTime.Parse(value[1].ToString());

            TimeSpan ts = edate - sdate;

            if (ts.TotalSeconds < 0)
            {
                strout = "NA";
            }
            else
            {
                strout = string.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);
            }

            return strout;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
