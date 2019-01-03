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

using GenericUndoRedo;
using System.IO;
using LuteScribe.Domain;
using System.Xml.Serialization;
using System;

namespace LuteScribe.ViewModel.Services
{
    class TabModelMemento : IMemento<ITabModelOwner>
    {
        private int _pieceIndex;
        private int _staveIndex;
        private MemoryStream _ms;

        //makes a deep clone of public properties using XMLSerialiser
        public static MemoryStream ToClone(object cloneThis)
        {
            MemoryStream ms = new MemoryStream();
            Type t = cloneThis.GetType();
            try
            {
                XmlSerializer ser = new XmlSerializer(cloneThis.GetType());
                ser.Serialize(ms, cloneThis);
                ms.Flush();
                ms.Position = 0;
            }
            catch (Exception ex)
            {
            }

            return ms;
        }

        public TabModel FromClone(MemoryStream ms)
        {
            XmlSerializer ser = new XmlSerializer(typeof(TabModel));
            return (TabModel)ser.Deserialize(_ms);
        }

        public TabModelMemento(TabModel tabModel, int pieceIndex)
        {
            //record a deep clone of the current model
            _ms = ToClone(tabModel);
            _pieceIndex = pieceIndex;
            _staveIndex = -1;
        }

        public TabModelMemento(TabModel tabModel, int pieceIndex, int staveIndex)
        {
            //record a deep clone of the current model
            _ms = ToClone(tabModel);
            _pieceIndex = pieceIndex;
            _staveIndex = staveIndex;
        }

        #region IMemento<ITabModelOwner> Members

        public IMemento<ITabModelOwner> Restore(ITabModelOwner target)
        {

            IMemento<ITabModelOwner> inverse;
            TabModel tabModel = FromClone(_ms);

            //performance decision here - it can take a while for WPF bound datagrid to reload the content
            //so if possible we just restore the stave not the whole piece
            if (_staveIndex > -1)
            {
                inverse  = new TabModelMemento(target.TabModel, _pieceIndex, _staveIndex);

                //stave restore only
                var restorePiece = tabModel.Pieces[_pieceIndex];
                var restoreStave = restorePiece.Staves[_staveIndex];

                var targetPiece = target.TabModel.Pieces[_pieceIndex];

                //remove from clone and add back into live
                restorePiece.Staves.Remove(restoreStave);
                targetPiece.Staves[_staveIndex] = restoreStave;

            }
            else
            {
                inverse = new TabModelMemento(target.TabModel, _pieceIndex);
                //default - restore whole piece
                target.TabModel = tabModel;
                //show the the right piece
                target.TabModel.ActivePiece = target.TabModel.Pieces[_pieceIndex];
            }


            return inverse;

        }

        #endregion
    }
}
