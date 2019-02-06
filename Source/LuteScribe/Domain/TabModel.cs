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
using System.Xml.Serialization;

namespace LuteScribe.Domain
{
    [Serializable]
    public class TabModel : ObservableObject
    {
        private ObservableCollection<Piece> _pieces;
        private Piece _activePiece;
        private string _version;

        /// <summary>
        /// Updates the ItemCount Property when the Stave collection changes.
        /// </summary>
        void OnPiecesChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (var piece in _pieces)
            {
                piece.TabModel = this;
            }

        }

        public void SanitiseModel()
        {
            var deletePieces = new List<Piece>();

            foreach (var piece in this.Pieces)
            {
                if (piece.Staves.Count == 0)
                {
                    //remove this empty piece
                    deletePieces.Add(piece);
                }
                else
                {
                    var deleteStaves = new List<Stave>();
                    foreach (var stave in piece.Staves)
                    {
                        if (stave.Chords.Count == 0)
                        {
                            //remove this empty stave
                            deleteStaves.Add(stave);
                        }
                    }
                    foreach (var stave in deleteStaves)
                    {
                        piece.Staves.Remove(stave);
                    }
                }
            }
            foreach (var piece in deletePieces)
            {
                this.Pieces.Remove(piece);
            }
        }


        /// <summary>
        /// represents the version of the file format - aids file format compatibility checking
        /// </summary>
        [XmlAttribute]
        public string Version
        {
            get { return _version; }
            set { _version = value; }

        }

        public ObservableCollection<Piece> Pieces
        {
            get
            {
                return _pieces;
            }
            set
            {
                _pieces = value;

                foreach (var piece in _pieces)
                {
                    piece.TabModel = this;

                }
                base.RaisePropertyChangedEvent("Pieces");
            }
        }

        [XmlIgnoreAttribute]
        public Piece ActivePiece
        {
            get
            {
                return _activePiece;
            }

            set
            {
                _activePiece = value;
                base.RaisePropertyChangedEvent("ActivePiece");
            }
        }

        public TabModel()
        {
            Version = "1.0";    //initial version of the model schema

            Pieces = new ObservableCollection<Piece>();

            // Subscribe to CollectionChanged event
            _pieces.CollectionChanged += OnPiecesChanged; //ideally would want to listen for changes to the content, not just collection by this not working yet...

        }


    }
}
