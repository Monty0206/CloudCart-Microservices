using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using EventBus.Abstractions;
using EventBus.Events;
using Newtonsoft.Json;
using System.Text;

namespace EventBus.RabbitMQ;

/// <summary>
/// RabbitMQ Event Bus Implementation
/// 
/// This class implements Event-Driven Architecture for microservices communication.
/// 
/// Why Event-Driven Architecture?
/// - Loose Coupling: Services don't need to know about each other
/// - Scalability: Async processing, services can scale independently
/// - Resilience: If a service is down, messages queue up and process when it's back
/// - Flexibility: Easy to add new services that react to events
/// 
/// RabbitMQ Concepts Used:
/// - Exchange: Routes messages to queues (we use Topic exchange for flexible routing)
/// - Queue: Stores messages until consumed
/// - Routing Key: Event name used for message routing
/// - Consumer: Service that subscribes to and processes messages
/// 
/// Real-world Example:
/// When stock updates (ProductStockUpdatedEvent):
/// - Order Service checks if pending orders can now be fulfilled
/// - Analytics Service updates inventory reports
/// - Notification Service alerts admins of low stock
/// All without the Product Service knowing these services exist!
/// </summary>
public class RabbitMQEventBus : IEventBus, IDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly string _exchangeName;
    private readonly Dictionary<string, List<Type>> _handlers; // Maps event names to handler types
    private readonly IServiceProvider _serviceProvider; // For resolving handler instances

    /// <summary>
    /// Constructor - Establishes RabbitMQ connection and declares exchange
    /// 
    /// Exchange Type: Topic
    /// - Allows flexible routing with patterns
    /// - Event name becomes the routing key
    /// - Services can subscribe to specific events or patterns
    /// 
    /// Durable: true
    /// - Exchange survives RabbitMQ server restart
    /// - Important for production environments
    /// </summary>
    public RabbitMQEventBus(string hostName, string exchangeName, IServiceProvider serviceProvider)
    {
        _exchangeName = exchangeName;
        _serviceProvider = serviceProvider;
        _handlers = new Dictionary<string, List<Type>>();

        // Create RabbitMQ connection and channel
        var factory = new ConnectionFactory() { HostName = hostName };
        _connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();

        // Declare topic exchange for event routing
        // Topic exchange allows pattern-based routing (e.g., "product.*", "order.*")
        _channel.ExchangeDeclareAsync(exchange: _exchangeName, type: ExchangeType.Topic, durable: true).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Publishes an event to the message bus
    /// 
    /// Process:
    /// 1. Get event name (used as routing key)
    /// 2. Serialize event to JSON
    /// 3. Convert to bytes
    /// 4. Publish to RabbitMQ exchange
    /// 
    /// The event flows: Producer → Exchange → Queue(s) → Consumer(s)
    /// 
    /// Example Flow:
    /// ProductService.UpdateStock() 
    /// → Publishes ProductStockUpdatedEvent
    /// → RabbitMQ routes to all subscribed queues
    /// → OrderService, AnalyticsService, NotificationService receive and process
    /// 
    /// Key Point: The publisher doesn't know or care who subscribes!
    /// This is the power of pub/sub pattern in microservices.
    /// </summary>
    public void Publish(IntegrationEvent @event)
    {
        // Use event class name as routing key (e.g., "ProductStockUpdatedEvent")
        var eventName = @event.GetType().Name;
        
        // Serialize event to JSON for transmission
        var message = JsonConvert.SerializeObject(@event);
        var body = Encoding.UTF8.GetBytes(message);

        // Publish to RabbitMQ exchange
        // mandatory: false means don't return message if no queue is bound
        _channel.BasicPublishAsync(
            exchange: _exchangeName,
            routingKey: eventName,
            mandatory: false,
            body: body).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Subscribes to an event type with a handler
    /// 
    /// Generic Parameters:
    /// - T: Event type to subscribe to (e.g., ProductStockUpdatedEvent)
    /// - TH: Handler type that will process the event
    /// 
    /// Process:
    /// 1. Create a durable queue for this event type
    /// 2. Bind queue to exchange with routing key (event name)
    /// 3. Register a consumer to process messages
    /// 4. Store handler type for event processing
    /// 
    /// Queue Configuration:
    /// - durable: true → Queue survives RabbitMQ restart
    /// - exclusive: false → Multiple consumers can connect
    /// - autoDelete: false → Queue persists when no consumers
    /// 
    /// Example Usage in Order Service:
    /// eventBus.Subscribe<ProductStockUpdatedEvent, ProductStockUpdatedHandler>();
    /// 
    /// When Product Service updates stock, Order Service automatically receives
    /// the event and can check if pending orders can now be fulfilled.
    /// </summary>
    public void Subscribe<T, TH>()
        where T : IntegrationEvent
        where TH : IIntegrationEventHandler<T>
    {
        var eventName = typeof(T).Name;
        var handlerType = typeof(TH);

        // First subscription to this event type
        if (!_handlers.ContainsKey(eventName))
        {
            _handlers[eventName] = new List<Type>();

            // Create queue name: exchange.eventName (e.g., cloudcart_event_bus.ProductStockUpdatedEvent)
            var queueName = $"{_exchangeName}.{eventName}";
            
            // Declare durable queue that persists messages
            _channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false).GetAwaiter().GetResult();
            
            // Bind queue to exchange with routing key
            _channel.QueueBindAsync(queue: queueName, exchange: _exchangeName, routingKey: eventName).GetAwaiter().GetResult();

            // Create async consumer for processing messages
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                await ProcessEvent(eventName, message);
            };

            // Start consuming messages
            // autoAck: true → Automatically acknowledge message receipt
            // (In production, consider manual ack for better reliability)
            _channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer).GetAwaiter().GetResult();
        }

        // Register handler type for this event
        if (!_handlers[eventName].Contains(handlerType))
        {
            _handlers[eventName].Add(handlerType);
        }
    }

    public void Unsubscribe<T, TH>()
        where T : IntegrationEvent
        where TH : IIntegrationEventHandler<T>
    {
        var eventName = typeof(T).Name;
        var handlerType = typeof(TH);

        if (_handlers.ContainsKey(eventName) && _handlers[eventName].Contains(handlerType))
        {
            _handlers[eventName].Remove(handlerType);
        }
    }

    private async Task ProcessEvent(string eventName, string message)
    {
        if (_handlers.ContainsKey(eventName))
        {
            foreach (var handlerType in _handlers[eventName])
            {
                var handler = _serviceProvider.GetService(handlerType);
                if (handler == null) continue;

                var eventType = _handlers[eventName].First().BaseType?.GetGenericArguments()[0];
                if (eventType == null) continue;

                var integrationEvent = JsonConvert.DeserializeObject(message, eventType);
                var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                
                await (Task)concreteType.GetMethod("HandleAsync")!.Invoke(handler, new object[] { integrationEvent! })!;
            }
        }
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
