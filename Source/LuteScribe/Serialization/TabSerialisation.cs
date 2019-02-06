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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;

namespace LuteScribe.Serialization
{
    public struct PrettyOptions
    {
        public string FlagStyle;
        public string CharStyle;
        public bool PadEndAndFlourish;

    }
    public static class TabSerialisation
    {
        public static string GenerateTab(ObservableCollection<Header> headers, List<Header> extraHeaders, PrettyOptions prettyOptions, bool prettify)
        {

            var builder = new StringBuilder();

            //emit normal headers
            builder.Append(GenerateTab(headers, prettyOptions, prettify));

            //emit extra headers
            foreach (var header in extraHeaders)
            {
                builder.Append(header.Content.ToString());
                builder.Append("\n");
            }

            return builder.ToString();

        }

        public static string GenerateTab(ObservableCollection<Header> headers, PrettyOptions prettyOptions, bool prettify)
        {

            var builder = new StringBuilder();
            var flagStyleSet = false;
            var charStyleSet = false;

            //determine if the piece already has explicit flag or char styles in which case we 
            //don't overrule them below
            flagStyleSet = HeaderFlagStyleSet(headers);
            charStyleSet = HeaderCharStyleSet(headers);

            //output new defaults before piece headers.
            if (prettify)
            {
                if (!flagStyleSet) { builder.Append("$flagstyle=" + prettyOptions.FlagStyle + "\n"); }
                if (!charStyleSet) { builder.Append("$charstyle=" + prettyOptions.CharStyle + "\n"); }
            }

            foreach (var header in headers)
            {
                builder.Append(header.Content);
                builder.Append("\n");       //output unix/mac style new lines - most samples in this format. TAB accepts both unix and win style

            }


            return builder.ToString();
        }

        public static string GenerateTab(Piece piece, PrettyOptions prettyOptions, bool prettify)
        {
            var builder = new StringBuilder();

            //standard headers
            builder.Append(GenerateTab(piece.Headers, prettyOptions, prettify));

            builder.Append("\n");

            piece.TabModel.SanitiseModel();     //remove empty pieces and staves

            var firstChord = piece.Staves[0].Chords[0];
            var lastStave = piece.Staves[piece.Staves.Count - 1];
            var lastChord = lastStave.Chords[lastStave.Chords.Count - 1];

            //generate body
            builder.Append(GenerateTab(piece.Staves, firstChord, lastChord, prettyOptions, prettify));

            return builder.ToString();
        }

        public static string GenerateTab(ObservableCollection<Stave> staves, Chord startChord, Chord endChord, PrettyOptions prettyOptions, bool prettify)
        {
            var builder = new StringBuilder();
            var hasFlourish = false;
            var totalLength = 0;
            var lastStavePad = 35;  //default when there is only one stave

            
            if (prettify)
            {
                if (staves.Count > 1)
                {
                    //calculate the average stave length of all but the last stave
                    for (var n = 0; n < staves.Count - 1; n++)
                    {
                        var stave = staves[n];
                        totalLength += stave.Chords.Count;
                    }
                    //pad length is 5 (approx length of flourish) less than the
                    //average length of preceding staves
                    lastStavePad = ((totalLength / (staves.Count - 1)) - 5);
                }

            }

            foreach (var stave in staves)
            {
                var staveEmpty = true;

                foreach (var chord in stave.Chords)
                {
                    var chordBuilder = new StringBuilder();

                    if (prettify)
                    {
                        //set a flag to indicate if this stave is empty or just has "e" marker
                        //(new pieces start like this)
                        if (chord.Flag != "e" & chord.ToString().Trim().Length > 0) { staveEmpty = false; }

                        //if it is end of piece of a non-empty last stave with the option set, we pad and add flourish
                        if (chord.Flag == "e" & prettyOptions.PadEndAndFlourish & !staveEmpty & (stave == staves[staves.Count - 1]))
                        {
                            //add a flourish to indicate end
                            if (!hasFlourish)
                            {
                                builder.Append("B\nq\n");
                            }
                            //pad with blank elements out to the pad width
                            for (int n = stave.Chords.Count; n < lastStavePad; n++)
                            {
                                builder.Append("3!" + "\n");
                            }
                            builder.Append("b\n");
                        }
                        //if this piece has a flourish set a flag so we won't add our own
                        if (FlagParser.IsFlourish(chord.Flag))
                        {
                            hasFlourish = true;
                        }

                    }

                    //output chord if we are beyond the start chord's stave or before the end chord's stave
                    //if the start or end chord are on the current chord, we must be between them (inclusive)
                    if (IsBetween(chord, startChord, endChord))
                    {


                        //if chord is empty - denoting a "same as previous", output an x for TAB
                        //which means we don't need them in our own data model
                        chordBuilder.Append("" == chord.Flag.Trim() ? "x" : chord.Flag);

                        //output notes
                        chordBuilder.Append(TabNote(chord.C1));
                        chordBuilder.Append(TabNote(chord.C2));
                        chordBuilder.Append(TabNote(chord.C3));
                        chordBuilder.Append(TabNote(chord.C4));
                        chordBuilder.Append(TabNote(chord.C5));
                        chordBuilder.Append(TabNote(chord.C6));
                        chordBuilder.Append(TabNote(chord.C7));

                        var chordContent = chordBuilder.ToString().TrimEnd();

                        //only output non-empty lines when generating tab, otherwise 
                        //you can get extra stave breaks
                        if (!String.IsNullOrWhiteSpace(chordContent))
                        {
                            builder.Append(chordContent);   //end of chord
                            builder.Append("\n");
                        }
                    }
                }

                builder.Append("\n");                //new stave

            }

            return builder.ToString();
        }

