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
    public class MarkTel : IValueConverter
    {
        public object Convert(object value,
                        Type targetType,
                        object parameter,
                        System.Globalization.CultureInfo culture)
        {
            string tel = value.ToString();

            if (!string.IsNullOrEmpty(tel))
            {
                string[] tels = tel.Split('_');
                if (tels.Length == 1)
                {
                    tel = GetMarkedTel(tel);
                }
                else if (tels.Length == 3)
                {
                    string[] tmptel = tels[2].Split('.');
                    if (tmptel.Length > 1)
                    {
                        tel = string.Format("{0}_{1}_{2}.{3}", tels[0], tels[1], GetMarkedTel(tmptel[0]), tmptel[1]);
                    }
                }
            }

            return tel;
        }

        public object ConvertBack(object value,
                                    Type targetType,
                                    object parameter,
                                    System.Globalization.CultureInfo culture)
        {
            return new NotSupportedException("ConvertBack is not supported");
        }

        private string GetMarkedTel(string _tel)
        {
            if (_tel.Length < 9) return _tel;

            string tel = string.Empty;

            if (_tel.Substring(0, 2) == "02")
            {
                if (_tel.Length == 9)
                {
                    tel = string.Format("{0}{1}{2}", _tel.Substring(0, 2), "***", _tel.Substring(5, 4));
                }
                else if (_tel.Length == 10)
                {
                    tel = string.Format("{0}{1}{2}", _tel.Substring(0, 2), "****", _tel.Substring(6, 4));
                }
                else
                {
                    tel = string.Format("{0}{1}{2}", _tel.Substring(0, _tel.Length - 8), "****", _tel.Substring(_tel.Length - 5, 4));
                }
            }
            else
            {
                if (_tel.Length == 10)
                {
                    tel = string.Format("{0}{1}{2}", _tel.Substring(0, 3), "***", _tel.Substring(6, 4));
                }
                else if (_tel.Length == 11)
                {
                    tel = string.Format("{0}{1}{2}", _tel.Substring(0, 3), "****", _tel.Substring(7, 4));
                }
                else
                {
                    tel = string.Format("{0}{1}{2}", _tel.Substring(0, _tel.Length - 8), "****", _tel.Substring(_tel.Length - 5, 4));
                }
            }

            return tel;
        }
    }
}
