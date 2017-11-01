using Newtonsoft.Json;
using Picturepark.SDK.V1.Contract.Interfaces;
using System.Collections.Generic;

namespace Picturepark.Microsite.Example.Contracts.Jobs
{
	public class JobPositions : IReference
	{
		[JsonProperty("JobPositions")]
		public List<JobPosition> Jobs { get; set; }

		public string refId { get; set; }
	}
}
