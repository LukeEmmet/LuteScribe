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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LuteScribe.Domain
{
    static class FlagParser
    {

        public static bool IsHeavyBarLineFlavour(string flag)
        {
            return CheckRegex(flag, "^[\\.byY]?[B]+[\\.b]?$");
        }

        public static bool IsFlourish(string flag)
        {
            return CheckRegex(flag, "^[qQm]$");
        }

        public static bool IsSimpleBarLine(string flag)
        {
            return (flag == "b");
        }

        public static bool IsHeavyBarLine(string flag)
        {
            return (flag == "B");
        }

        public static bool IsEnd(string flag)
        {
            return (flag == "e");
        }

        public static bool IsContinue(string flag)
        {
            return (flag == "x");
        }
        public static bool IsEmptyOrWhiteSpace(string flag)
        {
            return CheckRegex(flag, "^[ ]+$") || (flag == "");
        }

        public static bool IsRhythm(string flag) {
            return CheckRegex(flag, "^[\\.#yY]?[0-9wWL][\\.#]?$");
        }

        public static bool IsFontedSymbol(string flag)
        {
            return CheckRegex(flag, "^[dcCyY]$");
        }
        public static bool IsBarLine(string flag)
        {
            return CheckRegex(flag, "\\.?[Bb]+\\.?");
        }

        public static bool IsComment(string flag)
        {
            return CheckRegex(flag, "^%");

        }

        /// <summary>
        /// check if flag matches regex, tolerating null flags
        /// </summary>
        /// <param name="flag"></param>
        /// <param name="regexMatch"></param>
        /// <returns></returns>
        private static bool CheckRegex(string flag, string regexMatch)
        {
            var regex = new Regex(regexMatch);

            return (flag != null ? regex.IsMatch(flag) : false);
        }
    }
}
