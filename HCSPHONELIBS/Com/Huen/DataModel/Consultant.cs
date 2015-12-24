using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Collections.ObjectModel;

using Com.Huen.Sql;
using Com.Huen.Libs;

namespace Com.Huen.DataModel
{
    public class Consultant
    {
        public string telnum { get; set; }
        public string tellername { get; set; }
        public int calledsec { get; set; }
        public int callednum { get; set; }
        public int succeed { get; set; }
        public int absence { get; set; }
        public int failed { get; set; }
        public int avgcalledsec { get; set; }
    }

    public class Consultants
    {
        private ObservableCollection<Consultant> _list;

        public ObservableCollection<Consultant> GetList
        {
            get { return _list; }
        }

        public Consultants(string _chk, string _sdate, string _edate, string _teller)
        {
            DataTable dt = util.CreateDT2SP();

            dt.Rows.Add("@I_CHK", _chk);
            dt.Rows.Add("@I_SDATE", _sdate);
            dt.Rows.Add("@I_EDATE", _edate);
            dt.Rows.Add("@I_TELLER", _teller);

            using (FirebirdDBHelper db = new FirebirdDBHelper(util.strFBDBConn2))
            {
                try
                {
                    dt = db.GetDataTableSP("GET_LIST_BY_TELNUM", dt);
                }
                catch (FirebirdSql.Data.FirebirdClient.FbException fe)
                {
                    //throw fe;
                }
            }

            _list = new ObservableCollection<Consultant>(
                    (from _row in dt.AsEnumerable()
                     select new Consultant()
                     {
                         telnum = _row["O_TELNUM"].ToString()
                         ,
                         tellername = _row["O_TELLERNAME"].ToString()
                         ,
                         calledsec = int.Parse(_row["O_CALLEDSEC"].ToString())
                         ,
                         callednum = int.Parse(_row["O_CALLEDNUM"].ToString())
                         ,
                         succeed = int.Parse(_row["O_SUCCEED"].ToString())
                         ,
                         absence = int.Parse(_row["O_ABSENCE"].ToString())
                         ,
                         failed = int.Parse(_row["O_FAILED"].ToString())
                         ,
                         avgcalledsec = int.Parse(_row["O_AVGCALLEDSEC"].ToString())
                     }).ToList<Consultant>()
                );
        }
    }
}
