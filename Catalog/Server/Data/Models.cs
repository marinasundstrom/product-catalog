namespace Catalog.Data;

using System.ComponentModel.DataAnnotations.Schema;

public class ProductGroup
{
    public string Id { get; set; } = null!;

    public int? Seq { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public ProductGroup? Parent { get; set; }

    public string? Image { get; set; }

    public List<ProductGroup> SubGroups { get; } = new List<ProductGroup>();

    public List<Product> Products { get; } = new List<Product>();
}

public class Product
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public ProductGroup? Group { get; set; }

    public string? Description { get; set; } = null!;

    public string? SKU { get; set; }

    public string? UPC { get; set; }

    public string? Image { get; set; }

    public decimal? Price { get; set; }

    public bool HasVariants { get; set; } = false;

    public bool? AllCustom { get; set; }

    public List<ProductVariant> Variants { get; } = new List<ProductVariant>();

    public List<Option> Options { get; } = new List<Option>();

    public List<ProductOption> ProductOptions { get; } = new List<ProductOption>();

    public List<OptionGroup> OptionGroups { get; } = new List<OptionGroup>();

    public ProductVisibility Visibility { get; set; }
}

public enum ProductVisibility
{
    Unlisted,
    Listed
}

public class ProductOption
{
    public int Id { get; set; }

    public string ProductId { get; set; } = null!;

    public Product Product { get; set; } = null!;

    public string OptionId { get; set; } = null!;

    public Option Option { get; set; } = null!;

    public bool? IsSelected { get; set; }
}

public class OptionGroup
{
    public string Id { get; set; } = null!;

    public int? Seq { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public Product? Product { get; set; }

    public List<Option> Options { get; } = new List<Option>();

    public int? Min { get; set; }

    public int? Max { get; set; }
}

public class Option
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public OptionGroup? Group { get; set; }

    public OptionType OptionType { get; set; } = OptionType.Multiple;

    public bool IsRequired { get; set; }

    public bool IsSelected { get; set; }

    public string? SKU { get; set; }

    public decimal? Price { get; set; }

    public List<OptionValue> Values { get; } = new List<OptionValue>();

    public List<Product> Products { get; } = new List<Product>();

    [ForeignKey(nameof(DefaultValue))]
    public string? DefaultValueId { get; set; }

    public OptionValue? DefaultValue { get; set; }
}

public enum OptionType
{
    Single,
    Multiple
}

public class OptionValue
{
    public string Id { get; set; } = null!;

    public int? Seq { get; set; }

    public Option Option { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? SKU { get; set; }

    public decimal? Price { get; set; }

    public List<VariantValue> VariantValues { get; } = new List<VariantValue>();
}

public class ProductVariant
{
    public string Id { get; set; } = null!;

    public Product Product { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? SKU { get; set; }

    public string? UPC { get; set; }

    public string? Image { get; set; }

    public decimal? Price { get; set; }

    public List<VariantValue> Values { get; } = new List<VariantValue>();
}

public class VariantValue
{
    public int Id { get; set; }

    //public Product Product { get; set; } = null!;

    public ProductVariant Variant { get; set; } = null!;

    public Option Option { get; set; } = null!;

    public OptionValue Value { get; set; } = null!;
}