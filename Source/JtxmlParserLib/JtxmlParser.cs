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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace JtxmlParserLib
{
	public class JtxmlParser
	{
		private string mPath = "";
		private XmlDocument mDom = null;
		private int mNumSections = 0;

		public string Path
		{
			get
			{
				return mPath;
			}
		}


		public XmlDocument Dom
		{
			get
			{
				return mDom;
			}
		}


		public int NumSections
		{
			get
			{
				return mNumSections;
			}
		}


		public JtxmlParser()
		{
			mDom = new XmlDocument();
		}

		public List<string> SectionList()
		{

            var result = new List<string>();
			string Item = "";

			foreach (XmlElement curEl in mDom.DocumentElement.SelectNodes("//DjangoTabXML/sections/section/section-texts/track-text[@descriptor='section-name']"))
			{
				Item = curEl.InnerText;

                Item.Replace(Environment.NewLine, " ");
                Item.Replace("  ", " ");
				result.Add(Item);
			}

            return result;

		}
		public void Load(string Path)
		{
			StreamReader Reader = null;
			Object FS = new Object();

			mPath = Path;

			//load and sanitise the text - it doesnt seem to be valid XML :-/
			Reader = new StreamReader(new FileStream(Path, FileMode.Open, FileAccess.Read));
			string Text = Reader.ReadToEnd();
			Text = SanitiseRawXML(Text);

			mDom.LoadXml(Text);

			mNumSections = mDom.DocumentElement.SelectNodes("//DjangoTabXML/sections/section").Count;

		}

        public List<Section> Sections()
        {
            var numSections = NumSections;
            var result = new List<Section>();

            for (var n = 0; n < numSections; n++)
            {
                result.Add(ExtractSection(n + 1));  //expects 1 based index
            }

            return result;
        }

		public Section ExtractSection(int SectionIndex)
		{
			Section Section = new Section();
			Section.Construct(this);

			Section.ExtractSection(SectionIndex);

			return Section;

		}

		private string SanitiseRawXML(string s)
		{
            //for some reason JTXML are not valid XML files, since they state they are utf-8, 
            //yet they contain characters not correctly encoded...
            //this is an e-accute character - it looks ok when viewing with code page, yet not 
            //with utf-8 encoding, hence the xml parser fails
            //not sure what is going on here...

            string result = s;
            string Find = 0xE9.ToString();

            return result.Replace(Find, "é");       //probably plain 'e' is also OK given we dont really use it...

		}
	}
}