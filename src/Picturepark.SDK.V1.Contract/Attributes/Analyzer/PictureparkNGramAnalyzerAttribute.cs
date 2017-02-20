using System;

namespace Picturepark.SDK.V1.Contract.Attributes.Analyzer
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    public class PictureparkNGramAnalyzerAttribute : PictureparkAnalyzerAttribute, IPictureparkAttribute
    {
        public override AnalyzerBase CreateAnalyzer()
        {
            return new NGramAnalyzer { SimpleSearch = SimpleSearch };
        }
    }
}
