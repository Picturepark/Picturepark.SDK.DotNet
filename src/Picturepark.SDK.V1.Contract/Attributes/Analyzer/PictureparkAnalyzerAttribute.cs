using System;

namespace Picturepark.SDK.V1.Contract.Attributes.Analyzer
{
    public abstract class PictureparkAnalyzerAttribute : Attribute, IPictureparkAttribute
    {
        public bool SimpleSearch { get; set; }

        /// <summary>Creates an analyzer based on the attribute.</summary>
        /// <returns>The analyzer.</returns>
        public abstract AnalyzerBase CreateAnalyzer();
    }
}
