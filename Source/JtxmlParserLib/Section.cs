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
using System.Text;
using System.Xml;

namespace JtxmlParserLib
{
	public class Section
	{

		private string mLastFlag = "";
		private List<Stave> mStaves = null;
		private JtxmlParser mParser = null;

		public string Title = "";
		public string Author = "";
		public List<String> UnparsedElements = null;


		public void Construct(JtxmlParser Parser)
		{
			mParser = Parser;
		}

		internal Section()
		{
            mStaves = new List<Stave>();
            UnparsedElements = new List<String>();
            
		}


		public string ExtractSection(int SectionIndex)
		{

			Stave curStave = null;
			Chord curChord = null;
			string curFret = "";
			int curCourse = 0;
			string EventType = "";
			string RepeatType = "";
			int Status = 0;
			int Flag = 0;
			Chord LastChord = null;
			Chord FirstChord = null;

			UnparsedElements = new List<String>();
			mStaves = new List<Stave>();

			XmlElement XSection = (XmlElement) mParser.Dom.DocumentElement.SelectSingleNode("//DjangoTabXML/sections/section[@index='" + ((SectionIndex - 1).ToString()) + "']");

			XmlElement XEl = (XmlElement) XSection.SelectSingleNode("section-texts/track-text[@descriptor='section-name']");
			if (XEl != null)
			{
				Title = XEl.InnerText;
			}

			XEl = (XmlElement) XSection.SelectSingleNode("section-texts/track-text[@descriptor='section-author']");
			if (XEl != null)
			{
				Author = XEl.InnerText;
			}


            foreach (XmlElement XStave in XSection.SelectNodes("systems/system/instruments/instrument/staff[@count!='0']"))
			{

				if (0 < Convert.ToInt32(XStave.GetAttribute("count")))
				{
                    curStave = new Stave();
                    mStaves.Add(curStave);

                    mLastFlag = "b";


					foreach (XmlElement XChord in XStave.SelectNodes("event"))
					{

						curChord = new Chord();

						EventType = XChord.GetAttribute("type").ToString();

						if (String.IsNullOrEmpty(XChord.GetAttribute("flag")))
						{
							Flag = 0;
						}
						else
						{
							Flag = Convert.ToInt32(XChord.GetAttribute("flag").ToString());
						}
						Status = Convert.ToInt32(XChord.GetAttribute("status").ToString());

						if (EventType == "bar")
						{

							if (XChord.GetAttribute("bar-type").ToString() == "repeat")
							{
								RepeatType = XChord.GetAttribute("repeat-type").ToString();

								if (RepeatType == "both")
								{
									curStave.AddChord(".");
									curStave.AddChord("bb");
									curStave.AddChord(".");
								}
								else if (RepeatType == "left")
								{ 
									curStave.AddChord(".");
									curStave.AddChord("bb");
								}
								else if (RepeatType == "right")
								{ 
									curStave.AddChord("bb");
									curStave.AddChord(".");
								}
								else
								{
									curStave.AddChord("b");
								}

							}
							else
							{
								curStave.AddChord("b");
							}

							mLastFlag = "b";
						}
						else if (EventType == "chord")
						{ 

							curChord.Flag = GetFlag(XChord);

							foreach (XmlElement XNote in XChord.SelectNodes("notes/note"))
							{
								curCourse = Convert.ToInt32(XNote.GetAttribute("string").ToString()) + 1;

								curFret = GetFret(XNote, XSection);

								UpdateCourse(curChord, curCourse, curFret);
							}
							curStave.Chords.Add(curChord);

						}
						else if (EventType == "special-event")
						{ 
							//add a special event - it may add its own chords
							AddSpecialEvent(curStave, Flag, Status);
						}
						else if (EventType == "blank")
						{ 
							//do nothing at present
						}
						else
						{
							curChord.Flag = "%type: " + EventType;
							curStave.Chords.Add(curChord);

							UnparsedElements.Add(curChord.ToString());
						}
					}

					//start and end each stave with a barline if it does not already have one
					FirstChord = (Chord) curStave.Chords[0];
					if (!FirstChord.IsBarline())
					{
						curChord = new Chord();
						curChord.Flag = "b";
						curStave.Chords.Insert(0, curChord);
					}

					LastChord = (Chord) curStave.Chords[curStave.Chords.Count - 1];
					if (!LastChord.IsBarline())
					{
						curStave.AddBar();
					}

					mLastFlag = "b";
				}

			}


			Stave LastStave = (Stave) mStaves[mStaves.Count - 1];
			while (LastStave.Chords.Count == 0)
			{
				mStaves.RemoveAt(mStaves.Count - 1);

				LastStave = (Stave) mStaves[mStaves.Count - 1];
			}

			if (LastStave.Chords.Count > 0)
			{
				LastChord = (Chord) LastStave.Chords[LastStave.Chords.Count - 1];
				if (LastChord.Flag != "e")
				{
					LastStave.AddChord("e");
				}
			}
			else
			{

			}
			//return the string content.
			return ToString();
		}


