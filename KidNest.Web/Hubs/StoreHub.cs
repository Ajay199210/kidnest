using KidNest.Services.DTOs.Contents;
using KidNest.Services.DTOs.Products;
using Microsoft.AspNetCore.SignalR;

namespace KidNest.Web.Hubs
{
    public class StoreHub : Hub
    {
        // You can define methods that the client can call here
        // For now, no methods needed, just broadcasting updates

        // This will broadcast a message from the server to all connected clients
        public async Task NotifyProductAdded(ProductDTO productDTO)
        {
            await Clients.All.SendAsync("ProductAdded", productDTO);
        }

        public async Task NotifyProductUpdated(ProductDTO productDTO)
        //public async Task UpdateProductList()
        {
            await Clients.All.SendAsync("ProductUpdated", productDTO);
        }

        //public async Task NotifyCategoryAdded()
        //public async Task UpdateProductList()

        public async Task NotifyContentUpdated(ContentDTO contentDTO)
        {
            await Clients.All.SendAsync("ContentAdded", contentDTO);
        }
    }
}
