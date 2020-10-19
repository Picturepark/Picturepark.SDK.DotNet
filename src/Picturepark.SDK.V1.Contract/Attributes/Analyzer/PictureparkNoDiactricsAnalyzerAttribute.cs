using System;

namespace Picturepark.SDK.V1.Contract.Attributes.Analyzer
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class PictureparkNoDiactricsAnalyzerAttribute : PictureparkAnalyzerAttribute
    {
        public override AnalyzerBase CreateAnalyzer()
        {
            return new NoDiactricsAnalyzer();
        }
    }
}
