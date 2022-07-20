using System;
using System.Threading.Tasks;
using Picturepark.SDK.V1.CloudManager.Contract;
using Picturepark.SDK.V1.CloudManager.Tests.Fixtures;
using Xunit;

namespace Picturepark.SDK.V1.CloudManager.Tests;

[Trait("Stack-CloudManager", "Customer")]
public class CustomerTests : IClassFixture<ClientFixture>
{
    private readonly ClientFixture _fixture;

    public CustomerTests(ClientFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ShouldGetCustomerNotFoundException()
    {
        await Assert.ThrowsAsync<CustomerNotFoundException>(() => _fixture.Client.Customer.GetAsync($"{Guid.NewGuid():N}"));
    }
}