        private static bool IsBetween(Chord chord,  Chord startChord, Chord endChord)
        {
            //assumes start and end chord always on the same stave
            //as at present we cannot have a chord selection that spans multiple staves

            var curChord = ChordIndex(chord);
            var curStave = StaveIndex(chord);
            var startRegionChord = ChordIndex(startChord);
            var startRegionStave = StaveIndex(startChord);
            var endRegionChord = ChordIndex(endChord);
            var endRegionStave = StaveIndex(endChord);

            var result = false;


            //we iterate through something like this
            //0...[....
            //1........
            //2....]...
            //3........

            if (startRegionStave == endRegionStave)
            {
                result = (curStave == startRegionStave &
                          curChord >= startRegionChord &
                          curChord <= endRegionChord);

            }
            else if (curStave == startRegionStave)
            {
                result = (curChord >= startRegionChord);
            }

            else if (curStave == endRegionStave)
            {
                result = (curChord <= endRegionChord);
            }

            else
            {
                result = (curStave > startRegionStave &
                          curStave < endRegionStave);
            }
            


          
            return result;
            

        }

        private static int StaveIndex(Chord chord)
        {
            return chord.Stave.Staves.IndexOf(chord.Stave);
        }

        private static int ChordIndex(Chord chord)
        {
            return chord.Stave.Chords.IndexOf(chord);
        }
        private static bool HeaderCharStyleSet(ObservableCollection<Header> headers)
        {
            var result = false;
            foreach (var header in headers)
            {
                switch (header.Content) {
                    case "-b":
                        result = true;
                        break;
                    case "-n":
                        //mace
                        result = true;
                        break;
                    case "-N":
                        //robinson
                        result = true;
                        break;
                    case "-sItalNotes":
                        //small italian notes
                        result = true;
                        break;
                    case "-fc":
                        //reset to standard style
                        result = true;
                        break;
                    default:
                        if (header.Content != null)
                        {

                            if (Regex.IsMatch(header.Content, "\\$charstyle=.*"))
                            {
                                result = true;
                            }
                        }
                        break;
                     

                }
            }

            return result;
        }

        private static bool HeaderFlagStyleSet(ObservableCollection<Header> headers)
        {
            var result = false;
            foreach (var header in headers)
            {
                switch (header.Content)
                {
                    case "-b":
                        //baroque flags and chars
                        result = true;
                        break;
                    case "-F":
                        //modern style flags
                        result = true;
                        break;
                    case "-ff":
                        //standard flags
                        result = true;
                        break;
                    case "-D":
                        //dowland style
                        result = true;
                        break;
                    case "-italFlags":
                        //italian style
                        result = true;
                        break;
                    case "-O":
                        //dalza and shift notes to centre of line
                        result = true;
                        break;
                    case "-T":
                        //dalza style flags
                        result = true;
                        break;
                    default:

                        if (header.Content != null) {
                            if (Regex.IsMatch(header.Content, "\\$flagstyle=.*"))
                            {
                                result = true;
                            }
                        }
                        break;


                }
            }

            return result;
        }

        public static string GenerateTab(TabModel tabModel, PrettyOptions prettyOptions, bool prettify)
        {
            var builder = new StringBuilder();

            var count = 1;

            foreach (var section in tabModel.Pieces)
            {
                if (count > 1) {
                    //for multi-piece/section files, we output a delimiter as a special TAB comment
                    //which is used on re-loading to identify sections.
                    builder.Append("%=== new section ===\n");
                }

                builder.Append(GenerateTab(section, prettyOptions, prettify));

                count++;
            }

            return builder.ToString();
        }


        private static string TabNote(string cell)
        {
            if (cell == null)
            {
                return " ";
            }

            if (cell == "")
            {
                return " ";
            }

            return cell;
        }

    }
}
