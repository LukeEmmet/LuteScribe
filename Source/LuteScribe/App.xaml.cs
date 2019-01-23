using LuteScribe.Properties;
using LuteScribe.Singletons;
using System;
using System.IO;
using System.Windows;

namespace LuteScribe
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        
        private MainWindowViewModel _viewModel;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize main window and view model
            var mainWindow = new MainWindow();
            var viewModel = new MainWindowViewModel();

            _viewModel = viewModel;

            mainWindow.DataContext = viewModel;

            if (e.Args.Length == 1) 
            {
                var firstArg = e.Args[0];
                var loadPath = "";  //default is unset or invalid path which we wont try to load.

                //aids debugging - load sample data if it exists
                if (firstArg == "--load-previous")
                {
                    //option to try to load the most recently opened file
                    
                    var settings = new Settings();
                    if (settings.RecentFiles.Count > 0)
                    {
                        var lastPath = settings.RecentFiles[0];

                        //load previous data
                        if (File.Exists(lastPath))
                        {
                            loadPath = lastPath;
                        }
                    }
                    
                } else
                {
                    //path to load will be the one passed on command line
                    loadPath = firstArg;
                }

                if (loadPath.Length > 0)
                {
                    if (!File.Exists(loadPath))
                    {
                        MessageBox.Show("Error - passed path does not exist" + loadPath);
                    }
                    else
                    {

                        try
                        {
                            viewModel.OpenFile.Execute(loadPath);
                        }
                        catch (Exception err)
                        {
                            MessageBox.Show(string.Format("Cannot open {0}, reason: {1}", loadPath, err.Message));
                        }
                    }
                }
            }
            mainWindow.Show();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            MainWindow wnd = new MainWindow();
            if (e.Args.Length == 1)
                MessageBox.Show("Now opening file: \n\n" + e.Args[0]);
            wnd.Show();
        }





        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Session.Instance.Dispose();
        }
    }
}