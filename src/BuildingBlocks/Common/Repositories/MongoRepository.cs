using MongoDB.Driver;
using System.Linq.Expressions;
using Common.Models;

namespace Common.Repositories;

/// <summary>
/// Generic MongoDB Repository Implementation
/// 
/// This is a reusable, generic repository that can work with any entity type.
/// Design Pattern: Repository Pattern + Generic Programming
/// 
/// Benefits:
/// - Abstracts MongoDB-specific code from business logic
/// - Provides consistent data access interface across all entities
/// - Reduces code duplication (DRY principle)
/// - Makes unit testing easier with mockable interface
/// - Centralizes database operations for easier maintenance
/// 
/// The repository pattern is crucial in microservices for:
/// - Database abstraction (easy to switch from MongoDB to another DB)
/// - Consistent error handling
/// - Transaction management
/// - Caching layer addition without changing business logic
/// </summary>
public class MongoRepository<T> : IRepository<T> where T : BaseEntity
{
    private readonly IMongoCollection<T> _collection;

    /// <summary>
    /// Constructor - Initializes MongoDB collection
    /// </summary>
    /// <param name="database">MongoDB database instance (injected)</param>
    /// <param name="collectionName">Name of the MongoDB collection</param>
    public MongoRepository(IMongoDatabase database, string collectionName)
    {
        _collection = database.GetCollection<T>(collectionName);
    }

    public async Task<T?> GetByIdAsync(string id)
    {
        return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _collection.Find(predicate).ToListAsync();
    }

    public async Task<T> CreateAsync(T entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        await _collection.InsertOneAsync(entity);
        return entity;
    }

    public async Task<bool> UpdateAsync(string id, T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        var result = await _collection.ReplaceOneAsync(x => x.Id == id, entity);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _collection.DeleteOneAsync(x => x.Id == id);
        return result.DeletedCount > 0;
    }

    public async Task<long> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        if (predicate == null)
            return await _collection.CountDocumentsAsync(_ => true);
        
        return await _collection.CountDocumentsAsync(predicate);
    }

    /// <summary>
    /// Retrieves paginated results with optional filtering and sorting
    /// 
    /// Pagination is essential for:
    /// - Performance: Doesn't load all records into memory
    /// - User Experience: Faster page loads
    /// - Scalability: Can handle millions of records
    /// 
    /// This method demonstrates:
    /// - LINQ expression trees for flexible filtering
    /// - MongoDB aggregation pipeline optimization
    /// - Efficient counting with filtered queries
    /// </summary>
    public async Task<(IEnumerable<T> items, long totalCount)> GetPagedAsync(
        int pageNumber, 
        int pageSize, 
        Expression<Func<T, bool>>? filter = null,
        Expression<Func<T, object>>? orderBy = null)
    {
        // Build filter: Use provided filter or match all documents
        var filterDefinition = filter != null 
            ? Builders<T>.Filter.Where(filter) 
            : Builders<T>.Filter.Empty;

        // Get total count for pagination metadata (total pages calculation)
        var totalCount = await _collection.CountDocumentsAsync(filterDefinition);

        // Build query with pagination
        // Skip: Skips records from previous pages
        // Limit: Returns only requested page size
        var query = _collection.Find(filterDefinition)
            .Skip((pageNumber - 1) * pageSize)
            .Limit(pageSize);

        // Apply sorting if specified
        if (orderBy != null)
        {
            query = query.SortBy(orderBy);
        }

        // Execute query and return both items and total count
        var items = await query.ToListAsync();

        return (items, totalCount);
    }
}
