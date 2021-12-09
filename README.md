# Product Catalog

Flexible product catalog with Web API.

Based on Products with Options (with values). Also supports Product Variants.

## Description

Product can have Options.

Option can either be a single option to be picked, or not picked, or a set of items to pick a value from. E.g. Colors or Sizes.

Product Variants represent combinations of Option values. E.g. the combination of a Color and a Size, like Blue and Medium.

A Variant has its own SKU. Physically it is its own good in the warehouse.

## Sample data

Sample products: 
* T-shirt with variants (Color, Size)
* Sallad builder
* Pizza builder

## Technology

.NET 6, ASP.NET Core, Blazor UI based on Bootstrap 5.

Entity Framework Core and Sqlite Database

Database structure is based on this [post](https://stackoverflow.com/questions/24923469/modeling-product-variants) in StackOverlow.

## Run the project

### Visual Studio

Just open it in Visual Studio.

### From CLI

In the ```Catalog.Server``` run:

```sh
dotnet run
```

Or:

Run the project with .NET Tye, by running this in the solution folder:

```sh
tye run --watch
```

## Seed data

In ```Catalog.Server``` project, open ```Program.cs```.

Make sure that this line is uncommented:

```c#
await Seed.SeedAsync(app.Services)
```

Then restart the app.

Toggle line the comment again to prevent unecessary seeding, or else, Ids will constantly change.