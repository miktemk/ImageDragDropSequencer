using ImageDragDropSequencer.Code;
using ImageDragDropSequencer.ViewModel;
using Miktemk;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using IoPath = System.IO.Path;

namespace ImageDragDropSequencer.Controls
{
    [AddINotifyPropertyChangedInterface]
    public partial class ImageInteractionControl : UserControl
    {
        private readonly Pen penBlack = new Pen(Brushes.Black, 1);
        private readonly Pen penWhite = new Pen(Brushes.White, 1);
        private readonly string[] ProjectFileExts = { ".txt", ".json" };

        public ImageInteractionControl()
        {
            InitializeComponent();
            //DataContext = this; // .... Do NOT set DataContext here! Otherwise we cannot apply Binding to any DPs from MainWindow.xaml
        }

        #region ---------------- DraggyImageSequence -----------------------

        public DraggyImageSequenceVM DraggyImageSequence
        {
            get { return (DraggyImageSequenceVM)GetValue(DraggyImageSequenceProperty); }
            set { SetValue(DraggyImageSequenceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DraggyImageSequence.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DraggyImageSequenceProperty =
            DependencyProperty.Register("DraggyImageSequence", typeof(DraggyImageSequenceVM), typeof(ImageInteractionControl),
                        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnDraggyImageSequencePropertyChanged));

        private static void OnDraggyImageSequencePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var control = sender as ImageInteractionControl;
            if (control != null)
                control.OnDraggyImageSequenceChanged((DraggyImageSequenceVM)e.OldValue, (DraggyImageSequenceVM)e.NewValue);
        }

        private void OnDraggyImageSequenceChanged(DraggyImageSequenceVM oldValue, DraggyImageSequenceVM newValue)
        {
            if (oldValue != null)
                oldValue.PropertyChanged -= DraggyImageSequence_PropertyChanged;
            if (newValue != null)
                newValue.PropertyChanged += DraggyImageSequence_PropertyChanged;
        }

        private void DraggyImageSequence_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurImageFilenameFull")
                InvalidateVisual();
            else if (e.PropertyName == "VerticalScroll")
                InvalidateVisual();
        }

        #endregion

        #region ---------------- ImageSaveFolder -----------------------


        public string ImageSaveFolder
        {
            get { return (string)GetValue(ImageSaveFolderProperty); }
            set { SetValue(ImageSaveFolderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImageSaveFolder.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageSaveFolderProperty =
            DependencyProperty.Register("ImageSaveFolder", typeof(string), typeof(ImageInteractionControl),
                        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnImageSaveFolderPropertyChanged));

        private static void OnImageSaveFolderPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var control = sender as ImageInteractionControl;
            if (control != null)
                control.OnImageSaveFolderChanged((string)e.OldValue, (string)e.NewValue);
        }

        private void OnImageSaveFolderChanged(string oldValue, string newValue)
        {

        }

        #endregion

        #region ---------------- ImageSaveFolder -----------------------

        public ICommand CommandOnProjectFileDragged
        {
            get { return (ICommand)GetValue(CommandOnProjectFileDraggedProperty); }
            set { SetValue(CommandOnProjectFileDraggedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CommandOnProjectFileDragged.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandOnProjectFileDraggedProperty =
            DependencyProperty.Register("CommandOnProjectFileDragged", typeof(ICommand), typeof(ImageInteractionControl),
                        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnCommandOnProjectFileDraggedPropertyChanged));

        private static void OnCommandOnProjectFileDraggedPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var control = sender as ImageInteractionControl;
            if (control != null)
                control.OnCommandOnProjectFileDraggedChanged((ICommand)e.OldValue, (ICommand)e.NewValue);
        }

        private void OnCommandOnProjectFileDraggedChanged(ICommand oldValue, ICommand newValue)
        {

        }

        #endregion


        private void TreeControl_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Html, true))
                e.Effects = DragDropEffects.Copy;
            else if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
                e.Effects = DragDropEffects.All;
            else
                e.Effects = DragDropEffects.None;
        }

        private void TreeControl_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Html, true))
            {
                // image dragged from web browser
                var dragDropResult = e.Data.GetData(DataFormats.Html, true) as string;
                var newImageFilename = UtilsPath.GenerateGuidyFilename(GetNewImageFilenameTemplate());
                Utils.SaveHtmlContentToImage(dragDropResult, newImageFilename);
                PushImageIntoSequence(newImageFilename);
            }
            else if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
            {
                var filenames = (string[])e.Data.GetData(DataFormats.FileDrop, false);
                if (filenames.Any())
                {
                    var filename = filenames.FirstOrDefault();
                    var ext = IoPath.GetExtension(filename).ToLower();
                    if (ProjectFileExts.Contains(ext))
                        CommandOnProjectFileDragged?.Execute(filename);
                }
            }
        }

        private string GetNewImageFilenameTemplate()
        {
            if (ImageSaveFolder == null)
                return "{0}.jpg";
            return IoPath.Combine(ImageSaveFolder, "{0}.jpg");
        }

        private void PushImageIntoSequence(string newImageFilename)
        {
            if (DraggyImageSequence == null)
                return;
            var filenameLeaf = IoPath.GetFileName(newImageFilename);
            DraggyImageSequence.Images.Add(new DraggyImage(filenameLeaf));
        }

        protected override void OnRender(DrawingContext g)
        {
            base.OnRender(g);
            g.DrawRectangle(Brushes.AliceBlue, penWhite, new Rect(0, 0, ActualWidth, ActualHeight));
            if (DraggyImageSequence?.CurImageFilenameFull != null)
            {
                var bs = new BitmapImage(new Uri(DraggyImageSequence.CurImageFilenameFull, UriKind.Absolute));
                var imageRelativeHeight = bs.Height * ActualWidth / bs.Width;
                var yOffset = DraggyImageSequence.VerticalScroll * ActualHeight / 4;
                g.DrawImage(bs, new Rect(0, -yOffset, ActualWidth, imageRelativeHeight));
            }
        }

    }
}