		private void AddSpecialEvent(Stave Stave, int Flag, int Status)
		{

			Chord curChord = null;

			if (Flag == 0 && Status == 178)
			{
				//bB. bar with dots
				//Call Stave.AddChord("B")
				//Call Stave.AddChord("b")
				//Call Stave.AddChord(".")
			}
			else if (Flag == 0 && Status == 18)
			{ 
				Stave.AddChord("C"); //common time - no not really - fixme
			}
			else if (Flag == 0 && Status == 386)
			{ 
				Stave.AddChord("c"); //cut time - no not really - fixme
			}
			else
			{
				curChord = new Chord();
				curChord.Flag = "%special: flag=" + Flag.ToString() + ", status=" + Status.ToString();
				Stave.Chords.Add(curChord);

				UnparsedElements.Add(curChord.ToString());
			}


		}

		private string GetFlag(XmlElement XChord)
		{
			string result = "";
			string Flag = "x"; //default
			int FlagNum = Convert.ToInt32(XChord.GetAttribute("flag"));

			switch(FlagNum)
			{
                case 4096:
                case 0:
                    Flag = "W";
                    break;
                case 4097:
                case 1:
                    Flag = "w";
                    break;
                case 4098 : case 2 : 
					Flag = "0"; 
					break;
				case 4099 : case 3 : 
					Flag = "1"; 
					break;
				case 4100 : 
					Flag = "2"; 
					break;
				case 4101 : 
					Flag = "3"; 
					break;
				case 4102 : 
					Flag = "4"; 
					break;
				case 4103 : 
					Flag = "5"; 
					 
					break;
                case 4104:
                    Flag = "W.";
                    break;
                case 4105:
                    Flag = "w.";
                    break;
                case 4106 : 
					Flag = "0."; 
					break;
				case 4107 : 
					Flag = "1."; 
					break;
				case 4108 : 
					Flag = "2."; 
					break;
				case 4109 : 
					Flag = "3."; 
					break;
				case 4110 : 
					Flag = "4."; 
					break;
				case 4111 : 
					Flag = "5."; 
					 
					break;
				case 4 : 
					Flag = "2";  //means next one is hidden 
					break;
				case 5 : 
					Flag = "3";  //means next one is hidden 
					break;
				case 6 : 
					Flag = "4";  //means next one is hidden 
					break;
				case 7 : 
					Flag = "5";  //means next one is hidden 
					 
					break;
                case 8:
                    Flag = "W.";
                    break;
                case 9:
                    Flag = "w.";
                    break;
                case 10 : 
					Flag = "0."; 
					break;
				case 11 : 
					Flag = "1."; 
					break;
				case 12 : 
					Flag = "2."; 
					break;
				case 13 : 
					Flag = "3."; 
					break;
				case 14 : 
					Flag = "4."; 
					break;
				case 15 : 
					Flag = "5."; 
					 
					break;
				default:
					Flag = "%" + FlagNum.ToString();  //show as a tab comment so we can debug these 
					UnparsedElements.Add("flag: " + Flag); 
					 
					break;
			}

			if (mLastFlag == Flag)
			{
				result = "x";
			}
			else
			{
				result = Flag;
				mLastFlag = Flag;
			}

			return result;
		}

