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

using LuteScribe.Domain;
using LuteScribe.ViewModel.Services;
using System;
using System.Windows.Input;

namespace LuteScribe.ViewModel.Commands
{

    public class ShiftStaveFocusCommand : ICommand
    {

        // Member variables
        private readonly MainWindowViewModel _viewModel;

        public ShiftStaveFocusCommand(MainWindowViewModel viewModel)
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
            //FIXME - THIS COMMAND IS NOT CURRENTLY CALLED

            if (_viewModel.TabModel?.ActivePiece.SelectedItem?.SelectedItem == null) { return; }

            var staves = _viewModel.TabModel.ActivePiece.Staves;
            var stave = _viewModel.TabModel.ActivePiece.SelectedItem;
            var selectedChord = stave.SelectedItem;
            var staveIndex = staves.IndexOf(stave);
            var chordIndex = stave.Chords.IndexOf(selectedChord);
            var targetStave = default(Stave);
            var targetChord = default(Chord);

            var direction = long.Parse((string)parameter);

            if ((direction  > 0) && (staveIndex < staves.Count - 1))
            {
                //go down a stave
                targetStave = staves[staveIndex + 1];

            }
            else if ((direction < 0 ) && (staveIndex > 0))
            {
                //go up a stave
                targetStave = staves[staveIndex - 1];
            } else
            {
                targetStave = null;
            }

            if (targetStave != null)
            {
                _viewModel.SelectedItem = targetStave;

                if (targetStave.Chords.Count > 0)
                {
                    targetChord = (chordIndex < targetStave.Chords.Count - 1) ? targetStave.Chords[chordIndex] : targetStave.Chords[targetStave.Chords.Count - 1];

                    targetStave.SelectedItem = targetChord;
                }
            }

        }

    }
}
