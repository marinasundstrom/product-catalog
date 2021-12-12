﻿namespace CatalogTest;

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

    public async Task<IEnumerable<ApiProduct>> GetProducts(bool includeUnlisted = false)
    {
        var query = context.Products
            .AsSplitQuery()
            .AsNoTracking()
            .Include(pv => pv.Group)
            .AsQueryable();

        if (!includeUnlisted)
        {
            query = query.Where(x => x.Visibility == Data.ProductVisibility.Listed);
        }

        var products = await query.ToArrayAsync();

        return products.Select(x => new ApiProduct(x.Id, x.Name, x.Description, x.SKU, x.Image, x.Price, x.HasVariants, x.Visibility == Data.ProductVisibility.Listed ? ProductVisibility.Listed : ProductVisibility.Unlisted));
    }

    public async Task<ApiProduct?> GetProduct(string productId)
    {
        var product = await context.Products
            .AsSplitQuery()
            .AsNoTracking()
            .Include(pv => pv.Group)
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product == null) return null;

        return new ApiProduct(product.Id, product.Name, product.Description, product.SKU, product.Image, product.Price, product.HasVariants, product.Visibility == Data.ProductVisibility.Listed ? ProductVisibility.Listed : ProductVisibility.Unlisted);
    }

    public async Task<ApiProduct?> CreateProduct(ApiCreateProduct data)
    {
        var group = await context.ProductGroups
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == data.GroupId);

        var product = new Product()
        {
            Id = Guid.NewGuid().ToString(),
            Name = data.Name,
            Description = data.Description,
            Group = group,
            SKU = data.SKU,
            Price = data.Price,
            HasVariants = data.HasVariants
        };

        if (data.Visibility == null)
        {
            product.Visibility = Data.ProductVisibility.Unlisted;
        }
        else
        {
            product.Visibility = data.Visibility == ProductVisibility.Listed ? Data.ProductVisibility.Listed : Data.ProductVisibility.Unlisted;
        }

        context.Products.Add(product);

        await context.SaveChangesAsync();

        return new ApiProduct(product.Id, product.Name, product.Description, product.SKU, product.Image, product.Price, product.HasVariants, product.Visibility == Data.ProductVisibility.Listed ? ProductVisibility.Listed : ProductVisibility.Unlisted);
    }

    public async Task UpdateProductDetails(string productId, ApiUpdateProductDetails data)
    {
        var product = await context.Products
            .FirstAsync(x => x.Id == productId);

        var group = await context.ProductGroups
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == data.GroupId);

        product.Name = data.Name;
        product.Description = data.Description;
        product.Group = group;
        product.SKU = data.SKU;
        product.Price = data.Price;

        await context.SaveChangesAsync();
    }

    public async Task UpdateProductVisibility(string productId, ProductVisibility visibility)
    {
        var product = await context.Products
            .FirstAsync(x => x.Id == productId);

        product.Visibility = visibility == ProductVisibility.Listed ? Data.ProductVisibility.Listed : Data.ProductVisibility.Unlisted;

        await context.SaveChangesAsync();
    }

    public async Task<ApiOptionValue> CreateProductOptionValue(string productId, string optionId, ApiCreateProductOptionValue data)
    {
        var product = await context.Products
            .FirstAsync(x => x.Id == productId);

        var option = await context.Options
            .FirstAsync(x => x.Id == optionId);

        var value = new OptionValue
        {
            Name = data.Name,
            SKU = data.SKU,
            Price = data.Price
        };

        option.Values.Add(value);

        await context.SaveChangesAsync();

        return new ApiOptionValue(value.Id, value.Name, value.SKU, value.Price, value.Seq);
    }

    public async Task<ApiOption> CreateProductOption(string productId, ApiCreateProductOption data)
    {
        var product = await context.Products
            .FirstAsync(x => x.Id == productId);

        var group = await context.OptionGroups
            .FirstOrDefaultAsync(x => x.Id == data.GroupId);

        Option option = new()
        {
            Id = Guid.NewGuid().ToString(),
            Name = data.Name,
            Description = data.Description,
            SKU = data.SKU,
            Group = group,
            OptionType = data.OptionType == OptionType.Single ? Data.OptionType.Single : Data.OptionType.Multiple
        };

        foreach (var v in data.Values)
        {
            var value = new OptionValue
            {
                Id = Guid.NewGuid().ToString(),
                Name = v.Name,
                SKU = v.SKU,
                Price = v.Price
            };

            option.Values.Add(value);
        }

        product.Options.Add(option);

        await context.SaveChangesAsync();

        return new ApiOption(option.Id, option.Name, option.Description, option.OptionType == Data.OptionType.Single ? OptionType.Single : OptionType.Multiple, option.Group == null ? null : new ApiOptionGroup(option.Group.Id, option.Group.Name, option.Group.Seq, option.Group.Min, option.Group.Max), option.SKU, option.Price, option.IsSelected,
            option.Values.Select(x => new ApiOptionValue(x.Id, x.Name, x.SKU, x.Price, x.Seq)),
            option.DefaultValue == null ? null : new ApiOptionValue(option.DefaultValue.Id, option.DefaultValue.Name, option.DefaultValue.SKU, option.DefaultValue.Price, option.DefaultValue.Seq));
    }

    public async Task DeleteProductOption(string productId, string optionId)
    {
        var product = await context.Products
            .Include(x => x.Options)
            .FirstAsync(x => x.Id == productId);

        var option = product.Options
            .First(x => x.Id == optionId);

        product.Options.Remove(option);
        context.Options.Remove(option);

        await context.SaveChangesAsync();
    }

    public async Task<ApiOption> UpdateProductOption(string productId, string optionId, ApiUpdateProductOption data)
    {
        var product = await context.Products
            .AsNoTracking()
            .FirstAsync(x => x.Id == productId);

        var option = await context.Options
            .Include(x => x.Values)
            .Include(x => x.Group)
            .FirstAsync(x => x.Id == optionId);

        var group = await context.OptionGroups
            .FirstOrDefaultAsync(x => x.Id == data.GroupId);

        option.Name = data.Name;
        option.Description = data.Description;
        option.SKU = data.SKU;
        option.Group = group;
        option.OptionType = data.OptionType == OptionType.Single ? Data.OptionType.Single : Data.OptionType.Multiple;

        foreach (var v in data.Values)
        {
            if (v.Id == null)
            {
                var value = new OptionValue
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = v.Name,
                    SKU = v.SKU,
                    Price = v.Price
                };

                option.Values.Add(value);
                context.OptionValues.Add(value);
            }
            else
            {
                var value = option.Values.First(x => x.Id == v.Id);

                value.Name = v.Name;
                value.SKU = v.SKU;
                value.Price = v.Price;
            }
        }

        foreach (var v in option.Values.ToList())
        {
            if (context.Entry(v).State == EntityState.Added)
                continue;

            var value = data.Values.FirstOrDefault(x => x.Id == v.Id);

            if (value is null)
            {
                option.Values.Remove(v);
            }
        }

        await context.SaveChangesAsync();

        return new ApiOption(option.Id, option.Name, option.Description, option.OptionType == Data.OptionType.Single ? OptionType.Single : OptionType.Multiple, option.Group == null ? null : new ApiOptionGroup(option.Group.Id, option.Group.Name, option.Group.Seq, option.Group.Min, option.Group.Max), option.SKU, option.Price, option.IsSelected,
            option.Values.Select(x => new ApiOptionValue(x.Id, x.Name, x.SKU, x.Price, x.Seq)),
            option.DefaultValue == null ? null : new ApiOptionValue(option.DefaultValue.Id, option.DefaultValue.Name, option.DefaultValue.SKU, option.DefaultValue.Price, option.DefaultValue.Seq));
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

        return options.Select(x => new ApiOption(x.Id, x.Name, x.Description, x.OptionType == Data.OptionType.Single ? OptionType.Single : OptionType.Multiple, x.Group == null ? null : new ApiOptionGroup(x.Group.Id, x.Group.Name, x.Group.Seq, x.Group.Min, x.Group.Max), x.SKU, x.Price, x.IsSelected,
            x.Values.Select(x => new ApiOptionValue(x.Id, x.Name, x.SKU, x.Price, x.Seq)),
            x.DefaultValue == null ? null : new ApiOptionValue(x.DefaultValue.Id, x.DefaultValue.Name, x.DefaultValue.SKU, x.DefaultValue.Price, x.DefaultValue.Seq)));
    }

    public async Task DeleteProductOptionValue(string productId, string optionId, string valueId)
    {
        var product = await context.Products
            .AsSplitQuery()
            .Include(pv => pv.Options)
            .ThenInclude(pv => pv.Values)
            .FirstAsync(p => p.Id == productId);

        var option = product.Options.First(o => o.Id == optionId);

        var value = option.Values.First(o => o.Id == valueId);

        option.Values.Remove(value);
        context.OptionValues.Remove(value);

        await context.SaveChangesAsync();
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

        return options.Select(x => new ApiOption(x.Id, x.Name, x.Description, x.OptionType == Data.OptionType.Single ? OptionType.Single : OptionType.Multiple, x.Group == null ? null : new ApiOptionGroup(x.Group.Id, x.Group.Name, x.Group.Seq, x.Group.Min, x.Group.Max), x.SKU, x.Price, x.IsSelected,
            x.Values.Select(x => new ApiOptionValue(x.Id, x.Name, x.SKU, x.Price, x.Seq)),
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

        return options.Select(x => new ApiOption(x.Id, x.Name, x.Description, x.OptionType == Data.OptionType.Single ? OptionType.Single : OptionType.Multiple, x.Group == null ? null : new ApiOptionGroup(x.Group.Id, x.Group.Name, x.Group.Seq, x.Group.Min, x.Group.Max), x.SKU, x.Price, x.IsSelected,
            x.Values.Select(x => new ApiOptionValue(x.Id, x.Name, x.SKU, x.Price, x.Seq)),
            x.DefaultValue == null ? null : new ApiOptionValue(x.DefaultValue.Id, x.DefaultValue.Name, x.DefaultValue.SKU, x.DefaultValue.Price, x.DefaultValue.Seq)));
    }

    public async Task<IEnumerable<ApiOptionGroup>> GetOptionGroups(string productId)
    {
        var groups = await context.OptionGroups
            .AsTracking()
            .Include(x => x.Product)
            .Where(x => x.Product!.Id == productId)
            .ToListAsync();

        return groups.Select(group => new ApiOptionGroup(group.Id, group.Name, group.Seq, group.Min, group.Max));
    }


    public async Task<ApiOptionGroup?> CreateOptionGroup(string productId, ApiCreateProductOptionGroup data)
    {
        var product = await context.Products
            .FirstAsync(x => x.Id == productId);

        var group = new OptionGroup()
        {
            Id = Guid.NewGuid().ToString(),
            Name = data.Name,
            Description = data.Description,
            Min = data.Min,
            Max = data.Max
        };

        product.OptionGroups.Add(group);

        await context.SaveChangesAsync();

        return new ApiOptionGroup(group.Id, group.Name, group.Seq, group.Min, group.Max);
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
            Id = Guid.NewGuid().ToString(),
            Name = data.Name,
            Description = data.Description,
            SKU = data.SKU,
            Price = data.Price
        };

        foreach (var value in data.Values)
        {
            var option = product.Options.First(x => x.Id == value.OptionId);

            var value2 = option.Values.First(x => x.Id == value.ValueId);

            variant.Values.Add(new VariantValue()
            {
                Option = option,
                Value = value2
            });
        }

        product.Variants.Add(variant);

        await context.SaveChangesAsync();

        return new ApiProductVariant(variant.Id, variant.Name, variant.Description, variant.SKU, variant.Image, variant.Price,
            variant.Values.Select(x => new ApiProductVariantOption(x.Option.Id, x.Option.Name, x.Value.Name)));
    }

    public async Task DeleteVariant(string productId, string productVariantId)
    {
        var product = await context.Products
            .AsSplitQuery()
            .Include(pv => pv.Variants)
            .FirstAsync(x => x.Id == productId);

        var variant = product.Variants.First(x => x.Id == productVariantId);

        product.Variants.Remove(variant);
        context.ProductVariants.Remove(variant);

        await context.SaveChangesAsync();
    }

    public async Task<ApiProductVariant> UpdateVariant(string productId, string productVariantId, ApiUpdateProductVariant data)
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

        var variant = product.Variants.First(x => x.Id == productVariantId);

        variant.Name = data.Name;
        variant.Description = data.Description;
        variant.SKU = data.SKU;
        variant.Price = data.Price;

        foreach (var v in data.Options)
        {
            if (v.Id == null)
            {
                var option = product.Options.First(x => x.Id == v.OptionId);

                var value2 = option.Values.First(x => x.Id == v.ValueId);

                variant.Values.Add(new VariantValue()
                {
                    Option = option,
                    Value = value2
                });
            }
            else
            {
                var option = product.Options.First(x => x.Id == v.OptionId);

                var value2 = option.Values.First(x => x.Id == v.ValueId);

                var value = variant.Values.First(x => x.Id == v.Id);

                value.Option = option;
                value.Value = value2;
            }
        }

        foreach (var v in variant.Values.ToList())
        {
            if (context.Entry(v).State == EntityState.Added)
                continue;

            var value = data.Options.FirstOrDefault(x => x.Id == v.Id);

            if (value is null)
            {
                variant.Values.Remove(v);
            }
        }

        await context.SaveChangesAsync();

        return new ApiProductVariant(variant.Id, variant.Name, variant.Description, variant.SKU, variant.Image, variant.Price,
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

        return variants.Select(x => new ApiProductVariant(x.Id, x.Name, x.Description, x.SKU, x.Image, x.Price,
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

        return new ApiProductVariant(x.Id, x.Name, x.Description, x.SKU, x.Image, x.Price,
            x.Values.Select(x => new ApiProductVariantOption(x.Option.Id, x.Option.Name, x.Value.Name)));

    }

    public async Task<ApiProductVariant?> FindProductVariant(string productId, Dictionary<string, string?> selectedOptions)
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

            variants = variants.Where(x => x.Values.Any(vv => vv.Option.Id == selectedOption.Key && vv.Value.Id == selectedOption.Value));
        }

        var x = variants.SingleOrDefault((ProductVariant?)null);

        if (x is null) return null;

        return new ApiProductVariant(x.Id, x.Name, x.Description, x.SKU, x.Image, x.Price,
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

            variants = variants.Where(x => x.Values.Any(vv => vv.Option.Id == selectedOption.Key && vv.Value.Id == selectedOption.Value));
        }

        var values = variants
            .SelectMany(x => x.Values)
            .DistinctBy(x => x.Option)
            .Where(x => x.Option.Id == optionId)
            .Select(x => x.Value);

        return values.Select(x => new ApiOptionValue(x.Id, x.Name, x.Name, x.Price, x.Seq));
    }
}

