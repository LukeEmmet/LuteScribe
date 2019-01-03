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
using System.Globalization;
using System.Windows.Data;
using System.Windows;
using LuteScribe.Domain;

namespace LuteScribe.ValueConverters
{
    public class FlagBorderThicknessConverter : IMultiValueConverter
    {

        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            Thickness thickness = new Thickness();

            if (value[0] is string & value[1] is int)
            {
                var flag = (string)value[0];
                var seq = (int)value[1];
                var location = (string)parameter;

                thickness.Bottom = 1;


                if (flag is string)
                {
                    var flagValue = (string)flag;

                    if (FlagParser.IsBarLine(flagValue) || FlagParser.IsEnd(flagValue))
                    {
                        if (location == "lower")
                        {
                            if (FlagParser.IsHeavyBarLineFlavour(flagValue) || FlagParser.IsEnd(flagValue))
                            {
                                thickness.Right = 3;
                            }
                            else if (FlagParser.IsSimpleBarLine(flagValue) || FlagParser.IsEnd(flagValue))
                            {
                                thickness.Right = 1;

                            }
                            else
                            {
                                //mixed e.g. .bb or bb
                                thickness.Right = 1;

                            }
                        } else
                        {
                            //top stave never has a right border for a bar
                        }

                        //if we start with a bar line, no lower border
                        if (seq == 0)
                        {
                            thickness.Bottom = 0;
                        }
                    }



                }
            }
            return thickness;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}