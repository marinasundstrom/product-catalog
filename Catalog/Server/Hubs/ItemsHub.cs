using System;

using Microsoft.AspNetCore.SignalR;

namespace Catalog.Server.Hubs;

public class ItemsHub : Hub<IItemsClient>
{

}