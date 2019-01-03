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

using JtxmlParserLib;
using LuteScribe.Domain;
using LuteScribe.Singletons;
using LuteScribe.Serialization.Commandline;
using LuteScribe.Serialization.LsmlLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LuteScribe.Serialization.JtxmlLoader
{
    public class JtxmlToLsml
    {

        public TabModel LoadJtxml(string path)
        {
            var sessionPath = Session.Instance.SessionPath;
            var loader = new JtxmlLoader.JtxmlToLsml();
            var appDir = System.AppDomain.CurrentDomain.BaseDirectory;

            var gunzip = Path.GetFullPath(Path.Combine(appDir,"..\\..\\..\\Utils\\gzip124xN\\gzip.exe"));

            var pathToRead = path;

            if (IsJtz(path))
            {
                var guid = Guid.NewGuid();

                var temp = Path.Combine(sessionPath, guid + ".tmp");  // Path.GetTempFileName();

                SimpleLogger.Instance.LogMessage("Creating temp JTXML file");

                File.Copy(path, temp, true);
                 
                SimpleLogger.Instance.LogMessage("Setting temp JTXML file to be readable");

                //make file readable and writeable so unzip will work ok
                File.SetAttributes(temp, FileAttributes.Normal);
                File.SetLastWriteTime(temp, DateTime.Now);

                var ext = Path.GetExtension(temp).Remove(0, 1);     //convert .foo to foo
                var targetPath = Path.Combine(sessionPath, Path.GetFileNameWithoutExtension(temp));  //this will be the target path to read

                //decompress, force output, via stdout (piped into file), use suffix of the temp file
                var unzipCommand = String.Format("\"{0}\" -d -f -S \"{2}\" \"{1}\"", gunzip, temp, ext);
                //that should create targetPath

                //unzip content, checking whether the content was unzipped
                var res = (new ExecuteProcess()).LoggedExecute(unzipCommand);

                //check the exit code, and if unzip failed - may not be a zipped file, 
                //in which case just read it directly, 
                //rather than the expected unzipped target
                pathToRead = (res.Item1 == 0) ? targetPath : temp;

            }

            var guidOut = Guid.NewGuid();
            var outPath = Path.Combine(sessionPath, guidOut + ".tmp");

            SimpleLogger.Instance.LogMessage("Parsing JTXML ");

            var parser = new JtxmlParser();
            parser.Load(pathToRead);

            string tabContent = "";
            var sectionCount = 1;

            foreach (var section in parser.Sections())
            {
                SimpleLogger.Instance.LogMessage("Extracting section " + sectionCount);

                if (sectionCount > 1)
                {
                    //convention used by LuteScribe to parse multiple section TAB files
                    tabContent = tabContent + "%=== new section ===" + Environment.NewLine;
                }

                tabContent = tabContent + section.ToString();

                sectionCount++;

            }

            SimpleLogger.Instance.LogMessage("Writing TAB content to file " + outPath);

            //save out to temp file
            File.WriteAllText(outPath, tabContent);

            SimpleLogger.Instance.LogMessage("Reading TAB content extracted from Fandango JTXML file");

            //the output file will be tab
            var tabLoader = new TabToLsml();
            return tabLoader.LoadTab(outPath);
            
        }

        private bool IsJtz(string path) => (Path.GetExtension(path).ToLower() == ".jtz");
    }
}
