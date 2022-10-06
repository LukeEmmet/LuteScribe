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

using LuteScribe.Domain;
using LuteScribe.Singletons;
using LuteScribe.Serialization.Commandline;
using System;
using System.IO;
using System.Collections.Generic;

namespace LuteScribe.Serialization
{
    public class LuteConvLoader
    {
        public LuteConvLoader()
        {


        }



        public List<string> ListPieces(string path, string format)
        {
            var list = new List<string>();
            var appDir = System.AppDomain.CurrentDomain.BaseDirectory;


            SimpleLogger.Instance.LogMessage("Setting temp " + format + " file to be readable");
            var pathToRead = CopyToTemp(path);

            var luteConvPath = Path.GetFullPath(Path.Combine(appDir, "..\\..\\..\\Luteconv\\luteconv.exe"));
            var command = String.Format("\"{0}\" -s {1} -l \"{2}\"", luteConvPath, format, pathToRead);
            var result = (new ExecuteProcess()).LoggedExecute(command);


            if (result.Item1 == 0)      //check return code
            {
                //split on newlines
                foreach (var entry in result.Item2.Split('\n')) {
                    //format of the lines are "0: the title"
                    //so split into 2 entries on the space
                    var lineSplit = entry.Split(" ".ToCharArray(), 2);
                    if (lineSplit.Length == 2 )
                    {
                        list.Add(lineSplit[1].Trim());
                    }
                }

                return list;
            } else
            {
                return null;
            }
        }


        private string CopyToTemp(string path)
        {
            var sessionPath = Session.Instance.SessionPath;

            var pathToRead = path;

            var guid = Guid.NewGuid();

            var temp = sessionPath + "\\" + guid + ".tmp";  // Path.GetTempFileName();

            SimpleLogger.Instance.LogMessage("Creating temp file: " + temp);

            File.Copy(path, temp, true);


            //make file readable and writeable
            File.SetAttributes(temp, FileAttributes.Normal);
            File.SetLastWriteTime(temp, DateTime.Now);

            return temp;
        }

        /// <summary>
        /// Loads a file using Luteconv *->LSML route
        /// </summary>
        /// <returns>a TabModel of the loaded file</returns>
        public TabModel LoadFromFormat(string path, string format, int index)
        {
            var sessionPath = Session.Instance.SessionPath;
            var appDir = System.AppDomain.CurrentDomain.BaseDirectory;

            SimpleLogger.Instance.LogMessage("Setting temp " + format + " file to be readable");
            var pathToRead = CopyToTemp(path);

            var luteConvPath = Path.GetFullPath(Path.Combine(appDir, "..\\..\\..\\Luteconv\\luteconv.exe"));


            var guidOut = Guid.NewGuid();
            var outPath = sessionPath + "\\" + guidOut + ".tmp";

            var command = String.Format("\"{0}\" -s {1} -d lsml -i {2} \"{3}\" -o \"{4}\"", luteConvPath, format, index, pathToRead, outPath);
            var result = (new ExecuteProcess()).LoggedExecute(command);


            if (result.Item1 == 0)      //check return code
            {
                SimpleLogger.Instance.Log("Reading LSML content extracted from " + format + " file");

                var xml = result.Item2;     //get stdout
                var tabModel = XmlSerialization.ReadFromXmlFile<TabModel>(outPath);
                tabModel.SanitiseModel();

                return tabModel;

            }
            else
            {
                return null;
            }
        }


    } 
}

