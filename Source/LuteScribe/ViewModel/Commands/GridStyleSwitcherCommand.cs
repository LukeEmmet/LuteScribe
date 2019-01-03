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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace LuteScribe.ViewModel.Commands
{

    public class GridStyleSwitcherCommand : ICommand
    {

        // Member variables
        private readonly MainWindowViewModel _viewModel;

        public GridStyleSwitcherCommand(MainWindowViewModel viewModel)
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
            var find = default(string);
            var replace = default(string);

            if ((string) parameter == "N#")
            {
                find = "^#([1-5])$";
                replace = "$1#";
            } else
            {
                find = "^([1-5])#$";
                replace = "#$1";
            }

            var regex = new Regex(find);

            _viewModel.RecordUndoSnapshot();

            foreach (var stave in _viewModel.TabModel.ActivePiece.Staves)
            {
                foreach (var chord in stave.Chords)
                {
                    if (regex.IsMatch(chord.Flag))
                    {
                        chord.Flag = regex.Replace(chord.Flag, replace);
                    }
                }

            }
        }

    }
}
