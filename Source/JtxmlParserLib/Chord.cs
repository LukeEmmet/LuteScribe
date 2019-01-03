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
using System.Text;
using System.Text.RegularExpressions;

namespace JtxmlParserLib
{
	internal class Chord
	{

		public string Flag = "";
		public string C1 = "";
		public string C2 = "";
		public string C3 = "";
		public string C4 = "";
		public string C5 = "";
		public string C6 = "";
		public string C7 = "";

		public override string ToString()
		{
			StringBuilder s = new StringBuilder();

			s.Append((Flag == "") ? "x" : Flag);
			s.Append((C1 == "") ? " " : C1);
			s.Append((C2 == "") ? " " : C2);
			s.Append((C3 == "") ? " " : C3);
			s.Append((C4 == "") ? " " : C4);
			s.Append((C5 == "") ? " " : C5);
			s.Append((C6 == "") ? " " : C6);
			s.Append((C7 == "") ? " " : C7);

			return s.ToString();
		}

		public bool IsBarline()
		{

            Regex Regex = new Regex("^[bB\\.]+$");
			return Regex.IsMatch(Flag);

		}
	}

}