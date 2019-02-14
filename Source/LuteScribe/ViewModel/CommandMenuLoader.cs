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

using LuteScribe.ViewModel.Commands;
using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;

namespace LuteScribe.ViewModel
{
    class CommandMenuLoader : ObservableObject
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

        public CommandMenuLoader(string menuPath, MainWindowViewModel viewModel,  Func<MainWindowViewModel, ICommand> commandGenerator)
        {
            var dom = new XmlDocument();
            dom.Load(menuPath);

            var menuItems = new ObservableCollection<Control>();

            foreach (XmlElement xSection in dom.DocumentElement.SelectNodes("//CommandMenus/Section"))
            {
                var sectionMenu = new MenuItem();
                
                sectionMenu.Header = xSection.GetAttribute("name");
                menuItems.Add(sectionMenu);

                foreach (XmlElement el in xSection.SelectNodes("Command")) 
                {
                    if (el.GetAttribute("name") == "-")
                    {
                        sectionMenu.Items.Add(new Separator());
                    }
                    else
                    {
                        var menuItem = new MenuItem();
                        sectionMenu.Items.Add(menuItem);

                        if (el.GetAttribute("flag").Length > 0)
                        {
                            menuItem.Header = el.GetAttribute("flag") + "\t" + el.GetAttribute("name");
                            menuItem.Command = commandGenerator(viewModel);
                            menuItem.CommandParameter = el.GetAttribute("flag");
                        } else
                        {
                            //simply show some info, but dont enable the menu
                            menuItem.Header = el.GetAttribute("name");
                            menuItem.IsEnabled = false;
                        }
                    }
                }
            }

            MenuItems = menuItems;
        }
    }
}
