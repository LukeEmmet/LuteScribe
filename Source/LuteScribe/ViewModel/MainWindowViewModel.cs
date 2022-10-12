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
using LuteScribe.View;
using System.Windows;
using System;
using LuteScribe.Audio;
using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;
using ToastNotifications.Messages;


namespace LuteScribe
{
    public class MainWindowViewModel : ViewModelBase, ITabModelOwner, IDisposable
    {

        // Property variables
        private FileAssociateViewModel _fileAssociateViewModel;
        private FileAssociateWindow _fileAssociateWindow;
        private ChooseSubPiecesViewModel _chooseSubPiecesViewModel;
        private ChoosePieceWindow _chooseSubPieceWindow;
        private Stave _selectedItem;
        private int _selectedTab;
        private string _path;
        private TabModel _tabModel;
        private ObservableCollection<Control> _tabFlagMenus;
        private ObservableCollection<Control> _tabHeaderMenus;
        private Settings _settings;
        private string _previewPath;
        private PdfViewSettings _pdfViewSettings;
        private int _currentSection;
        private ObservableCollection<Control> _sectionMenus;
        private ObservableCollection<Control> _lastFileMenus;
        private bool _hasSections;
        private UndoRedoHistory<ITabModelOwner> _history;
        private Notifier _notifier;

        private string _playbackPath;
        private readonly AudioPlayback _audioPlayback;
        private bool _playbackVisible;


        public enum ToastMessageStyles
        {
            Information, Warning, Error,
            Success
        }
        public MainWindowViewModel()
        {
            this.Initialize();

            //TODO - move these out of this method if possible to be in initialize with others
            //but seems not possible whilst readonly
            PlayCommand = new DelegateCommand(Play);
            StopCommand = new DelegateCommand(Stop);
            PauseCommand = new DelegateCommand(Pause);

            (PlayCommand as DelegateCommand).IsEnabled = false;
            (StopCommand as DelegateCommand).IsEnabled = false;
            (PauseCommand as DelegateCommand).IsEnabled = false;
            PlaybackVisible = false;

            _audioPlayback = new AudioPlayback();


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
        public ICommand OpenLuteConv { get; set; }
        public ICommand CreatePs { get; set; }
        public ICommand CreatePdf { get; set; }
        public ICommand PreviewPdf { get; set; }
        public ICommand PrintPdf { get; set; }
        public ICommand PdfViewerNavigate { get; set; }
        public ICommand NavigateSection { get; set; }
        public ICommand NavigatePdfPage { get; set; }
        public ICommand ShowFileAssociateWindow { get; set; }
        public ICommand DeleteItem { get; set; }
        public ICommand CopyItems { get; set; }
        public ICommand CutItems { get; set; }
        public ICommand Undo { get; set; }
        public ICommand Redo { get; set; }
        public ICommand InsertItemBefore { get; set; }
        public ICommand InsertItemAfter { get; set; }
        public ICommand PasteParse { get; set; }

        public ICommand DeleteStaveEnd { get; set; }
        public ICommand DeleteStave { get; set; }
        public ICommand InsertStaveBreak { get; set; }

        public ICommand Reflow { get; set; }
        public ICommand ShiftStaveFocus { get; set; }
        public ICommand ApplicationQuit { get; set; }
        public ICommand NewStave { get; set; }
        public ICommand StripComments { get; set; }
        public ICommand GridStyleSwitcher { get; set; }
        public ICommand PlayPiece { get; set; }

        public ICommand PlayCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand StopCommand { get; }

        public ICommand ShowHelp { get; set; }
        public ICommand ShowHelpAbout { get; set; }
        public ICommand LaunchFile { get; set; }

        public UndoRedoHistory<ITabModelOwner> History
        {
            get { return _history; }
            set { _history = value; }
        }

        public void ToastNofify( string message, ToastMessageStyles style)
        {
            try
            {
                if (style == ToastMessageStyles.Information) { _notifier.ShowInformation(message); }
                if (style == ToastMessageStyles.Error) { _notifier.ShowError(message); }
                if (style == ToastMessageStyles.Success) { _notifier.ShowSuccess(message); }
                if (style == ToastMessageStyles.Warning) { _notifier.ShowWarning(message); }

            }
            catch
            {
                //for example main window might not be visible yet, so just ignore those.
            }
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

        public ObservableCollection<Control> TabHeaderMenus
        {
            get { return _tabHeaderMenus; }
            set
            {
                _tabHeaderMenus = value;
                base.RaisePropertyChangedEvent("TabHeaderMenus");
            }
        }

        public bool TabStavesSelected
        {
            get { return SelectedTab == 0; }
            set { SelectedTab = value ? 0 : SelectedTab; }
        }

        public bool TabHeadersSelected
        {
            get { return SelectedTab == 1; }
            set { SelectedTab = value ? 1 : SelectedTab; }
        }

        public bool TabPDFSelected
        {
            get { return SelectedTab == 2; }
            set { SelectedTab = value ? 2 : SelectedTab; }
        }

        public bool TabOptionsSelected
        {
            get { return SelectedTab == 3; }
            set { SelectedTab = value ? 3 : SelectedTab; }
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
                //bound stave wrap between 20 and 80
                const int min = 20;
                const int max = 80;
                int result = value;
                if (value < min) { result = min; }
                if (value > max) { result = max; }

                _settings.StaveWrap = result;
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

                //notify related properties...
                base.RaisePropertyChangedEvent("TabStavesSelected");
                base.RaisePropertyChangedEvent("TabHeadersSelected");
                base.RaisePropertyChangedEvent("TabPDFSelected");
                base.RaisePropertyChangedEvent("TabOptionsSelected");
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
                    return (System.IO.Directory.GetParent(_path).Name + "\\" + System.IO.Path.GetFileName(_path) + " - LuteScribe");
                }
            }
            set
            {
                //just notify
                base.RaisePropertyChangedEvent("WindowTitle");
            }
        }

