using System;

namespace Picturepark.SDK.V1.Contract.Attributes.Analyzer
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class PictureparkLanguageAnalyzerAttribute : PictureparkAnalyzerAttribute
    {
        public override AnalyzerBase CreateAnalyzer()
        {
            return new LanguageAnalyzer { SimpleSearch = SimpleSearch };
        }
    }
}
