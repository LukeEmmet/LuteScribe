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
using System.Windows;
using System.Windows.Data;

namespace LuteScribe.ValueConverters
{
    /// <summary>
    /// Workaround for a bug in the WPF DataGrid (WPF Toolkit version).
    /// </summary>
    /// <remarks>The WPF DataGrid (WPF Toolkit version) throws a FormatException if a view model
    /// includes a SelectedItem property. This value converter works aroun that bug. The value
    /// converter is taken from Nigel Spencer's Blog, which identifies the bug and provides this
    /// solution. See: http://blog.spencen.com/2009/04/30/problems-binding-to-selectedvalue-with-microsoftrsquos-wpf-datagrid.aspx?results=1(</remarks>
    public class IgnoreNewItemPlaceHolderConverter : IValueConverter
    {
        private const string NewItemPlaceholderName = "{NewItemPlaceholder}";

        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            return value;
        }

        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            if (value != null && value.ToString() == NewItemPlaceholderName)
            {
                value = DependencyProperty.UnsetValue;
            }
            return value;
        }
    }
}