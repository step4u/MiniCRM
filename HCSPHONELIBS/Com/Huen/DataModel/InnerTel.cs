using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections.ObjectModel;
using System.Data;
using System.ComponentModel;

using System.Windows;

using Com.Huen.Libs;
using Com.Huen.Sql;

namespace Com.Huen.DataModel
{
    [Serializable()]
    public class InnerTel : INotifyPropertyChanged
    {
        private string _tellername = string.Empty;
        private string _peernum = string.Empty;
        private DateTime _startedtime = DateTime.Now;
        private int _elapsedsecond = 0;
        private int _status = 0;

        public int Seq { get; set; }
        public string Telnum { get; set; }
        public string TellerName
        {
            get
            {
                return _tellername;
            }
            set
            {
                _tellername = value;
                this.OnPropertyChanged("TellerName");
            }
        }
        public string PeerNum
        {
            get
            {
                return _peernum;
            }
            set
            {
                _peernum = value;
                this.OnPropertyChanged("PeerNum");
            }
        }

        public DateTime StartedDatetime
        {
            get
            {
                return _startedtime;
            }
            set
            {
                _startedtime = value;
            }
        }

        public int ElapsedSecond
        {
            get
            {
                return _elapsedsecond;
            }
            set
            {
                _elapsedsecond = value;
                this.OnPropertyChanged("ElapsedSecond");
            }
        }

        public int Status
        {
            get
            {
                return _status;
            }
            set
            {
                _status = value;
                this.OnPropertyChanged("Status");
            }
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class InnerTels
    {
        private ObservableCollection<InnerTel> _list;
        public ObservableCollection<InnerTel> GetList
        {
            get { return _list; }
        }

        public InnerTels()
        {
            DataTable dt = null;

            try
            {
                using (FirebirdDBHelper db = new FirebirdDBHelper(util.strFBDBConn2))
                {
                    try
                    {
                        dt = db.GetDataTableSP("GET_INNERTELS");
                    }
                    catch (FirebirdSql.Data.FirebirdClient.FbException fe1)
                    {
                        //throw fe;
                        MessageBox.Show("test");
                    }
                }

                _list = new ObservableCollection<InnerTel>(
                        (from __row in dt.AsEnumerable()
                         select new InnerTel()
                         {
                             Seq = int.Parse(__row[0].ToString())
                             ,
                             Telnum = "   " + __row[1].ToString()
                             ,
                             TellerName = __row[2].ToString()
                         }
                        ).ToList<InnerTel>()
                    );

                //InnerTel _tmptel = new InnerTel() { Seq = 0, Telnum = "전체" };
                //_list.Insert(0, _tmptel);
            }
            catch (FirebirdSql.Data.FirebirdClient.FbException fe0)
            {
                _list = new ObservableCollection<InnerTel>();
                MessageBox.Show("Database 접속에 문제가 발생하였습니다.\r\n \"도구 → 환경설정 → 서버주소\"을 확인 후 다시 실행해 주세요.");
            }

        }

        public void Add(InnerTel _itel)
        {
            DataTable dt = util.CreateDT2SP();
            dt.Rows.Add("@telnum", _itel.Telnum);
            dt.Rows.Add("@tellername", _itel.TellerName);

            using (FirebirdDBHelper db = new FirebirdDBHelper(util.strFBDBConn2))
            {
                try
                {
                    db.BeginTran();
                    db.ExcuteSP("INS_INNERTELS", dt);
                    db.Commit();

                    _list.Add(_itel);
                }
                catch (FirebirdSql.Data.FirebirdClient.FbException fe)
                {
                    db.Rollback();
                }
            }
        }

        public void Modify(InnerTel _itel)
        {
            DataTable dt = util.CreateDT2SP();
            dt.Rows.Add("@seq", _itel.Seq);
            dt.Rows.Add("@telnum", _itel.Telnum);
            dt.Rows.Add("@tellername", _itel.TellerName);

            using (FirebirdDBHelper db = new FirebirdDBHelper(util.strFBDBConn2))
            {
                try
                {
                    db.BeginTran();
                    db.ExcuteSP("UDT_INNERTELS", dt);
                    db.Commit();

                    InnerTel __obj = _list.FirstOrDefault(x => x.Telnum == _itel.Telnum);
                    __obj.TellerName = _itel.TellerName;
                }
                catch (FirebirdSql.Data.FirebirdClient.FbException fe)
                {
                    db.Rollback();
                }
            }
        }

        public void Remove(InnerTel _itel)
        {
            DataTable dt = util.CreateDT2SP();
            dt.Rows.Add("@seq", _itel.Seq);

            using (FirebirdDBHelper db = new FirebirdDBHelper(util.strFBDBConn2))
            {
                try
                {
                    db.BeginTran();
                    db.ExcuteSP("RMV_INNERTELS", dt);
                    db.Commit();

                    InnerTel __obj = _list.FirstOrDefault(x => x.Seq == _itel.Seq);
                    _list.Remove(__obj);
                }
                catch (FirebirdSql.Data.FirebirdClient.FbException fe)
                {
                    db.Rollback();
                }
            }
        }
    }
}
