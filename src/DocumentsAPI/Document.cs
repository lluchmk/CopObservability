using Microsoft.EntityFrameworkCore;

namespace DocumentsAPI;

public class Document
{
    public Guid Id { get; set; }
    public DocumentStatus Status { get; set; }
    public string? DownloadUrl { get; set; }
}

public enum DocumentStatus
{
    Requested,
    Processing,
    Completed,
    Failed,
}

public class DocumentContext : DbContext
{
    public DbSet<Document> Documents { get; set; }

    public DocumentContext(DbContextOptions<DocumentContext> options)
        : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Document>()
            .HasKey(d => d.Id);
    }
}
