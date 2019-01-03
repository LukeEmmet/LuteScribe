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

using LuteScribe.ViewModel.Commands;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Xml;

namespace LuteScribe.ViewModel
{
    class TabFlagMenus : ObservableObject
    {
        private ObservableCollection<Control> _menuitems;

        public ObservableCollection<Control> MenuItems
        {
            get
            {
                return _menuitems;
            }
            set
            {
                _menuitems = value;
                base.RaisePropertyChangedEvent("MenuItems");

            }
        }

        public TabFlagMenus(string menuPath, MainWindowViewModel viewModel)
        {
            var dom = new XmlDocument();
            dom.Load(menuPath);

            var menuItems = new ObservableCollection<Control>();

            foreach (XmlElement el in dom.DocumentElement.SelectNodes("//TabFlags/Command"))
            {
                if (el.GetAttribute("name") == "-")
                {
                    menuItems.Add(new Separator());
                }
                else
                {
                    var menuItem = new MenuItem();
                    menuItem.Header = el.GetAttribute("flag") + "\t" + el.GetAttribute("name");
                    menuItem.Command = new InsertItemCommand(viewModel);
                    menuItem.CommandParameter = el.GetAttribute("flag");

                    menuItems.Add(menuItem);
                }
            }

            MenuItems = menuItems;
        }
    }
}