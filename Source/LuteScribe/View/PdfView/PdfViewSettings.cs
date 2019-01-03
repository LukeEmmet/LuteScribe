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

using MoonPdfLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuteScribe.View.PdfView
{
    public class PdfViewSettings : ObservableObject
    {

        private MoonPdfPanel _panel;


        public PdfViewSettings()
        {
  

        }

        public MoonPdfPanel MoonPdfPanel
        {
           set {
                _panel = value;

                ActivePage = _panel.GetCurrentPageNumber();
                ZoomLevel = _panel.CurrentZoom;

                base.RaisePropertyChangedEvent("ActivePage");
                base.RaisePropertyChangedEvent("NumPages");
                base.RaisePropertyChangedEvent("Zoom");

            }

            get
            {
                return _panel;
            }

        }
        public int ActivePage
        {
            get
            {
                if (_panel == null)
                {
                    return 0;
                }
                return _panel.GetCurrentPageNumber();
            }
            set
            {
                _panel.GotoPage(value);
                base.RaisePropertyChangedEvent("ActivePage");
            }

        }

        public int NumPages
        {
            get
            {
                if (_panel == null)
                {
                    return 0;
                }
                return _panel.TotalPages;
            }

            set { //does nothing at present
            }

        }

        public double ZoomLevel
        {
            get
            {
                if (_panel == null)
                {
                    return 0;
                }
                return _panel.CurrentZoom;
            }
            set
            {
                _panel.Zoom(value);
                base.RaisePropertyChangedEvent("ZoomLevel");
            }

        }
    }
}
