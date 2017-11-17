using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;
using System.Net.Http;

namespace Picturepark.SDK.V1
{
	public partial class TransferClient
	{
		private static readonly object s_lock = new object();

		private readonly BusinessProcessClient _businessProcessClient;
		private volatile List<string> _fileNameBlacklist;

		public TransferClient(BusinessProcessClient businessProcessClient, IPictureparkClientSettings settings, HttpClient httpClient)
			: this(settings, httpClient)
		{
			BaseUrl = businessProcessClient.BaseUrl;
			_businessProcessClient = businessProcessClient;
		}

		public async Task<Transfer> UploadFilesAsync(
			string transferName,
			IEnumerable<string> files,
			UploadOptions uploadOptions)
		{
			var filteredFileNames = FilterFilesByBlacklist(files.ToList());
			Transfer transfer = await CreateTransferAsync(filteredFileNames.Select(Path.GetFileName).ToList(), transferName);
			await UploadFilesAsync(transfer, filteredFileNames, uploadOptions);
			return transfer;
		}

		public async Task UploadFilesAsync(
			Transfer transfer,
			IEnumerable<string> files,
			UploadOptions uploadOptions)
		{
			uploadOptions = uploadOptions ?? new UploadOptions();

			var exceptions = new List<Exception>();

			// Limits concurrent downloads
			var throttler = new SemaphoreSlim(uploadOptions.ConcurrentUploads);

			var filteredFileNames = FilterFilesByBlacklist(files.ToList());

			// TODO: File by file uploads
			var allTasks = filteredFileNames
				.Select(file => Task.Run(async () =>
				{
					try
					{
						await UploadFileAsync(throttler, transfer.Id, file, uploadOptions.ChunkSize);
						uploadOptions.SuccessDelegate?.Invoke(file);
					}
					catch (Exception ex)
					{
						exceptions.Add(ex);
						uploadOptions.ErrorDelegate?.Invoke(ex);
					}
				}))
				.ToList();

			await Task.WhenAll(allTasks).ConfigureAwait(false);

			if (exceptions.Any())
				throw new AggregateException(exceptions);

			if (uploadOptions.WaitForTransferCompletion)
			{
				await WaitForCompletionAsync(transfer.BusinessProcessId);
			}
		}

		public async Task ImportTransferAsync(Transfer transfer, FileTransfer2ContentCreateRequest createRequest)
		{
			var importedTransfer = await ImportTransferAsync(transfer.Id, createRequest);
			await WaitForCompletionAsync(importedTransfer.BusinessProcessId);
		}

		public async Task<Transfer> CreateTransferAsync(CreateTransferRequest request)
		{
			var transfer = await CreateAsync(request);
			await WaitForCompletionAsync(transfer.BusinessProcessId);
			return transfer;
		}

		public async Task<Transfer> CreateTransferAsync(List<string> fileNames, string transferName)
		{
			var filteredFileNames = FilterFilesByBlacklist(fileNames);

			var request = new CreateTransferRequest
			{
				Name = string.IsNullOrEmpty(transferName) ? new Random().Next(1000, 9999).ToString() : transferName,
				TransferType = TransferType.FileUpload,
				Files = filteredFileNames.Select(i => new TransferUploadFile { FileName = i, Identifier = Guid.NewGuid().ToString() }).ToList()
			};

			var result = await CreateAsync(request);
			await WaitForStatesAsync(result.BusinessProcessId, new List<string> { TransferState.Created.ToString() });

			return result;
		}

		internal async Task<string> GetFileTransferIdFromTransferIdAsync(string transferId)
		{
			var request = new FileTransferSearchRequest()
			{
				Limit = 1000 // TODO: Isn't this bad for performance? Can't we just add a TermFilter with the transferId?
			};

			var result = await SearchFilesAsync(request);
			var fileTransferId = result.Results.Where(i => i.TransferId == transferId).Select(i => i.Id).FirstOrDefault();

			return fileTransferId;
		}

