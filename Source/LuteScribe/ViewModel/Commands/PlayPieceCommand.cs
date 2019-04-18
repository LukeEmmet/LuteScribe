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
using LuteScribe.View;

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
            var selection = (String)parameter;
            var result = false;

            //determine if there is a selection
            var selectedStave = _viewModel?.TabModel?.ActivePiece?.SelectedItem;
            var selectedItems = _viewModel?.TabModel?.ActivePiece?.SelectedItem?.SelectedItems;
            var selectionCount = (selectedItems == null) ? 0 : selectedItems.Count;
            
            switch (selection)
            {
                case "All":
                    result = true;
                    break;
                case "FromSelection":
                    result = selectionCount > 0;
                    break;
                case "Selection":
                    result = selectionCount > 0;
                    break;
                case "Stave":
                    result = (selectedStave != null);
                    break;
                default:
                    result = false;
                    break;

            }
            return result;
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

        private Tuple<Chord, Chord> SelectedChordRange(Piece piece)
        {
            var selectedChords = piece.SelectedItem.SelectedItems;
            var startSelectedChord = selectedChords[0];
            var endSelectedChord = selectedChords[selectedChords.Count - 1];

            return new Tuple<Chord, Chord>(startSelectedChord, endSelectedChord);
        }
        private Tuple<Chord, Chord> GetPlaybackScope(string playSelection, Piece piece)
        {
            piece.TabModel.SanitiseModel();     //remove empty staves

            var firstStave = piece.Staves[0];
            var lastStave = piece.Staves[piece.Staves.Count - 1];
            var firstChord = firstStave.Chords[0];
            var lastChord = lastStave.Chords[lastStave.Chords.Count - 1];

            var startChord = firstChord;
            var endChord = lastChord;

            switch (playSelection)
            {
                case "All":
                    startChord = firstChord;
                    endChord = lastChord;
                    break;
                case "Selection":
                    //only try to get a selection in this mode
                    startChord = SelectedChordRange(piece).Item1;
                    endChord = SelectedChordRange(piece).Item2;
                    break;
                case "FromSelection":
                    //only try to get a selection in this mode
                    startChord = SelectedChordRange(piece).Item1;
                    endChord = lastChord;
                    break;
                case "Stave":
                    //get the chords in the currently selected stave
                    var stave = piece.SelectedItem;
                    startChord = stave.Chords[0];
                    endChord = stave.Chords[stave.Chords.Count - 1];
                    break;
                default:
                    startChord = firstChord;
                    endChord = lastChord;
                    break;
            }

            //return a pair of chords
            return new Tuple<Chord, Chord>(startChord, endChord);
        }
        public void Execute(object parameter)
        {
            var appDir = System.AppDomain.CurrentDomain.BaseDirectory;
            var tabPath = Path.GetFullPath(Path.Combine(appDir, "..\\..\\..\\Tab\\bin\\tab.exe"));
            var fontPath = Path.GetFullPath(Path.Combine(appDir, "..\\..\\..\\Tab\\bin"));

            var findErrors = "tab 4\\.3\\.94 copyright 1995-2018 by Wayne Cripps(\\r\\n.+)";

            var guid = Guid.NewGuid();
            
            var patch = LookUpPatch(_viewModel.PlaybackPatch);
            var speed = LookUpTempo(_viewModel.PlaybackSpeed);

            //scope of playback is passed in as a parameter
            var selection = (string) parameter;

            
            var piece = _viewModel.TabModel.ActivePiece;
            var headers = piece.Headers;
            var staves = piece.Staves;

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

            var scope = GetPlaybackScope(selection, piece);

            var headerContent = TabSerialisation.GenerateTab(headers, extraHeaders, options, false);     //serialise active piece only
            var bodyContent = TabSerialisation.GenerateTab(staves, scope.Item1, scope.Item2, options, false);     //serialise active piece only

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

                //use a wait cursor as generating the wave might take a while
                //see https://stackoverflow.com/questions/3480966/display-hourglass-when-application-is-busy
                using (new WaitCursor())
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

                    result = exec.LoggedExecute(midiLaunch, false, false);

                    _viewModel.Playback(waveOut);
                }

            }

        }
    }
}
