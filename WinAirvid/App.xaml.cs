using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Deployment.Application;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace WinAirvid
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException +=
                         new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            DispatcherUnhandledException += App_DispatcherUnhandledException;
            base.OnStartup(e);

            CheckForShortcut();
        }

        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            ShowExceptionMsg(e.Exception);
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            ShowExceptionMsg(ex);            
        }

        void ShowExceptionMsg(Exception ex)
        {
            MessageBox.Show(ex.ToString(), "Uncaught Exception",
                            MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// This will create a Application Reference file on the users desktop
        /// if they do not already have one when the program is loaded.
        /// Check for them running the deployed version before doing this,
        /// so it doesn't kick it when you're running it from Visual Studio.
        /// </summary>
        static void CheckForShortcut()
        {
            if (System.Deployment.Application.ApplicationDeployment.IsNetworkDeployed)
            {
                ApplicationDeployment ad = ApplicationDeployment.CurrentDeployment;
                if (ad.IsFirstRun)  //first time user has run the app since installation or update
                {
                    Assembly code = Assembly.GetExecutingAssembly();
                    string company = string.Empty;
                    string description = string.Empty;
                    if (Attribute.IsDefined(code, typeof(AssemblyCompanyAttribute)))
                    {
                        AssemblyCompanyAttribute ascompany =
                            (AssemblyCompanyAttribute)Attribute.GetCustomAttribute(code,
                            typeof(AssemblyCompanyAttribute));
                        company = ascompany.Company;
                    }
                    if (Attribute.IsDefined(code, typeof(AssemblyDescriptionAttribute)))
                    {
                        AssemblyDescriptionAttribute asdescription =
                            (AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(code,
                            typeof(AssemblyDescriptionAttribute));
                        description = asdescription.Description;
                    }
                    if (company != string.Empty && description != string.Empty)
                    {
                        string desktopPath = string.Empty;
                        desktopPath = string.Concat(
                            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                            "\\", description, ".appref-ms");
                        string shortcutName = string.Empty;
                        shortcutName = string.Concat(
                            Environment.GetFolderPath(Environment.SpecialFolder.Programs),
                            "\\", company, "\\", description, ".appref-ms");
                        System.IO.File.Copy(shortcutName, desktopPath, true);
                    }
                }
            }
        }
    }
}
