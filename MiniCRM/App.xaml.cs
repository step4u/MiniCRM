using Com.Huen.Libs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace MiniCRM
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        void App_Startup(object sender, StartupEventArgs e)
        {
            string ui = string.Empty;
            string endian = string.Empty;

            try
            {
                for (int i = 0; i < e.Args.Length; i++)
                {
                    if (e.Args[i].Contains("-"))
                    {
                        switch (e.Args[i])
                        {
                            case "-provider":
                                ui = e.Args[i + 1];
                                switch(ui)
                                {
                                    case "KT":
                                    case "KCT":
                                        break;
                                    default:
                                        // MessageBox.Show(string.Format("{0} is not a compatible UI. Will work in KCT.", ui));
                                        ui = "KCT";
                                        break;
                                }
                                break;
                            case "-rendian":
                                endian = e.Args[i + 1];
                                switch (endian)
                                {
                                    case "true":
                                    case "t":
                                    case "T":
                                        util.IsRemoteLittleEndian = true;
                                        break;
                                    default:
                                        util.IsRemoteLittleEndian = false;
                                        break;
                                }
                                break;
                            default:
                                MessageBox.Show(string.Format("{0} is not a capable command.", e.Args[i]));
                                Environment.Exit(0);
                                break;
                        }
                    }
                }

                if (e.Args.Length == 0)
                {
                    ui = "KCT";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Environment.Exit(0);
            }
            
            //Properties["KT"] = (ResourceDictionary)Application.LoadComponent(new Uri("Dictionary-KT.xaml", UriKind.Relative));
            //Properties["KCT"] = (ResourceDictionary)Application.LoadComponent(new Uri("Dictionary-KCT.xaml", UriKind.Relative));

            var MergedDics = Application.Current.Resources.MergedDictionaries;
            MergedDics.Clear();

            MergedDics.Add((ResourceDictionary)Application.LoadComponent(new Uri("CommonDictionary.xaml", UriKind.Relative)));
            MergedDics.Add((ResourceDictionary)Application.LoadComponent(new Uri("Localization-ko_KR.xaml", UriKind.Relative)));
            MergedDics.Add((ResourceDictionary)Application.LoadComponent(new Uri(string.Format("Dictionary-{0}.xaml", ui), UriKind.Relative)));
            //Application.Current.Resources.MergedDictionaries.Add((ResourceDictionary)Application.LoadComponent(new Uri("Dictionary-KT.xaml", UriKind.Relative)));
        }
    }
}
