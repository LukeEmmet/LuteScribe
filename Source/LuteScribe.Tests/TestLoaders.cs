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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using LuteScribe.Serialization.LsmlLoader;
using System.IO;
using LuteScribe.Serialization.JtxmlLoader;
using LuteScribe.Serialization.FronimoLoader;

namespace LuteScribe.Tests
{
    [TestClass]
    public class TestLoaders
    {
        private string _testData;
        private TabToLsml _tabLoader;
        private JtxmlToLsml _jtxmlLoader;
        private Ft3ToLsml _ft3Loader;

        [TestInitialize]
        public  void SetupTestDataFolder()
        {
            var appDir = System.AppDomain.CurrentDomain.BaseDirectory;
            //use path.combine to take care of any missing slashes etc...
            _testData = Path.GetFullPath(Path.Combine(appDir, "..\\..\\TestData"));
            _tabLoader = new TabToLsml();
            _jtxmlLoader = new JtxmlToLsml();
            _ft3Loader = new Ft3ToLsml();
        }

        [TestMethod]
        public void TestReadMultiSectionTab()
        {

            var model = _tabLoader.LoadTab(Path.Combine(_testData, "MultiSection.tab"));
            Assert.AreEqual(3, model.Pieces.Count);

            //get 2nd piece
            var piece = model.Pieces[1];
            Assert.AreEqual("Aria detto Balletto", piece.Title);
            Assert.AreEqual(7, piece.Headers.Count);

        }

        [TestMethod]
        public void TestReadNoBreakHeaderBodyTab()
        {
            //sample data has no break between end of header and body
            var model = _tabLoader.LoadTab(Path.Combine(_testData, "NoBreakHeadersBody.tab"));
            Assert.AreEqual(1, model.Pieces.Count);

            var stave1 = model.Pieces[0].Staves[0];
            //get first note - should be i
            Assert.AreEqual("i", stave1.Chords[0].Flag);
            //first note is item 4
            var note = stave1.Chords[3];
            Assert.AreEqual("0", note.Flag);
            Assert.AreEqual("h", note.C1);

        }

        [TestMethod]
        public void TestLoadSingleTab()
        {
            var model = _tabLoader.LoadTab(Path.Combine(_testData, "Simple.tab"));
            Assert.AreEqual(1, model.Pieces.Count);
            var piece = model.Pieces[0];
            Assert.AreEqual("Simple piece", piece.Title);
            Assert.AreEqual(2, piece.Staves.Count);
            
            //second stave starts with a "1" flag chord
            Assert.AreEqual("1", piece.Staves[1].Chords[1].Flag);
        }

        [TestMethod]
        public void TestFt3Loader()
        {
            var model = _ft3Loader.LoadFt3(Path.Combine(_testData, "Simple.ft3"));
            Assert.AreEqual(1, model.Pieces.Count);
            Assert.AreEqual("99. Mr. Dowland's Midnight/John Dowland", model.Pieces[0].Title);

            //initially FT3 content comes in as a single stave that 
            //needs to be wrapped.
            Assert.AreEqual(1, model.Pieces[0].Staves.Count);
            
        }

        [TestMethod]
        public void TestFt37thCourseLoader()
        {
            var model = _ft3Loader.LoadFt3(Path.Combine(_testData, "Fretted7thCourse.ft3"));
            Assert.AreEqual(1, model.Pieces.Count, "number of pieces in Fretted7thCourse.ft3 should be 1");

            var piece = model.Pieces[0];

            //initially FT3 content comes in as a single stave that 
            //needs to be wrapped.
            piece.ReflowStaves(45);

            var stave2 = piece.Staves[1];

            
            Assert.AreEqual("a", stave2.Chords[1].C7);
            Assert.AreEqual("c", stave2.Chords[23].C7);

        }

        [TestMethod]
        public void TestFt38thCourseLoader()
        {
            var model = _ft3Loader.LoadFt3(Path.Combine(_testData, "Fretted8thCourse.ft3"));
            Assert.AreEqual(1, model.Pieces.Count, "number of pieces in Fretted8thCourse.ft3 should be 1");

            var piece = model.Pieces[0];

            //initially FT3 content comes in as a single stave that 
            //needs to be wrapped.
            piece.ReflowStaves(45);

            var stave1 = piece.Staves[0];


            Assert.AreEqual("/a", stave1.Chords[2].C7);
            Assert.AreEqual("b", stave1.Chords[20].C7);
            Assert.AreEqual("/c", stave1.Chords[22].C7);
            Assert.AreEqual("/a", stave1.Chords[24].C7);

        }

        [TestMethod]
        public void TestJtxmlLoader()
        {
            var model = _jtxmlLoader.LoadJtxml(Path.Combine(_testData, "Simple.jtxml"));
            Assert.AreEqual(2, model.Pieces.Count, "number of pieces in Simple.jtxml should be 2");

            var piece = model.Pieces[0];
            Assert.AreEqual(2, piece.Staves.Count, "number of staves should be 2");
            Assert.AreEqual("Section 1 name/Section 1 author", piece.Title);

            var firstStave = model.Pieces[0].Staves[0];
            Assert.AreEqual("b", firstStave.Chords[0].Flag);
            Assert.AreEqual("1", firstStave.Chords[1].Flag);
            Assert.AreEqual("a", firstStave.Chords[1].C1);

            //zipped format of the same
            model = _jtxmlLoader.LoadJtxml(Path.Combine(_testData, "Simple.jtz"));
            Assert.AreEqual(2, model.Pieces.Count, "number of pieces in simple.jtz should be 2");
            Assert.AreEqual(2, model.Pieces[0].Staves.Count);
        }
    }
}
