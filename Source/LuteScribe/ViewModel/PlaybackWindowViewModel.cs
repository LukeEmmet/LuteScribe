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

using System.Collections.ObjectModel;
using System.Windows.Input;
using LuteScribe.ViewModel.Commands;
using System.Linq;
using System.Collections.Generic;
using LuteScribe.Domain;
using LuteScribe.ViewModel;
using System.Windows.Controls;
using LuteScribe.Properties;
using LuteScribe.View.PdfView;
using System.Diagnostics;
using LuteScribe.Singletons;
using LuteScribe.ViewModel.Services;
using GenericUndoRedo;
using LuteScribe.Audio;
using LuteScribe.ViewModel;

namespace LuteScribe
{
    public class PlaybackWindowViewModel: ViewModelBase
    {


        private string _playbackPath;
        private readonly AudioPlayback _audioPlayback;

        public ICommand PlayCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand StopCommand { get; }


        public PlaybackWindowViewModel()
        {
            this.Initialize();

            _audioPlayback = new AudioPlayback();

            PlayCommand = new DelegateCommand(Play);
            StopCommand = new DelegateCommand(Stop);
            PauseCommand = new DelegateCommand(Pause);
            
        }
        
        internal void Play()
        {
            

            if (_playbackPath != null)
            {
                _audioPlayback.Play();
            }


            ////launch the file (generally would start Windows Media player app)
            //var launch = new LaunchFileCommand(this);
            //launch.Execute(_playbackPath);
        }

        internal void Stop()
        {
            _audioPlayback.Stop();
        }


        internal void Pause()
        {
            _audioPlayback.Pause();
            
        }

        public void Dispose()
        {
            _audioPlayback.Dispose();
        }


        public string PlaybackPath
        {
            get
            {
                return _playbackPath;
            }

            set
            {
                _playbackPath = value;

                _audioPlayback.Load(_playbackPath);

                RaisePropertyChangedEvent("PlaybackPath");
            }
        }

        /// <summary>
        /// Deletes the currently-selected item from the Stave.
        /// </summary>
        //public ICommand NewFile { get; set; }
        //public ICommand SaveFile { get; set; }
        /// <summary>
        /// Initializes this application.
        /// </summary>
        private void Initialize()
        {
            //// Initialize commands
            ////on file Menu
            //this.NewFile = new NewFileCommand(this);
            //this.SaveFile = new SaveFileCommand(this);

            //SelectedTab = 0;   //staves tab

            //NewFile.Execute(this);

            //RefreshLastFilesMenu();

            //// Update bindings
            //base.RaisePropertyChangedEvent("TabModel");

        }

    }

}

