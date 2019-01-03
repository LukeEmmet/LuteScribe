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
using System.Collections.Generic;

namespace JtxmlParserLib
{
	internal class Stave
	{
		public List<Chord> Chords;

		public Stave()
		{
            Chords = new List<Chord>();
		}

		public void AddBar()
		{
			AddChord("b");
		}

		public Chord AddChord(string Flag)
		{
			Chord curChord = new Chord();
			curChord.Flag = Flag;
            Chords.Add(curChord);
            
			return curChord;
		}

		public override string ToString()
		{
			StringBuilder s = new StringBuilder();
			s.Append("");

			foreach (Chord curChord in Chords)
			{
				s.Append(curChord.ToString() + Environment.NewLine);
			}

			return s.ToString();

		}
	}
}