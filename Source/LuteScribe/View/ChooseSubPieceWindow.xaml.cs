using LuteScribe.ViewModel;
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
    /// Interaction logic for ChoosePieceWindow.xaml
    /// </summary>
    public partial class ChoosePieceWindow : Window
    {
        public ChoosePieceWindow()
        {
            InitializeComponent();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            //unselect any chosen item
            (DataContext as ChooseSubPiecesViewModel).Selected = null;

            Close();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            //just close the window - the selected item is assigned
            Close();

        }

        private void SubPieces_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //just close the window - the selected item is assigned
            Close();

        }
    }
}
