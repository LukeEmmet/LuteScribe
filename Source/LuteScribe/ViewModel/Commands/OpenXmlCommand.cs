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
using LuteScribe.Serialization;
using System.Collections.ObjectModel;
using LuteScribe.ViewModel.Services;
using LuteScribe.Domain;
using Microsoft.Win32;
using System.IO;
using System.Xml;

namespace LuteScribe.ViewModel.Commands
{

    public class OpenXmlCommand : ICommand
    {

        // Member variables
        private readonly MainWindowViewModel _viewModel;

        public OpenXmlCommand(MainWindowViewModel viewModel)
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
            var lsmlPath = default(string);

            var suggestFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (passedPath != null)
            {
                lsmlPath = passedPath;
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
                openFileDialog.Filter = "LuteScribe files (*.lsml)|*.lsml|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == true)
                {
                    lsmlPath = openFileDialog.FileName;

                    openFile = true;
                }

            }

            if (openFile)
            {
                //var tabModel = XmlSerialization.ReadFromXmlFile<TabModel>(lsmlPath);
                var tabModel = XmlSerialization.LoadXML<TabModel>(GetLsml(lsmlPath).OuterXml);

                _viewModel.TabModel = tabModel;
                _viewModel.Path = Path.GetFullPath(lsmlPath);

                //the active section is the first one.
                _viewModel.TabModel.ActivePiece = _viewModel.TabModel.Pieces[0];
            }

        }

        XmlDocument GetLsml(string lsmlPath)
        {

            var dom = new XmlDocument();
            dom.Load(lsmlPath);

            //check to see if it is the early beta file format, that did not have multiple Pieces
            //in which case we wrap in a single piece and load that.
            if (dom.DocumentElement.SelectSingleNode(@"//TabModel/Pieces") == null)
            {
                //could check for other versions of LSML here and act accordingly
                var wrapperDom = new XmlDocument();
                wrapperDom.LoadXml("<TabModel Version=\"1.0\"><Pieces><Piece/></Pieces></TabModel>");
                wrapperDom.SelectSingleNode(@"//TabModel/Pieces/Piece").InnerXml = dom.DocumentElement.InnerXml;

                return wrapperDom;
            } else
            {
                return dom;
            }

        }

    }
}
