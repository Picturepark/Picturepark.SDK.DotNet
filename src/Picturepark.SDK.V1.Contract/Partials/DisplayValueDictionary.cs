namespace Picturepark.SDK.V1.Contract
{
    public partial class DisplayValueDictionary
    {
        /// <summary>
        /// Display value for <see cref="DisplayPatternType.Name"/>
        /// </summary>
        public string Name => this[nameof(DisplayPatternType.Name).ToLowerCamelCase()];

        /// <summary>
        /// Display value for <see cref="DisplayPatternType.Detail"/>
        /// </summary>
        public string Detail => this[nameof(DisplayPatternType.Detail).ToLowerCamelCase()];

        /// <summary>
        /// Display value for <see cref="DisplayPatternType.List"/>
        /// </summary>
        public string List => this[nameof(DisplayPatternType.List).ToLowerCamelCase()];

        /// <summary>
        /// Display value for <see cref="DisplayPatternType.Thumbnail"/>
        /// </summary>
        public string Thumbnail => this[nameof(DisplayPatternType.Thumbnail).ToLowerCamelCase()];

        /// <summary>
        /// Display value for <see cref="DisplayPatternType.DownloadFileName"/>
        /// </summary>
        /// <remarks>
        /// Only available for Contents.
        /// </remarks>
        public string DownloadFileName => this[nameof(DisplayPatternType.DownloadFileName).ToLowerCamelCase()];
    }
}