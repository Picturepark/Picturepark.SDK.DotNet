using System;

namespace Picturepark.SDK.V1.Contract.Attributes.Analyzer
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class PictureparkKeywordLowercaseAnalyzerAttribute : PictureparkAnalyzerAttribute
    {
        public override AnalyzerBase CreateAnalyzer()
        {
            return new KeywordLowercaseAnalyzer();
        }
    }
}
