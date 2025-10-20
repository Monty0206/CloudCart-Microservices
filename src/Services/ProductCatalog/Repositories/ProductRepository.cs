using ProductCatalog.Models;
using Common.Repositories;
using MongoDB.Driver;

namespace ProductCatalog.Repositories;

public interface IProductRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> GetByCategoryAsync(string category);
    Task<IEnumerable<Product>> SearchAsync(string searchTerm);
    Task<bool> UpdateStockAsync(string productId, int quantity);
}

public class ProductRepository : MongoRepository<Product>, IProductRepository
{
    private readonly IMongoCollection<Product> _products;

    public ProductRepository(IMongoDatabase database) 
        : base(database, "products")
    {
        _products = database.GetCollection<Product>("products");
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(string category)
    {
        return await _products
            .Find(p => p.Category == category && p.IsAvailable)
            .ToListAsync();
    }

    public async Task<IEnumerable<Product>> SearchAsync(string searchTerm)
    {
        var filter = Builders<Product>.Filter.Or(
            Builders<Product>.Filter.Regex(p => p.Name, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
            Builders<Product>.Filter.Regex(p => p.Description, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
            Builders<Product>.Filter.AnyIn(p => p.Tags, new[] { searchTerm })
        );

        return await _products.Find(filter).ToListAsync();
    }

    public async Task<bool> UpdateStockAsync(string productId, int quantity)
    {
        var update = Builders<Product>.Update
            .Inc(p => p.StockQuantity, quantity)
            .Set(p => p.UpdatedAt, DateTime.UtcNow);

        var result = await _products.UpdateOneAsync(
            p => p.Id == productId,
            update
        );

        return result.ModifiedCount > 0;
    }
}
