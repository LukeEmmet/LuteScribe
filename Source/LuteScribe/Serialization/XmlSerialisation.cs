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

using NAudio.Wave.Compression;
using System;
using System.IO;
using System.Net.Security;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace LuteScribe.Serialization
{
    //based on http://blog.danskingdom.com/saving-and-loading-a-c-objects-data-to-an-xml-json-or-binary-file/

    /// <summary>
    /// Functions for performing common XML Serialization operations.
    /// <para>Only public properties and variables will be serialized.</para>
    /// <para>Use the [XmlIgnore] attribute to prevent a property/variable from being serialized.</para>
    /// <para>Object to be serialized must have a parameterless constructor.</para>
    /// </summary>
    public static class XmlSerialization
    {
        /// <summary>
        /// Writes the given object instance to an XML file.
        /// <para>Only Public properties and variables will be written to the file. These can be any type though, even other classes.</para>
        /// <para>If there are public properties/variables that you do not want written to the file, decorate them with the [XmlIgnore] attribute.</para>
        /// <para>Object type must have a parameterless constructor.</para>
        /// </summary>
        /// <typeparam name="T">The type of object being written to the file.</typeparam>
        /// <param name="filePath">The file path to write the object instance to.</param>
        /// <param name="objectToWrite">The object instance to write to the file.</param>
        /// <param name="append">If false the file will be overwritten if it already exists. If true the contents will be appended to the file.</param>
        public static void WriteToXmlFile<T>(string filePath, T objectToWrite, bool append = false) where T : new()
        {
            //based on https://stackoverflow.com/questions/7461925/xml-loading-from-memory-stream-issue
            //and https://www.c-sharpcorner.com/article/serializing-and-deserializing-xml-string/

            var ser = new XmlSerializer(typeof(T));
            var memStream = new MemoryStream();
            var xmlWriter = new XmlTextWriter(memStream, Encoding.UTF8);
            xmlWriter.Namespaces = true;

            ser.Serialize(xmlWriter, objectToWrite);

            //point back to the start of the stream where we will load from
            memStream.Position = 0;

            var dom = new XmlDocument();
            dom.Load(memStream);

            xmlWriter.Close();
            memStream.Close();
            xmlWriter.Dispose();
            memStream.Dispose();


            TransformToLegacyContent(dom);
            dom.Save(filePath);
        }


        /// <summary>
        /// Reads an object instance from an XML file.
        /// <para>Object type must have a parameterless constructor.</para>
        /// </summary>
        /// <typeparam name="T">The type of object to read from the file.</typeparam>
        /// <param name="filePath">The file path to read the object instance from.</param>
        /// <returns>Returns a new instance of the object read from the XML file.</returns>
        public static T ReadFromXmlFile<T>(string filePath) where T : new()
        {
            var dom = new XmlDocument();
            dom.Load(filePath);

            return LoadXML<T>(dom.OuterXml);
        }

        //map published format to new format - legacy format has a collection of individual headers
        //but current model expects a single HeadersText
        private static void TransformFromLegacyContent(XmlDocument dom)
        {
            foreach (XmlElement piece in dom.DocumentElement.SelectNodes("Pieces/Piece"))
            {
                var headersText = "";
                var headersEl = piece.SelectSingleNode("Headers");

                foreach (XmlElement header in headersEl.SelectNodes("Header/Content")) {
                    headersText += header.InnerText + "\n";
                }

                var headersTextEl = dom.CreateElement("HeadersText");   //add new element
                headersTextEl.InnerText = headersText;
                piece.AppendChild(headersTextEl);

                piece.RemoveChild(headersEl);   //remove legacy element
            }
        }

        private static void TransformToLegacyContent(XmlDocument dom)
        {
            foreach (XmlElement piece in dom.DocumentElement.SelectNodes("Pieces/Piece"))
            {
                var headersEl = dom.CreateElement("Headers");
                piece.AppendChild(headersEl);

                var headersTextEl = piece.SelectSingleNode("HeadersText");
                foreach (string line in headersTextEl.InnerText.Split('\n'))
                {
                    var headerEl = dom.CreateElement("Header");
                    var contentEl = dom.CreateElement("Content");

                    headersEl.AppendChild(headerEl).AppendChild(contentEl);
                    contentEl.InnerText = line;
                }

                piece.RemoveChild(headersTextEl);       //remove unrequired element
            }
        }

        public static T LoadXML<T>(string content) where T: new()
        {

            var dom = new XmlDocument();
            dom.LoadXml(content);

            TransformFromLegacyContent(dom);

            using (var stringReader = new StringReader(dom.OuterXml))
            {
                var serializer = new XmlSerializer(typeof(T));

                return (T)serializer.Deserialize(stringReader);

            }
        }
    }
}
