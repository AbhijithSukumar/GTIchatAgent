using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using GtiMcpServer.DTO;

namespace GtiMcpServer
{
    /// <summary>
    /// Represents the Logistics toolset for the MCP Server.
    /// These tools allow an AI Agent to query operational logistics data 
    /// from the backend master data file.
    /// </summary>
    [McpServerToolType]
    public class LogisticsTools
    {
        /// <summary>
        /// Private helper to read and deserialize the JSON data source.
        /// </summary>
        /// <typeparam name="T">The type of DTO to deserialize.</typeparam>
        /// <param name="key">The dictionary key mapping to the requested data array.</param>
        private async Task<List<T>> GetData<T>(string key)
        {
            // Note: In production, consider caching this data to avoid repeated disk I/O.
            var jsonData = await File.ReadAllTextAsync("master_data.json");
            var fullData = JsonSerializer.Deserialize<Dictionary<string, List<T>>>(jsonData,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return fullData.GetValueOrDefault(key) ?? new List<T>();
        }

        [McpServerTool(Name = "get_my_orders")]
        [Description("Retrieves a list of all orders associated with a specific user. Use this when the user asks about their personal order history.")]
        public async Task<string> GetMyOrders(
            [Description("The unique system ID of the user. Mandatory: Always retrieve this from the system context.")] int userId)
            => JsonSerializer.Serialize((await GetData<Order>("orders")).Where(o => o.UserId == userId));

        [McpServerTool(Name = "get_shipments")]
        [Description("Retrieves all current active shipments across the logistics network.")]
        public async Task<string> GetShipments()
            => JsonSerializer.Serialize(await GetData<Shipment>("shipments"));

        [McpServerTool(Name = "get_inventory")]
        [Description("Retrieves current stock levels for items across all managed warehouses.")]
        public async Task<string> GetInventory()
            => JsonSerializer.Serialize(await GetData<Inventory>("inventory"));

        [McpServerTool(Name = "get_warehouses")]
        [Description("Retrieves a directory of all operational warehouses and their locations.")]
        public async Task<string> GetWarehouses()
            => JsonSerializer.Serialize(await GetData<Warehouse>("warehouses"));

        [McpServerTool(Name = "get_returns")]
        [Description("Retrieves information on customer returns, including pending requests and processed items.")]
        public async Task<string> GetReturns()
            => JsonSerializer.Serialize(await GetData<Return>("returns"));

        [McpServerTool(Name = "get_suppliers")]
        [Description("Retrieves a list of registered suppliers including their performance ratings and contact data.")]
        public async Task<string> GetSuppliers()
            => JsonSerializer.Serialize(await GetData<Supplier>("suppliers"));

        [McpServerTool(Name = "get_drivers")]
        [Description("Retrieves the current status (Available, Busy, Off-duty) of delivery personnel.")]
        public async Task<string> GetDrivers()
            => JsonSerializer.Serialize(await GetData<Driver>("drivers"));

        [McpServerTool(Name = "get_vehicles")]
        [Description("Retrieves the maintenance status and operational availability of the transport fleet.")]
        public async Task<string> GetVehicles()
            => JsonSerializer.Serialize(await GetData<Vehicle>("vehicles"));

        [McpServerTool(Name = "get_alerts")]
        [Description("Retrieves urgent logistics alerts, such as shipment delays, weather impact, or inventory shortages.")]
        public async Task<string> GetAlerts()
            => JsonSerializer.Serialize(await GetData<Alert>("alerts"));
    }
}
