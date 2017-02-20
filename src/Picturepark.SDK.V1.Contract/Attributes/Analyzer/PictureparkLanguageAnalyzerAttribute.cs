using System;

namespace Picturepark.SDK.V1.Contract.Attributes.Analyzer
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class PictureparkLanguageAnalyzerAttribute : PictureparkAnalyzerAttribute, IPictureparkAttribute
    {
        public override AnalyzerBase CreateAnalyzer()
        {
            return new LanguageAnalyzer { SimpleSearch = SimpleSearch };
        }
    }
}
