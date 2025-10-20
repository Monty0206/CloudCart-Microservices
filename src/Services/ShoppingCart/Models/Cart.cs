using Common.Models;
using MongoDB.Bson.Serialization.Attributes;

namespace ShoppingCart.Models;

public class Cart : BaseEntity
{
    [BsonElement("userId")]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("items")]
    public List<CartItem> Items { get; set; } = new();

    [BsonElement("totalAmount")]
    public decimal TotalAmount { get; set; }

    public void CalculateTotal()
    {
        TotalAmount = Items.Sum(i => i.Price * i.Quantity);
    }
}

public class CartItem
{
    [BsonElement("productId")]
    public string ProductId { get; set; } = string.Empty;

    [BsonElement("productName")]
    public string ProductName { get; set; } = string.Empty;

    [BsonElement("quantity")]
    public int Quantity { get; set; }

    [BsonElement("price")]
    public decimal Price { get; set; }

    [BsonElement("imageUrl")]
    public string ImageUrl { get; set; } = string.Empty;
}
