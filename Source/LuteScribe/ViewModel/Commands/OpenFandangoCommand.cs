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
using Microsoft.Win32;
using System.IO;
using LuteScribe.Serialization.JtxmlLoader;

namespace LuteScribe.ViewModel.Commands
{

    public class OpenFandangoCommand : ICommand
    {

        // Member variables
        private readonly MainWindowViewModel _viewModel;

        public OpenFandangoCommand(MainWindowViewModel viewModel)
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
            var suggestFile = default(string);
            var suggestFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var passedPath = (string)parameter;
            var openPath = false;
            var tabPath = default(string);

            if (passedPath != null)
            {
                tabPath = passedPath;
                openPath = true;
            }
            else
            {
                if (_viewModel.Path != null)
                {
                    //user has existing content

                    //TBD confirm discard current session
                    suggestFolder = Path.GetDirectoryName(_viewModel.Path);

                }

                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.FileName = suggestFile;
                openFileDialog.InitialDirectory = suggestFolder;
                openFileDialog.Multiselect = false;
                openFileDialog.Filter = "Fandango files (*.jtz,jtxml)|*.jtz;*.jtxml|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == true)
                {

                    tabPath = openFileDialog.FileName;
                    openPath = true;
                }
            }

            if (openPath)
            {
                var loader = new JtxmlToLsml();

                _viewModel.TabModel = loader.LoadJtxml(tabPath);
                _viewModel.Path = Path.GetFullPath(tabPath);

                _viewModel.TabModel.ActivePiece = _viewModel.TabModel.Pieces[0];

            }

        }

        private void DeleteFile(string path)
        {
            //temp path might be readonly - set to normal first - http://stackoverflow.com/questions/265896/how-do-i-delete-a-read-only-file
            File.SetAttributes(path, FileAttributes.Normal);
            File.Delete(path);
        }


    }


}
