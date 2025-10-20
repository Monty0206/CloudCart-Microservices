using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Services;
using ProductCatalog.DTOs;

namespace ProductCatalog.Controllers;

/// <summary>
/// Products API Controller - Presentation Layer
/// 
/// This controller handles HTTP requests for product operations.
/// RESTful API Design:
/// - GET: Retrieve resources
/// - POST: Create new resources
/// - PUT: Update entire resources
/// - PATCH: Partial updates
/// - DELETE: Remove resources
/// 
/// Best Practices Demonstrated:
/// - Async/await for scalability
/// - Proper HTTP status codes (200, 201, 400, 404)
/// - Dependency injection for loose coupling
/// - Route-based API versioning ready
/// - Input validation through DTOs
/// </summary>
[ApiController]
[Route("api/[controller]")] // Routes to: /api/products
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    /// <summary>
    /// Constructor with Dependency Injection
    /// The service is injected by ASP.NET Core's DI container
    /// This makes the controller easy to unit test with mock services
    /// </summary>
    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// GET /api/products
    /// Retrieves paginated list of products with optional filtering
    /// 
    /// Query Parameters:
    /// - page: Page number (default: 1)
    /// - pageSize: Items per page (default: 10)
    /// - category: Filter by category (optional)
    /// 
    /// Returns: 200 OK with paginated product list
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? category = null)
    {
        var result = await _productService.GetProductsAsync(page, pageSize, category);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// GET /api/products/{id}
    /// Retrieves a specific product by its ID
    /// 
    /// Returns:
    /// - 200 OK: Product found
    /// - 404 Not Found: Product doesn't exist
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(string id)
    {
        var result = await _productService.GetProductByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// GET /api/products/search?query=laptop
    /// Searches products by name, description, or tags
    /// 
    /// This demonstrates full-text search capabilities
    /// Uses MongoDB text indexes for efficient searching
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> SearchProducts([FromQuery] string query)
    {
        var result = await _productService.SearchProductsAsync(query);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// POST /api/products
    /// Creates a new product in the catalog
    /// 
    /// Returns:
    /// - 201 Created: Product created successfully with Location header
    /// - 400 Bad Request: Invalid input data
    /// 
    /// The CreatedAtAction returns a Location header pointing to the new resource
    /// This follows REST best practices for POST operations
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
    {
        var result = await _productService.CreateProductAsync(dto);
        return result.Success ? CreatedAtAction(nameof(GetProduct), new { id = result.Data!.Id }, result) : BadRequest(result);
    }

    /// <summary>
    /// PUT /api/products/{id}
    /// Updates an existing product
    /// 
    /// PUT vs PATCH:
    /// - PUT: Replaces the entire resource (or partial with nullable properties)
    /// - PATCH: Updates only specified fields
    /// 
    /// Returns:
    /// - 200 OK: Product updated successfully
    /// - 404 Not Found: Product doesn't exist
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(string id, [FromBody] UpdateProductDto dto)
    {
        var result = await _productService.UpdateProductAsync(id, dto);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// PATCH /api/products/{id}/stock
    /// Updates product stock quantity
    /// 
    /// Important: This publishes a domain event to RabbitMQ
    /// Other services (Order, Inventory, Notifications) can react to stock changes
    /// This demonstrates Event-Driven Architecture in microservices
    /// 
    /// Example: quantity = -5 (decreases stock by 5)
    ///          quantity = 10 (increases stock by 10)
    /// </summary>
    [HttpPatch("{id}/stock")]
    public async Task<IActionResult> UpdateStock(string id, [FromBody] int quantity)
    {
        var result = await _productService.UpdateStockAsync(id, quantity);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// DELETE /api/products/{id}
    /// Deletes a product from the catalog
    /// 
    /// Returns:
    /// - 200 OK: Product deleted successfully
    /// - 404 Not Found: Product doesn't exist
    /// 
    /// Note: Consider implementing soft delete in production
    /// (marking as inactive instead of physical deletion)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(string id)
    {
        var result = await _productService.DeleteProductAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
