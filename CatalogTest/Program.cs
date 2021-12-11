using CatalogTest;
using CatalogTest.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;

Console.WriteLine("Hello, World!");

var optionsBuilder = new DbContextOptionsBuilder<CatalogContext>();
optionsBuilder.UseSqlite("Data Source=mydb.db");

using CatalogContext context = new(optionsBuilder.Options);

await context.Database.EnsureDeletedAsync();
await context.Database.EnsureCreatedAsync();

context.Products.Add(new Product()
{
    Id = Guid.NewGuid().ToString(),
    Name = "Coffee",
    Description = "A cup of our own blend of Arabica beans from Colombia and Ethiopia.",
    SKU = "35635465",
    UPC = "2353575686",
});

//CreateShirt(context);

Seed.CreateShirt2(context);

await Seed.CreateKebabPlate(context);

await Seed.CreatePizza(context);

await context.SaveChangesAsync();

/*
var products = await context.Products.AsNoTracking().AsSingleQuery()
    .Include(x => x.Options)
    .Include(x => x.Variants)
    .ThenInclude(x => x.Values)
    .ThenInclude(x => x.Option)
    .Include(x => x.Variants)
    .ThenInclude(x => x.Values)
    .ThenInclude(x => x.Value)
    .ToArrayAsync();

Console.WriteLine(products.First().Name);
*/

var api = new Api(context);
var products2 = await api.GetProducts();
var product = products2.First();

Console.WriteLine(product.Name);

Console.WriteLine();

var options = await api.GetProductOptions(product.Id);

foreach (var option in options)
{
    Console.WriteLine($"* {option.Name}");

    var optionValues = await api.GetOptionValues(option.Id);

    foreach (var value in optionValues)
    {
        Console.WriteLine($"- {value.Name}");
    }

    Console.WriteLine();
}

var variants = await api.GetProductVariants(product.Id);

foreach (var variant in variants)
{
    Console.WriteLine($"# {variant.Description} (SKU: {variant.SKU})");

    var variantOptions = await api.GetProductVariantOptions(product.Id, variant.Id);

    foreach (var variantOption in variantOptions)
    {
        Console.WriteLine($"{variantOption.Name}: {variantOption.Value}");
    }

    Console.WriteLine();
}