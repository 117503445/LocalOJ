using System;
using System.Windows;
namespace LocalOJ
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public static string Path_root { get; } = AppDomain.CurrentDomain.BaseDirectory;
        public static string Path_File { get; } = Path_root + @"File\";

        private void Application_Startup(object sender, StartupEventArgs e)
        {
#if true
            TLib.Software.WPF_ExpectionHandler.HandleExpection(Current, AppDomain.CurrentDomain);
#endif
        }
    }
}
