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
using LuteScribe.Serialization.Commandline;
using System.IO;
using System.Windows;
using LuteScribe.Singletons;

namespace LuteScribe.ViewModel.Commands
{

    public class CreatePdfCommand : ICommand
    {

        // Member variables
        private readonly MainWindowViewModel _viewModel;

        public CreatePdfCommand(MainWindowViewModel viewModel)
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
            var navigator = new PdfViewerNavigate(_viewModel);
            
            var appDir = System.AppDomain.CurrentDomain.BaseDirectory;
            var gsPath = Path.GetFullPath(Path.Combine(appDir, "..\\..\\..\\Ghostscript\\Bin\\gswin32c.exe"));

            var userFile = _viewModel.Path;

            var guid = Guid.NewGuid();
            
            var tempPs = Path.Combine(Session.Instance.SessionPath, "tab-out.ps");       //don't make a very long path - TAB can die with malloc bug

            var psPath = tempPs;
            var pdfPath = (string)parameter;

            if (pdfPath == ".") {
                if (userFile == null)
                {
                    MessageBox.Show("Please save the tabulature file first.");
                    return;
                }

                
                //create PS first alongside the user file. If it is a multi-piece file, put piece number
                //in the file name so multiple pieces can be exported e.g. foo_piece1.pdf, foo_piece2.pdf
                pdfPath = Path.Combine(Path.GetDirectoryName(userFile),
                    Path.GetFileNameWithoutExtension(userFile) +
                    ((_viewModel.TabModel.Pieces.Count > 1) ? ("_piece" + (1 + _viewModel.CurrentSection).ToString()) : "" ) +
                     ".pdf");
            }

            var createPs = new CreatePsCommand(_viewModel);
            createPs.Execute(psPath);
            
            var gsCommand = String.Format("\"{0}\"" +
                    " -dSAFER -q -dNOPAUSE -dBATCH -sDEVICE=pdfwrite " +
                    " -sOutputFile=\"{1}\" " +
                    " -dSAFER -c .setpdfwrite -f \"{2}\" ", gsPath, pdfPath, psPath);

            var result = (new ExecuteProcess()).LoggedExecute(gsCommand);
            
            //when Ghostscript has an error item2 will be non-empty
            //and the return code will be not 0
            if (result.Item1 == 0)
            {
                if ((string)parameter == ".")
                {
                    _viewModel.ToastNofify("Created PDF: " + pdfPath, MainWindowViewModel.ToastMessageStyles.Success);
                }

            } else
            {
                //get std errors result
                _viewModel.ToastNofify(
                    "Error creating PDF. Please check it is not already open in a viewer: \n\n" +
                    result.Item3, MainWindowViewModel.ToastMessageStyles.Error);
            }
        }
    }
}
