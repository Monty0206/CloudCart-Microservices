using Common.Models;
using MongoDB.Bson.Serialization.Attributes;

namespace ProductCatalog.Models;

public class Product : BaseEntity
{
    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("price")]
    public decimal Price { get; set; }

    [BsonElement("category")]
    public string Category { get; set; } = string.Empty;

    [BsonElement("imageUrl")]
    public string ImageUrl { get; set; } = string.Empty;

    [BsonElement("stockQuantity")]
    public int StockQuantity { get; set; }

    [BsonElement("isAvailable")]
    public bool IsAvailable { get; set; } = true;

    [BsonElement("tags")]
    public List<string> Tags { get; set; } = new();
}
