using Picturepark.Microsite.Example.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Picturepark.Microsite.Example.Repository
{
	public interface IRepository<T>
	{
		Task<List<T>> List(int start, int limit, string searchString);

		Task<List<SearchResult>> Search(int start, int limit, string searchString);

		Task<T> Get(string id);
	}
}
