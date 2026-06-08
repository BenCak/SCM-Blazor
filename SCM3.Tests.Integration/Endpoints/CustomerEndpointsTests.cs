using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SCM3.Data.Entities;

namespace SCM3.Tests.Integration.Endpoints;

public class CustomerEndpointsTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public CustomerEndpointsTests(ApiFactory factory) => _factory = factory;

    [Fact]
    public async Task GetAll_ReturnsOkWithSeededCustomers()
    {
        await using var context = _factory.CreateDbContext();
        context.Customers.Add(new Customer { Name = "US Army" });
        await context.SaveChangesAsync();

        using var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/customers");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var customers = await response.Content.ReadFromJsonAsync<List<Customer>>();
        customers.Should().ContainSingle(c => c.Name == "US Army");
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenCustomerDoesNotExist()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/customers/999999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_ReturnsOkWithCustomer_WhenItExists()
    {
        await using var context = _factory.CreateDbContext();
        var customer = new Customer { Name = "USAF" };
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        using var client = _factory.CreateClient();
        var response = await client.GetAsync($"/api/customers/{customer.CustomerId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var found = await response.Content.ReadFromJsonAsync<Customer>();
        found.Should().NotBeNull();
        found!.Name.Should().Be("USAF");
    }

    [Fact]
    public async Task Post_CreatesCustomer_AndReturnsCreatedWithLocation()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/customers", new Customer { Name = "US Navy" });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<Customer>();
        created.Should().NotBeNull();
        created!.CustomerId.Should().BeGreaterThan(0);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Be($"/api/customers/{created.CustomerId}");
    }

    [Fact]
    public async Task Delete_SoftDeletesCustomer_SoSubsequentGetByIdReturnsNotFound()
    {
        await using var context = _factory.CreateDbContext();
        var customer = new Customer { Name = "Retiring Customer" };
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        using var client = _factory.CreateClient();
        var deleteResponse = await client.DeleteAsync($"/api/customers/{customer.CustomerId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await client.GetAsync($"/api/customers/{customer.CustomerId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
