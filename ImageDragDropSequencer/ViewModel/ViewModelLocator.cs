using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using GalaSoft.MvvmLight.Views;
using System;
using ImageDragDropSequencer.ViewModel.Service;

namespace ImageDragDropSequencer.ViewModel
{
    public class ViewModelLocator
    {
        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            // see: http://stackoverflow.com/questions/17594058/mvvm-light-there-is-already-a-factory-registered-for-inavigationservice
            if (!ViewModelBase.IsInDesignModeStatic)
            {
                SimpleIoc.Default.Register<MainViewModel>();
                SimpleIoc.Default.Register<MvvmDialogs.IDialogService>(() => new MvvmDialogs.DialogService());
                SimpleIoc.Default.Register<IFileService, FileService>();
            }
        }

        public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();

        public static void Cleanup() {}
    }
}