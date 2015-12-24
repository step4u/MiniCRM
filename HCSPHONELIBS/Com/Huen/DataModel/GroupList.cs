using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Com.Huen.Libs;
using Com.Huen.Sql;
using System.Data;
using Com.Huen.Interfaces;
using System.ComponentModel;
using FirebirdSql.Data.FirebirdClient;
using System.Linq;

namespace Com.Huen.DataModel
{
    public class GroupList : ITreeViewTop
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int idx = -1;
        private string name = string.Empty;
        private ObservableCollection<GroupList> children = null;
        private bool _IsSelected;

        public int Idx
        {
            get { return idx; }
            set { idx = value; }
        }

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null)
                    handler(this, new PropertyChangedEventArgs("Name"));
            }
        }

        public ObservableCollection<GroupList> Children
        {
            get;
            set;
        }

        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                _IsSelected = value;
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null)
                    handler(this, new PropertyChangedEventArgs("IsSelected"));
            }
        }
    }

    public class GroupLists : ObservableCollection<GroupList>
    {
        public GroupLists()
        {
            DataTable dt = null;

            using (FirebirdDBHelper db = new FirebirdDBHelper(util.GetFbDbStrConn()))
            {
                try
                {
                    dt = db.GetDataTableSP("GET_GROUPLIST");
                }
                catch (FirebirdSql.Data.FirebirdClient.FbException e)
                {
                    util.WriteLog(e.ErrorCode, e.Message);
                }
            }

            GroupList gltop = new GroupList() {
                Idx = 0,
                Name = Application.Current.FindResource("GROUPLIST_FIRST").ToString(),
                Children = new ObservableCollection<GroupList>()
            };

            foreach (DataRow myRow in dt.Rows)
            {
                GroupList glsub = new GroupList() {
                    Idx = string.IsNullOrEmpty(myRow["O_IDX"].ToString()) == false ? int.Parse(myRow["O_IDX"].ToString()) : 0,
                    Name = myRow["O_NAME"].ToString(),
                    Children = new ObservableCollection<GroupList>()
                };

                gltop.Children.Add(glsub);
            }

            this.Add(gltop);
        }

        public ObservableCollection<GroupList> getlist()
        {
            return this[0].Children;
        }

        public void add(GroupList item)
        {
            this[0].Children.Add(item);
        }

        public void update(GroupList item)
        {
            if (item.Idx == -1)
            {
                using (FirebirdDBHelper db = new FirebirdDBHelper(util.GetFbDbStrConn()))
                {
                    try
                    {
                        db.SetParameters("@I_NAME", FbDbType.VarChar, item.Name);

                        db.BeginTran();
                        string idx = db.GetDataSP("INS_GROUPS").ToString();
                        db.Commit();

                        item.Idx = string.IsNullOrEmpty(idx) == false ? int.Parse(idx) : -1;
                    }
                    catch (FbException e)
                    {
                        util.WriteLog(e.ErrorCode, e.Message);
                        db.Rollback();
                    }
                }
            }
            else
            {
                using (FirebirdDBHelper db = new FirebirdDBHelper(util.GetFbDbStrConn()))
                {
                    try
                    {
                        db.SetParameters("@I_IDX", FbDbType.Integer, item.Idx);
                        db.SetParameters("@I_NAME", FbDbType.VarChar, item.Name);

                        db.BeginTran();
                        db.ExcuteSP("MODI_GROUPS");
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

        public void remove(GroupList item)
        {
            using (FirebirdDBHelper db = new FirebirdDBHelper(util.GetFbDbStrConn()))
            {
                try
                {
                    db.SetParameters("@I_IDX", FbDbType.Integer, item.Idx);

                    db.BeginTran();
                    db.ExcuteSP("RMV_GROUPS");
                    db.Commit();

                    var itm = this.Items[0].Children.FirstOrDefault(x => x.Idx == item.Idx);

                    this.Items[0].Children.Remove(itm);
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
