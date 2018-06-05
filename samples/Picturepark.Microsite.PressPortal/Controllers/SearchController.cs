using Microsoft.AspNetCore.Mvc;
using Picturepark.Microsite.PressPortal.Contracts;
using Picturepark.Microsite.PressPortal.Repository;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Picturepark.Microsite.PressPortal.Controllers
{
    public class SearchViewModel
    {
        public string SearchString { get; set; }

        public List<SearchResult> SearchResults { get; set; }
    }

    public class SearchController : Controller
    {
        private readonly IPressReleaseRepository _pressReleaseRepository;

        public SearchController(IPressReleaseRepository pressReleaseRepository)
        {
            _pressReleaseRepository = pressReleaseRepository;
        }

        public async Task<IActionResult> Index([FromQuery]string search)
        {
            var result = await _pressReleaseRepository.Search(0, 10, search);

            var viewModel = new SearchViewModel
            {
                SearchString = search,
                SearchResults = result
            };

            return View(viewModel);
        }
    }
}