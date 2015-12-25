using System;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using Com.Huen.Sockets;
using Com.Huen.Libs;
using Com.Huen.DataModel;
using Com.Huen.Sql;
using FirebirdSql.Data.FirebirdClient;
using System.ServiceProcess;

namespace MiniCRM
{
    /// <summary>
    /// Interaction logic for Logon.xaml
    /// </summary>
    public partial class Logon : Window
    {
        private string userdatapath = string.Empty;
        private bool saveid = false;
        private bool autologon = false;
        private double top = 0.0d;
        private double left = 0.0d;

        public Logon()
        {
            InitializeComponent();

            this.Loaded += Logon_Loaded;
            this.KeyUp += Logon_KeyUp;

            ReadIni();
            InitWindow();
        }

        private void Logon_Loaded(object sender, RoutedEventArgs e)
        {
            ServiceController service;
            try
            {
                service = new ServiceController("FirebirdServerDefaultInstance");
                if (service.Status  == ServiceControllerStatus.Stopped)
                {
                    service.Start();
                }
            }
            catch (InvalidOperationException ex)
            {
                util.WriteLog(ex.Message);
                MessageBox.Show(Application.Current.FindResource("MSG_LOGON_SERVICE_NOTINSTALLED").ToString(), "MiniCRM");
                this.Close();
                return;
            }

            //FirebirdDBHelper db;
            //try
            //{
            //    db = new FirebirdDBHelper(util.GetFbDbStrConn());
            //}
            //catch (FbException ex)
            //{
            //    if (ex.ErrorCode == 335544721)
            //    {

            //    }
            //}
        }

        private void Logon_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
                this.Close();
        }

        private void InitWindow()
        {
            this.Top = top;
            this.Left = left;

            chksaveid.IsChecked = saveid;
            chkautologon.IsChecked = autologon;

            if (saveid)
            {
                txtid.Text = CoupleModeInfo.userid;
                txtpwd.Focus();
            }
            else
            {
                txtid.Focus();
            }
        }

        private void ReadIni()
        {
            userdatapath = string.Format(@"{0}\{1}", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MiniCRM");

            Ini ini = new Ini(string.Format(@"{0}\{1}", userdatapath, "env.ini"));

            CoupleModeInfo.pbxipaddress = ini.IniReadValue("PBX", "IP");
            CoupleModeInfo.pbxport = string.IsNullOrEmpty(ini.IniReadValue("PBX", "PORT")) == false ? int.Parse(ini.IniReadValue("PBX", "PORT")) : 31001;
            saveid = string.IsNullOrEmpty(ini.IniReadValue("LOGON", "SAVEID")) == false ? bool.Parse(ini.IniReadValue("LOGON", "SAVEID")) : false;
            autologon = string.IsNullOrEmpty(ini.IniReadValue("LOGON", "AUTOLOGON")) == false ? bool.Parse(ini.IniReadValue("LOGON", "AUTOLOGON")) : false;
            top = string.IsNullOrEmpty(ini.IniReadValue("LOGON", "TOP")) == false ? double.Parse(ini.IniReadValue("LOGON", "TOP")) : 0.0d;
            left = string.IsNullOrEmpty(ini.IniReadValue("LOGON", "LEFT")) == false ? double.Parse(ini.IniReadValue("LOGON", "LEFT")) : 0.0d;

            CoupleModeInfo.userid = ini.IniReadValue("LOGON", "ID");
        }

        private void SaveIni(string path)
        {
            Ini ini = new Ini(string.Format(@"{0}\{1}", path, "env.ini"));

            if (chksaveid.IsChecked == true ? true : false) ini.IniWriteValue("LOGON", "ID", txtid.Text);
            ini.IniWriteValue("LOGON", "SAVEID", chksaveid.IsChecked.ToString());
            ini.IniWriteValue("LOGON", "AUTOLOGON", chkautologon.IsChecked.ToString());
            ini.IniWriteValue("LOGON", "TOP", this.Top.ToString());
            ini.IniWriteValue("LOGON", "LEFT", this.Left.ToString());
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(CoupleModeInfo.pbxipaddress))
            {
                MessageBox.Show(Application.Current.FindResource("MSG_SETTINGS_PBX_IPADDRESS_EMPTY0").ToString());
                txtid.Focus();
                return;
            }

            if (CoupleModeInfo.pbxport < 31001)
            {
                MessageBox.Show(Application.Current.FindResource("MSG_SETTINGS_PBX_PORT_EMPTY0").ToString());
                txtid.Focus();
                return;
            }

            if (string.IsNullOrEmpty(txtid.Text.Trim()))
            {
                MessageBox.Show(Application.Current.FindResource("MSG_LOGON_EMPTYID").ToString());
                txtid.Focus();
                return;
            }
            if (string.IsNullOrEmpty(txtpwd.Password))
            {
                MessageBox.Show(Application.Current.FindResource("MSG_LOGON_EMPTYPWD").ToString());
                txtpwd.Focus();
                return;
            }

            byte[] buffer = null;
            try
            {
                IPEndPoint remoteIpEp = new IPEndPoint(IPAddress.Parse(CoupleModeInfo.pbxipaddress), 31005);
                UdpClient client = new UdpClient();
                client.Client.ReceiveTimeout = USRSTRUCTS.UDP_TIMEOUT;
                client.Connect(remoteIpEp);

                SoftphoneCommand_t author = new SoftphoneCommand_t()
                {
                    cmd = USRSTRUCTS.SOFTPHONE_CMD_EXTENSION_REQ,
                    UserId = txtid.Text,
                    Password = txtpwd.Password
                };

                buffer = util.GetBytes(author);

                int len = client.Send(buffer, buffer.Length);

                IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 0);
                buffer = client.Receive(ref ipep);
                client.Close();
            }
            catch (SocketException se)
            {
                if (se.SocketErrorCode == SocketError.TimedOut)
                {
                    MessageBox.Show(Application.Current.FindResource("MSG_SOCK_TIMEOUT").ToString());
                    return;
                }
                else
                {
                    // LOG 저장
                }
            }

            SoftphoneResponse_t authurized = util.GetObject<SoftphoneResponse_t>(buffer);

            if (authurized.extension == 0)
            {
                MessageBox.Show(Application.Current.FindResource("MSG_LOGON_FAIL").ToString());
                txtpwd.Password = string.Empty;
                txtid.Focus();
                txtid.SelectAll();
            }
            else
            {
                CoupleModeInfo.userid = txtid.Text.Trim();

                MainWindow mainwin = new MainWindow();
                mainwin.Show();
                this.Close();
            }
        }

        private void btnQuit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            Setttins settings = new Setttins();
            settings.Owner = this;
            settings.Show();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveIni(userdatapath);
        }

        private void txtpwd_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                this.btnOK_Click(this, e);
            }
        }
    }
}
