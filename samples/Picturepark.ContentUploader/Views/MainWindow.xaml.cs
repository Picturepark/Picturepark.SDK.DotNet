using System;
using System.Net;
using System.Windows;
using Picturepark.ContentUploader.ViewModels;

namespace Picturepark.ContentUploader.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Ssl3 |
                SecurityProtocolType.Tls12 |
                SecurityProtocolType.Tls11 |
                SecurityProtocolType.Tls;

            Loaded += OnLoaded;
        }

        public MainWindowModel Model => (MainWindowModel) Resources["ViewModel"];

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var args = Environment.GetCommandLineArgs();
            if (args.Length == 2)
                Model.FilePath = args[1];
        }
    }
}
