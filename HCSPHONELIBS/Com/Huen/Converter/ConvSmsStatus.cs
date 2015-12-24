using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Media;

using Com.Huen.Libs;
using Com.Huen.Sockets;


namespace Com.Huen.Converter
{
    public class ConvSmsStatus : IValueConverter
    {
        public object Convert(object value,
                Type targetType,
                object parameter,
                System.Globalization.CultureInfo culture)
        {
            string ostr = string.Empty;
            int val = string.IsNullOrEmpty(value.ToString()) == false ? int.Parse(value.ToString()) : -1;

            switch(val)
            {
                case USRSTRUCTS.STATUS_SMS_SUCCESS:
                    ostr = "성공";
                    break;
                case USRSTRUCTS.STATUS_SMS_DENY:
                    ostr = "수신거부";
                    break;
                case USRSTRUCTS.STATUS_CALL_SYSTEM_ERROR:
                    ostr = "시스템오류";
                    break;
                case USRSTRUCTS.STATUS_SMS_POWER_OFF:
                    ostr = "전화전원꺼짐";
                    break;
                case USRSTRUCTS.STATUS_SMS_NOT_SUPPORT:
                    ostr = "수신불가장치";
                    break;
                default:
                    ostr = "기타오류";
                    break;
            }

            return ostr;
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
