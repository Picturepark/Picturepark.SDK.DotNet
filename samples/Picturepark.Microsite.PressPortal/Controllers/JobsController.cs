using Microsoft.AspNetCore.Mvc;
using Picturepark.Microsite.PressPortal.Contracts;
using Picturepark.Microsite.PressPortal.Contracts.Jobs;
using Picturepark.Microsite.PressPortal.Helpers;
using Picturepark.SDK.V1.Contract;
using System.Linq;
using System.Threading.Tasks;
using Picturepark.Microsite.PressPortal.Services;
using Picturepark.SDK.V1.Contract.Extensions;

namespace Picturepark.Microsite.PressPortal.Controllers
{
    public class JobsController : Controller
    {
        private readonly IPictureparkServiceClient _client;

        public JobsController(IPictureparkServiceClient client)
        {
            _client = client;
        }

        public async Task<IActionResult> Jobs()
        {
            // JobsAtPicturepark
            var searchResult = await _client.Contents.SearchAsync(new ContentSearchRequest
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
            var jobsData = await _client.Contents.GetManyAsync(searchResult.Results.Select(i => i.Id), new []{ ContentResolveBehaviour.Content });

            // Convert to C# poco
            var jobsAtPicturepark = jobsData.AsContentItems<JobsAtPicturepark>().FirstOrDefault();

            return View(jobsAtPicturepark?.Content);
        }

        public async Task<IActionResult> JobDetail(string id)
        {
            var objectDetail = await _client.ListItems.GetAndConvertToAsync<JobPosition>(id, nameof(JobPosition));

            return View(objectDetail);
        }
    }
}
