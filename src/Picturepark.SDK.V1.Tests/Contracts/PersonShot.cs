using Picturepark.SDK.V1.Contract.Attributes;
using Picturepark.SDK.V1.Contract.Attributes.Analyzer;
using Picturepark.SDK.V1.Contract;
using System.Collections.Generic;

namespace Picturepark.SDK.V1.Tests.Contracts
{
    [PictureparkSchema(SchemaType.Layer)]
    [PictureparkDisplayPattern(DisplayPatternType.Name, TemplateEngine.DotLiquid, "{{data.personShot.description}}")]
    public class PersonShot
    {
        public List<Person> Persons { get; set; }

        public List<Country> Countries { get; set; }

        [PictureparkSimpleAnalyzer(SimpleSearch = true)]
        public string Description { get; set; }

        [PictureparkPathHierarchyAnalyzer(SimpleSearch = true)]
        public string Path { get; set; }
    }
}
