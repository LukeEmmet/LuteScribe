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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LuteScribe.Domain
{
    public class Piece : ObservableObject
    {
        private ObservableCollection<Stave> _staves;
        private ObservableCollection<Header> _headers;
        private Stave _selectedItem;
        private string _title;
        private TabModel _tabModel;

        
        /// <summary>
        /// called when the headers content is udpated
        /// </summary>
        public void SetTitleFromHeaders()
        {
            var currentTitle = Title;
            var newTitle = "(Untitled)";

            foreach (var header in _headers)
            {
                if (header.Content != null)
                {
                    var regex = new Regex("^{(.*)}$");
                    var match = regex.Match(header.Content);
                    if (match.Success)
                    {
                        newTitle = match.Groups[1].ToString();
                        break;
                    }
                }
            }

            //update if changed
            if (newTitle != currentTitle) { Title = newTitle; }
            
        }

        public void SetHeadersFromTitle()
        {
            var title = Title;

            foreach (var header in _headers)
            {
                if (header.Content != null)
                {
                    var regex = new Regex("^{(.*)}$");
                    var match = regex.Match(header.Content);
                    if (match.Success)
                    {
                        //only match first one...
                        header.Content = "{" + title + "}";
                        break;
                    }
                }
            }

        }
        /// <summary>
        /// Updates the ItemCount Property when the Stave collection changes.
        /// </summary>
        void OnStavesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (var stave in _staves)
            {
                stave.Section = this;
            }

        }

        public void ReflowStaves(int staveWrap)
        {
            var staves = Staves;
            var splitPoint = staveWrap;
            var staveIndex = 0;

            
            //delete all the stave endings
            var count = staves.Count;
            for (int n = 0; n < count; n++)
            {
                staves[0].DeleteStaveEnd();   //delete end of first stave n times
            }

            while ((staveIndex < staves.Count))
            {
                var stave = staves[staveIndex];
                var lastBar = default(Chord);

                int chordIndex = 0;

                
                while ((chordIndex < stave.Chords.Count))
                {

                    
                    var chord = stave.Chords[chordIndex];

                    if (chordIndex > splitPoint)
                    {
                        //break at last bar if any or current position
                        var splitChord = (lastBar == null) ? chord : lastBar;

                        stave.InsertStaveBreak(splitChord);
                        break;
                    }
                    else
                    {
                        //just continue looking on this stave...
                    }

                    //if this is a barline, remember as a 
                    //possible break point
                    lastBar = chord.IsBarLine ? chord : lastBar;

                    chordIndex++;

                }
                //if after getting to the end of the stave there are no more 
                //to go we can stop
                if (staveIndex == (staves.Count - 1)) {
                    break;
                }

                staveIndex++;

            }
        }

        void OnHeadersChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

            foreach (var header in _headers)
            {
                header.SetPiece(this);

            }

            SetTitleFromHeaders();
        }

        public ObservableCollection<Header> Headers
        {
            get
            {
                return _headers;
            }

            set
            {
                _headers = value;
                base.RaisePropertyChangedEvent("Headers");
            }
        }

        public ObservableCollection<Stave> Staves
        {
            get
            {
                return _staves;
            }
            set
            {
                _staves = value;

                foreach (var stave in _staves)
                {
                    stave.Section = this;

                }
                base.RaisePropertyChangedEvent("Staves");
            }
        }

        [XmlIgnoreAttribute]
        public Stave SelectedItem
        {
            get
            {
                return _selectedItem;
            }

            set
            {
                _selectedItem = value;
                base.RaisePropertyChangedEvent("SelectedItem");
            }
        }

        [XmlIgnoreAttribute]
        public String Title
        {
            get
            {
                return _title;
            }

            set
            {
                _title = value;
                SetHeadersFromTitle();
                base.RaisePropertyChangedEvent("Title");
            }
        }

        [XmlIgnoreAttribute]
        public TabModel TabModel
        {
            get
            {
                return _tabModel;
            }
            set
            {
                _tabModel = value;
                base.RaisePropertyChangedEvent("TabModel");
            }
        }
        public Piece()
        {
            Staves = new ObservableCollection<Stave>();
            Headers = new ObservableCollection<Header>();

            // Subscribe to CollectionChanged event
            _staves.CollectionChanged += OnStavesChanged;
            _headers.CollectionChanged += OnHeadersChanged;     //we set the piece property on each header so when content changes we can rescan for an update title.
            
        }
    }
}
