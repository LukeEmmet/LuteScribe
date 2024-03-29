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
using System.Linq;
using System.Text;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using LuteScribe.Domain;

namespace LuteScribe.ValueConverters
{
    public class FlagColourConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                var flagValue = (string)value;

                if (FlagParser.IsContinue(flagValue) || 
                    FlagParser.IsSimpleBarLine(flagValue) ||
                    FlagParser.IsHeavyBarLine(flagValue)
                    )
                {
                    return Brushes.White;
                }

                if (FlagParser.IsBarLine(flagValue))
                {
                    return Brushes.DarkGreen;
                }
                if (FlagParser.IsEnd(flagValue))
                {
                    return Brushes.Red;
                }

                if (FlagParser.IsComment(flagValue))
                {
                    return Brushes.Gray;
                }
            }
            return Brushes.Black;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}