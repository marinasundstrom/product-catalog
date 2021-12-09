using System.Globalization;

using Catalog.Client;

using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("sv-SE");
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CurrentCulture;

string UriString = $"https://localhost:5010/";

builder.Services.AddHttpClient(nameof(Catalog.Client.ProductsClient), (sp, http) =>
{
    http.BaseAddress = new Uri(UriString);
})
.AddTypedClient<Catalog.Client.IProductsClient>((http, sp) => new Catalog.Client.ProductsClient(http));

await builder.Build().RunAsync();