using System;
using System.Reflection;
using System.Windows;
using Microsoft.Win32;
using Picturepark.AssetUploader.ViewModels;

namespace Picturepark.AssetUploader.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        public MainWindowModel Model => (MainWindowModel) Resources["ViewModel"];

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var args = Environment.GetCommandLineArgs();
            if (args.Length == 2)
                Model.FilePath = args[1];
        }

        private void OnRegisterInContextMenu(object sender, RoutedEventArgs e)
        {
            Model.RunTaskAsync(() =>
            {
                using (var key = Registry.ClassesRoot.CreateSubKey(@"*\shell\PictureparkAssetUploader"))
                    key.SetValue("", "Upload to Picturepark server");

                using (var key = Registry.ClassesRoot.CreateSubKey(@"*\shell\PictureparkAssetUploader\command"))
                    key.SetValue("", "\"" + Assembly.GetEntryAssembly().Location + "\" %1");
            });
        }

        private void OnUnregisterInContextMenu(object sender, RoutedEventArgs e)
        {
            Model.RunTaskAsync(() =>
            {
                Registry.ClassesRoot.DeleteSubKeyTree(@"*\shell\PictureparkAssetUploader");
            });
        }
    }
}
