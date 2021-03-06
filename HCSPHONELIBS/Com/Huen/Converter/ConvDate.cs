﻿using System;
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
    public class ConvDate : IValueConverter
    {
        public object Convert(object value,
                        Type targetType,
                        object parameter,
                        System.Globalization.CultureInfo culture)
        {
            string strout = string.Empty;

            DateTime date = DateTime.Parse(value.ToString());

            strout = string.Format("{0}-{1:00}-{2:00}", date.Year, date.Month, date.Day);

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
