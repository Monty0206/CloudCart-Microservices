namespace EventBus.Events;

public class PaymentProcessedEvent : IntegrationEvent
{
    public string OrderId { get; set; } = string.Empty;
    public string PaymentId { get; set; } = string.Empty;
    public bool IsSuccessful { get; set; }
    public string Message { get; set; } = string.Empty;
}
