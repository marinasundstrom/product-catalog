namespace Catalog.Data;

using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.EntityFrameworkCore;

public class CatalogContext : DbContext
{
    public CatalogContext(DbContextOptions<CatalogContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        //optionsBuilder.LogTo(Console.WriteLine);

        //optionsBuilder.UseSqlite("Data Source=mydb.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(Product).Assembly);

        modelBuilder.Entity<Option>()
            .HasMany(p => p.Values)
            .WithOne(p => p.Option);

        modelBuilder.Entity<Option>()
            .HasOne(p => p.DefaultValue);

        modelBuilder.Entity<VariantValue>()
         .HasOne(m => m.Value).WithMany(m => m.VariantValues).OnDelete(DeleteBehavior.NoAction);
    }

    public DbSet<ProductGroup> ProductGroups { get; set; } = null!;

    public DbSet<Product> Products { get; set; } = null!;

    public DbSet<ProductOption> ProductOptions { get; set; } = null!;

    public DbSet<OptionGroup> OptionGroups { get; set; } = null!;

    public DbSet<Option> Options { get; set; } = null!;

    public DbSet<OptionValue> OptionValues { get; set; } = null!;

    public DbSet<ProductVariant> ProductVariants { get; set; } = null!;

    public DbSet<VariantValue> VariantValues { get; set; } = null!;
}