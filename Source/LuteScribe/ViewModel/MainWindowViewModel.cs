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

namespace LuteScribe
{
    public class MainWindowViewModel : ViewModelBase, ITabModelOwner
    {

        // Property variables
        private Stave _selectedItem;
        private int _selectedTab;
        private string _path;
        private TabModel _tabModel;
        private ObservableCollection<Control> _tabFlagMenus;
        private Settings _settings;
        private string _previewPath;
        private PdfViewSettings _pdfViewSettings;
        private int _currentSection;
        private ObservableCollection<Control> _sectionMenus;
        private ObservableCollection<Control> _lastFileMenus;
        private bool _hasSections;
        private UndoRedoHistory<ITabModelOwner> _history;

        public MainWindowViewModel()
        {
            this.Initialize();
        }



        /// <summary>
        /// Deletes the currently-selected item from the Stave.
        /// </summary>
        public ICommand NewFile { get; set; }
        public ICommand SaveFile { get; set; }
        public ICommand SaveXml { get; set; }
        public ICommand SaveTabModel { get; set; }
        public ICommand SaveTabPiece { get; set; }
        public ICommand OpenFile { get; set; }
        public ICommand OpenXml { get; set; }
        public ICommand OpenTab { get; set; }
        public ICommand OpenFronimo { get; set; }
        public ICommand OpenFandango { get; set; }
        public ICommand RevertFile { get; set; }
        public ICommand CreatePs { get; set; }
        public ICommand CreatePdf { get; set; }
        public ICommand PreviewPdf { get; set; }
        public ICommand PrintPdf { get; set; }
        public ICommand PdfViewerNavigate { get; set; }
        public ICommand NavigateSection { get; set; }
        public ICommand NavigatePdfPage { get; set; }
        public ICommand AssociateFileExtensions { get; set; }
        public ICommand DeleteItem { get; set; }
        public ICommand Undo { get; set; }
        public ICommand Redo { get; set; }
        public ICommand InsertItemBefore { get; set; }
        public ICommand InsertItemAfter { get; set; }
        public ICommand PasteParse { get; set; }

        public ICommand DeleteStaveEnd { get; set; }
        public ICommand InsertStaveBreak { get; set; }

        public ICommand Reflow { get; set; }
        public ICommand ShiftStaveFocus { get; set; }
        public ICommand ApplicationQuit { get; set; }
        public ICommand NewStave { get; set; }
        public ICommand StripComments { get; set; }
        public ICommand GridStyleSwitcher { get; set; }

        public ICommand ShowHelp { get; set; }
        public ICommand ShowHelpAbout { get; set; }
        public ICommand LaunchFile { get; set; }

        public UndoRedoHistory<ITabModelOwner> History
        {
            get { return _history; }
            set { _history = value; }
        }

