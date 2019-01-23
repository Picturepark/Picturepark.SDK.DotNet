﻿using System;
using System.IO;
using System.Reflection;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using MyToolkit.Command;
using MyToolkit.Dialogs;
using MyToolkit.Mvvm;
using MyToolkit.Storage;
using Newtonsoft.Json;
using Picturepark.ContentUploader.Views;
using Picturepark.ContentUploader.Views.OidcClient;
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
                !string.IsNullOrEmpty(ApiServer) &&
                !string.IsNullOrEmpty(CustomerAlias) &&
                !string.IsNullOrEmpty(FilePath));

            PropertyChanged += (sender, args) => UploadCommand.RaiseCanExecuteChanged();

            RegisterContextMenuCommand = new AsyncRelayCommand(RegisterContextMenuAsync);
            UnregisterContextMenuCommand = new AsyncRelayCommand(UnregisterContextMenuAsync);
        }

        #region Commands

        public AsyncRelayCommand UploadCommand { get; }

        public AsyncRelayCommand RegisterContextMenuCommand { get; }

        public AsyncRelayCommand UnregisterContextMenuCommand { get; }

        #endregion

        #region Configuration

        public string ApiServer
        {
#if DEBUG
            get { return ApplicationSettings.GetSetting("ApiServer", "https://devnext-api.preview-picturepark.com"); }
#else
            get { return ApplicationSettings.GetSetting("ApiServer", ""); }
#endif
            set { ApplicationSettings.SetSetting("ApiServer", value); }
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
            get { return ApplicationSettings.GetSetting("RedirectUri", "http://localhost/wpf"); }
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

        #endregion

        public string AccessToken { get; set; }

        public DateTime AccessTokenExpiration { get; set; }

        public string FilePath
        {
            get { return _filePath; }
            set { Set(ref _filePath, value); }
        }

        public async Task UploadAsync()
        {
            await RunTaskAsync(async () =>
            {
                if (File.Exists(FilePath))
                {
                    var fileName = Path.GetFileName(FilePath);

                    var accessToken = await GetAccessTokenAsync();
                    var authClient = new AccessTokenAuthClient(ApiServer, accessToken, CustomerAlias);
                    using (var client = new PictureparkService(new PictureparkServiceSettings(authClient)))
                    {
                        try
                        {
                            // Uploading
                            var transfer = await client.Transfer.UploadFilesAsync(fileName, new[] { (FileLocations) FilePath },
                                new UploadOptions { ChunkSize = 1024 * 1024, WaitForTransferCompletion = true });
                            
                            // Importing
                            await client.Transfer.ImportAndWaitForCompletionAsync(transfer.Transfer,
                                new ImportTransferRequest());

                            MessageBox.Show("The image has been successfully uploaded and imported.", "Image uploaded");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error occurred while uploading your file", "Error");
                        }
                    }
                }
            });
        }

        public override void HandleException(Exception exception)
        {
            ExceptionBox.Show("An error occurred", exception, Application.Current.MainWindow);
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
                try
                {
                    using (var key = Registry.ClassesRoot.CreateSubKey(@"*\shell\PictureparkContentUploader"))
                        key.SetValue("", "Upload to Picturepark server");

                    using (var key = Registry.ClassesRoot.CreateSubKey(@"*\shell\PictureparkContentUploader\command"))
                        key.SetValue("", "\"" + Assembly.GetEntryAssembly().Location + "\" %1");
                }
                catch (UnauthorizedAccessException ex)
                {
                    MessageBox.Show("Unable to register context menu, please run ContentUploader as administrator.", "Elevated rights required", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }

        private async Task UnregisterContextMenuAsync()
        {
            await RunTaskAsync(() =>
            {
                try
                {
                    Registry.ClassesRoot.DeleteSubKeyTree(@"*\shell\PictureparkContentUploader");
                }
                catch (ArgumentException ex)
                {
                    if (ex.Message.Contains("Cannot delete a subkey tree because the subkey does not exist."))
                        MessageBox.Show(
                            "Unable to unregister context menu because it either doesn't exist or you don't have enough rights. Try running ContentUploader as administrator.",
                            "Context menu not found", MessageBoxButton.OK, MessageBoxImage.Error);
                    else
                        throw;
                }
                catch (UnauthorizedAccessException ex)
                {
                    MessageBox.Show("Unable to unregister context menu, please run ContentUploader as administrator.", "Elevated rights required", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }
    }
}