public record class ApiProduct(string Id, string Name, string? Description, string? SKU, string? Image, decimal? Price, bool HasVariants, ProductVisibility? Visibility);

public record class ApiCreateProduct(string Name, bool HasVariants, string? Description, string? GroupId, string? SKU, decimal? Price, ProductVisibility? Visibility);


public record class ApiUpdateProductDetails(string Name, string? Description, string? SKU, string? Image, decimal? Price, string? GroupId);


public record class ApiOption(string Id, string Name, string? Description, OptionType OptionType, ApiOptionGroup? Group, string? SKU, decimal? Price, bool IsSelected, IEnumerable<ApiOptionValue> Values, ApiOptionValue? DefaultValue);

public record class ApiOptionValue(string Id, string Name, string? SKU, decimal? Price, int? Seq);

public record class ApiCreateProductOption(string Name, string? Description, OptionType OptionType, ApiOptionGroup? Group, string? SKU, decimal? Price, string? GroupId, IEnumerable<ApiCreateProductOptionValue> Values);

public record class ApiCreateProductOptionValue(string Name, string? SKU, decimal? Price);

public record class ApiUpdateProductOption(string Name, string? Description, OptionType OptionType, string? SKU, decimal? Price, string? GroupId, IEnumerable<ApiUpdateProductOptionValue> Values);

public record class ApiUpdateProductOptionValue(string? Id, string Name, string? SKU, decimal? Price);


public record class ApiOptionGroup(string Id, string Name, int? Seq, int? Min, int? Max);

public record class ApiCreateProductOptionGroup(string Name, string? Description, int? Min, int? Max);


public record class ApiProductVariant(string Id, string Name, string? Description, string? SKU, string? Image, decimal? Price, IEnumerable<ApiProductVariantOption> Options);

public record class ApiProductVariantOption(string Id, string Name, string Value);


public record class ApiCreateProductVariant(string Name, string? Description, string SKU, decimal Price, IEnumerable<ApiCreateProductVariantOption> Values);

public record class ApiCreateProductVariantOption(string OptionId, string ValueId);


public record class ApiUpdateProductVariant(string Name, string? Description, string SKU, decimal Price, IEnumerable<ApiUpdateProductVariantOption> Options);

public record class ApiUpdateProductVariantOption(int? Id, string OptionId, string ValueId);


public enum OptionType { Single, Multiple }

public enum ProductVisibility { Unlisted, Listed }

