# Product Catalog

Flexible product catalog with Web API.

Based on Products with Options (with values). Also supports Product Variants.

Watch [video](https://www.youtube.com/watch?v=4WSa6pAZUbk)

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

.NET 6, ASP.NET Core, Blazor UI based on MudBlazor.

Entity Framework Core and SQL Server database

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

### Expose Blobs via public URL 

To publicly expose Blobs via their URLs you have to change Azurite's configuration.

*(This requires Azurite to have been run once for the files to be created)*

Open the file ```WebApi/.data/azurite/__azurite_db_blob__.json```:

Add the ```"publicAccess": "blob"``` key-value in the section shown below:


```json
        {
            "name": "$CONTAINERS_COLLECTION$",
            "data": [
                {
                    "accountName": "devstoreaccount1",
                    "name": "images",
                    "properties": {
                        "etag": "\"0x1C839AE6CDF11F0\"",
                        "lastModified": "2021-05-14T15:08:51.726Z",
                        "leaseStatus": "unlocked",
                        "leaseState": "available",
                        "hasImmutabilityPolicy": false,
                        "hasLegalHold": false,
              --- >  "publicAccess": "blob" <---- 
                    },
                   // Omitted
        },
```

Then, restart Azurite.

## Seed data

In ```Catalog.Server``` project, open ```Program.cs```.

Make sure that this line is uncommented:

```c#
await Seed.SeedAsync(app.Services)
```

Then restart the app.

Toggle line the comment again to prevent unecessary seeding, or else, Ids will constantly change.
