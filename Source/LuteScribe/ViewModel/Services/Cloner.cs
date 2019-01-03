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
using System.IO;
using System.Xml.Serialization;

namespace LuteScribe.ViewModel.Services
{
    class Cloner
    {
        //makes a deep clone of public properties using XMLSerialiser
        public static object GetClone(object cloneThis)
        {
            object clone = null;
            Type t = cloneThis.GetType();
          try
            {
                XmlSerializer ser = new XmlSerializer(cloneThis.GetType());
                MemoryStream ms = new MemoryStream();
                ser.Serialize(ms, cloneThis);
                ms.Flush();
                ms.Position = 0;
                clone = ser.Deserialize(ms);
            }
            catch (Exception ex)
            {
            }
            return clone;
        }
    }
}