        public string FlagStyle
        {
            get
            {
                return _settings.FlagStyle;
            }
            set
            {
                _settings.FlagStyle = value;
                _settings.Save();
                base.RaisePropertyChangedEvent("FlagStyle");
            }
        }

        public string PlaybackPatch
        {
            get
            {
                return _settings.PlaybackPatch;
            }
            set
            {
                _settings.PlaybackPatch = value;
                _settings.Save();
                base.RaisePropertyChangedEvent("PlaybackPatch");
            }
        }

        public string PlaybackSpeed
        {
            get
            {
                return _settings.PlaybackSpeed;
            }
            set
            {
                _settings.PlaybackSpeed = value;
                _settings.Save();
                base.RaisePropertyChangedEvent("PlaybackSpeed");
            }
        }
        public string CharStyle
        {
            get
            {
                return _settings.CharStyle;
            }
            set
            {
                _settings.CharStyle = value;
                _settings.Save();
                base.RaisePropertyChangedEvent("CharStyle");
            }
        }


        internal void Play()
        {

            if (_playbackPath != null)
            {
                _audioPlayback.Play();
            }

        }

        internal void Stop()
        {
            _audioPlayback.Stop();
        }


        internal void Pause()
        {
            _audioPlayback.Pause();

        }

        public bool PlaybackVisible
        {
            get { return _playbackVisible; }
            set {
                _playbackVisible = value;
                RaisePropertyChangedEvent("PlaybackVisible");
            }
        }


