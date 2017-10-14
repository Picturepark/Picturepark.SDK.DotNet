using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;

namespace Picturepark.SDK.V1
{
	public partial class TransferClient
	{
		private static readonly object s_lock = new object();

		private readonly BusinessProcessClient _businessProcessClient;
		private volatile List<string> _fileNameBlacklist;

		public TransferClient(BusinessProcessClient businessProcessClient, IPictureparkClientSettings settings) : this(settings)
		{
			BaseUrl = businessProcessClient.BaseUrl;
			_businessProcessClient = businessProcessClient;
		}

		public async Task UploadFilesAsync(
			IEnumerable<string> files,
			string exportDirectory,
			Transfer transfer,
			int concurrentUploads = 4,
			int chunkSize = 1024 * 1024,
			bool waitForTransferCompletion = true,
			Action<string> successDelegate = null,
			Action<Exception> errorDelegate = null
			)
		{
			var exceptions = new List<Exception>();

			// Limits concurrent downloads
			var throttler = new SemaphoreSlim(concurrentUploads);

			var filteredFileNames = FilterFilesByBlacklist(files.ToList());

			// TODO: File by file uploads
			var allTasks = filteredFileNames.Select(file => Task.Run(async () =>
				{
					try
					{
						await UploadFileAsync(throttler, transfer.Id, file, chunkSize);
						successDelegate?.Invoke(file);
					}
					catch (Exception ex)
					{
						exceptions.Add(ex);
						errorDelegate?.Invoke(ex);
					}
				}))
				.ToList();

			await Task.WhenAll(allTasks).ConfigureAwait(false);

			if (exceptions.Any())
				throw new AggregateException(exceptions);

			if (waitForTransferCompletion)
			{
				await Wait4States(transfer.BusinessProcessId, TransferState.UploadCompleted.ToString());
			}
		}

		public async Task ImportTransferAsync(Transfer transfer, FileTransfer2ContentCreateRequest createRequest)
		{
			var importTransfer = await ImportTransferAsync(transfer.Id, createRequest);
			await Wait4States(importTransfer.BusinessProcessId, TransferState.ImportCompleted.ToString());
		}

		public async Task<Transfer> CreateTransferAsync(CreateTransferRequest request)
		{
			var result = await CreateAsync(request);
			await Wait4States(result.BusinessProcessId, TransferState.Created.ToString());
			return result;
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
			await Wait4States(result.BusinessProcessId, TransferState.Created.ToString());

			return result;
		}

		internal async Task<string> GetFileTransferIdFromTransferId(string transferId)
		{
			var request = new FileTransferSearchRequest()
			{
				Limit = 1000
			};

			var result = await SearchFilesAsync(request);
			var fileTransferId = result.Results.Where(i => i.TransferId == transferId).Select(i => i.Id).FirstOrDefault();

			return fileTransferId;
		}

		internal async Task UploadFileAsync(SemaphoreSlim throttler, string transferId, string absoluteFilePath, int chunkSize = 1024 * 1024)
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

		private async Task<BusinessProcessWaitResult> Wait4States(string businessProcessId, string states, int timeout = 10 * 60 * 1000)
		{
			var waitResult = await _businessProcessClient.WaitForStatesAsync(businessProcessId, states, timeout);

			if (waitResult.HasStateHit)
				return waitResult;

			var exception = waitResult.BusinessProcess.StateHistory.Single(i => i.Error != null);

			// TODO: Deserialize exception
			throw new Exception(exception.Error.Exception);
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

				var blacklist = GetBlacklist();
				_fileNameBlacklist = blacklist.Items.Select(i => i.Match.ToLowerInvariant()).ToList();
			}

			return _fileNameBlacklist;
		}
	}
}
