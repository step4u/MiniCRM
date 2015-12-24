using System;
using System.Windows.Data;

namespace Com.Huen.Converter
{
    public class Sec2HMS : IValueConverter
    {
        public object Convert(object value,
                        Type targetType,
                        object parameter,
                        System.Globalization.CultureInfo culture)
        {
            int _sec = string.IsNullOrEmpty(value.ToString()) == true ? 0 : int.Parse(value.ToString());
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
