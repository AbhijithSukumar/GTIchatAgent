namespace GtiMcpServer.DTO
{
    public record Order(string OrderId, int UserId, string Product, string Status);
    public record Shipment(string ShipmentId, string OrderId, string Carrier, string Location);
    public record Inventory(string ItemId, string Name, int Stock, string WarehouseId);
    public record Warehouse(string WarehouseId, string Location, int Capacity);
    public record Return(string ReturnId, string OrderId, string Reason, string Status);
    public record Supplier(string SupplierId, string Name, double Rating);
    public record Driver(string DriverId, string Name, string Status);
    public record Vehicle(string VehicleId, string Type, string Status);
    public record Alert(string AlertId, string Message, string Severity);
}
