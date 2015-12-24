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
    public class Calltime
    {
        public int total { get; set; }
        public string txt0 { get; set; }
        public int calledsec { get; set; }
        public string txt1 { get; set; }
        public int avgcalledsec { get; set; }
    }

    public class CalltimePivot
    {
        public string timerange { get; set; }
        public int totalnum { get; set; }
        public int col30 { get; set; }
        public int col60 { get; set; }
        public int col180 { get; set; }
        public int col300 { get; set; }
        public int col600 { get; set; }
        public int col1800 { get; set; }
        public int col3600 { get; set; }
        public int colall { get; set; }
        public int colavg { get; set; }
    }

    public class Calltimes
    {
        private List<CalltimePivot> _list;

        public List<CalltimePivot> GetList
        {
            get { return _list; }
        }

        public Calltimes(string _chk, string _sdate, string _edate, string _teller)
        {
            DataTable dt_worktime = null;
            using (FirebirdDBHelper db = new FirebirdDBHelper(util.strFBDBConn2))
            {
                try
                {
                    dt_worktime = db.GetDataTableSP("GET_LIST_WORKTIME");
                }
                catch (FirebirdSql.Data.FirebirdClient.FbException fe)
                {
                    //throw fe;
                }
            }

            DataTable dt_cdrs = util.CreateDT2SP();
            dt_cdrs.Rows.Add("@I_SDATE", _sdate);
            dt_cdrs.Rows.Add("@I_EDATE", _edate);
            dt_cdrs.Rows.Add("@I_TELLER", _teller);

            using (FirebirdDBHelper db = new FirebirdDBHelper(util.strFBDBConn2))
            {
                try
                {
                    dt_cdrs = db.GetDataTableSP("GET_CDR_LIST_BY_DATE", dt_cdrs);
                }
                catch (FirebirdSql.Data.FirebirdClient.FbException fe)
                {
                    //throw fe;
                }
            }

            var _cdrs = dt_cdrs.AsEnumerable().ToList();

#if true // foreach 사용
            _list = new List<CalltimePivot>();
            foreach (var wtime in dt_worktime.AsEnumerable())
            {
                var _lcdrs = _cdrs.Where(p => ((DateTime)p[3]).Hour == int.Parse(wtime[0].ToString()));
                //((DateTime)x[3]).Hour == int.Parse(wtime[0].ToString())

                CalltimePivot _pivot = new CalltimePivot() {
                    timerange = wtime[1].ToString()
                        ,
                    totalnum = _lcdrs.Count(x => ((DateTime)x[4] - (DateTime)x[3]).TotalSeconds > 0)
                        ,
                    col30 = _lcdrs.Count(x => (((DateTime)x[4]) - ((DateTime)x[3])).TotalSeconds > 0 && (((DateTime)x[4]) - ((DateTime)x[3])).TotalSeconds <= 30)
                        ,
                    col60 = _lcdrs.Count(x => (((DateTime)x[4]) - ((DateTime)x[3])).TotalSeconds > 30 && (((DateTime)x[4]) - ((DateTime)x[3])).TotalSeconds <= 60)
                        ,
                    col180 = _lcdrs.Count(x => (((DateTime)x[4]) - ((DateTime)x[3])).TotalSeconds > 60 && (((DateTime)x[4]) - ((DateTime)x[3])).TotalSeconds <= 180)
                        ,
                    col300 = _lcdrs.Count(x => (((DateTime)x[4]) - ((DateTime)x[3])).TotalSeconds > 180 && (((DateTime)x[4]) - ((DateTime)x[3])).TotalSeconds <= 300)
                        ,
                    col600 = _lcdrs.Count(x => (((DateTime)x[4]) - ((DateTime)x[3])).TotalSeconds > 300 && (((DateTime)x[4]) - ((DateTime)x[3])).TotalSeconds <= 600)
                        ,
                    col1800 = _lcdrs.Count(x => (((DateTime)x[4]) - ((DateTime)x[3])).TotalSeconds > 600 && (((DateTime)x[4]) - ((DateTime)x[3])).TotalSeconds <= 1800)
                        ,
                    col3600 = _lcdrs.Count(x => (((DateTime)x[4]) - ((DateTime)x[3])).TotalSeconds > 1800 && (((DateTime)x[4]) - ((DateTime)x[3])).TotalSeconds <= 3600)
                        ,
                    colall = _lcdrs.Count(x => (((DateTime)x[4]) - ((DateTime)x[3])).TotalSeconds > 3600)
                        ,
                    colavg = _lcdrs.Count(x => ((DateTime)x[4] - (DateTime)x[3]).TotalSeconds > 0) == 0 ? 0 : (int)_lcdrs.Sum(x => ((DateTime)x[4] - (DateTime)x[3]).TotalSeconds) / _lcdrs.Count(x => ((DateTime)x[4] - (DateTime)x[3]).TotalSeconds > 0)
                };

                _list.Add(_pivot);
            }
#endif

#if false // lamda 사용
            _list = new List<CalltimePivot>(
                (from wtime in dt_worktime.AsEnumerable()
                    select new CalltimePivot()
                    {
                        timerange = wtime[1].ToString()
                        ,
                        totalnum = _cdrs.Count(x => ((DateTime)x[3]).Hour == int.Parse(wtime[0].ToString()) && ((DateTime)x[4] - (DateTime)x[3]).TotalSeconds > 0)
                        ,
                        col30 = _cdrs.Count(x => ((DateTime)x[3]).Hour == int.Parse(wtime[0].ToString()) && (((DateTime)x[4]) - ((DateTime)x[3])).TotalSeconds > 0 && (((DateTime)x[4]) - ((DateTime)x[3])).TotalSeconds <= 30)
                        ,
                        col60 = _cdrs.Count(x => ((DateTime)x[3]).Hour == int.Parse(wtime[0].ToString()) && (((DateTime)x[4]) - ((DateTime)x[3])).TotalSeconds > 30 && (((DateTime)x[4]) - ((DateTime)x[3])).TotalSeconds <= 60)
                        ,
                        col180 = _cdrs.Count(x => ((DateTime)x[3]).Hour == int.Parse(wtime[0].ToString()) && (((DateTime)x[4]) - ((DateTime)x[3])).TotalSeconds > 60 && (((DateTime)x[4]) - ((DateTime)x[3])).TotalSeconds <= 180)
                        ,
                        col300 = _cdrs.Count(x => ((DateTime)x[3]).Hour == int.Parse(wtime[0].ToString()) && (((DateTime)x[4]) - ((DateTime)x[3])).TotalSeconds > 180 && (((DateTime)x[4]) - ((DateTime)x[3])).TotalSeconds <= 300)
                        ,
                        col600 = _cdrs.Count(x => ((DateTime)x[3]).Hour == int.Parse(wtime[0].ToString()) && (((DateTime)x[4]) - ((DateTime)x[3])).TotalSeconds > 300 && (((DateTime)x[4]) - ((DateTime)x[3])).TotalSeconds <= 600)
                        ,
                        col1800 = _cdrs.Count(x => ((DateTime)x[3]).Hour == int.Parse(wtime[0].ToString()) && (((DateTime)x[4]) - ((DateTime)x[3])).TotalSeconds > 600 && (((DateTime)x[4]) - ((DateTime)x[3])).TotalSeconds <= 1800)
                        ,
                        col3600 = _cdrs.Count(x => ((DateTime)x[3]).Hour == int.Parse(wtime[0].ToString()) && (((DateTime)x[4]) - ((DateTime)x[3])).TotalSeconds > 1800 && (((DateTime)x[4]) - ((DateTime)x[3])).TotalSeconds <= 3600)
                        ,
                        colall = _cdrs.Count(x => ((DateTime)x[3]).Hour == int.Parse(wtime[0].ToString()) && (((DateTime)x[4]) - ((DateTime)x[3])).TotalSeconds > 3600)
                        ,
                        colavg = _cdrs.Count(x => ((DateTime)x[3]).Hour == int.Parse(wtime[0].ToString()) && ((DateTime)x[4] - (DateTime)x[3]).TotalSeconds > 0) == 0 ? 0 : (int)_cdrs.Where(x => ((DateTime)x[3]).Hour == int.Parse(wtime[0].ToString())).Sum(x => (((DateTime)x[4]) - ((DateTime)x[3])).TotalSeconds) / _cdrs.Count(x => ((DateTime)x[3]).Hour == int.Parse(wtime[0].ToString()) && ((DateTime)x[4] - (DateTime)x[3]).TotalSeconds > 0)
                    }).ToList<CalltimePivot>()
            );
#endif

/*
            List<CDR> _cdrs = new List<CDR>(
                (from cdrRow in dt_cdrs.AsEnumerable()
                 select new CDR() {
                     index = string.IsNullOrEmpty(cdrRow["IDX"].ToString()) ? 0 : int.Parse(cdrRow["IDX"].ToString())
                     ,
                     inner_fseq = string.IsNullOrEmpty(cdrRow["INNER_FSEQ"].ToString()) ? 0 : int.Parse(cdrRow["INNER_FSEQ"].ToString())
                     ,
                     office_name = cdrRow["OFFICE_NAME"].ToString()
                     ,
                     startdate = DateTime.Parse(cdrRow["STARTDATE"].ToString())
                     ,
                     enddate = DateTime.Parse(cdrRow["ENDDATE"].ToString())
                     ,
                     caller_type = string.IsNullOrEmpty(cdrRow["CALLER_TYPE"].ToString()) ? 0 : int.Parse(cdrRow["CALLER_TYPE"].ToString())
                     ,
                     caller = cdrRow["CALLER"].ToString()
                     ,
                     caller_ipn_number = cdrRow["CALLER_IPN_NUMBER"].ToString()
                     ,
                     caller_group_code = cdrRow["CALLER_GROUP_CODE"].ToString()
                     ,
                     caller_group_name = cdrRow["CALLER_GROUP_NAME"].ToString()
                     ,
                     caller_human_name = cdrRow["CALLER_HUMAN_NAME"].ToString()
                     ,
                     callee = cdrRow["CALLEE"].ToString()
                     ,
                     callee_type = string.IsNullOrEmpty(cdrRow["CALLEE_TYPE"].ToString()) ? 0 : int.Parse(cdrRow["CALLEE_TYPE"].ToString())
                     ,
                     callee_ipn_number = cdrRow["CALLEE_IPN_NUMBER"].ToString()
                     ,
                     callee_group_code = cdrRow["CALLEE_GROUP_CODE"].ToString()
                     ,
                     callee_group_name = cdrRow["CALLEE_GROUP_NAME"].ToString()
                     ,
                     callee_human_name = cdrRow["CALLEE_HUMAN_NAME"].ToString()
                     ,
                     result = string.IsNullOrEmpty(cdrRow["RESULT"].ToString()) ? -1 : int.Parse(cdrRow["RESULT"].ToString())
                     ,
                     seq = string.IsNullOrEmpty(cdrRow["SEQ"].ToString()) ? 0 : int.Parse(cdrRow["SEQ"].ToString())
                 }).ToList()
                );

            _list = new List<CalltimePivot>(
                    (from wtime in dt_worktime.AsEnumerable()
                     select new CalltimePivot()
                     {
                         timerange = wtime[1].ToString()
                         ,
                         totalnum = _cdrs.Count(x => x == int.Parse(wtime[0].ToString()) && (x.enddate - x.startdate).TotalSeconds > 0)
                         ,
                         col30 = _cdrs.Count(x => x.startdate.Hour == int.Parse(wtime[0].ToString()) && (x.enddate - x.startdate).TotalSeconds > 0 && (x.enddate - x.startdate).TotalSeconds <= 30)
                         ,
                         col60 = _cdrs.Count(x => x.startdate.Hour == int.Parse(wtime[0].ToString()) && (x.enddate - x.startdate).TotalSeconds > 30 && (x.enddate - x.startdate).TotalSeconds <= 60)
                         ,
                         col180 = _cdrs.Count(x => x.startdate.Hour == int.Parse(wtime[0].ToString()) && (x.enddate - x.startdate).TotalSeconds > 60 && (x.enddate - x.startdate).TotalSeconds <= 180)
                         ,
                         col300 = _cdrs.Count(x => x.startdate.Hour == int.Parse(wtime[0].ToString()) && (x.enddate - x.startdate).TotalSeconds > 180 && (x.enddate - x.startdate).TotalSeconds <= 300)
                         ,
                         col600 = _cdrs.Count(x => x.startdate.Hour == int.Parse(wtime[0].ToString()) && (x.enddate - x.startdate).TotalSeconds > 300 && (x.enddate - x.startdate).TotalSeconds <= 600)
                         ,
                         col1800 = _cdrs.Count(x => x.startdate.Hour == int.Parse(wtime[0].ToString()) && (x.enddate - x.startdate).TotalSeconds > 600 && (x.enddate - x.startdate).TotalSeconds <= 1800)
                         ,
                         col3600 = _cdrs.Count(x => x.startdate.Hour == int.Parse(wtime[0].ToString()) && (x.enddate - x.startdate).TotalSeconds > 1800 && (x.enddate - x.startdate).TotalSeconds <= 3600)
                         ,
                         colall = _cdrs.Count(x => x.startdate.Hour == int.Parse(wtime[0].ToString()) && (x.enddate - x.startdate).TotalSeconds > 3600)
                         ,
                         colavg = (int)_cdrs.Where(x => x.startdate.Hour == int.Parse(wtime[0].ToString())).Sum(x => (x.enddate - x.startdate).TotalSeconds)
                     }).ToList<CalltimePivot>()
                );
 */

        }

#if false
        public Calltimes(string _chk, string _sdate, string _edate, string _teller)
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
                    dt = db.GetDataTableSP("GET_LIST_BY_HOURS_BAK3", dt);
                }
                catch (FirebirdSql.Data.FirebirdClient.FbException fe)
                {
                    //throw fe;
                }
            }

            List<Calltime> _oblist = new List<Calltime>(
                    (from _row in dt.AsEnumerable()
                     select new Calltime()
                     {
                         total = int.Parse(_row["O_TOTAL"].ToString())
                         ,
                         txt0 = _row["O_TXT0"].ToString()
                         ,
                         calledsec = int.Parse(_row["O_CALLED"].ToString())
                         ,
                         txt1 = _row["O_TXT1"].ToString()
                         ,
                         avgcalledsec = int.Parse(_row["O_AVGCALLED"].ToString())
                     }).ToList<Calltime>()
                );

            int _counterm = 8;
            int _avgcount = _oblist.Count() / _counterm;

            _list = new ObservableCollection<CalltimePivot>();
            for (int i = 0; i < _oblist.Count(); i += _counterm)
            {
                int k = 0;
                CalltimePivot _callpivot = new CalltimePivot();
                for (int j = i; j < i+_counterm; j++)
                {
                    _callpivot.timerange = _oblist[j].txt1;
                    _callpivot.totalnum = _oblist[j].total;
                    _callpivot.colavg = _oblist[j].avgcalledsec;
                    switch (k)
                    {
                        case 0:
                            _callpivot.col30 = _oblist[j].calledsec;
                            break;
                        case 1:
                            _callpivot.col60 = _oblist[j].calledsec;
                            break;
                        case 2:
                            _callpivot.col180 = _oblist[j].calledsec;
                            break;
                        case 3:
                            _callpivot.col300 = _oblist[j].calledsec;
                            break;
                        case 4:
                            _callpivot.col600 = _oblist[j].calledsec;
                            break;
                        case 5:
                            _callpivot.col1800 = _oblist[j].calledsec;
                            break;
                        case 6:
                            _callpivot.col3600 = _oblist[j].calledsec;
                            break;
                        case 7:
                            _callpivot.colall = _oblist[j].calledsec;
                            break;
                    }
                    k++;
                }
                _list.Add(_callpivot);
            }
        }
#endif
    }
}
