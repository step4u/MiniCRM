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
    public class Account : INotifyPropertyChanged
    {
        private string _id = string.Empty;
        private string _pwd = string.Empty;
        private string _name = string.Empty;
        private string _memo = string.Empty;

        public string ID { get; set; }
        public string PWD { get; set; }
        public string PWD_NEW { get; set; }
        public string NAME
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                this.OnPropertyChanged("NAME");
            }
        }
        public string MEMO
        {
            get
            {
                return _memo;
            }
            set
            {
                _memo = value;
                this.OnPropertyChanged("MEMO");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Accounts
    {
        private ObservableCollection<Account> _list;
        public ObservableCollection<Account> GetList
        {
            get { return _list; }
        }

        public Accounts()
        {
            DataTable dt = util.CreateDT2SP();
            dt.Rows.Add("@I_ID", util.userid);

            try
            {
                using (FirebirdDBHelper db = new FirebirdDBHelper(util.strFBDBConn2))
                {
                    try
                    {
                        DataTable _dt = db.GetDataTableSP("GET_USRS", dt);

                        _list = new ObservableCollection<Account>(
                                (from _row in _dt.AsEnumerable()
                                 select new Account()
                                 {
                                     ID = _row[0].ToString()
                                     ,
                                     NAME = _row[1].ToString()
                                 }
                                ).ToList<Account>()
                            );
                    }
                    catch (FirebirdSql.Data.FirebirdClient.FbException fe1)
                    {
                        //throw fe;
                    }
                }
            }
            catch (FirebirdSql.Data.FirebirdClient.FbException fe0)
            {
                _list = new ObservableCollection<Account>();
                MessageBox.Show("Database 접속에 문제가 발생하였습니다.\r\n \"도구 → 환경설정 → 서버주소\"을 확인 후 다시 실행해 주세요.");
            }

        }

        public void Add(Account _item)
        {
            DataTable dt = util.CreateDT2SP();
            dt.Rows.Add("@I_ID", _item.ID);
            dt.Rows.Add("@I_PWD", util.GetSHA1(_item.PWD));
            dt.Rows.Add("@I_NAME", _item.NAME);
            dt.Rows.Add("@I_MEMO", _item.MEMO);

            using (FirebirdDBHelper db = new FirebirdDBHelper(util.strFBDBConn2))
            {
                try
                {
                    db.BeginTran();
                    db.ExcuteSP("INS_USR", dt);
                    db.Commit();

                    _list.Add(_item);
                }
                catch (FirebirdSql.Data.FirebirdClient.FbException fe)
                {
                    db.Rollback();
                }
            }
        }

        public int Modify(Account _item)
        {
            int result = -1;

            DataTable dt = util.CreateDT2SP();
            dt.Rows.Add("@I_ID_MASTER", util.userid);
            dt.Rows.Add("@I_ID", _item.ID);
            dt.Rows.Add("@I_PWD", util.GetSHA1(_item.PWD));
            dt.Rows.Add("@I_PWD_NEW", util.GetSHA1(_item.PWD_NEW));
            dt.Rows.Add("@I_NAME", _item.NAME);
            dt.Rows.Add("@I_MEMO", _item.MEMO);

            using (FirebirdDBHelper db = new FirebirdDBHelper(util.strFBDBConn2))
            {
                try
                {
                    db.BeginTran();
                    result = int.Parse(db.GetDataSP("UDT_USR", dt).ToString());
                    db.Commit();

                    Account __obj = _list.FirstOrDefault(x => x.ID == _item.ID);
                    __obj.NAME = _item.NAME;
                    __obj.MEMO = _item.MEMO;
                }
                catch (FirebirdSql.Data.FirebirdClient.FbException fe)
                {
                    db.Rollback();
                }
            }

            return result;
        }

        public void Remove(Account _item)
        {
            DataTable dt = util.CreateDT2SP();
            dt.Rows.Add("@I_ID", _item.ID);

            using (FirebirdDBHelper db = new FirebirdDBHelper(util.strFBDBConn2))
            {
                try
                {
                    db.BeginTran();
                    db.ExcuteSP("RMV_USR", dt);
                    db.Commit();

                    Account __obj = _list.FirstOrDefault(x => x.ID == _item.ID);
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
