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
using System.Linq;

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

       
        private string LookUpPatch(string patchName)
        {
            var patch = "001";  //default

            switch (patchName.ToLower())
            {

                case "piano":
                    patch = "001";
                    break;
                case "harpsichord":
                    patch = "006";
                    break;
                case "nylon guitar":
                    patch = "024";
                    break;
                case "acoustic guitar":
                    patch = "025";
                    break;
                default:
                    patch = "024";
                    break;
            }

            return patch;
        }

        private string LookUpTempo(string tempoName)
        {
            var speed = "1.5";  //default
            switch (tempoName.ToLower())
            {
                case "slower":
                    speed = "0.75";
                    break;
                case "normal":
                    speed = "1";
                    break;
                case "faster":
                    speed = "1.5";
                    break;
                case "2 x faster":
                    speed = "2";
                    break;
                default:
                    speed = "1.5";
                    break;
            }

            return speed;
        }
        public void Execute(object parameter)
        {
            var appDir = System.AppDomain.CurrentDomain.BaseDirectory;
            var tabPath = Path.GetFullPath(Path.Combine(appDir, "..\\..\\..\\Tab\\bin\\tab.exe"));
            var fontPath = Path.GetFullPath(Path.Combine(appDir, "..\\..\\..\\Tab\\bin"));

            var findErrors = "tab 4\\.3\\.89 copyright 1995-2010 by Wayne Cripps(\\r\\n.+)";

            var guid = Guid.NewGuid();
            
            var patch = LookUpPatch(_viewModel.PlaybackPatch);

            //if no explicit speed passed in, use the user default preference
            var speed = (string)parameter;
            speed = LookUpTempo(speed != null ? speed : _viewModel.PlaybackSpeed);
   
            var headers = _viewModel.TabModel.ActivePiece.Headers;
            var staves = _viewModel.TabModel.ActivePiece.Staves;

            SimpleLogger.Instance.Log("Saving MIDI of current piece");

            //create options but we dont need to set anything
            var options = new PrettyOptions();

            //extra headers so we can control midi patch of output
            var extraHeaders = new List<Header>();

            //set the midi patch to be used, as passed in
            extraHeaders.Add(new Header("$midi-patch=" + patch));

            //if no existing tempo setting provide one (otherwise TAB plays it too fast with 
            //default tempo being 2)
            var tempoCount = (from header in headers where header.Content.StartsWith("$tempo=") select header).Count();
            if (tempoCount < 1)
            {
                extraHeaders.Add(new Header("$tempo=" + speed));
            }
            

            var headerContent = TabSerialisation.GenerateTab(headers, extraHeaders, options, false);     //serialise active piece only
            var bodyContent = TabSerialisation.GenerateTab(staves, options, false);     //serialise active piece only

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

                var wildMidi = "..\\..\\..\\Utils\\WildMidi\\wildmidi-static.exe";
                var config = "..\\..\\..\\Utils\\WildMidi\\Patches\\Freepats\\LuteScribe.cfg";

                var midiPlayer = Path.GetFullPath(Path.Combine(appDir, wildMidi));
                var configPath = Path.GetFullPath(Path.Combine(appDir, config));
                var waveOut = Path.GetFullPath(Path.Combine(Session.Instance.SessionPath, guid.ToString() + ".wav"));

                //buid command line to convert midi to wav using wildMidi
                var midiLaunch = String.Format("\"{0}\" -c \"{1}\" -o \"{2}\" -b \"{3}\"", midiPlayer, configPath, waveOut, midiFile);

                //run command -avoiding grabbing output as
                //it is not useful, and causes exexCommand to otherwise hang
                exec = new ExecuteProcess();
                result = exec.ExecuteCommand(midiLaunch, false, false);

                //launch in media player...
                LaunchWindowsMP(waveOut);

            }

        }

        private void LaunchWindowsMP(string midiFile)
        {
            //launch the file (generally would start Windows Media player app)
            var launch = new LaunchFileCommand(_viewModel);
            launch.Execute(midiFile);
        }
    }
}