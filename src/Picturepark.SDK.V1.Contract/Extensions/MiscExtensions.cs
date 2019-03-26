using System;

namespace Picturepark.SDK.V1.Contract.Extensions
{
    public static class MiscExtensions
    {
        public static string JoinByDot(this (string a, string b) self)
        {
            return self.a != null
                ? self.b != null ? $"{self.a}.{self.b}" : self.a
                : self.b ?? throw new Exception("unable to join two nulls");
        }
    }
}