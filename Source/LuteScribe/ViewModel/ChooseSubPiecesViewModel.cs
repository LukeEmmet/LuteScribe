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

using System.Collections.ObjectModel;
using System.Windows.Input;
using LuteScribe.ViewModel.Commands;
using System.Linq;
using System.Collections.Generic;
using LuteScribe.Domain;
using LuteScribe.ViewModel;
using System.Windows.Controls;
using LuteScribe.Properties;
using LuteScribe.View.PdfView;
using System.Diagnostics;
using LuteScribe.Singletons;
using LuteScribe.ViewModel.Services;
using GenericUndoRedo;
using LuteScribe.Audio;
using System.Reflection;

namespace LuteScribe.ViewModel
{
    public class SubPiece : ObservableObject
    {
        private string _title;
        private int _index;
        
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                base.RaisePropertyChangedEvent("Title");
            }
        }
        public int Index
        {
            get
            {
                return _index;
            }
            set
            {
                _index = value;
                base.RaisePropertyChangedEvent("Index");
            }
        }
    }


    public class ChooseSubPiecesViewModel : ViewModelBase
    {


        private ObservableCollection<SubPiece> _subPieces;
        private SubPiece _selected;

        private MainWindowViewModel _mainWindowViewModel;

        public ChooseSubPiecesViewModel(List<string> pieces)
        {
            _subPieces = new ObservableCollection<SubPiece>();

            var index = 0;
            foreach (var piece in pieces)
            {
                var subPiece = new SubPiece() { Index = index, Title = piece };

                _subPieces.Add(subPiece);

               //select the first one
               if (index == 0 )
                {
                    Selected = subPiece;
                }

                index++;
            }

        }

        public  SubPiece Selected
        {
            get
            {
                return _selected;
            }

            set { 
                _selected = value;
                base.RaisePropertyChangedEvent("Selected");

            }
        }
        public MainWindowViewModel MainWindowViewModel
        {
            set
            {
                _mainWindowViewModel = value;
            }
        }

        public ObservableCollection<SubPiece> SubPieces
        {
            get
            {
                return _subPieces;
            }
        }

        public void CloseWindow()
        {
            //pass up to mainviewmodel which holds a ref to this window
            _mainWindowViewModel.CloseChooseSubPieceWindow();
        }



    }


}

