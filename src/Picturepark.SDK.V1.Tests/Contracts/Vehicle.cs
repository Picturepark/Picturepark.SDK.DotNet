using System;
using System.Runtime.Serialization;
using Picturepark.SDK.V1.Contract;
using Picturepark.SDK.V1.Contract.Attributes;

namespace Picturepark.SDK.V1.Tests.Contracts
{
    [KnownType(typeof(Car))]
    [PictureparkSchema(SchemaType.List)]
    public class Vehicle
    {
        public int NumberOfWheels { get; set; }

        public int HorsePower { get; set; }
    }

    [PictureparkSchema(SchemaType.List, "Automobile")]
    public class Car : Vehicle
    {
        public string Model { get; set; }

        public int BootSize { get; set; }

        [PictureparkDate]
        public DateTime Introduced { get; set; }

        [PictureparkDateTime("yyyy-MM-dd hh:mm:ss")]
        public DateTime FirstPieceManufactured { get; set; }
    }
}