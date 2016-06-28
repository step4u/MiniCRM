using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System;
using System.Linq;
using Com.Huen.Sockets;
using System.Windows.Threading;
using System.Diagnostics;
using Com.Huen.Libs;
using Com.Huen.DataModel;
using FirebirdSql.Data.FirebirdClient;
using System.Collections.ObjectModel;

namespace MiniCRM
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private double top = 0.0d;
        private double left = 0.0d;
        private bool startpopup = false;

        private UdpCoupleMode client = null;
        private string userdatapath = string.Empty;

        private CallList curCall;
        private string lastcallnumber = string.Empty;
        private CALL_STATES callstate = CALL_STATES.NONE;
        private BEHAVIOR_STATES behavoir = BEHAVIOR_STATES.NONE;
        private CONNECTED_MODE connectedmode = CONNECTED_MODE.NONE;
        private RTPRecorderCouple recorder = null;
        private bool IsRecording = false;
        private MinicrmButtonStates btnsatate;

        public bool StartPopup
        {
            get { return this.startpopup; }
            set { this.startpopup = value; }
        }

        public MainWindow()
        {
            InitializeComponent();
            ReadIni();
            Init();

            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            pb = new PhoneBook();
            pb.Owner = this;
            if (startpopup)
            {
                // pb.Show();

                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    SearchCondition1 con = new SearchCondition1();
                    con.StartDate = pb.sdate.Value;
                    con.EndDate = pb.edate.Value;
                    con.Number = string.IsNullOrEmpty(pb.txtNumber.Text.Trim()) == false ? string.Format("%{0}%", pb.txtNumber.Text.Trim()) : string.Empty;

                    if (pb.calls == null)
                    {
                        pb.calls = new CallLists(con);
                        pb.dgCallList.ItemsSource = pb.calls;
                    }

                    pb.tabs.SelectedIndex = 1;
                }));
            }

            //btnsatate = new MinicrmButtonStates() { RecordBtn = (Style)Application.Current.FindResource("btnREC_off") };
            //this.DataContext = btnsatate;

            btnsatate = this.DataContext as MinicrmButtonStates;
        }

        private void Init()
        {
            this.Top = top;
            this.Left = left;

            client = new UdpCoupleMode();
            client.CallInvitingEvent += Couplemodeclient_CallInvitingEvent;
            // client.CallProceedingEvent += Couplemodeclient_CallProceedingEvent;

            client.CallRingInEvent += Couplemodeclient_CallRingInEvent;
            client.CallRingOutEvent += Couplemodeclient_CallRingOutEvent;
            client.CallFobidenEvent += Couplemodeclient_CallFobidenEvent;
            client.CallConnectedEvent += Client_CallConnectedEvent;
            client.CallTerminatedEvent += Couplemodeclient_CallTerminatedEvent;
            client.ServerNotRespondEvent += Couplemodeclient_ServerNotRespondEvent;
            client.SocketErrorEvent += Couplemodeclient_SocketErrorEvent;
            
            client.RegSuccessEvent += Couplemodeclient_RegSuccessEvent;
            client.RegSuccessNatEvent += Couplemodeclient_RegSuccessNatEvent;
            client.UnRegSuccessEvent += Couplemodeclient_UnRegSuccessEvent;
            client.SmsSentInfoRequestedEvent += Client_SmsSentInfoRequestedEvent;
            client.SmsSentCompletedEvent += Client_SmsSentCompletedEvent;
            client.SmsRecievedRequestedEvent += Client_SmsRecievedRequestedEvent;
            client.MakeCallSuccessEvent += Client_MakeCallSuccessEvent;
            client.MakeCallFailEvent += Client_MakeCallFailEvent;
            client.DropCallSuccessEvent += Client_DropCallSuccessEvent;
            client.DropCallFailEvent += Client_DropCallFailEvent;
            client.PickupCallSuccessEvent += Client_PickupCallSuccessEvent;
            client.PickupCallFailEvent += Client_PickupCallFailEvent;
            client.TransferCallSuccessEvent += Client_TransferCallSuccessEvent;
            client.TransferCallFailEvent += Client_TransferCallFailEvent;
            client.HoldCallSuccessEvent += Client_HoldCallSuccessEvent;
            client.HoldCallFailEvent += Client_HoldCallFailEvent;
            client.ActiveCallSuccessEvent += Client_ActiveCallSuccessEvent;
            client.ActiveCallFailEvent += Client_ActiveCallFailEvent;
            client.EnableRecordRequestSuccessEvent += Client_EnableRecordRequestSuccessEvent;
            client.DisableRecordRequestSuccessEvent += Client_DisableRecordRequestSuccessEvent;
            client.EnableRecordRequestOnNatSuccessEvent += Client_EnableRecordRequestOnNatSuccessEvent;

            // client.Register();

            // string str = Application.Current.FindResource("MSG_REG_SUCCESS").ToString();
            // Application.Current.Resources.MergedDictionaries[1] = (ResourceDictionary)Application.LoadComponent(new Uri("Localization-en_US.xaml", UriKind.Relative));
            // Application.Current.Resources.MergedDictionaries[2] = (ResourceDictionary)Application.LoadComponent(new Uri("Dictionary-KCT.xaml", UriKind.Relative));
            // str = Application.Current.FindResource("MSG_REG_SUCCESS").ToString();
        }



        private void Client_MakeCallSuccessEvent(object obj, CommandMsg msg)
        {
            
        }

        private void Client_MakeCallFailEvent(object obj, CommandMsg msg)
        {

        }

        private void Client_DropCallSuccessEvent(object obj, CommandMsg msg)
        {

        }

        private void Client_DropCallFailEvent(object obj, CommandMsg msg)
        {

        }

        private void Client_EnableRecordRequestSuccessEvent(object obj, CommandMsg msg)
        {
            if (msg.status == USRSTRUCTS.STATUS_NAT_SUCCESS)
            {
                client.RecordStartRequestOnNat(curCall.Cust_Tel);
                // recorder = new RTPRecorderCouple(msg.port, CONNECTED_MODE.NAT);
            }
            else
            {
                // recorder = new RTPRecorderCouple(msg.port, CONNECTED_MODE.PUBLIC);
                recorder = new RTPRecorderCouple(CONNECTED_MODE.PUBLIC);
            }

            // recorder = new RTPRecorderCouple(msg.port, connectedmode);

            // client.RecordStartRequestOnNat(curCall.Cust_Tel);

            IsRecording = true;
            this.SetMessage(Application.Current.FindResource("ISRECORDING").ToString());
        }

        private void Client_DisableRecordRequestSuccessEvent(object obj, CommandMsg msg)
        {
            if (recorder != null)
            {
                recorder.Close();
                recorder.Dispose();
                recorder = null;
            }

            IsRecording = false;
            this.SetMessage(Application.Current.FindResource("ISNOTRECORDING").ToString());
        }

        private void Client_EnableRecordRequestOnNatSuccessEvent(object obj, CommandMsg msg)
        {
            recorder = new RTPRecorderCouple(21011, CONNECTED_MODE.NAT);

            IsRecording = true;
            this.SetMessage(Application.Current.FindResource("ISRECORDING").ToString());
        }

        private void Client_ActiveCallSuccessEvent(object obj, CommandMsg msg)
        {

        }

        private void Client_ActiveCallFailEvent(object obj, CommandMsg msg)
        {

        }

        private void Client_HoldCallSuccessEvent(object obj, CommandMsg msg)
        {

        }

        private void Client_HoldCallFailEvent(object obj, CommandMsg msg)
        {
            
        }

        private void Client_TransferCallSuccessEvent(object obj, CommandMsg msg)
        {

            behavoir = BEHAVIOR_STATES.NONE;
        }

        private void Client_TransferCallFailEvent(object obj, CommandMsg msg)
        {

        }

        private void Client_PickupCallSuccessEvent(object obj, CommandMsg msg)
        {
        }

        private void Client_PickupCallFailEvent(object obj, CommandMsg msg)
        {

        }

        private void Client_SmsRecievedRequestedEvent(object obj, sms_msg msg)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                if (pb.smslist != null)
                {
                    Sms item = new Sms()
                    {
                        Cust_Tel = msg.receiverphones,
                        Memo = msg.message
                    };

                    pb.smslist.add(item);
                    pb.tabs.SelectedIndex = 2;
                }
            }));
        }

        private void Client_SmsSentCompletedEvent(object obj, sms_msg msg)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                SearchCondition1 con = new SearchCondition1();
                con.StartDate = pb.sdate.Value;
                con.EndDate = pb.edate.Value;
                // Smses smslists = new Smses();
                if (pb.smslist == null)
                    pb.smslist = new Smses(con);

                Customer itm = smscustomers.FirstOrDefault(x => x.Cellular.Equals(msg.receiverphones));

                Sms item = new Sms()
                {
                    Cust_Name = itm.Name,
                    Cust_Tel = msg.receiverphones,
                    Memo = msg.message,
                    Regdate = DateTime.Now,
                    Result = msg.status
                };

                pb.smslist.add(item);
                pb.dgSmsList.ItemsSource = pb.smslist;

                smscustomers.Remove(itm);
                this.SendSms(null, smsmsg, smssender);
            }));
        }

        private void Client_SmsSentInfoRequestedEvent(object obj, sms_msg msg)
        {
            client.ResponseSmsInfoRequested(msg);

            //Smses smslists = new Smses();
            //Sms item = new Sms() {
            //    Cust_Tel = msg.receiverphones,
            //    Memo = msg.message
            //};

            //smslists.add(item);

            //if (pb.calls != null)
            //{
            //    pb.smslist.add();
            //}
        }

        private void Couplemodeclient_UnRegSuccessEvent(object obj, CommandMsg msg)
        {
            SetMessage(Application.Current.FindResource("MSG_UNREG_SUCCESS").ToString());
        }

        private void Couplemodeclient_RegSuccessNatEvent(object obj, CommandMsg msg)
        {
            if (!client.IsRegistered)
            {
                client.RegTimerInit();
                ConnectSuccess(msg);
                client.IsRegistered = true;
            }

            connectedmode = CONNECTED_MODE.NAT;
        }

        private void Couplemodeclient_RegSuccessEvent(object obj, CommandMsg msg)
        {
            if (!client.IsRegistered)
            {
                ConnectSuccess(msg);
                client.IsRegistered = true;
            }

            connectedmode = CONNECTED_MODE.PUBLIC;
        }

        private void ReadIni()
        {
            userdatapath = string.Format(@"{0}\{1}", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MiniCRM");

            Ini ini = new Ini(string.Format(@"{0}\{1}", userdatapath, "env.ini"));

            top = string.IsNullOrEmpty(ini.IniReadValue("MAIN", "TOP")) == false ? double.Parse(ini.IniReadValue("MAIN", "TOP")) : 0.0d;
            left = string.IsNullOrEmpty(ini.IniReadValue("MAIN", "LEFT")) == false ? double.Parse(ini.IniReadValue("MAIN", "LEFT")) : 0.0d;

            startpopup = string.IsNullOrEmpty(ini.IniReadValue("ETC", "STARTPOPUP")) == false ? bool.Parse(ini.IniReadValue("ETC", "STARTPOPUP").ToString()) : false;
        }

        private void SaveIni()
        {
            Ini ini = new Ini(string.Format(@"{0}\{1}", userdatapath, "env.ini"));

            ini.IniWriteValue("MAIN", "TOP", this.Top.ToString());
            ini.IniWriteValue("MAIN", "LEFT", this.Left.ToString());
        }

        private void Couplemodeclient_SocketErrorEvent(object obj, System.Net.Sockets.SocketException e)
        {
            util.WriteLog(e.ErrorCode, e.Message);
            SetMessage(Application.Current.FindResource("MSG_SOCK_ERROR").ToString());
        }

        private void Couplemodeclient_ServerNotRespondEvent(object obj, System.Net.Sockets.SocketException e)
        {
            util.WriteLog(e.ErrorCode, e.Message);
            SetMessage(Application.Current.FindResource("MSG_SOCK_TIMEOUT").ToString());
        }

        private void Couplemodeclient_CallTerminatedEvent(object obj, CommandMsg msg)
        {
            SetMessage(string.Empty);
            SetNumber(string.Empty);

            if (callstate == CALL_STATES.CONNECTED)
            {
                //CallLists calllist = new CallLists();
                //curCall.Enddate = DateTime.Now;
                //calllist.modify(curCall);

                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    if (pb.calls == null)
                        pb.calls = new CallLists();

                    curCall.Enddate = DateTime.Now;

                    try
                    {
                        pb.calls.modify(curCall);

                        CallList cl = pb.dgridCustCallList.SelectedItem as CallList;
                        if (cl == null)
                        {
                            var items = pb.dgridCustCallList.ItemsSource as CallLists;
                            cl = items.FirstOrDefault(x => x.Idx == curCall.Idx);
                        }

                        if (cl != null)
                            cl.Enddate = curCall.Enddate;

                        cl = pb.dgCallList.SelectedItem as CallList;
                        if (cl == null)
                        {
                            var items = pb.dgCallList.ItemsSource as CallLists;
                            cl = items.FirstOrDefault(x => x.Idx == curCall.Idx);
                        }

                        if (cl != null)
                            cl.Enddate = curCall.Enddate;
                    }
                    catch (FbException e)
                    {
                        util.WriteLog(e.ErrorCode, e.Message);
                    }
                }));
            }

            if (recorder != null)
            {
                recorder.Close();
                recorder.Dispose();
                recorder = null;

                IsRecording = false;
            }

            if (curCall != null) curCall = null;

            this.UIChanging(msg.status);
        }

        private void Couplemodeclient_CallFobidenEvent(object obj, CommandMsg msg)
        {
            
        }

        private void Client_CallConnectedEvent(object obj, CommandMsg msg)
        {
            if (curCall == null) return;

            if (curCall.Cust_Idx > 0)
            {
                SetMessage(string.Format(Application.Current.FindResource("MSG_CALL_STATES_CONNECTED2").ToString(), curCall.Name));
            }
            else
            {
                SetMessage(Application.Current.FindResource("MSG_CALL_STATES_CONNECTED").ToString());
            }

            this.UIChanging(msg.status);
        }

        private void Couplemodeclient_CallRingOutEvent(object obj, CommandMsg msg)
        {
            if (curCall != null) return;

            curCall = new CallList()
            {
                Direction = 0,
                Cust_Tel = msg.to_ext,
                Startdate = DateTime.Now,
                ext = msg.from_ext,
                to_ext = msg.to_ext
            };

            //CallLists calllists = new CallLists();
            //calllists.add(curCall);

            Customer cust = pb.GetCustomerByTel(msg.to_ext);

            string strmsg = string.Empty;
            if (cust.Group_Idx < 1)
            {
                strmsg = Application.Current.FindResource("MSG_CALL_OUT").ToString();
            }
            else
            {
                curCall.Cust_Idx = cust.Idx;
                curCall.Name = cust.Name;
                strmsg = string.Format(Application.Current.FindResource("MSG_CALL_OUT2").ToString(), cust.Name);
            }

            SetMessage(strmsg);
            SetNumber(msg.to_ext);

            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                if (startpopup)
                {
                    if (pb == null)
                    {
                        pb = new PhoneBook();
                        pb.Owner = this;
                    }

                    pb.Show();
                }

                SearchCondition1 con = new SearchCondition1();
                con.StartDate = pb.sdate.Value;
                con.EndDate = pb.edate.Value;
                con.Number = string.IsNullOrEmpty(pb.txtNumber.Text.Trim()) == false ? string.Format("%{0}%", pb.txtNumber.Text.Trim()) : string.Empty;

                if (pb.calls == null)
                {
                    pb.calls = new CallLists(con);
                    pb.dgCallList.ItemsSource = pb.calls;
                }

                pb.calls.add(curCall);

                if (pb.Visibility == Visibility.Collapsed)
                {
                    pb.tabs.SelectedIndex = 1;
                    pb.dgridCustCallList.ItemsSource = pb.GetCallListByCustIdx(cust.Idx, curCall.Cust_Tel);
                    pb.Visibility = Visibility.Visible;
                    pb.flyCustomer.Header = Application.Current.FindResource("PB_FLYOUT_TITLE_CUST_INFO").ToString();
                    pb.flyCustomer.IsOpen = true;
                }
                else
                {
                    pb.tabs.SelectedIndex = 1;
                    pb.dgridCustCallList.ItemsSource = pb.GetCallListByCustIdx(cust.Idx, curCall.Cust_Tel);
                    pb.flyCustomer.DataContext = cust;
                    pb.flyCustomer.Header = Application.Current.FindResource("PB_FLYOUT_TITLE_CUST_INFO").ToString();
                    pb.flyCustomer.IsOpen = true;
                }
            }));

            lastcallnumber = msg.to_ext;
            this.UIChanging(msg.status);
        }

        private void Couplemodeclient_CallRingInEvent(object obj, CommandMsg msg)
        {
            if (curCall != null) return;

            curCall = new CallList()
            {
                Direction = 1,
                Cust_Tel = msg.from_ext,
                Startdate = DateTime.Now,
                ext = msg.from_ext,
                to_ext = msg.to_ext
            };

            Customer cust = pb.GetCustomerByTel(msg.from_ext);

            string strmsg = string.Empty;
            if (cust.Idx < 1)
            {
                pb.CUSTOMERSTATE = CUSTOMER_STATE.ADD;
                cust.Cellular = msg.from_ext;
                strmsg = Application.Current.FindResource("MSG_CALL_IN").ToString();
            }
            else
            {
                pb.CUSTOMERSTATE = CUSTOMER_STATE.MODIFY;
                curCall.Cust_Idx = cust.Idx;
                curCall.Name = cust.Name;
                strmsg = string.Format(Application.Current.FindResource("MSG_CALL_IN2").ToString(), cust.Name);
            }

            SetMessage(strmsg);
            SetNumber(msg.from_ext);

            // CallLists calllists = new CallLists();
            // calllists.add(curCall);

            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                if (startpopup)
                {
                    if (pb == null)
                    {
                        pb = new PhoneBook();
                        pb.Owner = this;
                    }

                    // pb.Show();
                }

                SearchCondition1 con = new SearchCondition1();
                con.StartDate = pb.sdate.Value;
                con.EndDate = pb.edate.Value;
                con.Number = string.IsNullOrEmpty(pb.txtNumber.Text.Trim()) == false ? string.Format("%{0}%", pb.txtNumber.Text.Trim()) : string.Empty;

                if (pb.calls == null)
                {
                    pb.calls = new CallLists(con);
                    pb.dgCallList.ItemsSource = pb.calls;
                }

                pb.calls.add(curCall);

                if (pb.Visibility == Visibility.Collapsed || pb.Visibility == Visibility.Hidden)
                {
                    pb.tabs.SelectedIndex = 1;
                    pb.dgridCustCallList.ItemsSource = pb.GetCallListByCustIdx(cust.Idx, curCall.Cust_Tel);
                    pb.btnCustMemo.Visibility = Visibility.Visible;
                    pb.FlyCustomer = cust;
                    pb.flyCustomer.Header = Application.Current.FindResource("PB_FLYOUT_TITLE_CUST_INFO").ToString();
                    pb.flyCustomer.IsOpen = true;

                    if (startpopup)
                    {
                        pb.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    pb.tabs.SelectedIndex = 1;
                    pb.dgridCustCallList.ItemsSource = pb.GetCallListByCustIdx(cust.Idx, curCall.Cust_Tel);
                    pb.btnCustMemo.Visibility = Visibility.Visible;
                    pb.FlyCustomer = cust;
                    pb.flyCustomer.Header = Application.Current.FindResource("PB_FLYOUT_TITLE_CUST_INFO").ToString();

                    if (startpopup)
                    {
                        pb.flyCustomer.IsOpen = true;
                    }
                }
            }));

            this.UIChanging(msg.status);
        }

        private void Couplemodeclient_CallProceedingEvent(object obj, CommandMsg msg)
        {
            
        }

        private void Couplemodeclient_CallInvitingEvent(object obj, CommandMsg msg)
        {
            
        }

        public void MakeCall(string tel)
        {
            client.MakeCall(tel);
            behavoir = BEHAVIOR_STATES.MAKECALL;
        }

        public void DropCall(CallList curcall)
        {
            client.DropCall(curcall);
        }

        public void PickupCall(string tel)
        {
            client.PickupCall(tel);
        }

        public void TransferCall(string tel)
        {
            client.TransferCall(tel);
        }

        public void HoldCall(string tel)
        {
            client.HoldCall(tel);
        }

        public void ActiveCall(string tel)
        {
            client.ActiveCall(tel);
        }

        private ObservableCollection<Customer> smscustomers = null;
        private string smsmsg = string.Empty;
        private string smssender = string.Empty;
        public void SendSms(Customers items, string strmsg, string sender)
        {
            if (items != null)
            {
                smscustomers = new ObservableCollection<Customer>(items.ToList());
                smsmsg = strmsg;
                smssender = sender;
            }

            var item = smscustomers.FirstOrDefault<Customer>();
            if (item == null)
            {
                smscustomers = null;
                smsmsg = string.Empty;
                smssender = string.Empty;
            }
            else
            {
                client.SendSms(item, strmsg, sender);
            }
        }

        private void DialPad_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            string whatis = btn.Content.ToString();

            switch (whatis)
            {
                case "☎":
                    if (string.IsNullOrEmpty(txt_number.Text.Trim()))
                    {
                        txt_message.Text = Application.Current.FindResource("MSG_CALL_STATES_EMPTY_NUM").ToString();
                        e.Handled = true;
                        return;
                    }
                    else
                    {
                        txt_message.Text = string.Empty;
                    }

                    if (IsTransfer)
                        this.TransferCall(txt_number.Text.Trim());
                    else
                        this.MakeCall(txt_number.Text.Trim());

                    //if (behavoir == BEHAVIOR_STATES.TRANSFER)
                    //{
                    //    this.TransferCall(txt_number.Text.Trim());
                    //}
                    //else if (behavoir == BEHAVIOR_STATES.NONE || behavoir == BEHAVIOR_STATES.NORMAL)
                    //{
                    //    this.MakeCall(txt_number.Text.Trim());
                    //}
                    break;
                case "CLR":
                    SetNumber(string.Empty);
                    SetMessage(string.Empty);
                    break;
                case "☏":
                    if (curCall != null)
                        this.DropCall(curCall);
                    break;
                case "1":
                case "2":
                case "3":
                case "4":
                case "5":
                case "6":
                case "7":
                case "8":
                case "9":
                case "0":
                case "*":
                case "#":
                    txt_number.Text += whatis;
                    break;
            }

            this.ClearFocus(btn);
        }

        private void ClearFocus(DependencyObject obj)
        {
            var scope = FocusManager.GetFocusScope(obj); // elem is the UIElement to unfocus
            FocusManager.SetFocusedElement(scope, null); // remove logical focus
            Keyboard.ClearFocus(); // remove keyboard focus
        }

        private void mainWin_KeyUp(object sender, KeyEventArgs e)
        {
            ModifierKeys modifierkey = e.KeyboardDevice.Modifiers;
            Key key = (Key)e.Key;
            string stringkey = string.Empty;
            string txtdialnum = txt_number.Text;

            if (key == Key.Back)
            {
                if (string.IsNullOrEmpty(txt_number.Text))
                {
                    return;
                }

                txt_number.Text = txtdialnum.Substring(0, txtdialnum.Length - 1);
                return;
            }
            else if (key == Key.Delete)
            {
                txt_number.Text = string.Empty;
            }
            else if (key == Key.Enter)
            {
                if (string.IsNullOrEmpty(txt_number.Text.Trim()))
                {
                    txt_message.Text = Application.Current.FindResource("MSG_CALL_STATES_EMPTY_NUM").ToString();
                    e.Handled = true;
                    return;
                }
                else
                {
                    txt_message.Text = string.Empty;
                }

                if (IsTransfer)
                    this.TransferCall(txt_number.Text.Trim());
                else
                    this.MakeCall(txt_number.Text.Trim());

                //if (behavoir == BEHAVIOR_STATES.TRANSFER)
                //{
                //    this.TransferCall(txt_number.Text.Trim());
                //}
                //else if (behavoir == BEHAVIOR_STATES.NONE || behavoir == BEHAVIOR_STATES.NORMAL)
                //{
                //    this.MakeCall(txt_number.Text.Trim());
                //}

                return;
            }

            switch (key)
            {
                case Key.NumPad1:
                case Key.D1:
                    stringkey = "1";
                    break;
                case Key.NumPad2:
                case Key.D2:
                    stringkey = "2";
                    break;
                case Key.NumPad3:
                case Key.D3:
                    if (modifierkey == ModifierKeys.Shift)
                    {
                        stringkey = "#";
                    }
                    else
                    {
                        stringkey = "3";
                    }
                    break;
                case Key.NumPad4:
                case Key.D4:
                    stringkey = "4";
                    break;
                case Key.NumPad5:
                case Key.D5:
                    stringkey = "5";
                    break;
                case Key.NumPad6:
                case Key.D6:
                    stringkey = "6";
                    break;
                case Key.NumPad7:
                case Key.D7:
                    stringkey = "7";
                    break;
                case Key.NumPad8:
                case Key.D8:
                    if (modifierkey == ModifierKeys.Shift)
                    {
                        stringkey = "*";
                    }
                    else
                    {
                        stringkey = "8";
                    }
                    break;
                case Key.NumPad9:
                case Key.D9:
                    stringkey = "9";
                    break;
                case Key.NumPad0:
                case Key.D0:
                    stringkey = "0";
                    break;
                case Key.Multiply:
                    stringkey = "*";
                    break;
                case Key.OemComma:
                    stringkey = ",";
                    break;
            }

            txt_number.Text += stringkey;
        }

        private void mainWin_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            ModifierKeys modifierkey = e.KeyboardDevice.Modifiers;
            Key key = (Key)e.Key;
            string stringkey = string.Empty;
            string txtdialnum = txt_number.Text;

            if (key == Key.Back)
            {
                if (string.IsNullOrEmpty(txt_number.Text))
                {
                    return;
                }

                txt_number.Text = txtdialnum.Substring(0, txtdialnum.Length - 1);
                return;
            }
            else if (key == Key.Delete)
            {
                txt_number.Text = string.Empty;
            }
            else if (key == Key.Enter)
            {
                if (string.IsNullOrEmpty(txt_number.Text.Trim()))
                {
                    txt_message.Text = Application.Current.FindResource("MSG_CALL_STATES_EMPTY_NUM").ToString();
                    e.Handled = true;
                    return;
                }

                if (IsTransfer)
                    this.TransferCall(txt_number.Text.Trim());
                else
                    this.MakeCall(txt_number.Text.Trim());

                //if (behavoir == BEHAVIOR_STATES.TRANSFER)
                //{
                //    this.TransferCall(txt_number.Text.Trim());
                //}
                //else if (behavoir == BEHAVIOR_STATES.NONE || behavoir == BEHAVIOR_STATES.NORMAL)
                //{
                //    this.MakeCall(txt_number.Text.Trim());
                //}

                return;
            }

            switch (key)
            {
                case Key.NumPad1:
                case Key.D1:
                    stringkey = "1";
                    break;
                case Key.NumPad2:
                case Key.D2:
                    stringkey = "2";
                    break;
                case Key.NumPad3:
                case Key.D3:
                    if (modifierkey == ModifierKeys.Shift)
                    {
                        stringkey = "#";
                    }
                    else
                    {
                        stringkey = "3";
                    }
                    break;
                case Key.NumPad4:
                case Key.D4:
                    stringkey = "4";
                    break;
                case Key.NumPad5:
                case Key.D5:
                    stringkey = "5";
                    break;
                case Key.NumPad6:
                case Key.D6:
                    stringkey = "6";
                    break;
                case Key.NumPad7:
                case Key.D7:
                    stringkey = "7";
                    break;
                case Key.NumPad8:
                case Key.D8:
                    if (modifierkey == ModifierKeys.Shift)
                    {
                        stringkey = "*";
                    }
                    else
                    {
                        stringkey = "8";
                    }
                    break;
                case Key.NumPad9:
                case Key.D9:
                    stringkey = "9";
                    break;
                case Key.NumPad0:
                case Key.D0:
                    stringkey = "0";
                    break;
                case Key.Multiply:
                    stringkey = "*";
                    break;
                case Key.OemComma:
                    stringkey = ",";
                    break;
            }

            txt_number.Text += stringkey;
        }

        private void SetMessage(string str)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                txt_message.Text = str;
            }));
        }

        private void SetNumber(string str)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                txt_number.Text = str;
            }));
        }

        private void ConnectSuccess(CommandMsg msg)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                if (msg.status == 0 || msg.status == 99)
                {
                    this.Title = this.Title + " (" + msg.from_ext + ")";
                    img_antena.ToolTip = Application.Current.FindResource("TOOTIP_MAIN_ANTENA_ON");
                    img_antena.Style = (Style)Application.Current.FindResource("antena_on");
                }
                else
                {
                    this.Title = Application.Current.FindResource("MSG_MAIN_TITLE").ToString();
                    img_antena.ToolTip = Application.Current.FindResource("TOOTIP_MAIN_ANTENA_OFF");
                    img_antena.Style = (Style)Application.Current.FindResource("antena_off");
                }
            }));
        }

        private void btnRecFolder_Click(object sender, RoutedEventArgs e)
        {
            ProcessStartInfo psi = new ProcessStartInfo("explorer.exe");
            psi.WindowStyle = ProcessWindowStyle.Normal;
            psi.Arguments = util.GetRecordFolder();
            Process.Start(psi);

            // this.ClearFocus((Button)sender);
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            MainSettings settings = new MainSettings();
            settings.Owner = this;
            settings.Show();

            this.ClearFocus((Button)sender);
        }

        public PhoneBook pb = null;
        private void btnBooks_Click(object sender, RoutedEventArgs e)
        {
            if (pb == null)
            {
                pb = new PhoneBook();
                pb.Owner = this;
                pb.Show();
            }
            else
            {
                pb.Show();
            }
            pb.tabs.SelectedIndex = 0;

            this.ClearFocus((Button)sender);
        }

        private void btnSms_Click(object sender, RoutedEventArgs e)
        {
            pb.Show();
            pb.tabs.SelectedIndex = 2;
            pb.flySms.IsOpen = true;

            this.ClearFocus((Button)sender);
        }

        private void btn_callpull_Copy_Click(object sender, RoutedEventArgs e)
        {
            pb.Show();
            pb.tabs.SelectedIndex = 1;

            // this.ClearFocus((Button)sender);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;

            SaveIni();
            client.UnRegister();
            client.StopSocket();

            if (IsRecording)
            {
                if (recorder != null)
                {
                    recorder.Close();
                    recorder.Dispose();
                    recorder = null;
                }
            }

            if (pb != null)
                pb.SaveIni();

            e.Cancel = false;
        }

        private void Window_Closed(object sender, EventArgs e)
        {

        }

        private void btn_redial_Click(object sender, RoutedEventArgs e)
        {
            //if (string.IsNullOrEmpty(lastcallnumber))
            //{
            //    e.Handled = true;
            //    this.ClearFocus((Button)sender);
            //    return;
            //}
            SetNumber(lastcallnumber);
            this.MakeCall(lastcallnumber);

            // this.ClearFocus((Button)sender);
        }

        private bool IsTransfer = false;
        private string prevnum = string.Empty;
        private string prevmsg = string.Empty;
        private void btn_callpush_Click(object sender, RoutedEventArgs e)
        {
            if (IsTransfer)
            {
                txt_message.Text = prevmsg;
                txt_number.Text = prevnum;
                SetNumber(prevnum);
                SetMessage(prevmsg);
                IsTransfer = false;
            }
            else
            {
                prevmsg = txt_message.Text;
                prevnum = txt_number.Text;
                SetNumber(string.Empty);
                SetMessage(Application.Current.FindResource("MSG_CALL_STATES_READY_TRANSFER").ToString());
                IsTransfer = true;
            }

            // btnCall.Focus();
            // this.ClearFocus((Button)sender);
        }

        private void btn_callpull_Click(object sender, RoutedEventArgs e)
        {
            this.PickupCall("*98");

            // this.ClearFocus((Button)sender);
        }

        private bool IsHold = false;
        private void btnHold_Click(object sender, RoutedEventArgs e)
        {
            if (IsHold)
            {
                this.ActiveCall(curCall.Cust_Tel);
                IsHold = false;
            }
            else
            {
                this.HoldCall(curCall.Cust_Tel);
                IsHold = true;
            }

            this.ClearFocus((Button)sender);
        }

        private void btnRecord_Click(object sender, RoutedEventArgs e)
        {
            if (IsRecording)
                client.RecordStopRequest(curCall.Cust_Tel);
            else
                client.RecordStartRequest(curCall.Cust_Tel);

            IsRecording = !IsRecording;

            this.ClearFocus((Button)sender);
        }

        private void UIChanging(byte status)
        {
            // return;

            Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
            {
                switch (status)
                {
                    case USRSTRUCTS.STATUS_CALL_RINGING:

                        btnsatate.RedialBtn = (Style)Application.Current.FindResource("btnRedial_off");
                        btn_redial.IsEnabled = false;

                        btnsatate.PullBtn = (Style)Application.Current.FindResource("btnCallPull_off");
                        btn_callpull.IsEnabled = false;

                        callstate = CALL_STATES.RING;

                        break;
                    case USRSTRUCTS.STATUS_CALL_CONNECTED:

                        btnsatate.TransferBtn = (Style)Application.Current.FindResource("btnCallPush_down");
                        btn_callpush.IsEnabled = true;

                        btnsatate.RecordBtn = (Style)Application.Current.FindResource("btnREC_down");
                        btnRecord.IsEnabled = true;

                        callstate = CALL_STATES.CONNECTED;

                        break;
                    case USRSTRUCTS.STATUS_CALL_TERMINATED:
                        if (!string.IsNullOrEmpty(lastcallnumber))
                        {
                            btnsatate.RedialBtn = (Style)Application.Current.FindResource("btnRedial_down");
                            btn_redial.IsEnabled = true;
                        }

                        btnsatate.PullBtn = (Style)Application.Current.FindResource("btnCallPull_down");
                        btn_callpull.IsEnabled = true;

                        btnsatate.TransferBtn = (Style)Application.Current.FindResource("btnCallPush_off");
                        btn_callpush.IsEnabled = false;

                        btnsatate.RecordBtn = (Style)Application.Current.FindResource("btnREC_off");
                        btnRecord.IsEnabled = false;

                        curCall = null;
                        callstate = CALL_STATES.NONE;
                        IsTransfer = false;
                        IsHold = false;

                        break;
                }
            }));
        }
    }

    enum CALL_STATES
    {
        NONE,
        RING,
        CONNECTED
    }

    enum BEHAVIOR_STATES
    {
        NONE,
        NORMAL,
        TRANSFER,
        HOLD,
        ACTIVE,
        MAKECALL
    }
}
