using Microsoft.EntityFrameworkCore;

namespace CatalogAPI;

public class Product
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
}

public class ProductsContext : DbContext
{
    public DbSet<Product> Products { get; set; }

    public ProductsContext(DbContextOptions<ProductsContext> options)
        : base(options)
    { }    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>()
            .HasKey(p => p.Id);

        modelBuilder.Entity<Product>()
            .HasData(new { Id = Guid.Parse("5ba58a44-ead2-4efa-96dd-b789101953e6"), Name = "Bic", Description = "May contain traces of lighters and razors" }
                , new { Id = Guid.Parse("9323c4f1-8a0b-4dda-9272-a96b4c59313f"), Name = "Pencil", Description = "Wooden and yellow, Green Lantern's worst enemy" }
                , new { Id = Guid.Parse("e7e45871-5885-462f-b6e7-85dec42e037e"), Name = "Notebook", Description = "Careful writing names in it, anything could happen" });
    }
}