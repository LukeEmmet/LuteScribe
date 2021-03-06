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
using LuteScribe.Serialization.Commandline;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using LuteScribe.Singletons;

namespace LuteScribe.ViewModel.Commands
{

    public class CreatePsCommand : ICommand
    {

        // Member variables
        private readonly MainWindowViewModel _viewModel;

        public CreatePsCommand(MainWindowViewModel viewModel)
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
            var appDir = System.AppDomain.CurrentDomain.BaseDirectory;
            var tabPath = appDir + "..\\..\\..\\Tab\\bin\\tab.exe";
            var fontPath = "..\\..\\..\\Tab\\bin";      //workaround for TAB bug - pass a relative path, longer paths for font may cause tab to fail

            var userFile = _viewModel.Path;


            var guid = Guid.NewGuid();

            //Note that the current version of TAB (4.3.*) requires files to have an explicit .tab extension :-/
            //N.B. this path must not be too long, otherwise TAB will die
            var temp = Session.Instance.SessionPath + "\\" + "temp.tab";

            var outPath = (string)parameter;

            switch (outPath)
            {
                case ".":
                    //means save in same location as user file
                    if (_viewModel.Path == null)
                    {
                        MessageBox.Show("Please save the file first.");
                        return;
                    }
                    outPath = Path.GetDirectoryName(userFile) + "\\" + Path.GetFileNameWithoutExtension(userFile) + ".ps";
                    break;
                default:
                    break;      //do nothing

            }


            //save content, prettified for viewing
            _viewModel.SaveTabPiece.Execute(new Tuple<string, bool>(temp, true));

            userFile = temp;

            //tab can die with a malloc problem if the paths are too long
            //dont expand fontpath to full path (tab bug!)
            tabPath = Path.GetFullPath(tabPath);
            userFile = Path.GetFullPath(userFile);
            outPath = Path.GetFullPath(outPath);

            //use the -q flag to work correctly with taboutputparser
            var command = String.Format("\"{0}\" \"{1}\" -q -fontpath \"{2}\" -o \"{3}\"", tabPath, userFile, fontPath, outPath);

            var result = (new ExecuteProcess()).LoggedExecute(command);

            var outputParser = new TabOutputParser(result.Item1, result.Item2, result.Item3);

            if (outputParser.HasErrors)
            {
               _viewModel.ToastNofify("TAB error creating printable output: \n" + outputParser.Messages, MainWindowViewModel.ToastMessageStyles.Error);
            }
            else
            {
                //didnt match the regex and is zero exit code - should have worked ok
                //if saved alongside user file give feedback of save
                if ((string)parameter == ".")
                {
                    MessageBox.Show("Success: content saved to: " + outPath);
                }

            }

        }

    }
}
