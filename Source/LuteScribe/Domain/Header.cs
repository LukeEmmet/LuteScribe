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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuteScribe.Domain
{
    public class Header : ObservableObject
    {
        private string _content;
        private Piece _piece;

        public Header()
        {
            //0 param constructor must exist for xml serialisation
            _content = "";  //default
        }

        public Header(string content)
        {
            _content = content;
        }

        public string Content
        {
            get
            {
                return _content;
            }
            set
            {
                _content = (value != null) ? value : "";  //if null set to empty string

                base.RaisePropertyChangedEvent("Content");      //not sure this really does anything useful at this stage

                if (_piece != null) { _piece.RescanTitle(); }   //notify piece it needs to update the title
            }

        }

        internal void SetPiece(Piece value)
        {
            _piece = value;
        }
    }
}