        public string PlaybackPath
        {
            get
            {
                return _playbackPath;
            }

            set
            {
                _playbackPath = value;

                _audioPlayback.Load(_playbackPath);

                (PlayCommand as DelegateCommand).IsEnabled = true;
                (PauseCommand as DelegateCommand).IsEnabled = true;
                (StopCommand as DelegateCommand).IsEnabled = true;
                PlaybackVisible = true;

                RaisePropertyChangedEvent("PlaybackPath");
            }
        }
        /// <summary>
        /// Initializes this application.
        /// </summary>
        private void Initialize()
        {



            // Initialize commands
            //on file Menu
            this.NewFile = new NewFileCommand(this);
            this.SaveFile = new SaveFileCommand(this);
            this.SaveXml = new SaveXmlCommand(this);
            this.SaveTabPiece = new SaveTabPieceCommand(this);
            this.SaveTabModel = new SaveTabModelCommand(this);
            this.OpenFile = new OpenFileCommand(this);
            this.OpenXml = new OpenXmlCommand(this);
            this.OpenTab = new OpenTabCommand(this);
            this.OpenLuteConv = new OpenLuteConvCommand(this);
            this.CreatePs = new CreatePsCommand(this);
            this.CreatePdf = new CreatePdfCommand(this);
            this.PreviewPdf = new PreviewPdfCommand(this);
            this.NavigatePdfPage = new NavigatePdfPageCommand(this);
            this.NavigateSection = new NavigateSectionCommand(this);
            this.PrintPdf = new PrintPdfCommand(this);
            this.PdfViewerNavigate = new PdfViewerNavigate(this);
            this.ApplicationQuit = new ApplicationQuitCommand(this);

            //on edit menu
            this.DeleteItem = new DeleteItemsCommand(this);
            this.CopyItems = new CopyItemsCommand(this);
            this.CutItems = new CutItemsCommand(this);
            this.Undo = new UndoCommand(this);
            this.Redo = new RedoCommand(this);
            this.PasteParse = new PasteCommand(this);

            //on stave menu
            this.DeleteStaveEnd = new DeleteStaveEndCommand(this);
            this.DeleteStave = new DeleteStaveCommand(this);
            this.InsertStaveBreak = new InsertStaveBreakCommand(this);
            this.InsertItemAfter = new InsertItemAfterCommand(this);
            this.InsertItemBefore = new InsertItemBeforeCommand(this);
            this.NewStave = new NewStaveCommand(this);

            //on tools menu
            this.Reflow = new ReflowCommand(this);
            this.StripComments = new StripCommentsCommand(this);
            this.GridStyleSwitcher = new GridStyleSwitcherCommand(this);
            this.PlayPiece = new PlayPieceCommand(this);

            //on help menu
            this.ShowHelp = new ShowHelpCommand(this);
            this.ShowHelpAbout = new ShowHelpAboutCommand(this);

            //on options tab
            this.ShowFileAssociateWindow = new ShowFileAssociateWindowCommand(this);


            //misc/other commands


            this.LaunchFile = new LaunchFileCommand(this);
            this.ShiftStaveFocus = new ShiftStaveFocusCommand(this);

            _fileAssociateViewModel = new FileAssociateViewModel();

            //create anon functions that return commands for inserting into menus...
            Func<MainWindowViewModel, InsertItemAfterCommand> NewInsertAfter = (vm) => { return new InsertItemAfterCommand(vm); };
            Func<MainWindowViewModel, InsertHeaderCommand> NewInsertHeader = (vm) => { return new InsertHeaderCommand(vm); };

            var FlagMenuLoader = new CommandMenuLoader(System.AppDomain.CurrentDomain.BaseDirectory + "Resources\\TabFlags.xml", this, NewInsertAfter);
            var HeaderMenuLoader = new CommandMenuLoader(System.AppDomain.CurrentDomain.BaseDirectory + "Resources\\TabHeaders.xml", this, NewInsertHeader);

            this.History = new UndoRedoHistory<ITabModelOwner>(this);
            this.TabFlagMenus = FlagMenuLoader.MenuItems;
            this.TabHeaderMenus = HeaderMenuLoader.MenuItems;

            _pdfViewSettings = new PdfViewSettings();
            _settings = new Settings();

            _notifier = new Notifier(cfg =>
            {
                //place the notifications approximately inside the main editing area
                //(not over the toolbar area) on the top-right hand side
                cfg.PositionProvider = new WindowPositionProvider(
                    parentWindow: Application.Current.MainWindow,
                    corner: Corner.TopRight,
                    offsetX: 15,
                    offsetY: 90);

                cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                    notificationLifetime: TimeSpan.FromSeconds(5),
                    maximumNotificationCount: MaximumNotificationCount.FromCount(5));

                cfg.Dispatcher = Application.Current.Dispatcher;
            });


            SelectedTab = 0;   //staves tab

            NewFile.Execute(this);

            RefreshLastFilesMenu();

            // Update bindings
            base.RaisePropertyChangedEvent("TabModel");

        }
        public void PopupFileAssociateWindow()
        {
            _fileAssociateWindow = new FileAssociateWindow();
            _fileAssociateWindow.DataContext = _fileAssociateViewModel;
            _fileAssociateViewModel.MainWindowViewModel = this;
            _fileAssociateViewModel.LoadFileTypes();
            _fileAssociateWindow.Owner = App.Current.MainWindow;
            _fileAssociateWindow.ShowDialog();
        }

        public int PopupSubPieceListWindow(List<string> pieces)
        {   //return index of piece in collection, or -1 if user cancelled

            _chooseSubPieceWindow = new ChoosePieceWindow();
            _chooseSubPiecesViewModel = new ChooseSubPiecesViewModel(pieces);
            _chooseSubPiecesViewModel.MainWindowViewModel = this;
            _chooseSubPieceWindow.DataContext = _chooseSubPiecesViewModel;
            _chooseSubPieceWindow.Owner = App.Current.MainWindow;
            _chooseSubPieceWindow.ShowDialog();

            return (_chooseSubPiecesViewModel.Selected == null ? -1 : _chooseSubPiecesViewModel.Selected.Index);
        }
        public void CloseFileAssociateWindow()
        {
            _fileAssociateWindow.Close();
        }

        public void CloseChooseSubPieceWindow()
        {
            _chooseSubPieceWindow.Close();
        }

        public void Playback(string playbackPath)
        {

            PlaybackPath = playbackPath;
            Play();

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

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    _audioPlayback.Dispose();
                    _notifier.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~MainWindowViewModel() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);

        }
        #endregion
    }

}

