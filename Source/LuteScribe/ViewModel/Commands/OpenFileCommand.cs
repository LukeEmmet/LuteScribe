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
using LuteScribe.Serialization;

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

            var appDir = System.AppDomain.CurrentDomain.BaseDirectory;
            var examplesPath = Path.GetFullPath(Path.Combine(appDir, "..\\..\\..\\SampleFiles"));

            var restoreLocation = true;

            if (passedPath == "..\\..\\..\\SampleFiles")
            {
                //passed in a folder to the examples
                passedPath = null;
                suggestFolder = examplesPath;
                restoreLocation = false;

            } else
            {
                if (_viewModel.Path != null)
                {
                    //user has existing content

                    //TBD confirm discard current session
                    suggestFolder = Path.GetDirectoryName(_viewModel.Path);

                }
            }


            if (passedPath != null)
            {
                path = passedPath;
                openFile = true;
            }
            else
            {
                

                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.FileName = suggestFile;
                openFileDialog.InitialDirectory = suggestFolder;
                openFileDialog.Multiselect = false;
                openFileDialog.RestoreDirectory = restoreLocation;

                var allFilesFilter = "" +
                    "Lute and guitar tabulature" +
                    "|" +
                    "*.lsml;" +
                    "*.tab;" +
                    "*.ft2;*.ft3;" +
                    "*.jtz;*.jtxml;" +
                    "*.abc;" +
                    "*.mei;" +
//                    "*.mnx;" +
                    "*.musicxml;*.mxl;" +
                     "*.tc" +
                   "|";

                openFileDialog.Filter = "" +
                    "LuteScribe XML files (*.lsml)|*.lsml|" +
                    "Tab files (*.tab)|*.tab|" +
                    "Fronimo files (*.ft2,*.ft3)|*.ft2;*.ft3|" +
                    "Fandango files (*.jtz,*.jtxml)|*.jtz;*.jtxml|" +
                    "ABC Tab files (*.abc)|*.abc|" +
                    "MEI files (*.mei)|*.mei|" +
                    //too experimental   at present - MNX spec not published yet.
                    //conversion will still work though
                    //"MNX files (*.mnx)|*.mnx|" +
                    "MusicXML files (*.musicxml,*.mxl)|*.musicxml;*.mxl|" + 
                    "TabCode files (*.tc)|*.tc|" +
                    "All files (*.*)|*.*";

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

                        default:


                            var loader = new LuteConvLoader();
                            var format = ext.Substring(1); //trim leading . to get type for luteconv
                            var pieces = loader.ListPieces(path, format);

                            var loadPieceIndex = 0;

                            if (pieces.Count > 1)
                            {
                                loadPieceIndex = _viewModel.PopupSubPieceListWindow(pieces);
                                if (loadPieceIndex < 0)
                                {
                                    return;     //user cancelled, just exit
                                }
                            }


                            var openLuteConv = _viewModel.OpenLuteConv;
                            openLuteConv.Execute(new Tuple<string, string, int>(path, format, loadPieceIndex));

                            break;

                    }

                    _viewModel.ShowSections(0);
                    _viewModel.UpdateLastFiles(path);



                    if (_viewModel.TabStavesSelected)
                    {
                        //leave view on staves tab
                    } else
                    {
                        //switch to pdf view tab
                        _viewModel.TabPDFSelected = true;
                        _viewModel.PreviewPdf.Execute(null);
                        
                    }

                    //reset any undo history
                    _viewModel.History.Clear();
                    _viewModel.RecordUndoSnapshot();

                    _viewModel.ToastNofify("Opened " + path, MainWindowViewModel.ToastMessageStyles.Success);


                }
                catch (Exception e)
            {
                MessageBox.Show(String.Format("Cannot open file {0}, reason: {1}", path, e.Message));
            }
        }


        }

    }
}
