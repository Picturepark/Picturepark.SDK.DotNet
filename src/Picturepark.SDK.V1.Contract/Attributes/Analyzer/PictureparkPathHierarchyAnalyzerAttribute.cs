using System;

namespace Picturepark.SDK.V1.Contract.Attributes.Analyzer
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class PictureparkPathHierarchyAnalyzerAttribute : PictureparkAnalyzerAttribute
    {
        /// <summary>Creates an analyzer based on the attribute.</summary>
        /// <returns>The analyzer.</returns>
        public override AnalyzerBase CreateAnalyzer()
        {
            return new PathHierarchyAnalyzer { SimpleSearch = SimpleSearch };
        }
    }
}
