using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections.ObjectModel;
using System.Data;
using System.ComponentModel;

using Com.Huen.Sql;
using Com.Huen.Libs;

namespace Com.Huen.DataModel
{
    public class Interview : INotifyPropertyChanged
    {
        private bool _chk = false;

        public bool CHK
        {
            get
            {
                return _chk;
            }
            set
            {
                _chk = value;
                this.OnPropertyChanged("CHK");
            }
        }
        public long seq { get; set; }
        public int fseq { get; set; }
        public string forward { get; set; }
        public string extention { get; set; }
        public string peernum { get; set; }
        public string recfile { get; set; }
        
        public double fnlen { get; set; }
        public DateTime regdate { get; set; }
        public string regyymmdd { get; set; }
        public string reghhmmss { get; set; }

        public string tellername { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Interviewes
    {
        private ObservableCollection<Interview> _list;

        public ObservableCollection<Interview> GetList
        {
            get { return _list; }
        }

        public Interviewes(int _id, string _sdate, string _edate, string _txt2, string _txt3)
        {
            DataTable dt = util.CreateDT2SP();

            dt.Rows.Add("@iseq", _id);
            dt.Rows.Add("@isdate", _sdate);
            dt.Rows.Add("@iedate", _edate);
            dt.Rows.Add("@itxt2", _txt2);
            dt.Rows.Add("@itxt3", _txt3);

            using ( FirebirdDBHelper db = new FirebirdDBHelper(util.strFBDBConn2))
            {
                try
                {
                    dt = db.GetDataTableSP("GET_INTERVIEWS", dt);
                }
                catch (FirebirdSql.Data.FirebirdClient.FbException fe)
                {
                    //throw fe;
                }
            }

            _list = new ObservableCollection<Interview>(
                    (from __row in dt.AsEnumerable()
                     select new Interview()
                     {
                         seq = long.Parse(__row[0].ToString())
                         ,
                         fseq = int.Parse(string.IsNullOrEmpty(__row[1].ToString()) ? "0" : __row[1].ToString())
                         ,
                         forward = __row[2].ToString()
                         ,
                         extention = __row[3].ToString()
                         ,
                         peernum = __row[4].ToString()
                         ,
                         recfile = __row[5].ToString()
                         ,
                         regdate = DateTime.Parse(__row[6].ToString())
                         ,
                         regyymmdd = __row[7].ToString()
                         ,
                         reghhmmss = __row[8].ToString()
                         ,
                         fnlen = double.Parse(__row[9].ToString())
                         ,
                         tellername = __row[10].ToString()
                     }).ToList<Interview>()
                );
        }
    }
}
