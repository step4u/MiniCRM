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

namespace Com.Huen.DataModel
{
    public class Customer : INotifyPropertyChanged
    {
        private bool _IsChecked = false;
        private bool _IsSelected = false;
        public int Idx { get; set; }
        public int Group_Idx { get; set; }
        public string Name { get; set; }
        public string Company { get; set; }
        public string Title { get; set; }
        public string Tel { get; set; }
        public string Cellular { get; set; }
        public string Extension { get; set; }
        public string Email { get; set; }
        public string Addr { get; set; }
        public string Etc { get; set; }

        public bool IsChecked
        {
            get
            {
                return _IsChecked;
            }
            set
            {
                _IsChecked = value;
                this.OnPropertyChanged("IsChecked");
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
                this.OnPropertyChanged("IsSelected");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Customers : ObservableCollection<Customer>
    {
        public Customers() { }

        public Customers(int idx)
        {
            DataTable dt;
            

            using (FirebirdDBHelper db = new FirebirdDBHelper(util.GetFbDbStrConn()))
            {
                try
                {
                    db.SetParameters("@I_GROUP_IDX", FbDbType.Integer, idx);

                    dt = db.GetDataTableSP("GET_CUSTOMER");

                    foreach (DataRow row in dt.Rows)
                    {
                        this.Add(new Customer() {
                                IsChecked = false,
                                IsSelected = false,
                                Idx = string.IsNullOrEmpty(row[0].ToString()) == false ? int.Parse(row[0].ToString()) : -1,
                                Group_Idx = string.IsNullOrEmpty(row[1].ToString()) == false ? int.Parse(row[1].ToString()) : -1,
                                Name = row[2].ToString(),
                                Company = row[3].ToString(),
                                Title = row[4].ToString(),
                                Tel = row[5].ToString(),
                                Cellular = row[6].ToString(),
                                Extension = row[7].ToString(),
                                Email = row[8].ToString(),
                                Addr = row[9].ToString(),
                                Etc = row[10].ToString()
                            });
                    }
                }
                catch (FbException e)
                {
                    util.WriteLog(e.ErrorCode, e.Message);
                }
            }
        }

        public void add(Customer item)
        {
            using (FirebirdDBHelper db = new FirebirdDBHelper(util.GetFbDbStrConn()))
            {
                try
                {
                    db.SetParameters("@I_GROUP_IDX", FbDbType.Integer, item.Group_Idx);
                    db.SetParameters("@I_NAME", FbDbType.VarChar, item.Name);
                    db.SetParameters("@I_COMPANY", FbDbType.VarChar, item.Company);
                    db.SetParameters("@I_TITLE", FbDbType.VarChar, item.Title);
                    db.SetParameters("@I_TEL", FbDbType.VarChar, item.Tel);
                    db.SetParameters("@I_CELLULAR", FbDbType.VarChar, item.Cellular);
                    db.SetParameters("@I_EXTENSION", FbDbType.VarChar, item.Extension);
                    db.SetParameters("@I_EMAIL", FbDbType.VarChar, item.Email);
                    db.SetParameters("@I_ADDR", FbDbType.VarChar, item.Addr);

                    db.BeginTran();
                    string idx = db.GetDataSP("INS_CUSTOMER").ToString();
                    db.Commit();

                    item.Idx = string.IsNullOrEmpty(idx) == false ? int.Parse(idx) : -1;
                    var itm = this.Items.FirstOrDefault(x => x.Group_Idx == item.Group_Idx);

                    // if (itm != null)
                        this.Add(item);

                    //if (this.Items.Count > 0)
                    //{
                    //    if (this.Items[0].Group_Idx == item.Group_Idx)
                    //        this.Add(item);
                    //}
                }
                catch (FbException e)
                {
                    util.WriteLog(e.ErrorCode, e.Message);
                    db.Rollback();
                }
            }
        }

        public void modify(Customer item)
        {
            using (FirebirdDBHelper db = new FirebirdDBHelper(util.GetFbDbStrConn()))
            {
                try
                {
                    db.SetParameters("@I_IDX", FbDbType.Integer, item.Idx);
                    db.SetParameters("@I_GROUP_IDX", FbDbType.Integer, item.Group_Idx);
                    db.SetParameters("@I_NAME", FbDbType.VarChar, item.Name);
                    db.SetParameters("@I_COMPANY", FbDbType.VarChar, item.Company);
                    db.SetParameters("@I_TITLE", FbDbType.VarChar, item.Title);
                    db.SetParameters("@I_TEL", FbDbType.VarChar, item.Tel);
                    db.SetParameters("@I_CELLULAR", FbDbType.VarChar, item.Cellular);
                    db.SetParameters("@I_EXTENSION", FbDbType.VarChar, item.Extension);
                    db.SetParameters("@I_EMAIL", FbDbType.VarChar, item.Email);
                    db.SetParameters("@I_ADDR", FbDbType.VarChar, item.Addr);

                    db.BeginTran();
                    db.ExcuteSP("MODI_CUSTOMER");
                    db.Commit();

                    if (this.Items.Count > 0)
                    {
                        if (this.Items[0].Group_Idx == item.Group_Idx)
                        {
                            var itm = this.FirstOrDefault(x => x.Idx == item.Idx);
                            int idx = this.IndexOf(itm);
                            this.RemoveAt(idx);
                            this.Insert(idx, item);
                        }
                    }
                }
                catch (FbException e)
                {
                    util.WriteLog(e.ErrorCode, e.Message);
                    db.Rollback();
                }
            }
        }

        public void remove(Customer item)
        {
            using (FirebirdDBHelper db = new FirebirdDBHelper(util.GetFbDbStrConn()))
            {
                try
                {
                    db.SetParameters("@I_IDX", FbDbType.Integer, item.Idx);

                    db.BeginTran();
                    db.ExcuteSP("RMV_CUSTOMER");
                    db.Commit();

                    if (this.Items.Count > 0)
                    {
                        if (this.Items[0].Group_Idx == item.Group_Idx)
                            this.Remove(item);
                    }
                }
                catch (FbException e)
                {
                    // util.WriteLog(e.ErrorCode, e.Message);
                    db.Rollback();
                    throw e;
                }
            }
        }
    }
}
