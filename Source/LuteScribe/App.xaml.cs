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
                var path = e.Args[0];

                if (!File.Exists(path))
                {
                    MessageBox.Show("Error - passed path does not exist" + path);
                }
                else
                {

                    try
                    {
                        viewModel.OpenFile.Execute(path);
                    } catch (Exception err)
                    {
                        MessageBox.Show(string.Format("Cannot open {0}, reason: {1}", path, err.Message));
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