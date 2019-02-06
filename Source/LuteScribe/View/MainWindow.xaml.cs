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

using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Controls;
using LuteScribe.Domain;
using System.IO;
using LuteScribe.ViewModel.Commands;
using System;
using LuteScribe.Singletons;
using LuteScribe.View;
using System.Collections.Generic;
using System.Collections;

namespace LuteScribe
{


    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        // Member variables
        private int _originalIndex;
        private DataGridRow _oldRow;
        private Chord _targetItem;
        private MainWindowViewModel _viewModel;
        private bool _cellBeingEdited;
        

        public MainWindow()
        {
            InitializeComponent();



        }

        public void LoadPdf(string fileName)
        {


            var moon = this.MoonPdf;
            
            _viewModel = (MainWindowViewModel)DataContext;

            if (_viewModel != null)
            {
                var cache = new Tuple<int, double>(1, 1.15);       //default is to view page 1 at 1.15 magnification (a reasonable default it seems) - 1.2 would be nice, but some bar lines not visible at that magnification
                var settings = _viewModel.PdfViewSettings;
                var panel = settings.MoonPdfPanel;

                if (panel != null && fileName == _viewModel.PreviewPath)
                {
                    cache = new Tuple<int, double>(settings.ActivePage, settings.ZoomLevel);
                }

                //load the file
                moon.OpenFile(fileName);
                settings.MoonPdfPanel = moon;

                settings.ActivePage = cache.Item1;
                settings.ZoomLevel = cache.Item2;

            }

        }

