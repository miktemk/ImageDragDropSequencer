using Miktemk.PropertyChanged;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageDragDropSequencer.ViewModel.Service;

namespace ImageDragDropSequencer.ViewModel
{
    [Serializable]
    public class DraggyImageSequenceVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private IFileService fileService;

        // settable properties
        public ObservableRangeCollection<DraggyImage> Images { get; } = new ObservableRangeCollection<DraggyImage>();
        public string Folder { get; set; }
        public int? CurImageIndex { get; private set; }
        /// <summary>
        /// Note: this number needs to then be converted to either 33% image height or 50% screen height.
        /// Implementation may vary, depending on desired outcome.
        /// </summary>
        public int VerticalScroll { get; private set; }

        // calculated properties
        public DraggyImage CurImage => (Images.Any() && CurImageIndex != null)
            ? Images[CurImageIndex ?? 0]
            : null;
        public string CurImageFilename => CurImage?.FilenameLeaf;
        public string CurImageFilenameFull => (CurImageFilename != null)
            ? Path.Combine(Folder ?? fileService.NewProjectTmpFolder, CurImage?.FilenameLeaf)
            : null;

        public DraggyImageSequenceVM(IFileService fileService)
        {
            this.fileService = fileService;
            Images.CollectionChanged += Images_CollectionChanged;
            CurImageIndex = null;
        }

        private void Images_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //set index to last image
            CurImageIndex = Images.Count - 1;
        }

        #region ------------- keyboard commands ------------------

        public void NextImage()
        {
            if (CurImageIndex >= Images.Count - 1)
                return;
            CurImageIndex++;
            VerticalScroll = 0;
        }
        public void PrevImage()
        {
            if (CurImageIndex <= 0)
                return;
            CurImageIndex--;
            VerticalScroll = 0;
        }
        public void ScrollUp()
        {
            if (VerticalScroll > 0)
                VerticalScroll--;
        }
        public void ScrollDown()
        {
            VerticalScroll++;
        }

        #endregion
    }

    public class DraggyImage
    {
        public string FilenameLeaf { get; set; }

        public DraggyImage() { }
        public DraggyImage(string filenameLeaf)
        {
            FilenameLeaf = filenameLeaf;
        }
    }

}
