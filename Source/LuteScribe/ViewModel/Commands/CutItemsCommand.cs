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

using LuteScribe.ViewModel.Services;
using System;
using System.Linq;
using System.Windows.Input;

namespace LuteScribe.ViewModel.Commands
{

    public class CutItemsCommand : ICommand
    {

        // Member variables
        private readonly MainWindowViewModel _viewModel;

        public CutItemsCommand(MainWindowViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        /// <summary>
        /// Whether this command can be executed.
        /// </summary>
        public bool CanExecute(object parameter)
        {
            return (_viewModel.TabModel?.ActivePiece.SelectedItem?.SelectedItem != null);

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
            if (_viewModel.TabModel?.ActivePiece.SelectedItem?.SelectedItem == null) { return; }

            var copyCommand = new CopyItemsCommand(_viewModel);
            var deleteCommand = new DeleteItemsCommand(_viewModel);

            //a cut is simply a copy followed by a delete
            //we dont record undo memo here as delete does it for us...
            copyCommand.Execute(parameter);
            deleteCommand.Execute(parameter);

            
        }

    }
}
