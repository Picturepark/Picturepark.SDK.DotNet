using Picturepark.Microsite.Example.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Picturepark.SDK.V1.Contract;

namespace Picturepark.Microsite.Example.Repository
{
	public interface IPressReleaseRepository : IRepository<ContentItem<PressRelease>>
	{
	}
}
