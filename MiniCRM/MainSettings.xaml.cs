using Com.Huen.Libs;
using System;
using System.Windows;
using System.IO;

namespace MiniCRM
{
    /// <summary>
    /// Interaction logic for MainSettings.xaml
    /// </summary>
    public partial class MainSettings : Window
    {
        private string path = string.Empty;
        private string pbxip = string.Empty;
        private string pbxport = string.Empty;
        private bool startpopup = false;

        public MainSettings()
        {
            InitializeComponent();
            InitWindow();
        }

        private void InitWindow()
        {
            ReadIni();
        }

        private void ReadIni()
        {
            path = string.Format(@"{0}\{1}", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MiniCRM");
            Ini ini = new Ini(string.Format(@"{0}\{1}", path, "env.ini"));

            pbxip = ini.IniReadValue("PBX", "IP");
            pbxport = ini.IniReadValue("PBX", "PORT");
            startpopup = string.IsNullOrEmpty(ini.IniReadValue("ETC", "STARTPOPUP")) == false ? bool.Parse(ini.IniReadValue("ETC", "STARTPOPUP").ToString()) : false;

            chbPopup.IsChecked = startpopup;
            txtIpAddr.Text = pbxip;
            txtPort.Text = pbxport;
            txtIpAddr.Focus();
        }

        private void SaveIni(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            Ini ini = new Ini(string.Format(@"{0}\{1}", path, "env.ini"));

            ini.IniWriteValue("PBX", "IP", txtIpAddr.Text.Trim());
            ini.IniWriteValue("PBX", "PORT", txtPort.Text.Trim());
            ini.IniWriteValue("ETC", "STARTPOPUP", chbPopup.IsChecked.ToString());
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtIpAddr.Text.Trim()))
            {
                MessageBox.Show(Application.Current.FindResource("MSG_SETTINGS_PBX_IPADDRESS_EMPTY").ToString());
                txtIpAddr.Focus();
                return;
            }
            if (string.IsNullOrEmpty(txtPort.Text.Trim()))
            {
                MessageBox.Show(Application.Current.FindResource("MSG_SETTINGS_PBX_PORT_EMPTY").ToString());
                txtPort.Focus();
                return;
            }

            SaveIni(path);
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
