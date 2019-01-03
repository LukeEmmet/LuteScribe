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

    public class NavigateSectionCommand : ICommand
    {

        // Member variables
        private readonly MainWindowViewModel _viewModel;

        public NavigateSectionCommand(MainWindowViewModel viewModel)
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
            parameter = (string)parameter;

            int nextSection = _viewModel.CurrentSection;

            switch (parameter)
            {
                case "previous":
                    nextSection = _viewModel.CurrentSection - 1;
                    break;
                case "next":
                    nextSection = _viewModel.CurrentSection + 1;
                    break;
                default:
                    int section;
                    var pagenum = int.TryParse((string)parameter, out section);
                    nextSection = section;
                    break;
            }

            if (nextSection >= _viewModel.TabModel.Pieces.Count)
            {
                MessageBox.Show("Currently at last section - no further sections.");
                nextSection = (_viewModel.TabModel.Pieces.Count - 1);    //keep at last
                return;
            }

            if (nextSection < 0)
            {
                MessageBox.Show("Currently at first section - no previous sections.");
                nextSection = 0;
                return;
            }


            if (nextSection != _viewModel.CurrentSection)
            {
                //reset undo history - we dont want to undo changes to previously selected piece
                _viewModel.History.Clear();

                //go to different section
                _viewModel.TabModel.ActivePiece = _viewModel.TabModel.Pieces[nextSection];

                //new undo snapshot from here
                _viewModel.RecordUndoSnapshot();


                _viewModel.CurrentSection = nextSection;
                _viewModel.CheckSectionMenu(nextSection);

                if (_viewModel.SelectedTab == 2)
                {
                    //currently preview is selected, so refresh it and switch to it
                    _viewModel.PreviewPdf.Execute(null);
                }

                try
                {
                    //goto page 1 in that section (might fail if none yet loaded)
                    _viewModel.NavigatePdfPage.Execute("first");
                } catch
                {
                    //ignore these at the moment...
                }
            }

        }

    }
}
