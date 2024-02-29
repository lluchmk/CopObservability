using Microsoft.EntityFrameworkCore;

namespace CustomersAPI;

public class Customer
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
}

public class CustomersContext : DbContext
{
    public DbSet<Customer> Customers { get; set; }

    public CustomersContext(DbContextOptions<CustomersContext> options)
        : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>()
            .HasKey(e => e.Id);

        modelBuilder.Entity<Customer>()
            .HasData(new { Id = Guid.Parse("87c77822-d53d-4db1-8e66-468e28102456"), Name = "Kevin Lluch" }
                , new { Id = Guid.Parse("3f617303-3844-4403-9017-4fb0bd0ac827"), Name = "Pedro Monge" }
                , new { Id = Guid.Parse("c654b145-1a4a-43b4-a741-87b186554edc"), Name = "Francisco Rubio" });
    }
}