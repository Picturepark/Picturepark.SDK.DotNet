using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Tests.Fixtures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Picturepark.SDK.V1.Tests
{
	public class TransferTests : IClassFixture<SDKClientFixture>
	{
		private readonly SDKClientFixture _fixture;
		private readonly PictureparkClient _client;

		public TransferTests(SDKClientFixture fixture)
		{
			_fixture = fixture;
			_client = _fixture.Client;
		}

		[Fact]
		[Trait("Stack", "Transfers")]
		public async Task ShouldCreateTransferFromFiles()
		{
			var transferName = new Random().Next(1000, 9999).ToString();
			var files = new List<string> { Path.Combine(_fixture.ExampleFilesBasePath, "0030_JabLtzJl8bc.jpg") };

			var result = await _client.Transfers.CreateTransferAsync(files, transferName);
			Assert.NotNull(result);
		}

		[Fact]
		[Trait("Stack", "Transfers")]
		public async Task ShouldCreateTransferFromWebUrls()
		{
			var transferName = "UrlImport " + new Random().Next(1000, 9999);

			var urls = new List<string>
			{
				"https://picturepark.com/wp-content/uploads/2013/06/home-marquee.jpg",
				"http://cdn1.spiegel.de/images/image-733178-900_breitwand_180x67-zgpe-733178.jpg",
				"http://cdn3.spiegel.de/images/image-1046236-700_poster_16x9-ymle-1046236.jpg",
				"https://dgpcopy.next-picturepark.com/Go/A6ZYegLz/D/15/1"
			};
			var request = new CreateTransferRequest
			{
				Name = transferName,
				TransferType = TransferType.WebDownload,
				WebLinks = urls.Select(url => new TransferWebLink { Url = url, Identifier = Guid.NewGuid().ToString() }).ToList()
			};

			var result = await _client.Transfers.CreateTransferAsync(request);
			Assert.NotNull(result);
		}

		[Fact]
		[Trait("Stack", "Transfers")]
		public async Task ShouldDeleteBatch()
		{
			var transferId = _fixture.GetRandomTransferId(TransferState.TransferReady, 10);

			Assert.True(transferId != string.Empty);

			await _client.Transfers.DeleteAsync(transferId);
		}

		[Fact]
		[Trait("Stack", "Transfers")]
		public async Task ShouldGetTransfer()
		{
			var transferId = _fixture.GetRandomTransferId(null, 20);
			TransferDetail result = await _client.Transfers.GetAsync(transferId);
			Assert.NotNull(result);
		}

		[Fact]
		[Trait("Stack", "Transfers")]
		public async Task ShouldGetFile()
		{
			var fileTransferId = _fixture.GetRandomFileTransferId(20);
			FileTransferDetail result = await _client.Transfers.GetFileAsync(fileTransferId);
			Assert.NotNull(result);
		}

		[Fact]
		[Trait("Stack", "Transfers")]
		public async Task ShouldSearchTransfers()
		{
			var request = new TransferSearchRequest { Limit = 5, SearchString = "*" };
			TransferSearchResult result = await _client.Transfers.SearchAsync(request);
			Assert.NotNull(result);
		}

		[Fact]
		[Trait("Stack", "Transfers")]
		public async Task ShouldSearchFiles()
		{
			var request = new FileTransferSearchRequest { Limit = 20, SearchString = "*" };
			FileTransferSearchResult result = await _client.Transfers.SearchFilesAsync(request);
			Assert.NotNull(result);
		}

		[Fact]
		[Trait("Stack", "Transfers")]
		public async Task ShouldUploadAndImportFiles()
		{
			const int desiredUploadFiles = 10;
			var transferName = nameof(ShouldUploadAndImportFiles) + "-" + new Random().Next(1000, 9999);

			var filesInDirectory = Directory.GetFiles(_fixture.ExampleFilesBasePath, "*").ToList();

			var numberOfFilesInDir = filesInDirectory.Count;
			var numberOfUploadFiles = Math.Min(desiredUploadFiles, numberOfFilesInDir);

			var randomNumber = new Random().Next(0, numberOfFilesInDir - numberOfUploadFiles);
			var importFilePaths = filesInDirectory.Skip(randomNumber).Take(numberOfUploadFiles).ToList();

			Transfer transfer = await _client.Transfers.CreateTransferAsync(importFilePaths.Select(Path.GetFileName).ToList(), transferName);

			await _client.Transfers.UploadFilesAsync(
				importFilePaths,
				_fixture.ExampleFilesBasePath,
				transfer,
				concurrentUploads: 4,
				chunkSize: 20 * 1024,
				successDelegate: Console.WriteLine,
				errorDelegate: Console.WriteLine);

			await ImportTransferAsync(transfer, transferName);
		}

		internal async Task ImportTransferAsync(Transfer transfer, string collectionName)
		{
			var request = new FileTransfer2ContentCreateRequest
			{
				TransferId = transfer.Id,
				ContentPermissionSetIds = new List<string>(),
				Metadata = null,
				LayerSchemaIds = new List<string>()
			};

			await _client.Transfers.ImportTransferAsync(transfer, request);
		}
	}
}
