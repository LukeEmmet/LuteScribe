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

namespace LuteScribe.ViewModel
{
    public class FileAssociation : ObservableObject
    {
        private string _description;
        private bool _checked;
        private string _fileType;
        private string _extension;

        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
                base.RaisePropertyChangedEvent("Description");
            }
        }
        public string FileType
        {
            get
            {
                return _fileType;
            }
            set
            {
                _fileType = value;
                base.RaisePropertyChangedEvent("FileType");
            }
        }
        public string Extension
        {
            get
            {
                return _extension;
            }
            set
            {
                _extension = value;
                base.RaisePropertyChangedEvent("Extension");
            }
        }
        public bool Checked
        {
            get
            {
                return _checked;
            }
            set
            {
                _checked = value;
                base.RaisePropertyChangedEvent("Checked");
            }
        }
    }


    public class FileAssociateViewModel : ViewModelBase
    {


        private ObservableCollection<FileAssociation> _fileAssociations;

        public ICommand AssociateFileExtensions { get; }
        private MainWindowViewModel _mainWindowViewModel;

        public void LoadFileTypes()
        {
            FileAssociations = new ObservableCollection<FileAssociation>();


            //by default we suggest LSML and TAB files.
            FileAssociations.Add(new FileAssociation
            {
                Checked = true,
                Extension = "lsml",
                FileType = "LuteScribe.XML",
                Description = "LuteScribe LSML files"
            });

            FileAssociations.Add(new FileAssociation
            {
                Checked = true,
                Extension = "tab",
                FileType = "TAB.Tabfile",
                Description = "TAB text files"
            });

            //the rest are optional as they may be associated with other applications
            FileAssociations.Add(new FileAssociation
            {
                Checked = AssociatedWithLuteScribe("Fronimo.FT2"),
                Extension = "ft2",
                FileType = "Fronimo.FT2",
                Description = "Fronimo FT2 files"
            });
            FileAssociations.Add(new FileAssociation
            {
                Checked = AssociatedWithLuteScribe("Fronimo.FT3"),
                Extension = "ft3",
                FileType = "Fronimo.FT3",
                Description = "Fronimo FT3 files"
            });
            FileAssociations.Add(new FileAssociation
            {
                Checked = AssociatedWithLuteScribe("Fandango.JTZ"),
                Extension = "jtz",
                FileType = "Fandango.JTZ",
                Description = "Fandango JTZ files"
            });
            FileAssociations.Add(new FileAssociation
            {
                Checked = AssociatedWithLuteScribe("Fandango.JTXML"),
                Extension = "jtxml",
                FileType = "Fandango.JTXML",
                Description = "Fandango JTXML files"
            });

        }
        public FileAssociateViewModel()
        {

            LoadFileTypes();
            AssociateFileExtensions = new AssociateFileExtensionsCommand(this);

        }

        private bool AssociatedWithLuteScribe(string fileType)
        {
            Microsoft.Win32.RegistryKey key3 = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software", true);

            key3.CreateSubKey("Classes");
            key3 = key3.OpenSubKey("Classes", true);

            key3.CreateSubKey(fileType);
            key3 = key3.OpenSubKey(fileType, true);

            key3.CreateSubKey("shell");
            key3 = key3.OpenSubKey("shell", true);

            key3.CreateSubKey("open");
            key3 = key3.OpenSubKey("open", true);

            key3.CreateSubKey("command");
            key3 = key3.OpenSubKey("command", true);

            var execCommand = (string)key3.GetValue("");

            key3.Close();

            var appDir = System.AppDomain.CurrentDomain.BaseDirectory;
            var ExecPath = appDir + "LuteScribe.exe";

            var luteScribeLaunch = "\"" + ExecPath + "\"" + " \"%1\"";
            var result = (execCommand != null & execCommand == luteScribeLaunch);

            return result;

        }

        public MainWindowViewModel MainWindowViewModel
        {
            set
            {
                _mainWindowViewModel = value;
            }
        }

        public void CloseWindow()
        {
            //pass up to mainviewmodel which holds a ref to this window
            _mainWindowViewModel.CloseFileAssociateWindow();
        }

        public ObservableCollection<FileAssociation> FileAssociations
        {
            get
            {
                return _fileAssociations;
            }

            set
            {
                _fileAssociations = value;

                RaisePropertyChangedEvent("FileAssociations");
            }
        }


    }


}