        /// <summary>
        /// The currently-selected Stave.
        /// </summary>
        public Stave SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                base.RaisePropertyChangedEvent("SelectedItem");
            }

        }

        public int CurrentSection
        {
            get { return _currentSection; }
            set
            {
                _currentSection = value;
                base.RaisePropertyChangedEvent("CurrentSection");
            }
        }

        public ObservableCollection<Control> LastFileMenus
        {
            get { return _lastFileMenus; }
            set
            {
                _lastFileMenus = value;
                base.RaisePropertyChangedEvent("LastFileMenus");

            }
        }
        public ObservableCollection<Control> TabFlagMenus
        {
            get { return _tabFlagMenus; }
            set
            {
                _tabFlagMenus = value;
                base.RaisePropertyChangedEvent("TabFlagMenus");
            }
        }

        public bool HasMultipleSections
        {
            get {

                return _hasSections;

            }
            set {
                _hasSections = value;
                base.RaisePropertyChangedEvent("HasMultipleSections");
            }
        }
        public ObservableCollection<Control> SectionMenus
        {
            get { return _sectionMenus; }
            set
            {
                _sectionMenus = value;
                base.RaisePropertyChangedEvent("SectionMenus");
            }
        }

        public TabModel TabModel
        {
            get { return _tabModel; }
            set
            {
                _tabModel = value;
                base.RaisePropertyChangedEvent("TabModel");
            }

        }

        public SimpleLogger DebugLog
        {
            get { return SimpleLogger.Instance; }
        }

        public bool PadEndAndFlourish
        {
            get
            {
                return _settings.PadEndAndFlourish;
            }

            set
            {
                _settings.PadEndAndFlourish = value;
                _settings.Save();
                base.RaisePropertyChangedEvent("PadEndAndFlourish");
            }
        }

        public int StaveWrap
        {
            get {
                return _settings.StaveWrap;
            }
            set
            {
                _settings.StaveWrap = value;
                _settings.Save();
                base.RaisePropertyChangedEvent("StaveWrap");
            }
        }

        public bool ViewTwoPages
        {
            get
            {
                return _settings.ViewTwoPages;
            }
            set
            {
                _settings.ViewTwoPages = value;
                _settings.Save();
                base.RaisePropertyChangedEvent("ViewTwoPages");
            }

        }
        public int SelectedTab
        {
            get
            {
                return _selectedTab;
            }
            set
            {
                _selectedTab = value;
                base.RaisePropertyChangedEvent("SelectedTab");
            }
        }
        public string Path
        {
            get { return _path; }
            set
            {
                _path = value;
                WindowTitle = value;        //propagate to window Title

                base.RaisePropertyChangedEvent("Path");
            }
        }

        public string PreviewPath
        {
            set
            {
                _previewPath = value;
                base.RaisePropertyChangedEvent("PreviewPath");
            }
            get
            {
                return _previewPath;
            }
        }

        public PdfViewSettings PdfViewSettings
        {
            set {
                _pdfViewSettings = value;
                base.RaisePropertyChangedEvent("PdfViewSettings");
            }
            get
            {
                return _pdfViewSettings;
            }
        }
        public string WindowTitle
        {
            get {

                System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                string version = fvi.FileVersion;

                if (_path == null)
                {
                    return "LuteScribe v" + version;
                }
                else
                {
                    return (System.IO.Directory.GetParent(_path).Name + "\\" + System.IO.Path.GetFileName(_path) + " - LuteScribe v" + version);
                }
            }
            set
            {
                //just notify
                base.RaisePropertyChangedEvent("WindowTitle");
            }
        }

        public string FlagStyle {
            get {
                return _settings.FlagStyle;
            }
            internal set {
                _settings.FlagStyle = value;
                _settings.Save();
                base.RaisePropertyChangedEvent("FlagStyle");
            }
        }

        public string CharStyle
        {
            get
            {
                return _settings.CharStyle;
            }
            internal set
            {
                _settings.CharStyle = value;
                _settings.Save();
                base.RaisePropertyChangedEvent("CharStyle");
            }
        }


        /// <summary>
        /// Initializes this application.
        /// </summary>
        private void Initialize()
        {
            // Initialize commands
            this.DeleteItem = new DeleteItemCommand(this);
            this.DeleteStaveEnd = new DeleteStaveEndCommand(this);
            this.InsertStaveBreak = new InsertStaveBreakCommand(this);
            this.InsertItemAfter = new InsertItemAfterCommand(this);
            this.InsertItemBefore = new InsertItemBeforeCommand(this);
            this.SaveFile = new SaveFileCommand(this);
            this.SaveXml = new SaveXmlCommand(this);
            this.SaveTabPiece = new SaveTabPieceCommand(this);
            this.SaveTabModel = new SaveTabModelCommand(this);
            this.OpenFile = new OpenFileCommand(this);
            this.OpenXml = new OpenXmlCommand(this);
            this.OpenTab = new OpenTabCommand(this);
            this.OpenFronimo = new OpenFronimoCommand(this);
            this.OpenFandango = new OpenFandangoCommand(this);
            this.CreatePs = new CreatePsCommand(this);
            this.CreatePdf = new CreatePdfCommand(this);
            this.PreviewPdf = new PreviewPdfCommand(this);
            this.NavigatePdfPage = new NavigatePdfPageCommand(this);
            this.NavigateSection = new NavigateSectionCommand(this);
            this.PrintPdf = new PrintPdfCommand(this);
            this.Reflow = new ReflowCommand(this);
            this.NewFile = new NewFileCommand(this);
            this.ShiftStaveFocus = new ShiftStaveFocusCommand(this);
            this.Undo = new UndoCommand(this);
            this.Redo = new RedoCommand(this);
            this.PasteParse = new PasteCommand(this);
            this.ApplicationQuit = new ApplicationQuitCommand(this);
            this.AssociateFileExtensions = new AssociateFileExtensionsCommand(this);
            this.NewStave = new NewStaveCommand(this);
            this.RevertFile = new RevertFileCommand(this);
            this.StripComments = new StripCommentsCommand(this);
            this.GridStyleSwitcher = new GridStyleSwitcherCommand(this);
            this.ShowHelp = new ShowHelpCommand(this);
            this.ShowHelpAbout = new ShowHelpAboutCommand(this);
            this.LaunchFile = new LaunchFileCommand(this);
            this.PdfViewerNavigate = new PdfViewerNavigate(this);

            var MenuLoader = new TabFlagMenus(System.AppDomain.CurrentDomain.BaseDirectory + "Resources\\TabFlags.xml", this);

            this.History = new UndoRedoHistory<ITabModelOwner>(this);
            this.TabFlagMenus = MenuLoader.MenuItems;

            _pdfViewSettings = new PdfViewSettings();
            _settings = new Settings();

            SelectedTab = 0;   //staves tab

            NewFile.Execute(this);

            RefreshLastFilesMenu();

            // Update bindings
            base.RaisePropertyChangedEvent("TabModel");

        }

        public void CheckSectionMenu(int itemToCheck)
        {
            int n = 0;
            foreach (var s in TabModel.Pieces)
            {
                MenuItem menu = (MenuItem) SectionMenus[n];

                menu.IsChecked = (itemToCheck == n);
                n++;
            }

        }

        public void LoadSectionMenus()
        {
            var menuItems = new ObservableCollection<Control>();
            int n = 0;
            foreach (var s in TabModel.Pieces)
            {
                var menuItem = new MenuItem();
                menuItem.Header = (1 + n).ToString() + " - " + s.Title;     //add index to make unique in case sections have same name

                menuItem.Command = new NavigateSectionCommand(this);
                menuItem.CommandParameter = n.ToString();
                menuItem.IsChecked = (n == CurrentSection);
                menuItems.Add(menuItem);

                n++;
            }
            SectionMenus = menuItems;

            
        }

        public void UpdateLastFiles(string path)
        {
            const int numShown = 5;     //show 5 items in the list
            var useFiles = new List<string>(numShown);

            useFiles.Add(path); //add the new one
            //add any persisted ones not already in the list
            useFiles.AddRange(from string f in _settings.RecentFiles where !useFiles.Contains(f) select f) ;

            //trim to length
            if (useFiles.Count > numShown)
            {
                //trim all items beyond the last required item
                useFiles.RemoveRange(numShown, useFiles.Count - numShown);
            }

            //update the settings
            _settings.RecentFiles.Clear();
            _settings.RecentFiles.AddRange(useFiles.ToArray());
            _settings.Save();
           
            RefreshLastFilesMenu();
        }

        public void RefreshLastFilesMenu()
        {
            var menuItems = new ObservableCollection<Control>();

            int n = 0;

            foreach (var lastFile in _settings.RecentFiles)
            {
                var menuItem = new MenuItem();
                menuItem.Header =  "_" + (n + 1).ToString() + ":  " + lastFile;     //add index to make unique in case sections have same name

                menuItem.Command = new OpenFileCommand(this);
                menuItem.CommandParameter = lastFile;
                menuItems.Add(menuItem);

                n++;
            }

            LastFileMenus = menuItems;
        }

        public void ShowSections(int checkedSection)
        {
            LoadSectionMenus();
            CheckSectionMenu(checkedSection);

            HasMultipleSections = (TabModel.Pieces.ToArray().Length > 1);
        }

        public void RecordUndoSnapshotStave()
        {
            var piece = TabModel.ActivePiece;
            var pieceIndex = TabModel.Pieces.IndexOf(piece);
            var staveIndex = piece.Staves.IndexOf(piece.SelectedItem);

            History.Do(new TabModelMemento(TabModel, pieceIndex, staveIndex));


        }

        public void RecordUndoSnapshot()
        {
            var piece = TabModel.ActivePiece;
            var pieceIndex = TabModel.Pieces.IndexOf(piece);

            History.Do(new TabModelMemento(TabModel, pieceIndex));


        }
    }

}

