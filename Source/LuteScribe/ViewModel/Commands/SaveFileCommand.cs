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
using LuteScribe.View;

namespace LuteScribe.ViewModel.Commands
{

    public class SaveFileCommand : ICommand
    {

        // Member variables
        private readonly MainWindowViewModel _viewModel;

        public SaveFileCommand(MainWindowViewModel viewModel)
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

        private bool IsUnsupportedFileSaveFormat(string path)
        {
            return ((Path.GetExtension(path) != ".lsml") && (Path.GetExtension(path) != ".tab"));
        }
        /// <summary>
        /// Invokes this command to perform its intended task. Pass parameter ="1" to force SaveAs dialog
        /// </summary>
        public void Execute(object parameter)
        {
            var appDir = System.AppDomain.CurrentDomain.BaseDirectory;
            var forceSaveAs = ((string)parameter == "1");
            var path = _viewModel.Path;
            var saveFile = false;

            if ((path == null) || IsUnsupportedFileSaveFormat(path) || forceSaveAs)
            {
                //if not lsml or tab, let the user know the file will be saved in TAB/LSML
                if (IsUnsupportedFileSaveFormat(path) && (path != null))
                {
                    MessageBox.Show(String.Format("LuteScribe cannot save in original file's format of \"{0}\". \n\n" +
                        "The tabulature will be saved in LSML or TAB format.", 
                        Path.GetExtension(path).ToUpper()));
                }

                //show save as dialog
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "" +
                    "LuteScribe files (*.lsml)|*.lsml|" +
                    "Tab files (*.tab)|*.tab";

                if (path != null)
                {
                    //filter by the current type
                    saveFileDialog.FilterIndex = (Path.GetExtension(path) == ".tab" ? 2 : 1);       //choose lsml unless tab already as default filter
                    saveFileDialog.FileName =  Path.GetFileNameWithoutExtension(path);
                    saveFileDialog.InitialDirectory = Path.GetDirectoryName(path);
                }

                if (saveFileDialog.ShowDialog() == true)
                {
                    path = saveFileDialog.FileName;
                    saveFile = true;
                }
            } else
            {
                saveFile = true;
            }

            if (saveFile) {
                var ext = Path.GetExtension(path).ToLower();
                try
                {
                    using (new WaitCursor())
                    {
                        switch (ext)
                        {
                            case ".lsml":
                                _viewModel.SaveXml.Execute(path);
                                _viewModel.Path = path; //update path on successful save
                                break;
                            case ".tab":
                                _viewModel.SaveTabModel.Execute(path);
                                _viewModel.Path = path; //update path on successful save
                                break;
                            default:
                                MessageBox.Show("Cannot save file of format: " + ext + " " + path);
                                break;

                        }
                    }

                }
                catch (Exception e)
                {
                    MessageBox.Show(String.Format("Cannot save file {0}, reason: {1}", path, e.Message));
                }
            }

        }
    }
}

