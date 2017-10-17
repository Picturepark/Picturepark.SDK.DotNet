using Picturepark.Microsite.Example.Contracts;
using Picturepark.SDK.V1.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Picturepark.Microsite.Example.Helpers
{
	public interface IServiceHelper
	{
		void EnsureSchemaExists<T>(Action<SchemaDetail> beforeCreateOrUpdateAction, bool update);
	}
}
