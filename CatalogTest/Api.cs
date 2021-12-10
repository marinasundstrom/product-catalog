namespace CatalogTest;

using System;

using CatalogTest.Data;

using Microsoft.EntityFrameworkCore;

public class Api
{
    private readonly CatalogContext context;

    public Api(CatalogContext context)
    {
        this.context = context;
    }

    public async Task<IEnumerable<ApiProduct>> GetProducts()
    {
        var products = await context.Products
            .AsSplitQuery()
            .AsNoTracking()
            .Include(pv => pv.Group)
            .ToArrayAsync();

        return products.Select(x => new ApiProduct(x.Id, x.Name, x.Description, x.SKU, x.Image, x.Price, x.HasVariants));
    }

    public async Task<ApiProduct?> GetProduct(string productId)
    {
        var x = await context.Products
            .AsSplitQuery()
            .AsNoTracking()
            .Include(pv => pv.Group)
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (x == null) return null;

        return new ApiProduct(x.Id, x.Name, x.Description, x.SKU, x.Image, x.Price, x.HasVariants);
    }

    public async Task<IEnumerable<ApiOption>> GetProductOptions(string productId)
    {
        var options = await context.Options
            .AsSplitQuery()
            .AsNoTracking()
            .Include(pv => pv.Group)
            .Include(pv => pv.Values)
            .Include(o => o.DefaultValue)
            .Where(p => p.Products.Any(x => x.Id == productId))
            .ToArrayAsync();

        return options.Select(x => new ApiOption(x.Id, x.Name, x.Group == null ? null : new ApiOptionGroup(x.Group.Id, x.Group.Name, x.Group.Seq, x.Group.Min, x.Group.Max), x.Values.Any(), x.SKU, x.Price, x.IsSelected,
            x.DefaultValue == null ? null : new ApiOptionValue(x.DefaultValue.Id, x.DefaultValue.Name, x.DefaultValue.SKU, x.DefaultValue.Price, x.DefaultValue.Seq)));
    }

    public async Task<IEnumerable<ApiOption>> GetOptions(bool includeChoices)
    {
        var query = context.Options
            .AsSplitQuery()
            .AsNoTracking()
            .Include(o => o.Group)
            .Include(o => o.Values)
            .Include(o => o.DefaultValue)
            .AsQueryable();

        /*
        if(includeChoices)
        {
            query = query.Where(x => !x.Values.Any());
        }
        */

        var options = await query.ToArrayAsync();

        return options.Select(x => new ApiOption(x.Id, x.Name, x.Group == null ? null : new ApiOptionGroup(x.Group.Id, x.Group.Name, x.Group.Seq, x.Group.Min, x.Group.Max), x.Values.Any(), x.SKU, x.Price, x.IsSelected,
            x.DefaultValue == null ? null : new ApiOptionValue(x.DefaultValue.Id, x.DefaultValue.Name, x.DefaultValue.SKU, x.DefaultValue.Price, x.DefaultValue.Seq)));
    }

    public async Task<IEnumerable<ApiOption>> GetOption(string optionId)
    {
        var options = await context.Options
            .AsSplitQuery()
            .AsNoTracking()
            .Include(pv => pv.Group)
            .Include(pv => pv.Values)
            .Where(o => o.Id == optionId)
            .ToArrayAsync();

        return options.Select(x => new ApiOption(x.Id, x.Name, x.Group == null ? null : new ApiOptionGroup(x.Group.Id, x.Group.Name, x.Group.Seq, x.Group.Min, x.Group.Max), x.Values.Any(), x.SKU, x.Price, x.IsSelected,
            x.DefaultValue == null ? null : new ApiOptionValue(x.DefaultValue.Id, x.DefaultValue.Name, x.DefaultValue.SKU, x.DefaultValue.Price, x.DefaultValue.Seq)));
    }

    public async Task<ApiProductVariant> CreateVariant(string productId, ApiCreateProductVariant data)
    {
        var product = await context.Products
            .AsSplitQuery()
            .Include(pv => pv.Variants)
                .ThenInclude(o => o.Values)
                .ThenInclude(o => o.Option)
            .Include(pv => pv.Variants)
                .ThenInclude(o => o.Values)
                .ThenInclude(o => o.Value)
            .Include(pv => pv.Options)
                .ThenInclude(o => o.Values)
            .FirstAsync(x => x.Id == productId);

        var variant = new ProductVariant() 
        {
            Id  = Guid.NewGuid().ToString(),
            Name = data.Name,
            Description = data.Description,
            SKU = data.SKU,
            Price = data.Price
        };

        foreach (var value in data.Values) 
        {
            var option = product.Options.First(x => x.Id == value.OptionId);

            var value2 = option.Values.First(x => x.Id == value.ValueId);

            variant.Values.Add(new VariantValue() {
                Option = option,
                Value = value2
            });
        }
        
        product.Variants.Add(variant);

        await context.SaveChangesAsync();

        return new ApiProductVariant(variant.Id, variant.Name, variant.SKU, variant.Image, variant.Price,
            variant.Values.Select(x => new ApiProductVariantOption(x.Option.Id, x.Option.Name, x.Value.Name)));    
    }

    public async Task<IEnumerable<ApiOptionValue>> GetOptionValues(string optionId)
    {
        var options = await context.OptionValues
            .AsSplitQuery()
            .AsNoTracking()
            .Include(pv => pv.Option)
            .ThenInclude(pv => pv.Group)
            .Where(p => p.Option.Id == optionId)
            .ToArrayAsync();

        return options.Select(x => new ApiOptionValue(x.Id, x.Name, x.SKU, x.Price, x.Seq));
    }

