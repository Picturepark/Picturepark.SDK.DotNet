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
using Picturepark.ContentUploader.Views.OidcClient;
using Picturepark.ContentUploader.Views;
using Newtonsoft.Json;
using System.Security;

namespace Picturepark.ContentUploader.ViewModels
{
    public class MainWindowModel : ViewModelBase
    {
        private string _filePath;

        public MainWindowModel()
        {
            UploadCommand = new AsyncRelayCommand(UploadAsync, () =>
                !string.IsNullOrEmpty(Server) &&
                !string.IsNullOrEmpty(CustomerAlias) &&
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
#if DEBUG
            get { return ApplicationSettings.GetSetting("Server", "https://devnext.preview-picturepark.com"); }
#else
            get { return ApplicationSettings.GetSetting("Server", ""); }
#endif
            set { ApplicationSettings.SetSetting("Server", value); }
        }

        public string IdentityServer
        {
#if DEBUG
            get { return ApplicationSettings.GetSetting("IdentityServer", "https://devnext-identity.preview-picturepark.com"); }
#else
            get { return ApplicationSettings.GetSetting("IdentityServer", ""); }
#endif
            set { ApplicationSettings.SetSetting("IdentityServer", value); }
        }

        public string ClientId
        {
            get { return ApplicationSettings.GetSetting("ClientId", ""); }
            set { ApplicationSettings.SetSetting("ClientId", value); }
        }

        public string ClientSecret
        {
            get { return ApplicationSettings.GetSetting("ClientSecret", ""); }
            set { ApplicationSettings.SetSetting("ClientSecret", value); }
        }

        public string RedirectUri
        {
            get { return ApplicationSettings.GetSetting("RedirectUri", ""); }
            set { ApplicationSettings.SetSetting("RedirectUri", value); }
        }

        public string CustomerId
        {
            get { return ApplicationSettings.GetSetting("CustomerId", ""); }
            set { ApplicationSettings.SetSetting("CustomerId", value); }
        }

        public string CustomerAlias
        {
            get { return ApplicationSettings.GetSetting("CustomerAlias", ""); }
            set { ApplicationSettings.SetSetting("CustomerAlias", value); }
        }

        public string RefreshToken
        {
            get { return ApplicationSettings.GetSetting("RefreshToken", ""); }
            set { ApplicationSettings.SetSetting("RefreshToken", value); }
        }

        public string AccessToken { get; set; }

        public DateTime AccessTokenExpiration { get; set; }

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

                await RunTaskAsync(async () =>
                {
                    var accessToken = await GetAccessTokenAsync();
                    var authClient = new AccessTokenAuthClient(Server, accessToken, CustomerAlias);
                    using (var client = new PictureparkClient(new PictureparkClientSettings(authClient)))
                    {
                        var transfer = await client.Transfers.CreateAsync(new CreateTransferRequest
                        {
                            Name = fileName,
                            TransferType = TransferType.FileUpload,
                            Files = new List<TransferUploadFile> { new TransferUploadFile { FileName = fileName, Identifier = fileName } }
                        });

                        using (var stream = File.OpenRead(FilePath))
                            await client.Transfers.UploadFileAsync(transfer.Id, fileName, new FileParameter(stream, fileName), fileName, 1, stream.Length, stream.Length, 1);
                    }
                });
            }
        }

        private async Task<string> GetAccessTokenAsync()
        {
            if (!string.IsNullOrEmpty(AccessToken) && AccessTokenExpiration > DateTime.Now)
            {
                return AccessToken;
            }

            var tenant = new { id = CustomerId, alias = CustomerAlias };
            var acrValues = "tenant:" + JsonConvert.SerializeObject(tenant);

            var settings = new OidcSettings
            {
                Authority = IdentityServer,
                ClientId = ClientId,
                ClientSecret = ClientSecret,
                RedirectUri = RedirectUri,
                Scope = "all_scopes openid profile picturepark_api offline_access",
                LoadUserProfile = true,
                AcrValues = acrValues,
                UsePkce = false
            };

            if (!string.IsNullOrEmpty(RefreshToken))
            {
                var refreshResult = await LoginWebView.RefreshTokenAsync(settings, RefreshToken);
                if (refreshResult.Success)
                {
                    AccessToken = refreshResult.AccessToken;
                    AccessTokenExpiration = refreshResult.AccessTokenExpiration;
                    RefreshToken = refreshResult.RefreshToken;

                    return AccessToken;
                }
            }

            var result = await LoginWebView.AuthenticateAsync(settings);
            if (result.Success)
            {
                AccessToken = result.AccessToken;
                AccessTokenExpiration = result.AccessTokenExpiration;
                RefreshToken = result.RefreshToken;

                return AccessToken;
            }

            throw new SecurityException("Could not authenticate with the Picturepark IDS: " + result.ErrorMessage);
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
