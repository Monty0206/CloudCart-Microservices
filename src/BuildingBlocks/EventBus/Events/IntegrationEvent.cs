namespace EventBus.Events;

/// <summary>
/// Base class for all integration events in the system
/// 
/// Integration Events vs Domain Events:
/// ====================================
/// - Domain Events: Internal to a service, handled within bounded context
/// - Integration Events: Cross-service communication, published to message bus
/// 
/// This base class provides:
/// - Unique identifier for event tracking and deduplication
/// - Timestamp for ordering and auditing
/// - Common structure for all events
/// 
/// Why Use Events?
/// ===============
/// Events enable Event-Driven Architecture where:
/// 1. Services react to things that have happened
/// 2. Services remain loosely coupled
/// 3. Business processes can span multiple services
/// 4. New features can be added without changing existing services
/// 
/// Example Event Flow (Saga Pattern for Order Processing):
/// 
/// 1. User places order → OrderCreatedEvent published
/// 2. Payment Service receives event → Processes payment
///    - Success → PaymentProcessedEvent (IsSuccessful = true)
///    - Failure → PaymentProcessedEvent (IsSuccessful = false)
/// 3. Order Service receives PaymentProcessedEvent
///    - If successful → Updates order status to "Paid", publishes OrderConfirmedEvent
///    - If failed → Marks order as "Payment Failed", publishes OrderCancelledEvent
/// 4. Product Service receives OrderConfirmedEvent → Reduces stock
/// 5. Notification Service receives events → Sends emails to customer
/// 
/// All services remain independent - they just react to events!
/// This is the Saga pattern for distributed transactions.
/// </summary>
public abstract class IntegrationEvent
{
    /// <summary>
    /// Unique identifier for this event instance
    /// Used for:
    /// - Idempotency (ensuring events are processed only once)
    /// - Debugging and tracing
    /// - Event correlation across services
    /// </summary>
    public Guid Id { get; private set; }
    
    /// <summary>
    /// UTC timestamp when event occurred
    /// Used for:
    /// - Event ordering
    /// - Audit trails
    /// - Time-based filtering
    /// - Detecting delayed message processing
    /// </summary>
    public DateTime OccurredOn { get; private set; }

    /// <summary>
    /// Constructor - Automatically generates ID and timestamp
    /// Protected so only derived event classes can instantiate
    /// </summary>
    protected IntegrationEvent()
    {
        Id = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
    }
}
