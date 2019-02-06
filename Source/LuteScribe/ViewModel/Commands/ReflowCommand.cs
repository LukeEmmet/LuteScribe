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
using System.Diagnostics;
using LuteScribe.ViewModel.Services;
using LuteScribe.View;

namespace LuteScribe.ViewModel.Commands
{

    public class ReflowCommand : ICommand
    {

        // Member variables
        private readonly MainWindowViewModel _viewModel;

        public ReflowCommand(MainWindowViewModel viewModel)
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

            using (new WaitCursor())
            {
                var splitPoint = _viewModel.StaveWrap;
                var pieceIndex = _viewModel.TabModel.Pieces.IndexOf(_viewModel.TabModel.ActivePiece);
                var modelClone = (TabModel)Cloner.GetClone(_viewModel.TabModel);

                var pieceClone = modelClone.Pieces[pieceIndex];

                var staves = pieceClone.Staves;

                _viewModel.History.BeginCompoundDo();

                _viewModel.RecordUndoSnapshot();


                pieceClone.ReflowStaves(_viewModel.StaveWrap);

                _viewModel.History.EndCompoundDo();


                //apply the updated clone, and restore the active piece
                _viewModel.TabModel = modelClone;
                _viewModel.TabModel.ActivePiece = _viewModel.TabModel.Pieces[pieceIndex];

                Debug.Print("Stave count at end: " + staves.Count);

                //update the pdf preview, but don't switch to it if we are not already there.
                var previewer = new PreviewPdfCommand(_viewModel);
                previewer.Execute(0);
            }

        }


    }
}
