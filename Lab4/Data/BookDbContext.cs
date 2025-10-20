using Microsoft.EntityFrameworkCore;
using Lab4.Models;

namespace Lab4.Data;

public class BookDbContext : DbContext
{
    public BookDbContext(DbContextOptions<BookDbContext> options) : base(options) { }

    public DbSet<Book> Books { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Author).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Isbn).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Category).IsRequired();
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.PublishedDate).IsRequired();
            entity.Property(e => e.CoverImageUrl).HasMaxLength(500);
            entity.Property(e => e.IsAvailable).IsRequired();
            entity.Property(e => e.StockQuantity).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.Year).IsRequired();
        });
    }
}
