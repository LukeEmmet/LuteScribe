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
using LuteScribe.Serialization.Commandline;
using System;
using System.IO;

namespace LuteScribe.Serialization.LsmlLoader
{
    public class TabToLsml
    {


        public TabModel LoadTab(string path)
        {

            var appDir = System.AppDomain.CurrentDomain.BaseDirectory;
            //use path.combine to take care of any missing slashes etc...
            var rebolPath = Path.GetFullPath(Path.Combine(appDir, "..\\..\\..\\Utils\\Rebol\\r3-core.exe"));
            var scriptPath = Path.GetFullPath(Path.Combine(appDir, "..\\..\\..\\FileConverters\\TabToXML.r"));

            //due to bug in rebol 3 at the time of writing (late 2017) there is a known bug in rebol 3 in 
            //working with command line parameters, so we need to escape quotes
            //see https://stackoverflow.com/questions/6721636/passing-quoted-arguments-to-a-rebol-3-script
            //also hypens are also problematic, so we base64 encode the whole thing
            var command = String.Format("\"{0}\" -cs \"{1}\" \\\"{2}\\\"", rebolPath, scriptPath, Base64Encode(path));

            var execProcess = new ExecuteProcess();

            var result  = execProcess.LoggedExecute(command);

            var tabModel = XmlSerialization.LoadXML<TabModel>(result.Item2);

            return tabModel;

        }

        /// <summary>
        /// base 64 function from https://stackoverflow.com/questions/11743160/how-do-i-encode-and-decode-a-base64-string
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }
}
