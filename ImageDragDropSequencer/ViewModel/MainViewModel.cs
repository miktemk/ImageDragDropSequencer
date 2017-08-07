using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using System;
using System.Linq;
using System.Windows;
using ImageDragDropSequencer.Code;
using Miktemk.Wpf.ViewModels;
using Miktemk;
using System.IO;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.SaveFile;
using ImageDragDropSequencer.ViewModel.Service;

namespace ImageDragDropSequencer.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private IDialogService dialogSerive;
        private IFileService fileService;

        public DraggyImageSequenceVM DraggyImageSequence { get; private set; }
        public string ImageSaveFolder => DraggyImageSequence?.Folder ?? fileService.NewProjectTmpFolder;

        // commands
        public ICommand CommandOnProjectFileDragged { get; }
        public ICommand CommandSave { get; }
        public ICommand CommandNextImage { get; }
        public ICommand CommandPrevImage { get; }
        public ICommand CommandScrollUp{ get; }
        public ICommand CommandScrollDown { get; }

        public MainViewModel(IFileService fileService, IDialogService dialogSerive)
        {
            this.dialogSerive = dialogSerive;
            this.fileService = fileService;

            CommandOnProjectFileDragged = new RelayCommand<string>(OnProjectFileDragged);
            CommandSave = new RelayCommand(OnSave);
            CommandNextImage = new RelayCommand(() => DraggyImageSequence?.NextImage());
            CommandPrevImage = new RelayCommand(() => DraggyImageSequence?.PrevImage());
            CommandScrollUp = new RelayCommand(() => DraggyImageSequence?.ScrollUp());
            CommandScrollDown = new RelayCommand(() => DraggyImageSequence?.ScrollDown());

            DraggyImageSequence = new DraggyImageSequenceVM(fileService);
        }

        private void OnSave()
        {
            // TODO: sue dialog service to show save file dialog
            var saveDialogConfig = new SaveFileDialogSettings
            {
                Filter = "JSON (*.json)|*.json|Text files (*.txt)|*.txt",
            };
            var result = dialogSerive.ShowSaveFileDialog(this, saveDialogConfig);
            if (result != true)
                return;

            var filenameOut = saveDialogConfig.FileName;
            if (DraggyImageSequence.Folder == null)
            {
                var folderImages = UtilsPath.GetFullPathNoExtension(filenameOut);
                DraggyImageSequence.Folder = folderImages;
                UtilsPath.CreateDirectoryIfNotExists(folderImages);
                foreach (var imageVM in DraggyImageSequence.Images)
                {
                    var imagePathOld = Path.Combine(fileService.NewProjectTmpFolder, imageVM.FilenameLeaf);
                    var imagePathNew = Path.Combine(folderImages, imageVM.FilenameLeaf);
                    File.Move(imagePathOld, imagePathNew);
                }
                RaisePropertyChanged("ImageSaveFolder");
            }
            Utils.SaveImageSequenceFile(DraggyImageSequence, filenameOut);
        }

        private void OnProjectFileDragged(string filename)
        {
            if (DraggyImageSequence.Images.Any())
            {
                var result = MessageBox.Show("Discard current image sequence?", "Discard?", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (result == MessageBoxResult.OK)
                {
                    var newDoc = Utils.LoadImageSequenceFile(filename);
                    if (newDoc == null)
                        MessageBox.Show("File does not exista!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    else
                        DraggyImageSequence = newDoc;
                }
            }
        }
    }
}