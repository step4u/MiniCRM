using Com.Huen.Libs;
using Com.Huen.Sql;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Linq;
using FirebirdSql.Data.FirebirdClient;
using System.Runtime.InteropServices;

namespace Com.Huen.DataModel
{
    public class CallList : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _IsChecked = false;
        private bool _IsSelected = false;
        public int Idx { get; set; }
        public int Cust_Idx { get; set; }
        public string Name { get; set; }
        public int Direction { get; set; }
        public string Cust_Tel { get; set; }
        public DateTime Startdate { get; set; }
        private DateTime _Enddate;
        private string _Memo;

        public bool IsChecked
        {
            get
            {
                return _IsChecked;
            }
            set
            {
                _IsChecked = value;
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null)
                    handler(this, new PropertyChangedEventArgs("IsChecked"));
            }
        }

        public bool IsSelected
        {
            get
            {
                return _IsSelected;
            }
            set
            {
                _IsSelected = value;
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null)
                    handler(this, new PropertyChangedEventArgs("IsSelected"));
            }
        }

        public DateTime Enddate
        {
            get
            {
                return _Enddate;
            }
            set
            {
                _Enddate = value;
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null)
                    handler(this, new PropertyChangedEventArgs("Enddate"));
            }
        }

        public string Memo
        {
            get
            {
                return _Memo;
            }
            set
            {
                _Memo = value;
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null)
                    handler(this, new PropertyChangedEventArgs("Memo"));
            }
        }

        public void savememo()
        {
            using (FirebirdDBHelper db = new FirebirdDBHelper(util.GetFbDbStrConn()))
            {
                try
                {
                    db.SetParameters("@I_IDX", FbDbType.Integer, this.Idx);
                    db.SetParameters("@I_MEMO", FbDbType.VarChar, util.encStr(this.Memo));

                    db.BeginTran();
                    db.ExcuteSP("MODI_CALL_LIST_MEMO");
                    db.Commit();
                }
                catch (FbException e)
                {
                    util.WriteLog(e.ErrorCode, e.Message);
                    db.Rollback();
                    throw e;
                }
            }
        }
    }

    public class CallLists : ObservableCollection<CallList>
    {
        public CallLists() { }

        public CallLists(SearchCondition1 val)
        {
            DataTable dt;

            using (FirebirdDBHelper db = new FirebirdDBHelper(util.GetFbDbStrConn()))
            {
                try
                {
                    db.SetParameters("@I_SDATE", FbDbType.TimeStamp, val.StartDate);
                    db.SetParameters("@I_EDATE", FbDbType.TimeStamp, val.EndDate);
                    db.SetParameters("@I_NUMBER", FbDbType.VarChar, val.Number);

                    dt = db.GetDataTableSP("GET_CALL_LIST");

                    foreach (DataRow row in dt.Rows)
                    {
                        this.Add(new CallList() {
                            IsChecked = false,
                            IsSelected = false,
                            Idx = string.IsNullOrEmpty(row[0].ToString()) == false ? int.Parse(row[0].ToString()) : -1,
                            Cust_Idx = string.IsNullOrEmpty(row[1].ToString()) == false ? int.Parse(row[1].ToString()) : -1,
                            Name = row[2].ToString(),
                            Direction = string.IsNullOrEmpty(row[3].ToString()) == false ? int.Parse(row[3].ToString()) : -1,
                            Cust_Tel = row[4].ToString(),
                            Startdate = string.IsNullOrEmpty(row[5].ToString()) == false ? DateTime.Parse(row[5].ToString()) : new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local),
                            Enddate = string.IsNullOrEmpty(row[6].ToString()) == false ? DateTime.Parse(row[6].ToString()) : new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local),
                            Memo = util.decStr(row[7].ToString())
                        });
                    }
                }
                catch (FbException e)
                {
                    util.WriteLog(e.ErrorCode, e.Message);
                }
            }
        }

        public void add(CallList item)
        {
            DataTable dt;

            using (FirebirdDBHelper db = new FirebirdDBHelper(util.GetFbDbStrConn()))
            {
                try
                {
                    db.SetParameters("@I_DIRECTION", FbDbType.Integer, item.Direction);
                    db.SetParameters("@I_CUST_TEL", FbDbType.VarChar, item.Cust_Tel);
                    db.SetParameters("@I_STARTDATE", FbDbType.TimeStamp, item.Startdate);

                    db.BeginTran();
                    dt = db.GetDataTableSP("INS_CALL_LIST");
                    db.Commit();

                    foreach (DataRow row in dt.Rows)
                    {
                        item.Idx = string.IsNullOrEmpty(row[0].ToString()) == false ? int.Parse(row[0].ToString()) : 0;
                        item.Cust_Idx = string.IsNullOrEmpty(row[1].ToString()) == false ? int.Parse(row[1].ToString()) : 0;
                    }

                    this.Insert(0, item);
                }
                catch (FbException e)
                {
                    util.WriteLog(e.ErrorCode, e.Message);
                    db.Rollback();
                }
            }
        }

        public void modify(CallList item)
        {
            using (FirebirdDBHelper db = new FirebirdDBHelper(util.GetFbDbStrConn()))
            {
                try
                {
                    db.SetParameters("@I_IDX", FbDbType.Integer, item.Idx);
                    db.SetParameters("@I_ENDDATE", FbDbType.TimeStamp, item.Enddate);

                    db.BeginTran();
                    db.ExcuteSP("MODI_CALL_LIST");
                    db.Commit();

                    var itm = this.FirstOrDefault(x => x.Idx == item.Idx);

                    //if (itm != null)
                    //{
                    //    int idx = this.IndexOf(itm);
                    //    this.RemoveAt(idx);
                    //    this.Insert(idx, item);
                    //}
                }
                catch (FbException e)
                {
                    // util.WriteLog(e.ErrorCode, e.Message);
                    db.Rollback();
                    throw e;
                }
                catch (Exception e)
                {
                    util.WriteLog(e.Message);
                }
            }
        }

        public void remove(CallList item)
        {
            using (FirebirdDBHelper db = new FirebirdDBHelper(util.GetFbDbStrConn()))
            {
                try
                {
                    db.SetParameters("@I_IDX", FbDbType.Integer, item.Idx);

                    db.BeginTran();
                    db.ExcuteSP("RMV_CALL_LIST");
                    db.Commit();

                    this.Remove(item);
                }
                catch (FbException e)
                {
                    util.WriteLog(e.ErrorCode, e.Message);
                    db.Rollback();
                }
            }
        }
    }
}
