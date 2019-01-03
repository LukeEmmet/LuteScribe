﻿//===================================================
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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuteScribe.Singletons
{

    /// <summary>
    /// simple singleton logger, based on
    /// http://csharpindepth.com/Articles/General/Singleton.aspx#cctor
    /// </summary>
    public class Session : ObservableObject
    {
        private static readonly Session instance = new Session();

        private string _sessionPath;

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static Session()
        {
        }

        private Session()
        {

            CreateSessionFolder();
        }

        public static Session Instance
        {
            get
            {
                return instance;
            }
        }

        
        private void CreateSessionFolder()
        {

            _sessionPath = GetTemporaryDirectory("lutescribe_");

        }

        private string GetTemporaryDirectory(string prefix)
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), prefix + Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }

        private void DestroySessionFolder()
        {
            //assumes the pdf previwer does not retain a lock on any files therein
            //(for example acrobat will, but we use moonpdf)
            GC.Collect();
            Directory.Delete(_sessionPath, true);

        }

        public string SessionPath
        {
            get { return _sessionPath; }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            DestroySessionFolder();
        }

    }
}
