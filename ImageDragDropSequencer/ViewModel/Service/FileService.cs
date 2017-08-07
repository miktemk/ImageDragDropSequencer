using ImageDragDropSequencer.Code;
using Miktemk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageDragDropSequencer.ViewModel.Service
{
    public interface IFileService
    {
        string NewProjectTmpFolder { get; }
    }
    public class FileService : IFileService
    {
        public string NewProjectTmpFolder { get; }

        public FileService()
        {
            NewProjectTmpFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Globals.TmpImageFolderName);
            UtilsPath.CreateDirectoryIfNotExists(NewProjectTmpFolder);
        }
    }
}
