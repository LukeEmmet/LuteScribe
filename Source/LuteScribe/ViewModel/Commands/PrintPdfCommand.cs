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
using LuteScribe.Singletons;

namespace LuteScribe.ViewModel.Commands
{

    public class PrintPdfCommand : ICommand
    {

        // Member variables
        private readonly MainWindowViewModel _viewModel;

        public PrintPdfCommand(MainWindowViewModel viewModel)
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
            var pdfCreator = new CreatePdfCommand(_viewModel);
            var appDir = System.AppDomain.CurrentDomain.BaseDirectory;

            var gsPath = appDir + "..\\..\\..\\Ghostscript\\Bin\\gswin32c.exe";

            gsPath = Path.GetFullPath(gsPath);


            navigator.Execute(appDir + @"Resources\creating-pdf.pdf");

            //cleanup the last preview file if it exists..
            var lastPreviewPath = _viewModel.PreviewPath;

            if ((lastPreviewPath != null) && (File.Exists(lastPreviewPath)))
            {
                File.SetAttributes(lastPreviewPath, FileAttributes.Normal);
                    File.Delete(lastPreviewPath);
            }

            var fileName = (_viewModel.Path != null) ? Path.GetFileNameWithoutExtension(_viewModel.Path) : "blank";
            var previewPath = Session.Instance.SessionPath + "\\" + fileName + ".pdf";

            pdfCreator.Execute(previewPath);
            navigator.Execute(previewPath);

            _viewModel.PreviewPath = previewPath;

            var gsCommand = String.Format("\"{0}\"" +
                " -dSAFER -q -dNOPAUSE -dBATCH -sDEVICE=mswinpr2 " +
                " \"{1}\" ", gsPath, previewPath);

            //assumes no lock is held on previous pdf file (acrobat will lock it, but we use moonpdf)
            var exec = new ExecuteProcess();
            var result = exec.ExecuteCommand(gsCommand);


        }


    }
}
