namespace EventBus.Events;

public class ProductStockUpdatedEvent : IntegrationEvent
{
    public string ProductId { get; set; } = string.Empty;
    public int NewStockQuantity { get; set; }
    public int QuantityChanged { get; set; }
}
