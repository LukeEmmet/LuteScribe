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
using System.Windows;
using System.Windows.Controls;

namespace LuteScribe.ViewModel.Commands
{

    public class OpenFileCommand : ICommand
    {

        // Member variables
        private readonly MainWindowViewModel _viewModel;

        public OpenFileCommand(MainWindowViewModel viewModel)
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

            var passedPath = (string)parameter;
            var suggestFile = default(string);
            var openFile = false;
            var path = default(string);

            var suggestFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (passedPath != null)
            {
                path = passedPath;
                openFile = true;
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

                var allFilesFilter = "Tabulature files (*.lsml,*.tab,*.ft2,*.ft3)|*.lsml;*.tab;*.ft2;*.ft3|";

                openFileDialog.Filter = "" +
                    "LuteScribe XML files (*.lsml)|*.lsml|" +
                    "Tab files (*.tab)|*.tab|" +
                    "Fronimo files (*.ft2,*.ft3)|*.ft2;*.ft3";

                //only offer to open fandango files if converter is enabled
                //since this is still experimental
                if (_viewModel.OpenFandango.CanExecute(null))               {
                    allFilesFilter = "Tabulature files (*.lsml,*.tab,*.ft2,*.ft3,*.jtz,*.jtxml)|*.lsml;*.tab;*.ft2;*.ft3;*.jtz;*.jtxml|";
                    openFileDialog.Filter += "|Fandango files (*.jtz,*.jtxml)|*.jtz;*.jtxml";
                }

                openFileDialog.Filter += "|All files (*.*)|*.*";

                openFileDialog.Filter = allFilesFilter + openFileDialog.Filter;


                if (openFileDialog.ShowDialog() == true)
                {
                    path = openFileDialog.FileName;

                    openFile = true;
                }

            }

            if (openFile)
            {
                var ext = Path.GetExtension(path).ToLower();

                _viewModel.HasMultipleSections = false;

                try
                {
                    switch (ext)
                {
                    case ".lsml":
                            _viewModel.OpenXml.Execute(path);
                            break;
                        case ".tab":
                            _viewModel.OpenTab.Execute(path);
                            break;
                        case ".ft2":
                            _viewModel.OpenFronimo.Execute(path);
                            break;
                        case ".ft3":
                            _viewModel.OpenFronimo.Execute(path);
                            break;
                        case ".jtz":
                            _viewModel.OpenFandango.Execute(path);
                            break;
                        case ".jtxml":
                            _viewModel.OpenFandango.Execute(path);
                            break;
                        default:
                            MessageBox.Show("Cannot open file of unknown format: " + ext + " " + path);
                            break;
                    }

                    _viewModel.ShowSections(0);
                    _viewModel.UpdateLastFiles(path);

                    const int pdfTab = 2;
                    const int stavesTab = 0;

                 

                    if (_viewModel.SelectedTab == stavesTab)
                    {
                        //leave view on staves tab
                    } else
                    {
                        //switch to pdf view tab
                        _viewModel.SelectedTab = pdfTab;
                        _viewModel.PreviewPdf.Execute(null);
                        
                    }

                    //reset any undo history
                    _viewModel.History.Clear();
                    _viewModel.RecordUndoSnapshot();

            }
                catch (Exception e)
            {
                MessageBox.Show(String.Format("Cannot open file {0}, reason: {1}", path, e.Message));
            }
        }


        }

    }
}