		private async Task UploadFileAsync(SemaphoreSlim throttler, string transferId, string absoluteFilePath, int chunkSize = 1024 * 1024)
		{
			var fileName = Path.GetFileName(absoluteFilePath);
			var identifier = fileName;

			var fileSize = new FileInfo(absoluteFilePath).Length;
			var relativePath = fileName;
			var totalChunks = (long)Math.Ceiling((decimal)fileSize / chunkSize);

			var uploadTasks = new List<Task>();

			for (var chunkNumber = 1; chunkNumber <= totalChunks; chunkNumber++)
			{
				await throttler.WaitAsync();

				var number = chunkNumber;
				uploadTasks.Add(Task.Run(async () =>
				{
					try
					{
						using (var fileStream = File.OpenRead(absoluteFilePath))
						{
							var currentChunkSize = chunkSize;
							var position = (number - 1) * chunkSize;

							// last chunk may have a different size.
							if (number == totalChunks)
							{
								currentChunkSize = (int)(fileSize - position);
							}

							var buffer = new byte[currentChunkSize];
							fileStream.Position = position;

							await fileStream.ReadAsync(buffer, 0, currentChunkSize);

							using (var memoryStream = new MemoryStream(buffer))
							{
								await UploadFileAsync(
									transferId,
									identifier,
									new FileParameter(memoryStream, fileName),
									relativePath,
									number,
									currentChunkSize,
									fileSize,
									totalChunks);
							}
						}
					}
					finally
					{
						throttler.Release();
					}
				}));
			}

			await Task.WhenAll(uploadTasks).ConfigureAwait(false);
		}

		private async Task<BusinessProcessWaitResult> WaitForCompletionAsync(string businessProcessId, int timeout = 10 * 60 * 1000)
		{
			// TODO: Remove BusinessProcessExtensions and use methods in BusinessProcessClient
			var waitResult = await _businessProcessClient.WaitForCompletionAsync(businessProcessId, timeout);

			if (waitResult.HasLifeCycleHit)
				return waitResult;

			var error = waitResult.BusinessProcess.StateHistory.SingleOrDefault(i => i.Error != null);

			// Not finished
			if (error == null)
			{
				throw new TimeoutException($"Wait for business process on completion timed out after {timeout / 1000} seconds");
			}

			// Throw deserialized exception
			var exception = DeserializeException(error.Error.Exception);
			throw exception;
		}

		private async Task<BusinessProcessWaitResult> WaitForStatesAsync(string businessProcessId, IEnumerable<string> states, int timeout = 10 * 60 * 1000)
		{
			// TODO: Remove BusinessProcessExtensions and use methods in BusinessProcessClient
			var waitResult = await _businessProcessClient.WaitAsync(businessProcessId, states, null, timeout);

			if (waitResult.HasStateHit)
				return waitResult;

			var error = waitResult.BusinessProcess.StateHistory.SingleOrDefault(i => i.Error != null);

			// Not finished
			if (error == null)
			{
				throw new TimeoutException($"Wait for business process on completion timed out after {timeout / 1000} seconds");
			}

			// Throw deserialized exception
			var exception = DeserializeException(error.Error.Exception);
			throw exception;
		}

		private List<string> FilterFilesByBlacklist(List<string> files)
		{
			var blacklist = GetCachedBlacklist();
			var filteredFileNames = files.Where(i => !blacklist.Contains(Path.GetFileName(i).ToLowerInvariant())).ToList();
			return filteredFileNames;
		}

		private List<string> GetCachedBlacklist()
		{
			if (_fileNameBlacklist != null)
				return _fileNameBlacklist;

			lock (s_lock)
			{
				if (_fileNameBlacklist != null)
					return _fileNameBlacklist;

				var blacklist = GetBlacklist(); // TODO: GetCachedBlacklist: Use async version
				_fileNameBlacklist = blacklist.Items.Select(i => i.Match.ToLowerInvariant()).ToList();
			}

			return _fileNameBlacklist;
		}
	}
}
