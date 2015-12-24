using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections.ObjectModel;
using System.Data;
using System.ComponentModel;
using System.Globalization;

using Com.Huen.Sql;
using Com.Huen.Libs;

namespace Com.Huen.DataModel
{
    public class CDRs
    {
        private DateTime _startdate;
        private DateTime _enddate;
        public string startdate
        {
            get
            {
                CultureInfo ci = new CultureInfo("ko-KR");
                return _startdate.ToString("yyyy-MM-dd hh:mm:ss", ci);
            }
            set
            {
                _startdate = DateTime.Parse(value);
            }
        }
        public string enddate
        {
            get
            {
                CultureInfo ci = new CultureInfo("ko-KR");
                return _enddate.ToString("yyyy-MM-dd hh:mm:ss", ci);
            }
            set
            {
                _enddate = DateTime.Parse(value);
            }
        }
        public string callernum { get; set; }
        public string callername { get; set; }
        public string calleenum { get; set; }
        public string calleename { get; set; }
    }

    public class CDRLists
    {
        private ObservableCollection<CDRs> _list;

        public ObservableCollection<CDRs> GetList
        {
            get { return _list; }
        }

        public CDRLists(string _sdate, string _edate, string _gubun, string _teller)
        {
            DataTable dt = util.CreateDT2SP();

            dt.Rows.Add("@i_sdate", _sdate);
            dt.Rows.Add("@i_edate", _edate);
            dt.Rows.Add("@i_gubun", _gubun);
            dt.Rows.Add("@i_teller", _teller);

            using ( FirebirdDBHelper db = new FirebirdDBHelper(util.strFBDBConn2))
            {
                try
                {
                    dt = db.GetDataTableSP("GET_CDR_LIST_BY_DATE2", dt);
                }
                catch (FirebirdSql.Data.FirebirdClient.FbException fe)
                {
                    //throw fe;
                }
            }

            _list = new ObservableCollection<CDRs>(
                    (from _row in dt.AsEnumerable()
                     select new CDRs()
                     {
                         startdate = _row[2].ToString()
                         ,
                         enddate = _row[3].ToString()
                         ,
                         callernum = _row[5].ToString()
                         ,
                         callername = _row[9].ToString()
                         ,
                         calleenum = _row[10].ToString()
                     }).ToList<CDRs>()
                );
        }
    }
}
