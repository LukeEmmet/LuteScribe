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

namespace LuteScribe.Serialization.FronimoLoader
{
    public class Ft3ToLsml
    {
        public Ft3ToLsml()
        {


        }

        public TabModel LoadFt3(string path)
        {
            var sessionPath = Session.Instance.SessionPath;
            var appDir = System.AppDomain.CurrentDomain.BaseDirectory;
            var rebolPath = Path.GetFullPath(Path.Combine(appDir, "..\\..\\..\\Utils\\Rebol\\rebol.exe"));

            //rebol 2 on windows 10 x64 has some limitations in that it cannot seem to load
            //scripts that are in a protected area of windows (e.g. beneath program files)
            //by passing in the full path e.g. c:\program files(x86)\foo\bar\script.r
            //however it works ok if you pass in a relative path - which is what we do instead
            var scriptPath = "..\\..\\..\\FileConverters\\FronimoToXML.r";       //dont build full path to this - use relative path otherwise can fail to load on win10 x64
            var gunzip = Path.GetFullPath(Path.Combine(appDir, "..\\..\\..\\Utils\\gzip124xN\\gzip.exe"));

            var pathToRead = path;

            if (IsFt3(path))
            {
                var guid = Guid.NewGuid();

                var temp = sessionPath + "\\" + guid + ".tmp";  // Path.GetTempFileName();

                SimpleLogger.Instance.LogMessage("Creating temp Ft3 file");

                File.Copy(path, temp, true);

                SimpleLogger.Instance.LogMessage("Setting temp Ft3 file to be readable");

                //make file readable and writeable so unzip will work ok
                File.SetAttributes(temp, FileAttributes.Normal);
                File.SetLastWriteTime(temp, DateTime.Now);

                var ext = Path.GetExtension(temp).Remove(0, 1);     //convert .foo to foo
                var targetPath = sessionPath + "\\" + Path.GetFileNameWithoutExtension(temp);  //this will be the target path to read

                //decompress, force output, via stdout (piped into file), use suffix of the temp file
                var unzipCommand = String.Format("\"{0}\" -d -f -S \"{2}\" \"{1}\"", gunzip, temp, ext);
                //that should create targetPath

                //unzip content, checking whether the content was unzipped
                var res = (new ExecuteProcess()).LoggedExecute(unzipCommand);

                //check the exit code, and if unzip failed - may not be a zip styled fronimo file, just read it directly, 
                //rather than the expected unzipped target
                pathToRead = (res.Item1 == 0) ? targetPath : temp;

            }

            var guidOut = Guid.NewGuid();
            var outPath = sessionPath + "\\" + guidOut + ".tmp";

            var command = String.Format("\"{0}\" -cs \"{1}\" \"{2}\" \"{3}\"", rebolPath, scriptPath, pathToRead, outPath);
            var result = (new ExecuteProcess()).LoggedExecute(command);


            if (result.Item1 == 0)      //check return code
            {
                SimpleLogger.Instance.Log("Reading LSML content extracted from Fronimo file");

                var xml = result.Item2;     //get stdout
                var tabModel = XmlSerialization.ReadFromXmlFile<TabModel>(outPath);
                tabModel.SanitiseModel();

                return tabModel;

            } else {
                return null;
            }

        }



        public bool IsFt3(string path)
        {
            return (Path.GetExtension(path).ToLower() == ".ft3");
        }

    }
}

