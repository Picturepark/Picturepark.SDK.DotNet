using Picturepark.Microsite.PressPortal.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Picturepark.Microsite.PressPortal.Repository
{
    public interface IRepository<T>
    {
        Task<List<T>> List(int start, int limit, string searchString);

        Task<List<SearchResult>> Search(int start, int limit, string searchString);

        Task<T> Get(string id);
    }
}
