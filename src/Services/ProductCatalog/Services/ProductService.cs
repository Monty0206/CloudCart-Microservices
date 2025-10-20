using ProductCatalog.Models;
using ProductCatalog.DTOs;
using ProductCatalog.Repositories;
using EventBus.Abstractions;
using EventBus.Events;
using Common.DTOs;

namespace ProductCatalog.Services;

/// <summary>
/// Product Service Interface - Business Logic Contract
/// 
/// This interface defines the contract for product-related operations.
/// Benefits of using interfaces:
/// - Enables dependency injection and loose coupling
/// - Makes unit testing easier with mock implementations
/// - Provides a clear contract for the service layer
/// - Follows SOLID principles (Dependency Inversion Principle)
/// </summary>
public interface IProductService
{
    /// <summary>Creates a new product in the catalog</summary>
    Task<ApiResponse<ProductDto>> CreateProductAsync(CreateProductDto dto);
    
    /// <summary>Retrieves a product by its unique identifier</summary>
    Task<ApiResponse<ProductDto>> GetProductByIdAsync(string id);
    
    /// <summary>Gets paginated list of products with optional category filtering</summary>
    Task<ApiResponse<PaginatedResult<ProductDto>>> GetProductsAsync(int page, int pageSize, string? category = null);
    
    /// <summary>Updates an existing product's information</summary>
    Task<ApiResponse<ProductDto>> UpdateProductAsync(string id, UpdateProductDto dto);
    
    /// <summary>Soft or hard deletes a product from the catalog</summary>
    Task<ApiResponse<bool>> DeleteProductAsync(string id);
    
    /// <summary>Searches products by name, description, or tags</summary>
    Task<ApiResponse<IEnumerable<ProductDto>>> SearchProductsAsync(string searchTerm);
    
    /// <summary>Updates product stock quantity and publishes inventory event</summary>
    Task<ApiResponse<bool>> UpdateStockAsync(string productId, int quantity);
}

