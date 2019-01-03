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
using System.Windows;
using System.Windows.Controls;

namespace LuteScribe.ViewModel.Commands
{

    public class NewFileCommand : ICommand
    {

        // Member variables
        private readonly MainWindowViewModel _viewModel;

        public NewFileCommand(MainWindowViewModel viewModel)
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
            var model = new TabModel();

            _viewModel.TabModel = model;
            _viewModel.Path = null;

            var newSection = new Piece();
            model.Pieces.Add(newSection);
            model.ActivePiece = newSection;

            var section = model.ActivePiece;
            var stave = new Stave();
            var chords = stave.Chords;
            section.Staves.Add(stave);

            for (int n = 0; n < _viewModel.StaveWrap - 1; n++)
            {
                chords.Add(new Chord(" , , , , , , , ".Split(',')));
            }
            chords.Add(new Chord("e, , , , , , , ".Split(',')));

            section.Headers.Add(new Header("{Untitled}"));


            //show first section
            _viewModel.ShowSections(0);

            //show the staves edit tab
            var tab = (TabItem)Application.Current.MainWindow.FindName("stavesTab");
            tab.IsSelected = true;

            //reset history on this file
            _viewModel.History.Clear();
            _viewModel.RecordUndoSnapshot();

        }

    }
}
