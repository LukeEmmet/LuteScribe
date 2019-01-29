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
using System.IO;
using LuteScribe.Singletons;
using System.Collections.Generic;
using LuteScribe.Domain;
using LuteScribe.Serialization.Commandline;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace LuteScribe.ViewModel.Commands
{

    public class PlayPieceCommand : ICommand
    {

        // Member variables
        private readonly MainWindowViewModel _viewModel;

        public PlayPieceCommand(MainWindowViewModel viewModel)
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

       
        public void Execute(object parameter)
        {
            var appDir = System.AppDomain.CurrentDomain.BaseDirectory;
            var tabPath = Path.GetFullPath(Path.Combine(appDir, "..\\..\\..\\Tab\\bin\\tab.exe"));
            var fontPath = Path.GetFullPath(Path.Combine(appDir, "..\\..\\..\\Tab\\bin"));

            var findErrors = "tab 4\\.3\\.89 copyright 1995-2010 by Wayne Cripps(\\r\\n.+)";

            var guid = Guid.NewGuid();

            var patch = (string)parameter;

            SimpleLogger.Instance.Log("Saving MIDI of current piece");

            //create options but we dont need to set anything
            var options = new PrettyOptions();

            //extra headers so we can control midi patch of output
            var extraHeaders = new List<Header>();

            extraHeaders.Add(new Header("$midi-patch=" + patch));

            var headerContent = TabSerialisation.GenerateTab(_viewModel.TabModel.ActivePiece.Headers, extraHeaders, options, false);     //serialise active piece only
            var bodyContent = TabSerialisation.GenerateTab(_viewModel.TabModel.ActivePiece.Staves, options, false);     //serialise active piece only

            var tabFile = Path.Combine(Session.Instance.SessionPath, "temp.tab");
            var midiFile = Path.Combine(Session.Instance.SessionPath, guid.ToString() +  ".mid");

            //save tab to temp path
            File.WriteAllText(tabFile, headerContent + "\n" + bodyContent);

            var midiConvertCommand = String.Format("\"{0}\" \"{1}\" -fontpath \"{2}\" -midi -o \"{3}\"", tabPath, tabFile, fontPath, midiFile);


            //convert to midi
            var exec = new ExecuteProcess();
            var result = exec.LoggedExecute(midiConvertCommand);

            //if TAB has errors, they will be in the std output, 
            //and exit code will probably be -1, or at least not 0
            if (result.Item1 != 0)
            {
                var match = (new Regex(findErrors)).Match(result.Item2);
                if (match.Success)
                {
                    MessageBox.Show("Error from TAB: " + match.Groups[1].ToString());
                }
                else
                {
                    MessageBox.Show("Error from TAB: " +
                        "exit code: " + result.Item1 +
                        " message: + " + result.Item2 +
                        " error: " + result.Item3);
                }
            }
            else
            {
                //MessageBox.Show("Midi file created: " + midiFile);

                //launch the file (generally would start Windows Media player app)
                var launch = new LaunchFileCommand(_viewModel);
                launch.Execute(midiFile);
            }

        }

    }
}