		private string NormaliseContent(string s)
		{
			string Res = s;

            //white space
            Res = Res.Replace('\n', ' ');
			Res = Res.Replace('\r', ' ');

            //replace forward slashes - as TAB uses them to seperate left/right formatting
            Res = Res.Replace("/", " ");

            //sort out any double spaces
            Res = Res.Replace("  ", " ");

            //trim leading and trailing space
            Res.Trim();

			return Res;
		}

		private void UpdateCourse(Chord c, int Course, string Fret)
		{
			switch(Course)
			{
				case 1 : 
					c.C1 = Fret; 
					break;
				case 2 : 
					c.C2 = Fret; 
					break;
				case 3 : 
					c.C3 = Fret; 
					break;
				case 4 : 
					c.C4 = Fret; 
					break;
				case 5 : 
					c.C5 = Fret; 
					break;
				case 6 : 
					c.C6 = Fret; 
					break;
				case 7 : 
					c.C7 = Fret; 
					break;
			}

		}

		private string GetFret(XmlElement XNote, XmlElement XSection)
		{
			int Course = Convert.ToInt32(XNote.GetAttribute("string").ToString()) + 1;
			int Pitch = Convert.ToInt32(XNote.GetAttribute("pitch").ToString());

			//look up in instrument definition
			XmlElement XString = (XmlElement) XSection.SelectSingleNode("instruments/instrument[@index='0']/tablature-definition/tuning/strings/string[@index='" + ((Course - 1).ToString()) + "']");

			if (XString == null)
			{
				UnparsedElements.Add("Could not get string for note: " + XNote.InnerXml);
				return "?";
			}
			else
			{
				return FretLetter(Pitch, Convert.ToInt32(XString.GetAttribute("midi-pitch")) + 23);
			}

		}

		public string FretLetter(int NoteIndex, int CourseStart)
		{
			int Offset = (NoteIndex - CourseStart) + 1; //assumes zero index in pitch definition
			string Letters = "abcdefghiklmnopqrstuvwxyz";       //N.B. no "j" in canonical tabulature

			if (Offset > Letters.Length || (Offset < 1))
			{
				//FretLetter = "%note index=" & Offset
				//UnparsedElements.Add (FretLetter)
				return ""; //seems to indicate empty note
			}
			else
			{
				return Letters.Substring(Offset - 1, Math.Min(1, Letters.Length - (Offset - 1)));
			}
		}

		public override string ToString()
		{
			StringBuilder s = new StringBuilder();

            s.Append("{");
            //first header must be title as LuteScribe parses on that to identify each new section
            if (String.CompareOrdinal(Title, "") > 0)
            {
                s.Append( NormaliseContent(Title));
            }
            else
            {
                s.Append("Untitled");
            }
            //output author info
            if (String.CompareOrdinal(Author, "") > 0)
			{
				s.Append("/" + NormaliseContent(Author));
			}

            s.Append("}" + Environment.NewLine);

            //output basic formatting headers
            string OtherHeaders = "" +
                                  "$flagstyle=thin" + Environment.NewLine +
                                  "$charstyle=robinson" + Environment.NewLine +
                                  "%Converted by LuteScribe JTXML to TAB converter" + Environment.NewLine;

			
			s.Append(OtherHeaders);
			s.Append(Environment.NewLine);

            var count = 0;
			foreach (Stave Stave in mStaves)
			{
                count++;
                if (count > 0) { s.Append(Environment.NewLine); }

				s.Append(Stave.ToString());
			}

			return s.ToString();

		}
	}
}