using Com.Huen.Libs;
using Com.Huen.Sql;
using FirebirdSql.Data.FirebirdClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;

namespace Com.Huen.DataModel
{
    public class Sms
    {
        public int Idx { get; set; }
        public int Cust_Idx { get; set; }
        public string Cust_Name { get; set; }
        public string Cust_Tel { get; set; }
        public DateTime? Regdate { get; set; }
        public string Memo { get; set; }
        public int Result { get; set; }
    }

    public class Smses : ObservableCollection<Sms>
    {
        public Smses() { }

        public Smses(SearchCondition1 val)
        {
            DataTable dt;

            using (FirebirdDBHelper db = new FirebirdDBHelper(util.GetFbDbStrConn()))
            {
                try
                {
                    db.SetParameters("@I_SDATE", FbDbType.TimeStamp, val.StartDate);
                    db.SetParameters("@I_EDATE", FbDbType.TimeStamp, val.EndDate);
                    db.SetParameters("@I_NUMBER", FbDbType.VarChar, val.Number);

                    dt = db.GetDataTableSP("GET_SMS_LIST");

                    foreach (DataRow row in dt.Rows)
                    {
                        this.Add(new Sms()
                        {
                            Idx = string.IsNullOrEmpty(row[0].ToString()) == false ? int.Parse(row[0].ToString()) : -1,
                            Cust_Idx = string.IsNullOrEmpty(row[1].ToString()) == false ? int.Parse(row[1].ToString()) : -1,
                            Cust_Name = row[2].ToString(),
                            Cust_Tel = row[3].ToString(),
                            Regdate = string.IsNullOrEmpty(row[4].ToString()) == false ? DateTime.Parse(row[4].ToString()) : new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local),
                            Memo = row[5].ToString(),
                            Result = string.IsNullOrEmpty(row[6].ToString()) == false ? int.Parse(row[6].ToString()) : -1
                        });
                    }
                }
                catch (FbException e)
                {
                    util.WriteLog(e.ErrorCode, e.Message);
                }
            }
        }

        public void add(Sms item)
        {
            DataTable dt;

            using (FirebirdDBHelper db = new FirebirdDBHelper(util.GetFbDbStrConn()))
            {
                try
                {
                    db.SetParameters("@I_CUST_TEL", FbDbType.VarChar, item.Cust_Tel);
                    db.SetParameters("@I_MEMO", FbDbType.Text, item.Memo);
                    db.SetParameters("@I_RESULT", FbDbType.SmallInt, item.Result);

                    db.BeginTran();
                    dt = db.GetDataTableSP("INS_SMS_LIST");
                    db.Commit();

                    foreach (DataRow row in dt.Rows)
                    {
                        item.Idx = string.IsNullOrEmpty(row[0].ToString()) == false ? int.Parse(row[0].ToString()) : -1;
                        item.Cust_Idx = string.IsNullOrEmpty(row[1].ToString()) == false ? int.Parse(row[1].ToString()) : -1;
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

        public void remove(Sms item)
        {
            using (FirebirdDBHelper db = new FirebirdDBHelper(util.GetFbDbStrConn()))
            {
                try
                {
                    db.SetParameters("@I_IDX", FbDbType.Integer, item.Idx);

                    db.BeginTran();
                    db.ExcuteSP("RMV_SMS_LIST");
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
