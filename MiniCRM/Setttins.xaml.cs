using System;
using System.IO;
using System.Windows;
using Com.Huen.Libs;
using Com.Huen.DataModel;

namespace MiniCRM
{
    /// <summary>
    /// Interaction logic for Setttins.xaml
    /// </summary>
    public partial class Setttins : Window
    {
        private string userdatapath = string.Empty;
        private string pbxip = string.Empty;
        private string pbxport = string.Empty;

        public Setttins()
        {
            InitializeComponent();

            InitWindow();
        }

        private void InitWindow()
        {
            userdatapath = string.Format(@"{0}\{1}", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MiniCRM");
            ReadIni(userdatapath);
        }

        private void ReadIni(string path)
        {
            Ini ini = new Ini(string.Format(@"{0}\{1}", path, "env.ini"));

            pbxip = ini.IniReadValue("PBX", "IP");
            pbxport = ini.IniReadValue("PBX", "PORT");

            txtIPAddress.Text = pbxip;
            txtPort.Text = pbxport;
            txtIPAddress.Focus();
        }

        private void SaveIni(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            Ini ini = new Ini(string.Format(@"{0}\{1}", path, "env.ini"));

            ini.IniWriteValue("PBX", "IP", txtIPAddress.Text.Trim());
            ini.IniWriteValue("PBX", "PORT", txtPort.Text.Trim());

            CoupleModeInfo.pbxipaddress = txtIPAddress.Text.Trim();
            CoupleModeInfo.pbxport = string.IsNullOrEmpty(txtPort.Text.Trim()) == false ? int.Parse(txtPort.Text.Trim()) : 31001;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtIPAddress.Text.Trim()))
            {
                MessageBox.Show(Application.Current.FindResource("MSG_SETTINGS_PBX_IPADDRESS_EMPTY").ToString());
                txtIPAddress.Focus();
                return;
            }
            if (string.IsNullOrEmpty(txtPort.Text.Trim()))
            {
                MessageBox.Show(Application.Current.FindResource("MSG_SETTINGS_PBX_PORT_EMPTY").ToString());
                txtPort.Focus();
                return;
            }

            if (string.IsNullOrEmpty(CoupleModeInfo.pbxipaddress))
                CoupleModeInfo.pbxipaddress = txtIPAddress.Text.Trim();

            if (!CoupleModeInfo.pbxport.ToString().Equals(txtPort.Text.Trim()))
                CoupleModeInfo.pbxport = int.Parse(txtPort.Text.Trim());

            SaveIni(userdatapath);
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Logon parent = (Logon)this.Owner;
            parent.txtid.Focus();
        }
    }
}