/// <summary>
/// Product Service Implementation - Business Logic Layer
/// 
/// This class contains all business logic for product management.
/// Responsibilities:
/// - Validates business rules
/// - Coordinates between repository and external services
/// - Transforms domain models to/from DTOs
/// - Publishes domain events to the message bus
/// 
/// Design Pattern: Service Layer Pattern
/// - Separates business logic from data access and presentation
/// - Makes the code more maintainable and testable
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly IEventBus _eventBus;

    /// <summary>
    /// Constructor - Dependency Injection
    /// Dependencies are injected rather than created internally (Inversion of Control)
    /// This enables better testing and loose coupling between components
    /// </summary>
    public ProductService(IProductRepository repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    /// <summary>
    /// Creates a new product in the catalog
    /// Process:
    /// 1. Map DTO to domain model
    /// 2. Persist to database via repository
    /// 3. Return standardized API response with created product
    /// </summary>
    public async Task<ApiResponse<ProductDto>> CreateProductAsync(CreateProductDto dto)
    {
        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Category = dto.Category,
            ImageUrl = dto.ImageUrl,
            StockQuantity = dto.StockQuantity,
            Tags = dto.Tags
        };

        var created = await _repository.CreateAsync(product);
        return ApiResponse<ProductDto>.SuccessResponse(MapToDto(created), "Product created successfully");
    }

    public async Task<ApiResponse<ProductDto>> GetProductByIdAsync(string id)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product == null)
            return ApiResponse<ProductDto>.ErrorResponse("Product not found");

        return ApiResponse<ProductDto>.SuccessResponse(MapToDto(product));
    }

    public async Task<ApiResponse<PaginatedResult<ProductDto>>> GetProductsAsync(int page, int pageSize, string? category = null)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(
            page,
            pageSize,
            category != null ? p => p.Category == category && p.IsAvailable : p => p.IsAvailable,
            p => p.CreatedAt
        );

        var result = new PaginatedResult<ProductDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = (int)totalCount,
            PageNumber = page,
            PageSize = pageSize
        };

        return ApiResponse<PaginatedResult<ProductDto>>.SuccessResponse(result);
    }

    public async Task<ApiResponse<ProductDto>> UpdateProductAsync(string id, UpdateProductDto dto)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product == null)
            return ApiResponse<ProductDto>.ErrorResponse("Product not found");

        if (dto.Name != null) product.Name = dto.Name;
        if (dto.Description != null) product.Description = dto.Description;
        if (dto.Price.HasValue) product.Price = dto.Price.Value;
        if (dto.Category != null) product.Category = dto.Category;
        if (dto.ImageUrl != null) product.ImageUrl = dto.ImageUrl;
        if (dto.StockQuantity.HasValue) product.StockQuantity = dto.StockQuantity.Value;
        if (dto.IsAvailable.HasValue) product.IsAvailable = dto.IsAvailable.Value;
        if (dto.Tags != null) product.Tags = dto.Tags;

        await _repository.UpdateAsync(id, product);
        return ApiResponse<ProductDto>.SuccessResponse(MapToDto(product), "Product updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteProductAsync(string id)
    {
        var deleted = await _repository.DeleteAsync(id);
        if (!deleted)
            return ApiResponse<bool>.ErrorResponse("Product not found");

        return ApiResponse<bool>.SuccessResponse(true, "Product deleted successfully");
    }

    public async Task<ApiResponse<IEnumerable<ProductDto>>> SearchProductsAsync(string searchTerm)
    {
        var products = await _repository.SearchAsync(searchTerm);
        var dtos = products.Select(MapToDto);
        return ApiResponse<IEnumerable<ProductDto>>.SuccessResponse(dtos);
    }

    /// <summary>
    /// Updates product stock quantity
    /// 
    /// This method demonstrates Event-Driven Architecture:
    /// 1. Updates the stock in the database
    /// 2. Publishes an event to RabbitMQ
    /// 3. Other microservices can subscribe to this event (Order Service, Notification Service, etc.)
    /// 
    /// This decouples services - they don't need direct HTTP calls to each other
    /// Benefits:
    /// - Services remain independent and loosely coupled
    /// - Asynchronous processing improves performance
    /// - Easy to add new subscribers without changing this code
    /// - Resilient to temporary service outages
    /// </summary>
    public async Task<ApiResponse<bool>> UpdateStockAsync(string productId, int quantity)
    {
        // Retrieve the current product state
        var product = await _repository.GetByIdAsync(productId);
        if (product == null)
            return ApiResponse<bool>.ErrorResponse("Product not found");

        // Update stock quantity in database
        var updated = await _repository.UpdateStockAsync(productId, quantity);
        if (!updated)
            return ApiResponse<bool>.ErrorResponse("Failed to update stock");

        // Publish domain event to message bus
        // This notifies other microservices about the stock change
        // Example subscribers: Order Service (to check availability), Analytics Service, etc.
        var @event = new ProductStockUpdatedEvent
        {
            ProductId = productId,
            NewStockQuantity = product.StockQuantity + quantity,
            QuantityChanged = quantity
        };
        _eventBus.Publish(@event);

        return ApiResponse<bool>.SuccessResponse(true, "Stock updated successfully");
    }

    /// <summary>
    /// Maps domain model to Data Transfer Object (DTO)
    /// 
    /// Why use DTOs?
    /// - Decouples internal domain models from API contracts
    /// - Prevents over-posting vulnerabilities (exposing sensitive fields)
    /// - Allows different representations for different API versions
    /// - Can aggregate data from multiple sources
    /// - Reduces payload size by excluding unnecessary fields
    /// 
    /// Note: In production, consider using AutoMapper for complex mappings
    /// </summary>
    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Category = product.Category,
            ImageUrl = product.ImageUrl,
            StockQuantity = product.StockQuantity,
            IsAvailable = product.IsAvailable,
            Tags = product.Tags,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}