        private static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            var parent = element;
            while (parent != null)
            {
                var correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }
            return null;
        }

        /// <summary>
        /// Updates the grid as a drag progresses
        /// </summary>
        private void OnMainGridCheckDropTarget(object sender, DragEventArgs e)
        {
            var row = FindVisualParent<DataGridRow>(e.OriginalSource as UIElement);

            /* If we are over a row that contains a Chord, set 
             * the drop-line above that row. Otherwise, do nothing. */

            // Set the DragDropEffects 
            if ((row == null) || !(row.Item is Chord))
            {
                e.Effects = DragDropEffects.None;
            }
            else
            {
                var currentIndex = row.GetIndex();

                // Erase old drop-line
                if (_oldRow != null) _oldRow.BorderThickness = new Thickness(0);

                // Draw new drop-line
                var direction = (currentIndex - _originalIndex);
                if (direction < 0) row.BorderThickness = new Thickness(0, 2, 0, 0);
                else if (direction > 0) row.BorderThickness = new Thickness(0, 0, 0, 2);

                // Reset old row
                _oldRow = row;
            }
        }

        /// <summary>
        /// Gets the view model from the data Context and assigns it to a member variable.
        /// </summary>
        private void OnMainGridDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _viewModel = (MainWindowViewModel)this.DataContext;
        }

        /// <summary>
        /// Process a row drop on the DataGrid.
        /// </summary>
        private void OnMainGridDrop(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;

            // Verify that this is a valid drop and then store the drop target
            var row = FindVisualParent<DataGridRow>(e.OriginalSource as UIElement);
            if (row != null)
            {
                _targetItem = row.Item as Chord;
                if (_targetItem != null)
                {
                    e.Effects = DragDropEffects.Move;
                }
            }

            // Erase last drop-line
            if (_oldRow != null) _oldRow.BorderThickness = new Thickness(0, 0, 0, 0);
        }

        /// <summary>
        /// Processes a drag in the main grid.
        /// </summary>
        private void OnMainGridMouseMove(object sender, MouseEventArgs e)
        {
            // Exit if shift key and left mouse button aren't pressed
            if (e.LeftButton != MouseButtonState.Pressed) return;
            if (Keyboard.Modifiers != ModifierKeys.Shift) return;

            /* We use the m_MouseDirection value in the 
             * OnMainGridCheckDropTarget() event handler. */

            // Find the row the mouse button was pressed on
            var row = FindVisualParent<DataGridRow>(e.OriginalSource as FrameworkElement);
            _originalIndex = row.GetIndex();

            // If the row was already selected, begin drag
            if ((row != null) && row.IsSelected)
            {
                // Get the chord represented by the selected row
                var selectedItem = (Chord) row.Item;
                var finalDropEffect = DragDrop.DoDragDrop(row, selectedItem, DragDropEffects.Move);
                if ((finalDropEffect == DragDropEffects.Move) && (_targetItem != null))
                {
                    /* A drop was accepted. Determine the index of the item being 
                     * dragged and the drop location. If they are different, then 
                     * move the selectedItem to the new location. */

                    //// Move the dragged item to its drop position
                    //var oldIndex = _viewModel.Stave.IndexOf(selectedItem);
                    //var newIndex = _viewModel.Stave.IndexOf(_targetItem);
                    //if (oldIndex != newIndex) _viewModel.Stave.Move(oldIndex, newIndex);
                    //_targetItem = null;
                }
            }
        }

        private void OnMainGridKeyDown(object sender, KeyEventArgs e)
        {
        }

        private void ClearCell(DataGridCellInfo cellInfo)
        {
           
                var newValue = "";      //we will clear the content of the underlying property (assumes is a string)

            //check the item corresponds to a chord
            //e.g. the very last one on the stave, is a "New item placeholder".
            if (cellInfo.Item.GetType().Name == "Chord")
            {
                //use reflection to directly set the property to empty string
                //see https://stackoverflow.com/questions/7737345/how-can-i-set-the-value-of-a-datagrid-cell-using-its-column-and-row-index-values
                var chord = (Chord)cellInfo.Item;
                var item = (System.Windows.Data.Binding)((DataGridTextColumn)cellInfo.Column).Binding;
                var propertyName = item.Path.Path;
                var propertyInfo = chord.GetType().GetProperty(propertyName);
                propertyInfo.SetValue(chord, newValue);   //set new value to empty string
            }
        }

        private void OnMainGridPreviewKeyDown(object sender, KeyEventArgs e)
        {
            var grid = (DataGrid)sender;


            var viewModel = (MainWindowViewModel)DataContext;

            //record undo snapshot if user presses delete
            if (e.Key == Key.Delete)
            {
                //what we want is that delete key clears the current cell 
                //so behaviour is similar to a using a spreadsheet.
                //Normally delete key will delete the whole row which is
                //not what we want. We want to get to the underlying propert
                //of the selected cell
                
                //The following delete in the grid will only clear the current cell and not affect other 
                //staves so we just record an undo for the stave
                viewModel.RecordUndoSnapshotStave();

                if (grid.SelectedItems.Count == 1)
                {
                    //only clear the current cell
                    ClearCell(grid.CurrentCell);
                } else
                {
                    //clear all cells in the chords
                    foreach (var cellInfo in grid.SelectedCells)
                    {
                        ClearCell(cellInfo);
                    }
                }

                //mark the event as handled so no further processing takes place
                e.Handled = true;
                
            }



            //since we have rotated the grid we need to handle the cursor keys differently
            switch (e.Key)
            {
                case Key.Left:
                    CommitAnyCellEdits(grid);
                    Send(Key.Up);
                    e.Handled = true;
                    break;
                case Key.Right:
                    CommitAnyCellEdits(grid);
                    Send(Key.Down);
                    e.Handled = true;
                    break;
                case Key.Down:
                    CommitAnyCellEdits(grid);
                    Send(Key.Right);
                    e.Handled = true;
                    break;
                case Key.Up:
                    CommitAnyCellEdits(grid);
                    Send(Key.Left);
                    e.Handled = true;
                    break;
                default:
                    break;
            }

        }

        private void CommitAnyCellEdits(DataGrid grid)
        {
            if (_cellBeingEdited)
            {
                grid.CommitEdit();

            }
        }

        /// <summary>
        ///   Sends the specified key
        ///   from http://stackoverflow.com/questions/11572411/sendkeys-send-method-in-wpf-application
        /// </summary>
        /// <param name="key">The key.</param>
        public static void Send(Key key)
        {
            if (Keyboard.PrimaryDevice != null)
            {
                if (Keyboard.PrimaryDevice.ActiveSource != null)
                {
                    var e = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, key)
                    {
                        RoutedEvent = Keyboard.KeyDownEvent
                    };
                    InputManager.Current.ProcessInput(e);

                    // Note: Based on your requirements you may also need to fire events for:
                    // RoutedEvent = Keyboard.PreviewKeyDownEvent
                    // RoutedEvent = Keyboard.KeyUpEvent
                    // RoutedEvent = Keyboard.PreviewKeyUpEvent
                }
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            //as per advice on https://gist.github.com/KennethanCeyer/08304f69cf513e898fd3
            //the following approach lets us only respond to a specific tab item selection
            //otherwise whenever the tab is updated, or its content changes, there can be many
            //spurious change events

            int tabItem = ((sender as TabControl)).SelectedIndex;
 
            
            if (e.Source is TabControl)     // This is a soultion of those problem.
            {   
                var viewModel = (MainWindowViewModel)DataContext;

                if (viewModel != null)
                {
                    //viewModel.LogMessage("selection changed on tab " + tabItem);

                    if (tabItem == 2)
                    {

                        //there might be some uncommitted edits on the currently edited stave 
                        //so we need to commit them otherwise the preview wont reflect them
                        CommitPendingGridEdits();
                        
                        SimpleLogger.Instance.LogMessage("Pdf tab selected - updating preview");

                        //regenerate the pdf view when its tab is switched to
                        var generatePreview = new PreviewPdfCommand(viewModel);
                        generatePreview.Execute(null);
                    }
                }
            }
        }

        private void CommitPendingGridEdits()
        {
            //use a utility class to delve into the controls tree
            ChildControls ccChildren = new ChildControls();

            //traverse down the tree to find the grids from the collection
            //of grids
            foreach (object o in ccChildren.GetChildren(StaveGrids, 5))
            {
                if (o is DataGrid)
                {
                    var grid = (o as DataGrid);

                    //not sure why we need 2 commits, perhaps the first one commits the cell
                    //then we commit the row or something like that. Otherwise if the cell
                    //was being edited it doesnt get committed back to the data model
                    grid.CommitEdit();
                    grid.CommitEdit();

                }
            }
        }

        private void OnMainGridPreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
        {
            _cellBeingEdited = true;
        }

        private void OnMainGridCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            _cellBeingEdited = false;
        }

        private void OnMainGridLostFocus(object sender, RoutedEventArgs e)
        {
            //this would seem to be the natural place
            //to commit any uncommitted cells, but it appears to be too late.
            //so for now we catch the tab_selected event to commit any unsaved changes...
            //and this is commented out

            //Control ctrl = FocusManager.GetFocusedElement(this) as Control;
            //if (ctrl != null) {
            //    if (ctrl.Parent != null && ctrl.Parent.GetType() != typeof(DataGridCell)) {
            //        CommitPendingGridEdits();
            //    }
            //}
        }

        private void UpdateChordSelected(IList changedItems, bool value)
        {
            foreach (var item in changedItems)
            {
                if (item.GetType().Name == "Chord")     //ignore NewItemPlaceholder at end...
                {
                    var chord = (Chord)item;
                    chord.IsSelected = value;
                }
            }

        }
        private void OnMainGridSelectionChange(object sender, SelectionChangedEventArgs e)
        {
            //update the chords to be marked as selected or not
            UpdateChordSelected(e.AddedItems, true);
            UpdateChordSelected(e.RemovedItems, false);

        }

        private void OnStaveGridsSelectionChange(object sender, SelectionChangedEventArgs e)
        {
            //visually deselect any items on the last stave's grid
            ChildControls ccChildren = new ChildControls();

            //traverse down the tree to find the grids from the collection
            //of grids - we will see if any correspond to the changed selection
            foreach (object o in ccChildren.GetChildren(StaveGrids, 5))
            {
                if (o is DataGrid)
                {
                    var grid = (o as DataGrid);

                    //get the bound stave
                    var curStave = (Stave) grid.DataContext;

                    //deselect any items on the grid of the deseleted stave
                    foreach (var item in e.RemovedItems)
                    {
                        if (item is Stave)
                        {
                            if ((Stave) item == curStave)
                            {
                                //this was a deselected grid - deselect its cells
                                grid.SelectedItems.Clear();
                            }
                        }
                    }

                    //set the selected grid to be the newly selected one
                    foreach (var item in e.AddedItems)
                    {
                        if (item is Stave)
                        {
                            var listBox = (ListBox)sender;
                            listBox.SelectedItem = (Stave)item;
                        }
                    }

                }
            }
        }
    }
}