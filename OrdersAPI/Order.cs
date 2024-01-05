using Microsoft.EntityFrameworkCore;

namespace OrdersAPI;

public class Order
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid ProductId { get; set; }
    public int Amount { get; set; }
}

public class OrdersContext : DbContext
{
    public DbSet<Order> Orders { get; set; }

    public OrdersContext(DbContextOptions<OrdersContext> options)
        : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>()
            .HasKey(o => o.Id);

        modelBuilder.Entity<Order>().HasData(
            new { Id = Guid.NewGuid(), CustomerId = Guid.Parse("87c77822-d53d-4db1-8e66-468e28102456"), ProductId = Guid.Parse("5ba58a44-ead2-4efa-96dd-b789101953e6"), Amount = 15 },
            new { Id = Guid.NewGuid(), CustomerId = Guid.Parse("3f617303-3844-4403-9017-4fb0bd0ac827"), ProductId = Guid.Parse("e7e45871-5885-462f-b6e7-85dec42e037e"), Amount = 8 },
            new { Id = Guid.NewGuid(), CustomerId = Guid.Parse("3f617303-3844-4403-9017-4fb0bd0ac827"), ProductId = Guid.Parse("e7e45871-5885-462f-b6e7-85dec42e037e"), Amount = 13 },
            new { Id = Guid.NewGuid(), CustomerId = Guid.Parse("87c77822-d53d-4db1-8e66-468e28102456"), ProductId = Guid.Parse("e7e45871-5885-462f-b6e7-85dec42e037e"), Amount = 19 },
            new { Id = Guid.NewGuid(), CustomerId = Guid.Parse("87c77822-d53d-4db1-8e66-468e28102456"), ProductId = Guid.Parse("e7e45871-5885-462f-b6e7-85dec42e037e"), Amount = 64 },
            new { Id = Guid.NewGuid(), CustomerId = Guid.Parse("c654b145-1a4a-43b4-a741-87b186554edc"), ProductId = Guid.Parse("9323c4f1-8a0b-4dda-9272-a96b4c59313f"), Amount = 2 },
            new { Id = Guid.NewGuid(), CustomerId = Guid.Parse("c654b145-1a4a-43b4-a741-87b186554edc"), ProductId = Guid.Parse("e7e45871-5885-462f-b6e7-85dec42e037e"), Amount = 98 },
            new { Id = Guid.NewGuid(), CustomerId = Guid.Parse("87c77822-d53d-4db1-8e66-468e28102456"), ProductId = Guid.Parse("e7e45871-5885-462f-b6e7-85dec42e037e"), Amount = 13 });
    }
}
