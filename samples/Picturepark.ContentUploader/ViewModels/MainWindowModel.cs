using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using MyToolkit.Command;
using MyToolkit.Dialogs;
using MyToolkit.Mvvm;
using MyToolkit.Storage;
using Picturepark.SDK.V1;
using Picturepark.SDK.V1.Authentication;
using Picturepark.SDK.V1.Contract;

namespace Picturepark.ContentUploader.ViewModels
{
	public class MainWindowModel : ViewModelBase
	{
		private string _filePath;

		public MainWindowModel()
		{
			UploadCommand = new AsyncRelayCommand(UploadAsync, () =>
				!string.IsNullOrEmpty(Server) &&
				!string.IsNullOrEmpty(Username) &&
				!string.IsNullOrEmpty(Password) &&
				!string.IsNullOrEmpty(FilePath));

			PropertyChanged += (sender, args) => UploadCommand.RaiseCanExecuteChanged();

			RegisterContextMenuCommand = new AsyncRelayCommand(RegisterContextMenuAsync);
			UnregisterContextMenuCommand = new AsyncRelayCommand(UnregisterContextMenuAsync);
		}
		
		public AsyncRelayCommand UploadCommand { get; }

		public AsyncRelayCommand RegisterContextMenuCommand { get; }

		public AsyncRelayCommand UnregisterContextMenuCommand { get; }

		public string Server
		{
			get { return ApplicationSettings.GetSetting("Server", ""); }
			set { ApplicationSettings.SetSetting("Server", value); }
		}

		public string Username
		{
			get { return ApplicationSettings.GetSetting("Username", ""); }
			set { ApplicationSettings.SetSetting("Username", value); }
		}

		public string Password
		{
			get { return ApplicationSettings.GetSetting("Password", ""); }
			set { ApplicationSettings.SetSetting("Password", value); }
		}

		public string FilePath
		{
			get { return _filePath; }
			set { Set(ref _filePath, value); }
		}

		public override void HandleException(Exception exception)
		{
			ExceptionBox.Show("An error occurred", exception, Application.Current.MainWindow);
		}

		private async Task UploadAsync()
		{
			if (File.Exists(FilePath))
			{
				var fileName = Path.GetFileName(FilePath);

				// TODO: TEMPORARY DISABLED - UNCOMMENT & FIX 
				////await RunTaskAsync(async () =>
				////{
				////    var authClient = new UsernamePasswordAuthClient(Server, Username, Password);
				////    using (var client = new PictureparkClient(authClient))
				////    {
				////        var transfer = await client.Transfers.CreateAsync(new CreateTransferRequest
				////        {
				////            Name = fileName,
				////            TransferType = TransferType.FileUpload,
				////            Files = new List<TransferUploadFile> { new TransferUploadFile { FileName = fileName, Identifier = fileName } }
				////        });

				////        using (var stream = File.OpenRead(FilePath))
				////            await client.Transfers.UploadFileAsync(transfer.Id, fileName, new FileParameter(stream, fileName), fileName, 1, stream.Length, stream.Length, 1);
				////    }
				////});
			}
		}

		private async Task RegisterContextMenuAsync()
		{
			await RunTaskAsync(() =>
			{
				using (var key = Registry.ClassesRoot.CreateSubKey(@"*\shell\PictureparkContentUploader"))
					key.SetValue("", "Upload to Picturepark server");

				using (var key = Registry.ClassesRoot.CreateSubKey(@"*\shell\PictureparkContentUploader\command"))
					key.SetValue("", "\"" + Assembly.GetEntryAssembly().Location + "\" %1");
			});
		}

		private async Task UnregisterContextMenuAsync()
		{
			await RunTaskAsync(() =>
			{
				Registry.ClassesRoot.DeleteSubKeyTree(@"*\shell\PictureparkContentUploader");
			});
		}
	}
}
