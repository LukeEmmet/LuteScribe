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
            foreach (var chord in _chords)
            {
                chord.Stave = this;
            }
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
            var previousChord = Chords[selectedIndex - 1];


            if (selectedChord.Flag == "b")
            {
                var newBar = new Chord();
                newBar.Flag = "b";
                newStave.Chords.Add(newBar);

            }
            else if (previousChord.Flag == "b")
            {
                selectedChord = previousChord;

                var newBar = new Chord();
                newBar.Flag = "b";
                newStave.Chords.Add(newBar);

            }

            //iterate from the next chord to the end of the stave and
            //add to the next stave
            for (int n = lastChords.IndexOf(selectedChord) + 1; n < lastChords.Count; n++)
            {
                var chord = lastChords[n];
                newStave.Chords.Add(chord);
                removeList.Add(chord);
            }

            foreach (var chord in removeList.ToArray())
            {
                Chords.Remove(chord);
            }

            return newStave;

        }
    }

}
