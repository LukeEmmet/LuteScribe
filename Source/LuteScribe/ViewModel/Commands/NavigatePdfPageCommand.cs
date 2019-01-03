//===================================================
//    LuteScribe, a GUI tool to view and edit lute tabulature 
//    (and for related plucked instruments).

//    Copyright (C) 2018, Luke Emmet 

//    Email: luke [dot] emmet [at] orlando-lutes [dot] com

//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.

//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.

//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <https://www.gnu.org/licenses/>.
//===================================================


using System;
using System.Windows.Input;
using System.Windows;

namespace LuteScribe.ViewModel.Commands
{

    public class NavigatePdfPageCommand : ICommand
    {

        // Member variables
        private readonly MainWindowViewModel _viewModel;

        public NavigatePdfPageCommand(MainWindowViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        /// <summary>
        /// Whether this command can be executed.
        /// </summary>
        public bool CanExecute(object parameter)
        {
            return true;
        }

        /// <summary>
        /// Fires when the CanExecute status of this command changes.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Invokes this command to perform its intended task.
        /// </summary>
        public void Execute(object parameter)
        {
            var command = (string)parameter;

            var moonPdf = (MoonPdfLib.MoonPdfPanel)Application.Current.MainWindow.FindName("MoonPdf");
            switch (command)
            {
                case "first":
                    moonPdf.GotoFirstPage();
                    break;
                case "previous":
                    moonPdf.GotoPreviousPage();
                    break;
                case "next":
                    moonPdf.GotoNextPage();
                    break;
                case "last":
                    moonPdf.GotoLastPage();
                    break;
                case "zoomin":
                    moonPdf.ZoomIn();
                    break;
                case "zoomout":
                    moonPdf.ZoomOut();
                    break;
                default:
                    var page = (int)parameter;
                    moonPdf.GotoPage(page);
                    break;
            }

            _viewModel.PdfViewSettings.MoonPdfPanel = moonPdf;

        }


    }
}
