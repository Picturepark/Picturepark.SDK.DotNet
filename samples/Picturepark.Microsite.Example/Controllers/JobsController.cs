using Microsoft.AspNetCore.Mvc;
using Picturepark.Microsite.Example.Contracts;
using Picturepark.Microsite.Example.Contracts.Jobs;
using Picturepark.Microsite.Example.Helpers;
using Picturepark.SDK.V1.Contract;
using System.Linq;
using System.Threading.Tasks;
using Picturepark.Microsite.Example.Services;
using Picturepark.SDK.V1.Contract.Extensions;

namespace Picturepark.Microsite.Example.Controllers
{
	public class JobsController : Controller
	{
		private readonly IPictureparkAccessTokenService _client;

		public JobsController(IPictureparkAccessTokenService client)
		{
			_client = client;
		}

		public async Task<IActionResult> Jobs()
		{
			// JobsAtPicturepark
			var searchResult = await _client.Content.SearchAsync(new ContentSearchRequest
			{
				Start = 0,
				Limit = 10,
				Filter = new TermFilter
				{
					Field = "contentSchemaId",
					Term = "JobsAtPicturepark"
				}
			});

			// Fetch details
			var jobsData = await _client.Content.GetManyAsync(searchResult.Results.Select(i => i.Id), new[] { ContentResolveBehavior.Content });

			// Convert to C# poco
			var jobsAtPicturepark = jobsData.AsContentItems<JobsAtPicturepark>().FirstOrDefault();

			return View(jobsAtPicturepark?.Content);
		}

		public async Task<IActionResult> JobDetail(string id)
		{
			var objectDetail = await _client.ListItem.GetAndConvertToAsync<JobPosition>(id, nameof(JobPosition));

			return View(objectDetail);
		}
	}
}
