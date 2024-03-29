﻿//===================================================
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

using LuteScribe.ViewModel.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Serialization;

namespace LuteScribe.Domain
{
    public class Stave : ObservableObject
    {
        private ObservableCollection<Chord> _chords;
        private Piece _section;
        private Chord _selectedItem;

        public Stave()
        {
            this._chords = new ObservableCollection<Chord>();
            _chords.CollectionChanged += Chords_CollectionChanged;
        }

        private void Chords_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //keep the sequence numbers up to date
            //set stave reference to cover insertions so they have the correct
            //stave reference
            //this is much faster than using list.IndexOf(item) on each item

            //only index beyond the point of change (earlier ones are fine)
            var indexFrom = 0;
            if (e.NewStartingIndex > -1) {
                indexFrom = e.NewStartingIndex;
            } else if (e.OldStartingIndex < e.NewStartingIndex & e.OldStartingIndex > -1) {
                indexFrom = e.OldStartingIndex;
            } else
            {
                indexFrom = 0;
            }
            
            if (indexFrom > _chords.Count - 1)
            {
                indexFrom = _chords.Count - 1;
            }

            //if we deleted the last chord, just return
            if (_chords.Count == 0 )
            {
                return;
            }
            for (var n = indexFrom; n < _chords.Count; n++)
            {
                var chord = _chords[n];
                chord.Stave = this;
                chord.SequenceNumber = n;
            }

            //this is the simpler way to do it - do all on the stave
            //maybe this is fine really...
            //int seq = 0;
            //foreach (var chord in _chords)
            //{
            //    chord.Stave = this;
            //    chord.SequenceNumber = seq;
            //    seq = seq + 1;
            //}
        }

        public ObservableCollection<Chord> Chords
        {
            get
            {
                return this._chords;
            }
            set
            {
                _chords = value;
                base.RaisePropertyChangedEvent("Chords");
            }
        }
        [XmlIgnoreAttribute]
        public Stave Next => _section.Staves[_section.Staves.IndexOf(this) + 1];

        [XmlIgnoreAttribute]
        public Stave Previous => _section.Staves[_section.Staves.IndexOf(this) - 1];

        [XmlIgnoreAttribute]
        public ObservableCollection<Stave> Staves
        {
            get
            {
                return this._section.Staves;
            }
        }

        [XmlIgnoreAttribute]
        public Piece Section
        {
            get
            {
                return this._section;
            }
            set
            {
                this._section = value;
                base.RaisePropertyChangedEvent("Section");
            }
        }

        [XmlIgnoreAttribute]
        public List<Chord> SelectedItems
        {
            get
            {
                return (from chord in Chords where chord.IsSelected select chord).ToList();
            }
        }

        [XmlIgnoreAttribute]
        public Chord SelectedItem
        {
            get
            {
                return _selectedItem;
            }

            set {
                _selectedItem = value;
                base.RaisePropertyChangedEvent("SelectedItem");

                _section.SelectedItem = this;

            }
        }
        public void AcquireLeft()
        {
            if ((this == Staves.First()) || (Previous.Chords.Count == 0)) { return; }

            //move last item from stave n - 1 to stave n
            var moveChord = Previous.Chords.Last();

            //move from end of one onto start of the next
            Previous.Chords.Remove(moveChord);
            Chords.Add(moveChord);
            Chords.Move(Chords.IndexOf(moveChord), Chords.IndexOf(Chords.First()));

        }

        public void AcquireRight()
        {

            if ((this == Staves.Last()) || (Next.Chords.Count == 0)) { return; }

            //move last item from stave n + 1 to stave n
            var moveChord = Next.Chords.First();

            //move from end of one onto start of the next
            Next.Chords.Remove(moveChord);
            Chords.Add(moveChord);
            Chords.Move(Chords.IndexOf(moveChord), Chords.IndexOf(Chords.Last()));
        }


        public void DeleteStaveEnd()
        {
            var stave = this;
            //apart from thhe very last stave,  join up with next
            //effectively deleting the break between them
            if (stave.Staves.IndexOf(stave) < stave.Staves.Count - 1)
            {

                var nextStave = stave.Next;

                //if last item of this and first of next are both plain barlines
                //delete the second one
                var lastChord = stave.Chords[stave.Chords.Count - 1];

                if (nextStave.Chords.Count > 0)
                {
                    var firstChord = nextStave.Chords[0];
                    if ((lastChord.Flag == "b") && (firstChord.Flag == "b"))
                    {
                        nextStave.Chords.Remove(firstChord);
                    }

                    foreach (var chord in nextStave.Chords)
                    {
                        // add to this
                        chord.Stave = nextStave;
                        stave.Chords.Add(chord);
                    }
                }
                //remove next stave
                stave.Staves.Remove(nextStave);
            }

        }

        public Stave InsertStaveBreak(Chord breakAtChord)
        {
            var staves = _section.Staves;
            var newStave = new Stave();

            //add new stave and put it just after the current one
            staves.Add(newStave);

            staves.Move(staves.IndexOf(newStave), staves.IndexOf(this) + 1);

            var selectedChord = breakAtChord;

            var lastChords = Chords;
            var removeList = new List<Chord>();
            var selectedIndex = lastChords.IndexOf(selectedChord);


            if (selectedChord.Flag == "b")
            {
                var newBar = new Chord();
                newBar.Flag = "b";
                newStave.Chords.Add(newBar);

            }
            else if (selectedIndex > 0)
            {
               //only examine previous if there is one..
               var previousChord = Chords[selectedIndex - 1];
               if (previousChord.Flag == "b")
                {
                    selectedChord = previousChord;

                    var newBar = new Chord();
                    newBar.Flag = "b";
                    newStave.Chords.Add(newBar);

                }
            }

            //collect up the chords beyond the selected one and move onto a temporary list
            //before removing from current stave and onto next
            for (int n = lastChords.IndexOf(selectedChord) + 1; n < lastChords.Count; n++)
            {
                var chord = lastChords[n];
                removeList.Add(chord);
            }
            foreach (var chord in removeList.ToArray())
            {
                Chords.Remove(chord);
                newStave.Chords.Add(chord);
                chord.Stave = newStave;
            }


            return newStave;

        }
    }

}
