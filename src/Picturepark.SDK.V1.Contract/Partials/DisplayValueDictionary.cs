namespace Picturepark.SDK.V1.Contract
{
    public partial class DisplayValueDictionary
    {
        public string Name => this[nameof(DisplayPatternType.Name).ToLowerCamelCase()];

        public string Detail => this[nameof(DisplayPatternType.Detail).ToLowerCamelCase()];

        public string List => this[nameof(DisplayPatternType.List).ToLowerCamelCase()];

        public string Thumbnail => this[nameof(DisplayPatternType.Thumbnail).ToLowerCamelCase()];
    }
}