using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Data;
using System.Windows;

using Com.Huen.Sql;
using Com.Huen.Libs;

namespace Com.Huen.DataModel
{
    public class StatisticRealStatus
    {
        private string _label = string.Empty;
        public string Label
        {
            get
            {
                string _val = string.Empty;
                switch (_label)
                {
                    case "1":
                        _val = "통화성공";
                        break;
                    case "2":
                        _val = "부재중";
                        break;
                    case "3":
                        _val = "연결실패";
                        break;
                    default:
                        _val = "전체통화";
                        break;
                }
                return _val;
            }
            set
            {
                _label = value;
            }
        }
        public int Value1 { get; set; }
    }

    public class RealStatusList
    {
        public List<StatisticRealStatus> Items { get; set; }

        public RealStatusList(int _chk, string _sdate, string _edate)
        {
            DataTable dt = util.CreateDT2SP();
            dt.Rows.Add("@i_chk", _chk);
            dt.Rows.Add("@i_sdate", _sdate);
            dt.Rows.Add("@i_edate", _edate);

            try
            {
                using (FirebirdDBHelper db = new FirebirdDBHelper(util.strFBDBConn2))
                {
                    try
                    {
                        dt = db.GetDataTableSP("GET_REALTIME_STATUS", dt);
                    }
                    catch (FirebirdSql.Data.FirebirdClient.FbException fe1)
                    {
                        //throw fe;
                    }
                }

                Items = new List<StatisticRealStatus>(
                        (from _row in dt.AsEnumerable()
                         select new StatisticRealStatus()
                         {
                             Label = _row[0].ToString()
                             ,
                             Value1 = int.Parse(_row[1].ToString())
                         }
                        )
                    );
            }
            catch (FirebirdSql.Data.FirebirdClient.FbException fe0)
            {
            }
        }
    }
}