    public async Task<IEnumerable<ApiProductVariant>> GetProductVariants(string productId)
    {
        var variants = await context.ProductVariants
            .AsSplitQuery()
            .AsNoTracking()
            .Include(pv => pv.Product)
            .Include(pv => pv.Values)
            .ThenInclude(pv => pv.Option)
            .Include(pv => pv.Values)
            .ThenInclude(pv => pv.Value)
            .Where(pv => pv.Product.Id == productId)
            .ToArrayAsync();

        return variants.Select(x => new ApiProductVariant(x.Id, x.Name, x.SKU, x.Image, x.Price,
            x.Values.Select(x => new ApiProductVariantOption(x.Option.Id, x.Option.Name, x.Value.Name))));
    }

    public async Task<ApiProductVariant> GetProductVariant(string productId, string productVariantId)
    {
        var x = await context.ProductVariants
            .AsSplitQuery()
            .AsNoTracking()
            .Include(pv => pv.Product)
            .Include(pv => pv.Values)
            .ThenInclude(pv => pv.Option)
            .ThenInclude(o => o.DefaultValue)
            .Include(pv => pv.Values)
            .ThenInclude(pv => pv.Value)
            .FirstOrDefaultAsync(pv => pv.Product.Id == productId && pv.Id == productVariantId);

        return new ApiProductVariant(x.Id, x.Name, x.SKU, x.Image, x.Price,
            x.Values.Select(x => new ApiProductVariantOption(x.Option.Id, x.Option.Name, x.Value.Name)));

    }

    public async Task<ApiProductVariant> FindProductVariant(string productId, Dictionary<string, string?> selectedOptions)
    {
        IEnumerable<ProductVariant> variants = await context.ProductVariants
            .AsSplitQuery()
            .AsNoTracking()
            .Include(pv => pv.Product)
            .Include(pv => pv.Values)
            .ThenInclude(pv => pv.Option)
            .Include(pv => pv.Values)
            .ThenInclude(pv => pv.Value)
            .Where(pv => pv.Product.Id == productId)
            .ToArrayAsync();

        foreach (var selectedOption in selectedOptions)
        {
            if (selectedOption.Value is null)
                continue;

            variants = variants.Where(x => x.Values.Any(vv => vv.Option.Id == selectedOption.Key && vv.Value.Name == selectedOption.Value));
        }

        var x = variants.First();

        return new ApiProductVariant(x.Id, x.Name, x.SKU, x.Image, x.Price,
            x.Values.Select(x => new ApiProductVariantOption(x.Option.Id, x.Option.Name, x.Value.Name)));
    }

    public async Task<IEnumerable<ApiProductVariantOption>> GetProductVariantOptions(string productId, string productVariantId)
    {
        var variantOptionValues = await context.VariantValues
            .AsSplitQuery()
            .AsNoTracking()
            .Include(pv => pv.Value)
            .Include(pv => pv.Option)
            .ThenInclude(o => o.DefaultValue)
            .Include(pv => pv.Variant)
            .ThenInclude(p => p.Product)
            .Where(pv => pv.Variant.Product.Id == productId && pv.Variant.Id == productVariantId)
            .ToArrayAsync();

        return variantOptionValues.Select(x => new ApiProductVariantOption(x.Option.Id, x.Option.Name, x.Value.Name));
    }

    public async Task<IEnumerable<ApiOptionValue>> GetAvailableOptionValues(string productId, string optionId, IDictionary<string, string?> selectedOptions)
    {
        IEnumerable<ProductVariant> variants = await context.ProductVariants
          .AsSplitQuery()
          .AsNoTracking()
          .Include(pv => pv.Product)
          .Include(pv => pv.Values)
          .ThenInclude(pv => pv.Option)
          .Include(pv => pv.Values)
          .ThenInclude(pv => pv.Value)
          .Where(pv => pv.Product.Id == productId)
          .ToArrayAsync();

        foreach (var selectedOption in selectedOptions)
        {
            if (selectedOption.Value is null)
                continue;

            variants = variants.Where(x => x.Values.Any(vv => vv.Option.Id == selectedOption.Key && vv.Value.Name == selectedOption.Value));
        }

        var values = variants
            .SelectMany(x => x.Values)
            .DistinctBy(x => x.Option)
            .Where(x => x.Option.Id == optionId)
            .Select(x => x.Value);

        return values.Select(x => new ApiOptionValue(x.Id, x.Name, x.Name, x.Price, x.Seq));
    }
}

public record ApiCreateProductVariant(string Name, string? Description, string SKU, decimal Price, IEnumerable<ApiCreateProductVariantOption> Values);

public record ApiCreateProductVariantOption(string OptionId, string ValueId);

public record class ApiProduct(string Id, string Name, string Description, string? SKU, string? Image, decimal? Price, bool HasVariants);

public record class ApiProductVariant(string Id, string Description, string? SKU, string? Image, decimal? Price, IEnumerable<ApiProductVariantOption> Options);

public record class ApiProductVariantOption(string Id, string Name, string Value);

public record class ApiOption(string Id, string Name, ApiOptionGroup? Group, bool HasValues, string? SKU, decimal? Price, bool IsSelected, ApiOptionValue? DefaultValue);

public record class ApiOptionGroup(string Id, string Name, int? Seq, int? Min, int? Max);

public record class ApiOptionValue(string Id, string Value, string? SKU, decimal? Price, int? Seq);