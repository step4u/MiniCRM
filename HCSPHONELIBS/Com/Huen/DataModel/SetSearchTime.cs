using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com.Huen.DataModel
{
    public class SetSearchTime
    {
        private DateTime _sdate;
        private DateTime _edate;

        public SetSearchTime()
        {
            DateTime now = DateTime.Now;
            SDATE = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
            EDATE = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59);
        }
        public DateTime SDATE
        {
            get
            {
                return _sdate;
            }
            set
            {
                _sdate = value;
            }
        }

        public DateTime EDATE
        {
            get
            {
                return _edate;
            }
            set
            {
                _edate = value;
            }
        }
    }
}
