namespace Picturepark.SDK.V1.Contract
{
	public class ContentItem<T>
	{
		public string Id { get; set; }

		public T Content { get; set; }

		public UserAudit Audit { get; set; }
	}
}
