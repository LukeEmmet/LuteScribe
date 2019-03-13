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

using System.Windows.Input;
using LuteScribe.ViewModel;
using LuteScribe.Audio;

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
            

        }

    }

}

