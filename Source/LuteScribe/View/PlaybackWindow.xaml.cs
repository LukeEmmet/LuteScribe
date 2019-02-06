using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LuteScribe.View
{
    /// <summary>
    /// Interaction logic for PlaybackWindow.xaml
    /// </summary>
    public partial class PlaybackWindow : Window
    {
        public PlaybackWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var view = (PlaybackWindowViewModel)DataContext;

            view.Stop();
            view.Dispose();
            
        }
    }
}
