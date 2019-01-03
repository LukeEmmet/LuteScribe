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
using System.Windows.Input;
using System.Windows;
using System.Runtime.InteropServices;
using System.IO;

namespace LuteScribe.ViewModel.Commands
{

    public class AssociateFileExtensionsCommand : ICommand
    {

        // Member variables
        private readonly MainWindowViewModel _viewModel;

        public AssociateFileExtensionsCommand(MainWindowViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        /// <summary>
        /// Whether this command can be executed.
        /// </summary>
        public bool CanExecute(object parameter)
        {
            return true;
        }

        /// <summary>
        /// Fires when the CanExecute status of this command changes.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Invokes this command to perform its intended task.
        /// </summary>
        public void Execute(object parameter)
        {
            var appDir = System.AppDomain.CurrentDomain.BaseDirectory;
            var ExecPath = appDir + "LuteScribe.exe";
            var iconPath = Path.GetFullPath(appDir + "Resources\\ringtones.ico");


            var userPress = MessageBox.Show("Associate LSML, FT3 and TAB files with LuteScribe?", "File association", MessageBoxButton.YesNo);
            if (userPress == MessageBoxResult.Yes)
            {
                CreateFileAssociation(ExecPath, ".lsml", "LuteScribe.XML", iconPath);
                CreateFileAssociation(ExecPath, ".ft3", "Fronimo.FT3", iconPath);
                CreateFileAssociation(ExecPath, ".tab", "TAB.TabFile", iconPath);

                // Tell explorer the file association has been changed
                SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);

                MessageBox.Show("File associations LSML, TF3 and TAB will now open with LuteScribe.");
            }


        }

        //based in part on https://stackoverflow.com/questions/69761/how-to-associate-a-file-extension-to-the-current-executable-in-c-sharp
        private void CreateFileAssociation(string ExecPath, string ext, string fileType, string iconPath)
        {
            /***********************************/
            /**** Key1: Create ".abc" entry ****/
            /***********************************/
            Microsoft.Win32.RegistryKey key1 = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software", true);

            key1.CreateSubKey("Classes");
            key1 = key1.OpenSubKey("Classes", true);

            key1.CreateSubKey(ext);
            key1 = key1.OpenSubKey(ext, true);
            key1.SetValue("", fileType); // Set default key value

            key1.Close();

            /*******************************************************/
            /**** Key2: Create "DemoKeyValue\DefaultIcon" entry ****/
            /*******************************************************/
            Microsoft.Win32.RegistryKey key2 = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software", true);

            key2.CreateSubKey("Classes");
            key2 = key2.OpenSubKey("Classes", true);

            key2.CreateSubKey(fileType);
            key2 = key2.OpenSubKey(fileType, true);

            key2.CreateSubKey("DefaultIcon");
            key2 = key2.OpenSubKey("DefaultIcon", true);
            key2.SetValue("", "\"" + iconPath + "\""); // Set default key value

            key2.Close();

            /**************************************************************/
            /**** Key3: Create "DemoKeyValue\shell\open\command" entry ****/
            /**************************************************************/
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
            key3.SetValue("", "\"" + ExecPath + "\"" + " \"%1\""); // Set default key value

            key3.Close();
        }


        [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);
    }
}
