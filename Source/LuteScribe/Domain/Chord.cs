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

using System.Xml.Serialization;

namespace LuteScribe.Domain
{
    public class Chord : ObservableObject
    {

        // Property variables
        private int _sequenceNumber;
        private string _flag;

        private string _c1;
        private string _c2;
        private string _c3;
        private string _c4;
        private string _c5;
        private string _c6;
        private string _c7;

        private Stave _stave;
        private bool _isSelected;

        /// <summary>
        /// Default constructor
        /// </summary>
        public Chord()
        {
            _flag = "";
        }

        /// <summary>
        /// Paramterized constructor.
        /// </summary>
        public Chord(string flag)
        {
            _flag = flag;
        }

        /// <summary>
        /// Paramterized constructor.
        /// </summary>
        public Chord(string flag, int itemIndex)
        {
            _flag = flag;
            _sequenceNumber = itemIndex;
        }

        /// <summary>
        /// parse an array, must be at least one item long, any missing entries are set to empty string
        /// </summary>
        public Chord(string[] stringArray)
        {
            _flag = stringArray[0];

            _c1 = (stringArray.GetUpperBound(0) >= 1) ? stringArray[1] : "";
            _c2 = (stringArray.GetUpperBound(0) >= 2) ? stringArray[2] : "";
            _c3 = (stringArray.GetUpperBound(0) >= 3) ? stringArray[3] : "";
            _c4 = (stringArray.GetUpperBound(0) >= 4) ? stringArray[4] : "";
            _c5 = (stringArray.GetUpperBound(0) >= 5) ? stringArray[5] : "";
            _c6 = (stringArray.GetUpperBound(0) >= 6) ? stringArray[6] : "";
            _c7 = (stringArray.GetUpperBound(0) >= 7) ? stringArray[7] : "";

        }

        [XmlIgnoreAttribute]
        public Stave Stave
        {
            get
            {
                return _stave;
            }

            set
            {
                _stave = value;
                base.RaisePropertyChangedEvent("Stave");
            }
        }
        
        [XmlIgnoreAttribute]
        public bool IsSelected {
            get { return _isSelected; }
            set {
                _isSelected = value;
                base.RaisePropertyChangedEvent("IsSelected");   //TBD - would be good to have this directly affect the WPF selection (not working yet)
            }

        }

        /// <summary>
        /// The sequential position of this item in a list of items.
        /// </summary>
        [XmlIgnoreAttribute]
        public int SequenceNumber
        {
            get
            {
                return _sequenceNumber;
            }

            set
            {
                _sequenceNumber = value;
                base.RaisePropertyChangedEvent("SequenceNumber");
                base.RaisePropertyChangedEvent("Flag");     //as how the flag is painted depends on sequence number (e.g. first bar line treated differently)
            }
        }

        /// <summary>
        /// The Flag of the chord.
        /// </summary>
        public string Flag
        {
            get { return _flag; }

            set
            {
                //trim the value so no extra spaces get into the data model
                //see also other course lines
                _flag = value.Trim();

                if (_flag == "x") { _flag = ""; }
                
                base.RaisePropertyChangedEvent("Flag");
                base.RaisePropertyChangedEvent("IsBarLine");
            }
        }

        public string C1
        {
            get { return _c1; }

            set
            {
                //trim the value so no extra spaces get into the data model
                //see also other course lines
                _c1 = value.Trim();
                base.RaisePropertyChangedEvent("C1");
            }
        }

        public string C2
        {
            get { return _c2; }

            set
            {
                _c2 = value.Trim();
                base.RaisePropertyChangedEvent("C2");
            }
        }

        public string C3
        {
            get { return _c3; }

            set
            {
                _c3 = value.Trim();
                base.RaisePropertyChangedEvent("C3");
            }
        }

        public string C4
        {
            get { return _c4; }

            set
            {
                _c4 = value.Trim();
                base.RaisePropertyChangedEvent("C4");
            }
        }

        public string C5
        {
            get { return _c5; }

            set
            {
                _c5 = value.Trim();
                base.RaisePropertyChangedEvent("C5");
            }
        }

        public string C6
        {
            get { return _c6; }

            set
            {
                _c6 = value.Trim();
                base.RaisePropertyChangedEvent("C6");
            }
        }

        public string C7
        {
            get { return _c7; }

            set
            {
                _c7 = value.Trim();
                base.RaisePropertyChangedEvent("C7");
            }
        }

        [XmlIgnoreAttribute]
        public bool IsBarLine
        {
            get {
                return FlagParser.IsBarLine (Flag);
            }
        }

        [XmlIgnoreAttribute]
        public bool IsComment
        {
            get
            {
                return FlagParser.IsComment(Flag);
            }
        }

        /// <summary>
        /// Sets the item name as its ToString() value.
        /// </summary>
        /// <returns>The name of the item.</returns>
        public override string ToString()
        {
            return Flag + C1 + C2 + C3 + C4 + C5 + C6 + C7;
        }

    }
}
