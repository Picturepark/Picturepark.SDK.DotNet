using Newtonsoft.Json;
using NJsonSchema.Converters;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Picturepark.SDK.V1.Conversion;

namespace Picturepark.SDK.V1.Tests.Contracts
{
    [PictureparkReference]
    [KnownType(typeof(SoccerPlayer))]
    [KnownType(typeof(SoccerTrainer))]
    [PictureparkSchema(SchemaType.List)]
    [JsonConverter(typeof(JsonInheritanceConverter), "kind")]
    [PictureparkDisplayPattern(DisplayPatternType.Name, TemplateEngine.DotLiquid, "{{data.person.firstname}} {{data.person.lastName}}")]
    [PictureparkDisplayPattern(DisplayPatternType.List, TemplateEngine.DotLiquid, "{{data.person.firstname}} {{data.person.lastName}}, {{data.person.emailAddress}}")]
    [PictureparkDisplayPattern(DisplayPatternType.Thumbnail, TemplateEngine.DotLiquid, "{{data.person.firstname}} {{data.person.lastName}}")]
    [PictureparkDisplayPattern(DisplayPatternType.Detail, TemplateEngine.DotLiquid, "{{data.person.firstname}} {{data.person.lastName}}")]
    public class Person : ReferenceObject
    {
        [PictureparkSearch(Index = true, SimpleSearch = true, Boost = 10)]
        [PictureparkString(MultiLine = true)]
        [PictureparkRequired]
        public string Firstname { get; set; }

        [PictureparkSearch(Index = true, SimpleSearch = true, Boost = 10)]
        [PictureparkRequired]
        public string LastName { get; set; }

        [PictureparkPattern(@"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z")]
        [PictureparkRequired]
        public string EmailAddress { get; set; }

        [PictureparkSearch(Index = true, SimpleSearch = false, Boost = 0)]
        public DateTime BirthDate { get; set; }

        // Usage for recursions
        [PictureparkSearch(Index = true, SimpleSearch = true, Boost = 10)]
        public Person Child { get; set; }

        [PictureparkSearch(Index = true, SimpleSearch = true, Boost = 10)]
        public List<Person> Siblings { get; set; }
    }

    public class SoccerPlayer : Person
    {
        [PictureparkTagbox("{ 'kind': 'TermFilter', 'field': 'contentType', Term: 'FC Aarau' }")]
        public Club Club { get; set; }

        [PictureparkTagbox("{ 'kind': 'TermFilter', 'field': 'contentType', Term: 'Krummbein' }")]
        public List<Pet> OwnsPets { get; set; }

        public List<Addresses> Addresses { get; set; }
    }

    public class SoccerTrainer : Person
    {
        [PictureparkDate]
        [JsonConverter(typeof(DateFieldConverter))]
        public DateTime TrainerSince { get; set; }

        public List<Club> PreviousClubs { get; set; }
    }

    [PictureparkSchema(SchemaType.Struct)]
    public class Addresses
    {
        [PictureparkRequired]
        public string Name { get; set; }

        public Pet SecurityPet { get; set; }
    }

    [PictureparkReference]
    [PictureparkSchema(SchemaType.List)]
    public class Club : ReferenceObject
    {
        [MaxLength(10)]
        public string Name { get; set; }

        public string Country { get; set; }
    }

    [PictureparkReference]
    [KnownType(typeof(Dog))]
    [KnownType(typeof(Cat))]
    [JsonConverter(typeof(JsonInheritanceConverter), "kind")]
    [PictureparkSchema(SchemaType.List)]
    public class Pet : ReferenceObject
    {
        public string Name { get; set; }
    }

    public class Dog : Pet
    {
        public bool PlaysCatch { get; set; }
    }

    public class Cat : Pet
    {
        public bool ChasesLaser { get; set; }
    }
}
