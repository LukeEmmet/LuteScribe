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
using LuteScribe.Domain;
using LuteScribe.ViewModel.Services;
using System.Windows;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace LuteScribe.ViewModel.Commands
{

    public class ShowHelpCommand : ICommand
    {

        // Member variables
        private readonly MainWindowViewModel _viewModel;

        public ShowHelpCommand(MainWindowViewModel viewModel)
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

            //TBD - would be nice to start a single instance of help - this will show multiple windows
            //maybe there is a canonical way to launch help from WPF app that achieves this?

            var helpFile = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory,
                 "..\\..\\..\\Help",
                 "LuteScribeHelp.chm");

            var start = new ProcessStartInfo("hh.exe", helpFile);
            var process = Process.Start(start);       

        }

    }
}
