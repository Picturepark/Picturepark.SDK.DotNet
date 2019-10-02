namespace Picturepark.SDK.V1.Contract
{
    public partial class DisplayValueDictionary
    {
        /// <summary>
        /// Name display value.
        /// </summary>
        public string Name => this[nameof(DisplayPatternType.Name).ToLowerCamelCase()];

        /// <summary>
        /// Detail display value.
        /// </summary>
        public string Detail => this[nameof(DisplayPatternType.Detail).ToLowerCamelCase()];

        /// <summary>
        /// List display value.
        /// </summary>
        public string List => this[nameof(DisplayPatternType.List).ToLowerCamelCase()];

        /// <summary>
        /// Thumbnail display value.
        /// </summary>
        public string Thumbnail => this[nameof(DisplayPatternType.Thumbnail).ToLowerCamelCase()];
    }
}