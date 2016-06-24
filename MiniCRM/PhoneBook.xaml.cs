using System.Linq;
using System;
using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;
using Com.Huen.DataModel;
using System.Windows.Media;
using System.Windows.Controls;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Com.Huen.Controls;
using System.Data;
using Com.Huen.Sql;
using Com.Huen.Libs;
using FirebirdSql.Data.FirebirdClient;
using System.Text;
using System.Text.RegularExpressions;

namespace MiniCRM
{
    /// <summary>
    /// Interaction logic for PhoneBook.xaml
    /// </summary>
    public partial class PhoneBook : MetroWindow
    {
        private MainWindow owner;

        private string userdatapath = string.Empty;
        private double top = 0.0d;
        private double left = 0.0d;
        private double width = 0.0d;
        private double height = 0.0d;

        public Customer FlyCustomer
        {
            get
            {
                return this.flycustomer;
            }
            set
            {
                this.flycustomer = value;
                this.flyCustomer.DataContext = this.flycustomer;
            }
        }

        public CUSTOMER_STATE CUSTOMERSTATE
        {
            get { return this.CustState; }
            set { this.CustState = value; }
        }

        #region DataGrid datum
        private GroupLists glist;
        public Customers customers;
        public CallLists calls;
        public Smses smslist;
        #endregion

        // public List<FlyoutsControl> flyouts = new List<FlyoutsControl>();

        public PhoneBook()
        {
            InitializeComponent();
            ReadIni();
            Init();

            this.Loaded += PhoneBook_Loaded;
        }

        private void PhoneBook_Loaded(object sender, RoutedEventArgs e)
        {
            owner = (MainWindow)this.Owner;
        }

        private ObservableCollection<GroupList> groupitems;
        private void Init()
        {
            this.Top = top;
            this.Left = left;
            this.Width = width;
            this.Height = height;

            glist = new GroupLists();
            this.DataContext = glist;

            groupitems = new ObservableCollection<GroupList>(glist.getlist());
            groupitems.Insert(0, new GroupList() { Idx = 0, Name = Application.Current.FindResource("CMB_TXT_HEAD").ToString() });
            //foreach (var item in glist.getlist())
            //{
            //    groupitems.Add(item);
            //}

            cmbGroup.ItemsSource = groupitems;
        }

        private void ReadIni()
        {
            userdatapath = string.Format(@"{0}\{1}", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MiniCRM");

            Ini ini = new Ini(string.Format(@"{0}\{1}", userdatapath, "env.ini"));

            top = string.IsNullOrEmpty(ini.IniReadValue("PHONEBOOK", "TOP")) == false ? double.Parse(ini.IniReadValue("PHONEBOOK", "TOP")) : 0.0d;
            left = string.IsNullOrEmpty(ini.IniReadValue("PHONEBOOK", "LEFT")) == false ? double.Parse(ini.IniReadValue("PHONEBOOK", "LEFT")) : 0.0d;
            width = string.IsNullOrEmpty(ini.IniReadValue("PHONEBOOK", "WIDTH")) == false ? double.Parse(ini.IniReadValue("PHONEBOOK", "WIDTH")) : 0.0d;
            height = string.IsNullOrEmpty(ini.IniReadValue("PHONEBOOK", "HEIGHT")) == false ? double.Parse(ini.IniReadValue("PHONEBOOK", "HEIGHT")) : 0.0d;
        }

        public void SaveIni()
        {
            Ini ini = new Ini(string.Format(@"{0}\{1}", userdatapath, "env.ini"));

            ini.IniWriteValue("PHONEBOOK", "TOP", this.Top.ToString());
            ini.IniWriteValue("PHONEBOOK", "LEFT", this.Left.ToString());
            ini.IniWriteValue("PHONEBOOK", "WIDTH", this.Width.ToString());
            ini.IniWriteValue("PHONEBOOK", "HEIGHT", this.Height.ToString());
        }

        private void ToggleFlyout(int index)
        {
            var flyout = this.Flyouts.Items[index] as Flyout;
            if (flyout == null)
            {
                return;
            }

            switch (index)
            {
                case 0:
                    // 고객 추가 / 수정
                    break;
                case 1:
                    // options flyout
                    break;
                case 2:
                    // logs
                    flyout.Width = 300;
                    break;
                case 3:
                    // absence call
                    flyout.Width = 300;
                    break;
            }

            flyout.IsOpen = !flyout.IsOpen;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;

            SaveIni();
            this.Hide();

            this.flyCustomer.IsOpen = false;
            this.flyCustMemo.IsOpen = false;

            // this.Visibility = Visibility.Collapsed;
            // ((MainWindow)this.Owner).pb = null;
        }

        private void tabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl tab = e.Source as TabControl;

            if (tab == null)
            {
                e.Handled = true;
                return;
            }

            switch (tab.SelectedIndex)
            {
                case 0:
                    sdate.IsEnabled = false;
                    edate.IsEnabled = false;
                    txtNumber.IsEnabled = false;
                    btnSearch.IsEnabled = false;
                    break;
                case 1:
                case 2:
                    sdate.IsEnabled = true;
                    edate.IsEnabled = true;
                    txtNumber.IsEnabled = true;
                    btnSearch.IsEnabled = true;
                    break;
            }
        }

        private void tvGroup_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var val = e.NewValue as GroupList;

            if (val.Idx < 0)
            {
                e.Handled = true;
                return;
            }

            customers = new Customers(val.Idx);
            dgCustList.ItemsSource = customers;
        }

