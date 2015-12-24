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
            Properties["KT"] = (ResourceDictionary)Application.LoadComponent(new Uri("Dictionary-KT.xaml", UriKind.Relative));
            Properties["KCT"] = (ResourceDictionary)Application.LoadComponent(new Uri("Dictionary-KCT.xaml", UriKind.Relative));
        }
    }
}
