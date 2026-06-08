using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SCM3.Data.DbContext;
using SCM3.Data.Entities;
using SCM3.Data.Repositories;

namespace SCM3.Tests.Integration.Repositories;

public class CustomerRepositoryTests
{
    private static SCM3DbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SCM3DbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new SCM3DbContext(options);
    }

    [Fact]
    public async Task GetAllAsync_ExcludesSoftDeletedCustomers_AndOrdersByName()
    {
        await using var context = CreateContext();
        context.Customers.AddRange(
            new Customer { Name = "US Navy" },
            new Customer { Name = "US Army" },
            new Customer { Name = "Retired Customer", IsDeleted = true });
        await context.SaveChangesAsync();

        var repo = new CustomerRepository(context);
        var result = await repo.GetAllAsync();

        result.Select(c => c.Name).Should().Equal("US Army", "US Navy");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenCustomerIsSoftDeleted()
    {
        await using var context = CreateContext();
        var deleted = new Customer { Name = "Retired Customer", IsDeleted = true };
        context.Customers.Add(deleted);
        await context.SaveChangesAsync();

        var repo = new CustomerRepository(context);
        var result = await repo.GetByIdAsync(deleted.CustomerId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCustomer_WhenItExistsAndIsNotDeleted()
    {
        await using var context = CreateContext();
        var customer = new Customer { Name = "USAF" };
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var repo = new CustomerRepository(context);
        var result = await repo.GetByIdAsync(customer.CustomerId);

        result.Should().NotBeNull();
        result!.Name.Should().Be("USAF");
    }

    [Fact]
    public async Task AddAsync_PersistsTheCustomer()
    {
        await using var context = CreateContext();
        var repo = new CustomerRepository(context);

        await repo.AddAsync(new Customer { Name = "US Army" });

        var customers = await context.Customers.ToListAsync();
        customers.Should().ContainSingle(c => c.Name == "US Army");
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesRatherThanRemovingTheRow()
    {
        await using var context = CreateContext();
        var customer = new Customer { Name = "US Navy" };
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var repo = new CustomerRepository(context);
        await repo.DeleteAsync(customer.CustomerId);

        var stored = await context.Customers.FindAsync(customer.CustomerId);
        stored.Should().NotBeNull();
        stored!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_DoesNothing_WhenCustomerDoesNotExist()
    {
        await using var context = CreateContext();
        var repo = new CustomerRepository(context);

        var act = () => repo.DeleteAsync(999);

        await act.Should().NotThrowAsync();
    }
}