        private void tvGroup_ContextMenuOpening_1(object sender, ContextMenuEventArgs e)
        {
            TreeView view = e.Source as TreeView;
            GroupList item = view.SelectedItem as GroupList;

            if (item == null)
            {
                e.Handled = true;
                return;
            }

            ContextMenu cm = view.ContextMenu;

            for (int i = 0; i < cm.Items.Count; i++)
            {
                if (cm.Items[i].GetType() == typeof(Separator))
                    continue;

                MenuItem mi = cm.Items[i] as MenuItem;
                mi.IsEnabled = true;
            }

            if (item.Idx > 0)
            {
                MenuItem mi = cm.Items[0] as MenuItem;
                mi.IsEnabled = false;
            }
            else
            {
                for (int i = 1; i < 5; i++)
                {
                    if (cm.Items[i].GetType() == typeof(Separator))
                        continue;

                    MenuItem mi = cm.Items[i] as MenuItem;
                    mi.IsEnabled = false;
                }
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            // 그룹 추가

            MenuItem menuItem = (MenuItem)e.Source;
            ContextMenu contextMenu = (ContextMenu)menuItem.Parent;
            TreeView view = (TreeView)contextMenu.PlacementTarget;
            var item = view.Items[0] as GroupList;
            item.IsSelected = false;

            GroupList list = new GroupList() { Idx = -1, Name = string.Empty, Children = new ObservableCollection<GroupList>(), IsSelected = true };
            glist.add(list);
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            // 그룹 수정
            MenuItem menuItem = (MenuItem)e.Source;
            ContextMenu contextMenu = (ContextMenu)menuItem.Parent;
            WpfTreeViewInPlaceEditControl view = (WpfTreeViewInPlaceEditControl)contextMenu.PlacementTarget;
            // var item = view.SelectedItem as GroupList;

            view.IsInEditMode = true;
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            // 그룹 삭제
            MenuItem menuItem = (MenuItem)e.Source;
            ContextMenu contextMenu = (ContextMenu)menuItem.Parent;
            WpfTreeViewInPlaceEditControl view = (WpfTreeViewInPlaceEditControl)contextMenu.PlacementTarget;
            var lists = view.ItemsSource as GroupLists;
            var selitem = view.SelectedItem as GroupList;

            if (selitem.Idx == 0)
            {
                e.Handled = true;
                return;
            }

            try
            {
                int idx = lists[0].Children.IndexOf(selitem);
                lists.remove(selitem);

                try
                {
                    lists[0].Children[idx].IsSelected = true;
                }
                catch (Exception ee)
                {
                    lists[0].IsSelected = true;
                }

                // groupitems.Remove(selitem);
                var item = groupitems.FirstOrDefault(x => x.Idx == selitem.Idx);
                if (item == null)
                {
                    //idx = groupitems.IndexOf(item);
                    //groupitems.Remove(selitem);
                }
                else
                {
                    idx = groupitems.IndexOf(item);
                    groupitems = new ObservableCollection<GroupList>(glist.getlist());
                    groupitems.Insert(0, new GroupList() { Idx = 0, Name = Application.Current.FindResource("CMB_TXT_HEAD").ToString() });
                    cmbGroup.ItemsSource = null;
                    cmbGroup.ItemsSource = groupitems;

                    if (flyCustomer.IsOpen)
                    {
                        if (cmbGroup.Items.Count > 0)
                        {
                            cmbGroup.SelectedIndex = idx - 1;
                        }
                    }
                }
            }
            catch (FbException ex)
            {
                if (ex.ErrorCode == 335544466
                    && ex.Message.Contains("CUSTOMER")
                    && ex.Message.Contains("FK_CUSTOMER_1"))
                {
                    e.Handled = true;
                    MessageBox.Show(Application.Current.FindResource("MSG_ERR_GROUP_REMOVE_EXIST_CUSTOMER").ToString(), Application.Current.FindResource("MSGBOX_TXT_TITLE").ToString());
                }
            }
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            // 고객 추가
            flycustomer = new Customer() { Group_Idx = ((GroupList)tvGroup.SelectedItem).Idx };
            flyCustomer.DataContext = flycustomer;
            flyCustomer.Header = Application.Current.FindResource("PB_FLYOUT_TITLE_CUST_ADD").ToString();
            flyCustomer.IsOpen = true;

            CustState = CUSTOMER_STATE.ADD;
        }

        private void MenuItem_Click_4_1(object sender, RoutedEventArgs e)
        {
            // 고객 excel로 추가
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "Excel 97-2013|*.xls";
            openFileDialog.FilterIndex = 1;
            openFileDialog.ShowDialog();

            string xlsfilename = openFileDialog.FileName;
            openFileDialog.Dispose();

            DataSet ds = ExcelHelper.OpenExcelDB(xlsfilename);

            if (ds != null)
            {
                if (ds.Tables.Count == 1)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        Customer _customer = new Customer() {
                            Group_Name = row[0].ToString().Trim(),
                            Name = row[1].ToString().Trim(),
                            Company = row[2].ToString().Trim(),
                            Title = row[3].ToString().Trim(),
                            Tel = row[4].ToString().Trim(),
                            Cellular = row[5].ToString().Trim(),
                            Extension = row[6].ToString().Trim(),
                            Email = row[7].ToString().Trim(),
                            Addr = row[8].ToString().Trim()
                        };
                        customers.importExcel(_customer);
                    }
                }
            }
        }

        private void MenuItem_Click_5_1(object sender, RoutedEventArgs e)
        {
            // 고객 excel로 저장
            System.Windows.Forms.SaveFileDialog saveDialog = new System.Windows.Forms.SaveFileDialog();
            saveDialog.Filter = "Excel 97-2013 (*.xls)|*.xls";

            if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DataSet _ds = new DataSet();
                DataTable _dt = new DataTable();
                _dt.Columns.Add("그룹명", typeof(string));
                _dt.Columns.Add("고객명", typeof(string));
                _dt.Columns.Add("회사명", typeof(string));
                _dt.Columns.Add("직급", typeof(string));
                _dt.Columns.Add("회사전화", typeof(string));
                _dt.Columns.Add("휴대전화", typeof(string));
                _dt.Columns.Add("회사내선", typeof(string));
                _dt.Columns.Add("이메일", typeof(string));
                _dt.Columns.Add("주소", typeof(string));

                foreach (Customer item in dgCustList.Items)
                {
                    _dt.Rows.Add(item.Group_Name, item.Name, item.Company, item.Title, item.Tel, item.Cellular, item.Extension, item.Email, item.Addr);
                }

                _ds.Tables.Add(_dt);

                ExcelHelper.SaveExcelDB(saveDialog.FileName, _ds, true);

                MessageBox.Show("주소록 저장이 완료되었습니다.", "알림!!");
            }
        }

        private void dgCustList_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            GroupList gselitem = tvGroup.SelectedItem as GroupList;

            DataGrid view = e.Source as DataGrid;
            Customer item = view.SelectedItem as Customer;

            ContextMenu cm = view.ContextMenu;

            for (int i = 0; i < cm.Items.Count; i++)
            {
                MenuItem mi = cm.Items[i] as MenuItem;
                mi.IsEnabled = true;
                mi.Header = Application.Current.FindResource(string.Format("PB_DATAGRID_CUST_MENU_HEADER_{0}", i));
            }

            if (gselitem == null || item == null)
            {
                e.Handled = true;

                (cm.Items[0] as MenuItem).IsEnabled = true;
                (cm.Items[1] as MenuItem).IsEnabled = false;
                (cm.Items[2] as MenuItem).IsEnabled = false;
                (cm.Items[3] as MenuItem).IsEnabled = false;
                (cm.Items[4] as MenuItem).IsEnabled = false;
                (cm.Items[5] as MenuItem).IsEnabled = false;
                (cm.Items[6] as MenuItem).IsEnabled = false;

                e.Handled = false;

                return;
            }

            if (item == null)
                ((MenuItem)cm.Items[2]).IsEnabled = false;

            if (!string.IsNullOrEmpty(item.Tel))
            {
                ((MenuItem)cm.Items[3]).Header = string.Format("{0} ({1})", Application.Current.FindResource("PB_DATAGRID_CUST_MENU_HEADER_3"), item.Tel);
            }
            else
            {
                ((MenuItem)cm.Items[3]).IsEnabled = false;
            }

            if (!string.IsNullOrEmpty(item.Cellular))
            {
                ((MenuItem)cm.Items[4]).Header = string.Format("{0} ({1})", Application.Current.FindResource("PB_DATAGRID_CUST_MENU_HEADER_4"), item.Cellular);
            }
            else
            {
                ((MenuItem)cm.Items[4]).IsEnabled = false;
            }

            if (!string.IsNullOrEmpty(item.Extension))
            {
                ((MenuItem)cm.Items[5]).Header = string.Format("{0} ({1})", Application.Current.FindResource("PB_DATAGRID_CUST_MENU_HEADER_5"), item.Extension);
            }
            else
            {
                ((MenuItem)cm.Items[5]).IsEnabled = false;
            }
        }

        private void dgCustList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // 고객 수정
            DataGrid view = e.Source as DataGrid;
            Customer item = view.SelectedItem as Customer;

            if (item == null)
            {
                e.Handled = true;
                return;
            }

            flycustomer = new Customer() { Idx = item.Idx, Group_Idx = item.Group_Idx, Name = item.Name, Company = item.Company, Title = item.Title, Tel = item.Tel, Cellular = item.Cellular, Extension = item.Extension, Email = item.Email, Addr = item.Addr };

            flyCustomer.DataContext = flycustomer;
            // cmbGroup.SelectedValue = flycustomer.Group_Idx;
            flyCustomer.Header = Application.Current.FindResource("PB_FLYOUT_TITLE_CUST_EDIT").ToString();
            dgridCustCallList.ItemsSource = GetCallListByCustIdx(flycustomer.Idx, flycustomer.Cellular);
            flyCustomer.IsOpen = true;

            CustState = CUSTOMER_STATE.MODIFY;
        }

        private Customer flycustomer = null;
        private CUSTOMER_STATE CustState = CUSTOMER_STATE.NONE;
        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            CustState = CUSTOMER_STATE.ADD;

            // 고객 추가
            GroupList gselitem = tvGroup.SelectedItem as GroupList;
            if (gselitem != null)
            {
                flycustomer = new Customer() { Group_Idx = gselitem.Idx };
            }
            else
            {
                flycustomer = new Customer();
            }

            flyCustomer.DataContext = flycustomer;
            flyCustomer.Header = Application.Current.FindResource("PB_FLYOUT_TITLE_CUST_ADD").ToString();
            flyCustomer.IsOpen = true;
        }

        private void MenuItem_Click_5(object sender, RoutedEventArgs e)
        {
            CustState = CUSTOMER_STATE.MODIFY;

            // 고객 수정
            MenuItem menuitem = (MenuItem)e.Source;
            ContextMenu cm = (ContextMenu)menuitem.Parent;
            DataGrid view = (DataGrid)cm.PlacementTarget;
            Customer item = (Customer)view.SelectedItem;

            flycustomer = new Customer() { Idx = item.Idx, Group_Idx = item.Group_Idx, Name = item.Name, Company = item.Company, Title = item.Title, Tel = item.Tel, Cellular = item.Cellular, Extension = item.Extension, Email = item.Email, Addr = item.Addr };

            flyCustomer.DataContext = flycustomer;
            // cmbGroup.SelectedValue = flycustomer.Group_Idx;
            flyCustomer.Header = Application.Current.FindResource("PB_FLYOUT_TITLE_CUST_EDIT").ToString();
            flyCustomer.IsOpen = true;
        }

        private void MenuItem_Click_6(object sender, RoutedEventArgs e)
        {
            // 고객 삭제
            MenuItem menuitem = (MenuItem)e.Source;
            ContextMenu cm = (ContextMenu)menuitem.Parent;
            DataGrid view = (DataGrid)cm.PlacementTarget;

            int checkedcount = 0;

            foreach (Customer itm in view.Items)
            {
                if (itm.IsChecked)
                    checkedcount++;
            }

            if (checkedcount > 0)
            {
                List<Customer> viewlist = new List<Customer>(view.Items.Cast<Customer>().ToList().Where(x => x.IsChecked == true));

                foreach (Customer itm in viewlist)
                {
                    if (itm.IsChecked)
                    {
                        try
                        {
                            customers.remove(itm);
                        }
                        catch (FbException ex)
                        {
                            if (ex.ErrorCode == 335544466
                                && ex.Message.Contains("CALL_LISTS")
                                && ex.Message.Contains("FK_CALL_LISTS_1"))
                            {
                                e.Handled = true;
                                MessageBox.Show(Application.Current.FindResource("MSG_ERR_CUSTOMER_REMOVE_EXIST_CALLLIST").ToString(), Application.Current.FindResource("MSGBOX_TXT_TITLE").ToString());
                            }
                        }
                    }
                }
            }
            else
            {
                Customer item = (Customer)view.SelectedItem;

                try
                {
                    customers.remove(item);
                }
                catch (FbException ex)
                {
                    if (ex.ErrorCode == 335544466
                        && ex.Message.Contains("CALL_LISTS")
                        && ex.Message.Contains("FK_CALL_LISTS_1"))
                    {
                        e.Handled = true;
                        MessageBox.Show(Application.Current.FindResource("MSG_ERR_CUSTOMER_REMOVE_EXIST_CALLLIST").ToString(), Application.Current.FindResource("MSGBOX_TXT_TITLE").ToString());
                    }
                }
            }

            if (chbHeader.IsChecked == true ? true : false) chbHeader.IsChecked = false;
        }

        private void MenuItem_Click_7(object sender, RoutedEventArgs e)
        {
            // 일반전화 걸기
            MenuItem menuitem = (MenuItem)e.Source;
            ContextMenu cm = (ContextMenu)menuitem.Parent;
            DataGrid view = (DataGrid)cm.PlacementTarget;
            Customer item = (Customer)view.SelectedItem;

            owner.MakeCall(item.Tel);
        }

        private void MenuItem_Click_8(object sender, RoutedEventArgs e)
        {
            // 휴대전화 걸기
            MenuItem menuitem = (MenuItem)e.Source;
            ContextMenu cm = (ContextMenu)menuitem.Parent;
            DataGrid view = (DataGrid)cm.PlacementTarget;
            Customer item = (Customer)view.SelectedItem;

            owner.MakeCall(item.Cellular);
        }

        private void MenuItem_Click_9(object sender, RoutedEventArgs e)
        {
            // 내선전화 걸기
            MenuItem menuitem = (MenuItem)e.Source;
            ContextMenu cm = (ContextMenu)menuitem.Parent;
            DataGrid view = (DataGrid)cm.PlacementTarget;
            Customer item = (Customer)view.SelectedItem;

            owner.MakeCall(item.Extension);
        }

        private void MenuItem_Click_10(object sender, RoutedEventArgs e)
        {
            // SMS 보내기
            MenuItem menuitem = (MenuItem)e.Source;
            ContextMenu cm = (ContextMenu)menuitem.Parent;
            DataGrid view = (DataGrid)cm.PlacementTarget;

            Customers smscustlist = new Customers(); ;
            foreach (Customer item in view.ItemsSource)
            {
                if (item.IsChecked)
                    smscustlist.Add(item);
            }

            if (smscustlist.Count < 1)
            {
                Customer item = (Customer)view.SelectedItem;
                smscustlist = new Customers();
                smscustlist.Add(item);
                dgSmsReceiverList.ItemsSource = smscustlist;
            }
            else
            {
                dgSmsReceiverList.ItemsSource = smscustlist;
            }

            flySms.IsOpen = true;
        }

        private void btnCustMemo_Click(object sender, RoutedEventArgs e)
        {
            Customer tmpcustomer = FlyCustomer;
            CallList tmpcall = calls.FirstOrDefault(x => x.Cust_Idx == tmpcustomer.Idx);
            flyCustMemo.DataContext = tmpcall;
            flyCustMemo.IsOpen = true;
        }

        private void btnCustSave_Click(object sender, RoutedEventArgs e)
        {
            // 고객 추가/수정 save
            if (cmbGroup.SelectedIndex < 1)
            {
                MessageBox.Show(Application.Current.FindResource("MSG_ERR_CUSTOMER_EMPTY_GROUP").ToString(), Application.Current.FindResource("MSGBOX_TXT_TITLE").ToString());
                cmbGroup.Focus();
                return;
            }
            if (string.IsNullOrEmpty(flycustomer.Name))
            {
                MessageBox.Show(Application.Current.FindResource("MSG_ERR_CUSTOMER_EMPTY_NAME").ToString(), Application.Current.FindResource("MSGBOX_TXT_TITLE").ToString());
                txtName.Focus();
                return;
            }
            if (string.IsNullOrEmpty(flycustomer.Tel) && string.IsNullOrEmpty(flycustomer.Cellular) && string.IsNullOrEmpty(flycustomer.Extension))
            {
                MessageBox.Show(Application.Current.FindResource("MSG_ERR_CUSTOMER_EMPTY_TEL").ToString(), Application.Current.FindResource("MSGBOX_TXT_TITLE").ToString());
                if (string.IsNullOrEmpty(txtName.Text))
                    txtName.Focus();
                if (string.IsNullOrEmpty(txtCellular.Text))
                    txtCellular.Focus();
                if (string.IsNullOrEmpty(txtExtension.Text))
                    txtExtension.Focus();
                return;
            }

            if (customers == null)
                customers = new Customers();

            if (CustState == CUSTOMER_STATE.ADD)
            {
                customers.add(flycustomer);
            }
            else if (CustState == CUSTOMER_STATE.MODIFY)
            {
                customers.modify(flycustomer);
            }

            flyCustomer.IsOpen = false;

            CustState = CUSTOMER_STATE.NONE;
        }

        private void btnCustCancel_Click(object sender, RoutedEventArgs e)
        {
            // 고객 추가/수정 취소
            flyCustomer.IsOpen = false;
            CustState = CUSTOMER_STATE.NONE;
        }

        private void tbCustMemo_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.S)
            {
                if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
                {
                    try
                    {
                        CallList item = flyCustMemo.DataContext as CallList;
                        TextBox tb = e.Source as TextBox;
                        item.Memo = tb.Text;
                        item.savememo();

                        CallList cl = dgCallList.SelectedItem as CallList;
                        if (cl == null)
                        {
                            var items = dgCallList.ItemsSource as CallLists;
                            cl = items.FirstOrDefault(x => x.Idx == item.Idx);
                        }

                        if (cl != null)
                            cl.Memo = item.Memo;

                        CallList cl2 = dgridCustCallList.SelectedItem as CallList;
                        if (cl2 == null)
                        {
                            var items = dgridCustCallList.ItemsSource as CallLists;
                            cl2 = items.FirstOrDefault(x => x.Idx == item.Idx);
                        }

                        if (cl2 != null)
                            cl2.Memo = item.Memo;

                        flyCustMemo.IsOpen = false;
                    }
                    catch (FbException ex)
                    {
                        util.WriteLog(ex.ErrorCode, ex.Message);
                    }
                }
            }
        }


        private void btnMemoSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CallList item = flyCustMemo.DataContext as CallList;
                TextBox tb = tbCustMemo;
                item.Memo = tb.Text;
                item.savememo();

                CallList cl = dgCallList.SelectedItem as CallList;
                if (cl == null)
                {
                    var items = dgCallList.ItemsSource as CallLists;
                    cl = items.FirstOrDefault(x => x.Idx == item.Idx);
                }

                if (cl != null)
                    cl.Memo = item.Memo;

                CallList cl2 = dgridCustCallList.SelectedItem as CallList;
                if (cl2 == null)
                {
                    var items = dgridCustCallList.ItemsSource as CallLists;
                    cl2 = items.FirstOrDefault(x => x.Idx == item.Idx);
                }

                if (cl2 != null)
                    cl2.Memo = item.Memo;

                flyCustMemo.IsOpen = false;
            }
            catch (FbException ex)
            {
                util.WriteLog(ex.ErrorCode, ex.Message);
            }
        }

        private void flyCustomer_IsOpenChanged(object sender, RoutedEventArgs e)
        {
            if (((Flyout)e.Source).IsOpen)
            {

            }
            else
            {
                flyCustomer.Header = Application.Current.FindResource("PB_DATAGRID_CUST_TOP_LABEL_0").ToString();
                btnCustSave.Visibility = Visibility.Visible;
                btnCustMemo.Visibility = Visibility.Collapsed;
                this.CustState = CUSTOMER_STATE.NONE;
            }
        }

        private void dgCallList_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            DataGrid view = e.Source as DataGrid;
            CallList item = view.SelectedItem as CallList;

            if (item == null)
            {
                e.Handled = true;
                return;
            }

            ContextMenu cm = view.ContextMenu;

            for (int i = 0; i < cm.Items.Count; i++)
            {
                MenuItem mi = cm.Items[i] as MenuItem;
                mi.IsEnabled = true;
                if (i == 0)
                    mi.Header = Application.Current.FindResource("DG_CALLIST_MENIITEM_HEADER_0");
            }

            if (!string.IsNullOrEmpty(item.Cust_Tel))
            {
                ((MenuItem)cm.Items[0]).Header = string.Format("{0} ({1})", Application.Current.FindResource("DG_CALLIST_MENIITEM_HEADER_0"), item.Cust_Tel);
            }
            else
            {
                ((MenuItem)cm.Items[0]).IsEnabled = false;
            }
        }

        private void dgCallList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // CALL LIST
            // 고객 정보
            DataGrid view = e.Source as DataGrid;
            CallList item = view.SelectedItem as CallList;

            if (item == null)
            {
                e.Handled = true;
                return;
            }

            flycustomer = GetCustomerByIdx(item.Cust_Idx);

            if (flycustomer.Idx == -1)
            {
                flycustomer = new Customer() { Group_Idx = 0, Idx = item.Cust_Idx, Cellular = item.Cust_Tel };
                flyCustomer.Header = Application.Current.FindResource("PB_DATAGRID_CUST_TOP_LABEL_0").ToString();
                CustState = CUSTOMER_STATE.ADD;
            }
            else
            {
                flyCustomer.Header = Application.Current.FindResource("PB_DATAGRID_CUST_TOP_LABEL_1").ToString();
                CustState = CUSTOMER_STATE.MODIFY;
            }

            btnCustSave.Visibility = Visibility.Visible;
            dgridCustCallList.ItemsSource = GetCallListByCustIdx(flycustomer.Idx, flycustomer.Cellular);
            flyCustomer.DataContext = flycustomer;
            flyCustomer.IsOpen = true;
        }

        private void MenuItem_Click_11(object sender, RoutedEventArgs e)
        {
            // 전화 걸기
            MenuItem menuitem = (MenuItem)e.Source;
            ContextMenu cm = (ContextMenu)menuitem.Parent;
            DataGrid view = (DataGrid)cm.PlacementTarget;
            CallList item = (CallList)view.SelectedItem;

            if (item == null)
            {
                e.Handled = true;
                return;
            }

            owner.MakeCall(item.Cust_Tel);
        }

        private void MenuItem_Click_12(object sender, RoutedEventArgs e)
        {
            // CALL LIST
            // 고객 정보
            MenuItem menuitem = (MenuItem)e.Source;
            ContextMenu cm = (ContextMenu)menuitem.Parent;
            DataGrid view = (DataGrid)cm.PlacementTarget;
            CallList item = (CallList)view.SelectedItem;

            flycustomer = GetCustomerByIdx(item.Cust_Idx);

            if (flycustomer == null)
            {
                flycustomer = new Customer() { Group_Idx = 0, Idx = item.Cust_Idx, Cellular = item.Cust_Tel };
                flyCustomer.Header = Application.Current.FindResource("PB_DATAGRID_CUST_TOP_LABEL_0").ToString();
                CustState = CUSTOMER_STATE.ADD;
            }
            else
            {
                flyCustomer.Header = Application.Current.FindResource("PB_DATAGRID_CUST_TOP_LABEL_1").ToString();
                CustState = CUSTOMER_STATE.MODIFY;
            }

            btnCustSave.Visibility = Visibility.Visible;
            flyCustomer.DataContext = flycustomer;
            flyCustomer.IsOpen = true;
        }

        private void MenuItem_Click_13(object sender, RoutedEventArgs e)
        {
            // CALL LIST
            // 메모
            MenuItem menuitem = (MenuItem)e.Source;
            ContextMenu cm = (ContextMenu)menuitem.Parent;
            DataGrid view = (DataGrid)cm.PlacementTarget;
            CallList item = (CallList)view.SelectedItem;

            CallList tmp = new CallList() { IsSelected = item.IsSelected, IsChecked = item.IsChecked, Idx = item.Idx, Cust_Idx = item.Cust_Idx, Name = item.Name, Direction = item.Direction, Cust_Tel = item.Cust_Tel, Startdate = item.Startdate, Enddate = item.Enddate, Memo = item.Memo };

            flyCustMemo.DataContext = tmp;
            flyCustMemo.Header = string.Format(Application.Current.FindResource("PB_FLYOUT_TITLE_CUST_MEMO").ToString(), item.Cust_Tel);
            flyCustMemo.IsOpen = true;
        }

        private void MenuItem_Click_14(object sender, RoutedEventArgs e)
        {
            // 삭제
            MenuItem menuitem = (MenuItem)e.Source;
            ContextMenu cm = (ContextMenu)menuitem.Parent;
            DataGrid view = (DataGrid)cm.PlacementTarget;
            CallList item = (CallList)view.SelectedItem;

            calls.remove(item);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchCondition1 con = new SearchCondition1();
            con.StartDate = sdate.Value;
            con.EndDate = edate.Value;
            con.Number = string.IsNullOrEmpty(txtNumber.Text.Trim()) == false ? string.Format("%{0}%", txtNumber.Text.Trim()) : string.Empty;

            if (tabs.SelectedIndex == 1)
            {
                calls = new CallLists(con);
                dgCallList.ItemsSource = calls;
            }
            else if (tabs.SelectedIndex == 2)
            {
                smslist = new Smses(con);
                dgSmsList.ItemsSource = smslist;
            }
        }

        // SMS flyout
        private void MenuItem_Click_15(object sender, RoutedEventArgs e)
        {
            // sms flyout datagrid context menuitem 0
            // 리스트 삭제
            MenuItem menuitem = (MenuItem)e.Source;
            ContextMenu cm = (ContextMenu)menuitem.Parent;
            DataGrid view = (DataGrid)cm.PlacementTarget;
            Customer item = (Customer)view.SelectedItem;
            var items = dgSmsReceiverList.ItemsSource as Customers;

            items.Remove(item);
        }

        private void dgSmsList_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            DataGrid view = e.Source as DataGrid;
            Sms item = view.SelectedItem as Sms;

            if (item == null)
            {
                e.Handled = true;
                return;
            }

            ContextMenu cm = view.ContextMenu;

            for (int i = 0; i < cm.Items.Count; i++)
            {
                MenuItem mi = cm.Items[i] as MenuItem;
                mi.IsEnabled = true;
                if (i == 0)
                    mi.Header = Application.Current.FindResource("SMS_DG_MENUITEM_HEADER_0");
            }

            if (!string.IsNullOrEmpty(item.Cust_Tel))
            {
                ((MenuItem)cm.Items[0]).Header = string.Format("{0} ({1})", Application.Current.FindResource("SMS_DG_MENUITEM_HEADER_0"), item.Cust_Tel);
            }
            else
            {
                ((MenuItem)cm.Items[0]).IsEnabled = false;
            }
        }

        private void dgSmsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGrid view = (DataGrid)e.Source;
            Sms item = (Sms)view.SelectedItem;

            if (item == null)
            {
                e.Handled = true;
                return;
            }

            tbSms.Text = util.decStr(item.Memo);
            btnAddSms.IsEnabled = false;
            btnSendSms.IsEnabled = false;
            dgSmsList.IsEnabled = false;
            tbSms.IsReadOnly = true;
            txtSmsReceiver.IsEnabled = false;
            flySms.IsOpen = true;
        }

        private void MenuItem_Click_16(object sender, RoutedEventArgs e)
        {
            // SMS 탭, SMS 보내기
            MenuItem menuitem = (MenuItem)e.Source;
            ContextMenu cm = (ContextMenu)menuitem.Parent;
            DataGrid view = (DataGrid)cm.PlacementTarget;

            Sms item = (Sms)view.SelectedItem;
            Customer tmpitem = new Customer() { Group_Idx = item.Cust_Idx, Name = item.Cust_Name, Cellular = item.Cust_Tel };
            Customers smscustlist = new Customers();
            smscustlist.Add(tmpitem);
            dgSmsReceiverList.ItemsSource = smscustlist;
            flySms.IsOpen = true;
        }

        private void MenuItem_Click_17(object sender, RoutedEventArgs e)
        {
            // SMS 탭, 고객정보
            MenuItem menuitem = (MenuItem)e.Source;
            ContextMenu cm = (ContextMenu)menuitem.Parent;
            DataGrid view = (DataGrid)cm.PlacementTarget;
            Sms item = (Sms)view.SelectedItem;

            flycustomer = GetCustomerByIdx(item.Cust_Idx);

            if (flycustomer == null)
            {
                flycustomer = new Customer() { Group_Idx = 0, Idx = item.Cust_Idx, Cellular = item.Cust_Tel };
                flyCustomer.Header = Application.Current.FindResource("PB_DATAGRID_CUST_TOP_LABEL_0").ToString();
                CustState = CUSTOMER_STATE.ADD;
            }
            else
            {
                flyCustomer.Header = Application.Current.FindResource("PB_DATAGRID_CUST_TOP_LABEL_1").ToString();
                CustState = CUSTOMER_STATE.MODIFY;
            }

            btnCustSave.Visibility = Visibility.Visible;
            flyCustomer.DataContext = flycustomer;
            flyCustomer.IsOpen = true;
        }

        private void MenuItem_Click_18(object sender, RoutedEventArgs e)
        {
            // SMS 탭, 삭제
            MenuItem menuitem = (MenuItem)e.Source;
            ContextMenu cm = (ContextMenu)menuitem.Parent;
            DataGrid view = (DataGrid)cm.PlacementTarget;
            Sms item = (Sms)view.SelectedItem;

            smslist.remove(item);
        }

        private int txtLength = 0;
        private void tbSms_KeyUp(object sender, KeyEventArgs e)
        {
            if (txtLength >= 80)
            {
                if (e.Key != Key.Back)
                    e.Handled = true;
            }
        }

        private void tbSms_KeyDown(object sender, KeyEventArgs e)
        {
            if (txtLength >= 80)
            {
                if (e.Key != Key.Back)
                    e.Handled = true;
            }
        }

        private void tbSms_TextChanged(object sender, TextChangedEventArgs e)
        {
            // SMS flyout message box counter
            txtLength = Encoding.Default.GetByteCount(tbSms.Text);

            txtCounter.Text = string.Format("{0} / 80", txtLength);
        }

        private void btnAddSms_Click(object sender, RoutedEventArgs e)
        {
            // SMS Flyout customer list add
            if (string.IsNullOrEmpty(txtSmsReceiver.Text.Trim()))
            {
                // e.Handled = true;
                return;
            }

            Customer cust = GetCustomerByTel(txtSmsReceiver.Text.Trim());
            if (cust.Idx < 1)
            {
                cust = new Customer() { Cellular = txtSmsReceiver.Text.Trim() };
                var items = dgSmsReceiverList.ItemsSource as Customers;
                if (items == null)
                {
                    Customers custs = new Customers();
                    custs.Add(cust);
                    dgSmsReceiverList.ItemsSource = custs;
                }
                else
                {
                    var item = items.FirstOrDefault(x => x.Cellular.Equals(txtSmsReceiver.Text.Trim()));
                    if (item != null)
                    {
                        txtSmsReceiver.Text = string.Empty;
                        return;
                    }

                    items.Add(cust);
                    // dgSmsReceiverList.ItemsSource = items;
                }
            }
            else
            {
                var items = dgSmsReceiverList.ItemsSource as Customers;
                if (items == null)
                {
                    Customers custs = new Customers();
                    custs.Add(cust);
                    dgSmsReceiverList.ItemsSource = custs;
                }
                else
                {
                    var item = items.FirstOrDefault(x => x.Cellular.Equals(txtSmsReceiver.Text.Trim()));
                    if (item != null)
                    {
                        txtSmsReceiver.Text = string.Empty;
                        return;
                    }

                    items.Add(cust);
                    // dgSmsReceiverList.ItemsSource = items;
                }
            }

            txtSmsReceiver.Text = string.Empty;
        }

        private void txtSmsReceiver_KeyDown(object sender, KeyEventArgs e)
        {
            if (string.IsNullOrEmpty(txtSmsReceiver.Text.Trim()))
            {
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Enter)
                btnAddSms_Click(btnAddSms, new RoutedEventArgs());
        }

        private void txtSmsReceiver_KeyUp(object sender, KeyEventArgs e)
        {
            //if (string.IsNullOrEmpty(txtSmsReceiver.Text.Trim()))
            //{
            //    e.Handled = true;
            //    return;
            //}

            //if (e.Key == Key.Enter)
            //    btnAddSms_Click(btnAddSms, new RoutedEventArgs());
        }

        private void txtSmsReceiver_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9^\r]+");
            bool chk = regex.IsMatch(e.Text);
            e.Handled = chk;

            if (e.Text.Equals("\r"))
            {
                if (string.IsNullOrEmpty(txtSmsReceiver.Text.Trim()))
                {
                    e.Handled = true;
                    return;
                }

                btnAddSms_Click(btnAddSms, new RoutedEventArgs());
            }
        }

        private void btnSendSms_Click(object sender, RoutedEventArgs e)
        {
            // SMS flyout, SEND SMS
            if (string.IsNullOrEmpty(tbSms.Text.Trim()))
            {
                e.Handled = true;
                return;
            }

            if (dgSmsReceiverList.Items.Count < 1)
            {
                e.Handled = true;
                return;
            }

            var items = dgSmsReceiverList.ItemsSource as Customers;
            owner.SendSms(items, tbSms.Text.Trim(), CoupleModeInfo.userid);
            flySms.IsOpen = false;
        }

        private void dgSmsReceiverList_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            // SMS Flyout datagrid
            DataGrid view = e.Source as DataGrid;
            Customer item = view.SelectedItem as Customer;

            if (item == null)
            {
                e.Handled = true;
                return;
            }
        }

        private void flySms_ClosingFinished(object sender, RoutedEventArgs e)
        {
            var customers = dgSmsReceiverList.ItemsSource;
            if (customers == null)
            {
                e.Handled =  true;

                tbSms.Text = string.Empty;
                dgSmsReceiverList.ItemsSource = null;
                txtCounter.Text = "0 / 80";
                btnAddSms.IsEnabled = true;
                btnSendSms.IsEnabled = true;
                dgSmsList.IsEnabled = true;
                tbSms.IsReadOnly = false;
                txtSmsReceiver.IsEnabled = true;

                return;
            }

            foreach (Customer item in customers)
            {
                item.IsChecked = false;
            }

            tbSms.Text = string.Empty;
            dgSmsReceiverList.ItemsSource = null;
            txtCounter.Text = "0 / 80";
            btnAddSms.IsEnabled = true;
            btnSendSms.IsEnabled = true;
            dgSmsList.IsEnabled = true;
            tbSms.IsReadOnly = false;
        }

        public Customer GetCustomerByIdx(int idx)
        {
            DataTable dt;
            Customer cust = null;

            using (FirebirdDBHelper db = new FirebirdDBHelper(util.GetFbDbStrConn()))
            {
                try
                {
                    db.SetParameters("@I_IDX", FbDbType.Integer, idx);
                    dt = db.GetDataTableSP("GET_CUSTOMER_BY_IDX");

                    foreach (DataRow row in dt.Rows)
                    {
                        cust = new Customer()
                        {
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
                        };
                    }
                }
                catch (FbException ex)
                {
                    util.WriteLog(ex.ErrorCode, ex.Message);
                }
            }

            return cust;
        }

        public Customer GetCustomerByTel(string tel)
        {
            DataTable dt;
            Customer cust = null;

            using (FirebirdDBHelper db = new FirebirdDBHelper(util.GetFbDbStrConn()))
            {
                try
                {
                    db.SetParameters("@I_IDX", FbDbType.VarChar, tel);
                    dt = db.GetDataTableSP("GET_CUSTOMER_BY_TEL");

                    foreach (DataRow row in dt.Rows)
                    {
                        cust = new Customer()
                        {
                            IsChecked = false,
                            IsSelected = false,
                            Idx = string.IsNullOrEmpty(row[0].ToString()) == false ? int.Parse(row[0].ToString()) : -1,
                            Group_Idx = string.IsNullOrEmpty(row[1].ToString()) == false ? int.Parse(row[1].ToString()) : 0,
                            Name = row[2].ToString(),
                            Company = row[3].ToString(),
                            Title = row[4].ToString(),
                            Tel = row[5].ToString(),
                            Cellular = row[6].ToString(),
                            Extension = row[7].ToString(),
                            Email = row[8].ToString(),
                            Addr = row[9].ToString(),
                            Etc = row[10].ToString()
                        };
                    }
                }
                catch (FbException ex)
                {
                    util.WriteLog(ex.ErrorCode, ex.Message);
                }
            }

            return cust;
        }

        public CallLists GetCallListByCustIdx(int cust_idx, string cust_tel)
        {
            DataTable dt;
            CallLists lists = new CallLists();

            using (FirebirdDBHelper db = new FirebirdDBHelper(util.GetFbDbStrConn()))
            {
                try
                {
                    db.SetParameters("@I_IDX", FbDbType.Integer, cust_idx);
                    db.SetParameters("@I_CUST_TEL", FbDbType.VarChar, cust_tel);

                    dt = db.GetDataTableSP("GET_CALL_LIST2");

                    foreach (DataRow row in dt.Rows)
                    {
                        lists.Add(new CallList()
                        {
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
                catch (FbException ex)
                {
                    util.WriteLog(ex.ErrorCode, ex.Message);
                }
            }

            return lists;
        }

        private void tvGroup_KeyUp(object sender, KeyEventArgs e)
        {
            // e.Handled = true;

            //if (e.Key == Key.Enter)
            //{
            //    var source = e.Source as WpfTreeViewInPlaceEditControl;
            //    var lists = source.ItemsSource as GroupLists;
            //    var selitem = source.SelectedItem as GroupList;

            //    try
            //    {
            //        lists.update(selitem);
            //        groupitems.Add(selitem);
            //        selitem.IsSelected = false;
            //    }
            //    catch (FbException ex)
            //    {
            //        e.Handled = true;
            //    }
            //}

            if (e.Key == Key.Delete)
            {
                var source = e.Source as WpfTreeViewInPlaceEditControl;
                var lists = source.ItemsSource as GroupLists;
                var selitem = source.SelectedItem as GroupList;

                if (selitem.Idx == 0)
                {
                    e.Handled = true;
                    return;
                }

                try
                {
                    int idx = lists[0].Children.IndexOf(selitem);
                    lists.remove(selitem);

                    try
                    {
                        lists[0].Children[idx].IsSelected = true;
                    }
                    catch (Exception ee)
                    {
                        lists[0].IsSelected = true;
                    }

                    // groupitems.Remove(selitem);
                    var item = groupitems.FirstOrDefault(x => x.Idx == selitem.Idx);
                    if (item == null)
                    {
                        //idx = groupitems.IndexOf(item);
                        //groupitems.Remove(selitem);
                    }
                    else
                    {
                        idx = groupitems.IndexOf(item);
                        groupitems = new ObservableCollection<GroupList>(glist.getlist());
                        groupitems.Insert(0, new GroupList() { Idx = 0, Name = Application.Current.FindResource("CMB_TXT_HEAD").ToString() });
                        cmbGroup.ItemsSource = null;
                        cmbGroup.ItemsSource = groupitems;

                        if (flyCustomer.IsOpen)
                        {
                            if (cmbGroup.Items.Count > 0)
                            {
                                cmbGroup.SelectedIndex = idx - 1;
                            }
                        }
                    }
                }
                catch (FbException ex)
                {
                    if (ex.ErrorCode == 335544466
                        && ex.Message.Contains("CUSTOMER")
                        && ex.Message.Contains("FK_CUSTOMER_1"))
                    {
                        e.Handled = true;
                        MessageBox.Show(Application.Current.FindResource("MSG_ERR_GROUP_REMOVE_EXIST_CUSTOMER").ToString(), Application.Current.FindResource("MSGBOX_TXT_TITLE").ToString());
                    }
                }
            }
            //if (e.Key == Key.Insert)
            //{
            //    var source = e.Source as WpfTreeViewInPlaceEditControl;
            //    var lists = source.ItemsSource as GroupLists;
            //    var selitem = source.SelectedItem as GroupList;
            //    selitem.IsSelected = false;

            //    GroupList list = new GroupList() { Idx = -1, Name = string.Empty, Children = new ObservableCollection<GroupList>(), IsSelected = true };
            //    glist.add(list);
            //}
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            var chkbox = sender as CheckBox;

            if (chkbox.IsChecked == true ? true : false)
            {
                foreach (Customer item in dgCustList.Items)
                {
                    item.IsChecked = true;
                }
            }
            else
            {
                foreach (Customer item in dgCustList.Items)
                {
                    item.IsChecked = false;
                }
            }
        }

        private void dgridCustCallList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // CALL LIST
            // 메모
            DataGrid view = (DataGrid)e.Source;
            CallList item = (CallList)view.SelectedItem;

            CallList tmp = new CallList() { IsSelected = item.IsSelected, IsChecked = item.IsChecked, Idx = item.Idx, Cust_Idx = item.Cust_Idx, Name = item.Name, Direction = item.Direction, Cust_Tel = item.Cust_Tel, Startdate = item.Startdate, Enddate = item.Enddate, Memo = item.Memo };

            flyCustMemo.DataContext = tmp;
            flyCustMemo.Header = string.Format(Application.Current.FindResource("PB_FLYOUT_TITLE_CUST_MEMO").ToString(), item.Cust_Tel);
            flyCustMemo.IsOpen = true;
        }

        private void tvGroup_CancelEditEvent(object sender, KeyEventArgs e)
        {
            var source = sender as WpfTreeViewInPlaceEditControl;
            var lists = source.ItemsSource as GroupLists;
            var selitem = source.SelectedItem as GroupList;

            try
            {
                if (selitem.Idx == -1)
                {
                    selitem.IsSelected = false;
                    lists[0].Children.Remove(selitem);
                }
            }
            catch (FbException ex)
            {
                e.Handled = true;
            }
        }

        private void tvGroup_CommitEditedTextEvent(object sender, KeyEventArgs e)
        {
            var source = sender as WpfTreeViewInPlaceEditControl;
            var lists = source.ItemsSource as GroupLists;
            var selitem = source.SelectedItem as GroupList;
            var newValue = e.Source as TextBox;

            GroupList olditem = new GroupList() { Idx = selitem.Idx, Name = selitem.Name, IsSelected = selitem.IsSelected, Children = new ObservableCollection<GroupList>(selitem.Children.ToList()) };

            selitem.Name = newValue.Text;

            try
            {
                lists.update(selitem);
                selitem.IsSelected = false;

                var item = groupitems.FirstOrDefault(x => x.Idx == selitem.Idx);
                if (item == null)
                {
                    groupitems.Add(selitem);
                }
                else
                {
                    int idx = groupitems.IndexOf(item);
                    groupitems = new ObservableCollection<GroupList>(glist.getlist());
                    groupitems.Insert(0, new GroupList() { Idx = 0, Name = Application.Current.FindResource("CMB_TXT_HEAD").ToString() });
                    cmbGroup.ItemsSource = null;
                    cmbGroup.ItemsSource = groupitems;

                    if (flyCustomer.IsOpen)
                    {
                        cmbGroup.SelectedIndex = idx;
                    }
                }
            }
            catch (FbException ex)
            {
                e.Handled = true;
                MessageBox.Show(Application.Current.FindResource("MSG_ERR_GROUP_UPDATE").ToString(), Application.Current.FindResource("MSGBOX_TXT_TITLE").ToString());
            }
        }

        private void tvGroup_DeleteItemEvent(object sender, KeyEventArgs e)
        {
            var source = e.Source as WpfTreeViewInPlaceEditControl;
            var lists = source.ItemsSource as GroupLists;
            var selitem = source.SelectedItem as GroupList;

            if (selitem.Idx == 0)
            {
                e.Handled = true;
                return;
            }

            try
            {
                int idx = lists[0].Children.IndexOf(selitem);
                lists.remove(selitem);

                try
                {
                    lists[0].Children[idx].IsSelected = true;
                }
                catch (Exception ee)
                {
                    lists[0].IsSelected = true;
                }

                // groupitems.Remove(selitem);
                var item = groupitems.FirstOrDefault(x => x.Idx == selitem.Idx);
                if (item == null)
                {
                    //idx = groupitems.IndexOf(item);
                    //groupitems.Remove(selitem);
                }
                else
                {
                    idx = groupitems.IndexOf(item);
                    groupitems = new ObservableCollection<GroupList>(glist.getlist());
                    groupitems.Insert(0, new GroupList() { Idx = 0, Name = Application.Current.FindResource("CMB_TXT_HEAD").ToString() });
                    cmbGroup.ItemsSource = null;
                    cmbGroup.ItemsSource = groupitems;

                    if (flyCustomer.IsOpen)
                    {
                        if (cmbGroup.Items.Count > 0)
                        {
                            cmbGroup.SelectedIndex = idx - 1;
                        }
                    }
                }
            }
            catch (FbException ex)
            {
                if (ex.ErrorCode == 335544466
                    && ex.Message.Contains("CUSTOMER")
                    && ex.Message.Contains("FK_CUSTOMER_1"))
                {
                    e.Handled = true;
                    MessageBox.Show(Application.Current.FindResource("MSG_ERR_GROUP_REMOVE_EXIST_CUSTOMER").ToString(), Application.Current.FindResource("MSGBOX_TXT_TITLE").ToString());
                }
            }
        }

        private void tvGroup_LostFocusItemEvent(object sender, RoutedEventArgs e)
        {
            var source = sender as WpfTreeViewInPlaceEditControl;
            var lists = source.ItemsSource as GroupLists;
            var selitem = source.SelectedItem as GroupList;

            if (string.IsNullOrEmpty(selitem.Name))
            {
                try
                {
                    if (selitem.Idx == -1)
                    {
                        selitem.IsSelected = false;
                        lists[0].Children.Remove(selitem);
                    }
                }
                catch (FbException ex)
                {
                    e.Handled = true;
                }
            }
            else
            {
                var newValue = e.Source as TextBox;

                GroupList olditem = new GroupList() { Idx = selitem.Idx, Name = selitem.Name, IsSelected = selitem.IsSelected, Children = new ObservableCollection<GroupList>(selitem.Children.ToList()) };

                selitem.Name = newValue.Text;

                try
                {
                    lists.update(selitem);
                    selitem.IsSelected = false;

                    var item = groupitems.FirstOrDefault(x => x.Idx == selitem.Idx);
                    if (item == null)
                    {
                        groupitems.Add(selitem);
                    }
                    else
                    {
                        int idx = groupitems.IndexOf(item);
                        groupitems = new ObservableCollection<GroupList>(glist.getlist());
                        groupitems.Insert(0, new GroupList() { Idx = 0, Name = Application.Current.FindResource("CMB_TXT_HEAD").ToString() });
                        cmbGroup.ItemsSource = null;
                        cmbGroup.ItemsSource = groupitems;

                        if (flyCustomer.IsOpen)
                        {
                            cmbGroup.SelectedIndex = idx;
                        }
                    }
                }
                catch (FbException ex)
                {
                    e.Handled = true;
                    MessageBox.Show(Application.Current.FindResource("MSG_ERR_GROUP_UPDATE").ToString(), Application.Current.FindResource("MSGBOX_TXT_TITLE").ToString());
                }
            }
        }

    }

    public enum CUSTOMER_STATE
    {
        NONE,
        ADD,
        MODIFY
    }
}
