using Picturepark.SDK.V1.Contract;

namespace Picturepark.SDK.V1.Tests.FluentAssertions
{
    public static class UserAssertionsExtensions
    {
        public static UserAssertions Should(this User instance) => new UserAssertions(instance);
    }
}