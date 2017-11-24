using Newtonsoft.Json;
using Picturepark.SDK.V1.Contract;
using System.Collections.Generic;

namespace Picturepark.Microsite.Example.Contracts.Jobs
{
	public class JobPositions : ReferenceObject
	{
		[JsonProperty("JobPositions")]
		public List<JobPosition> Jobs { get; set; }
	}
}
