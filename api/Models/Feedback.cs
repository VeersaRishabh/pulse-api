using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace api.Models;

public class Feedback
{
    [BsonId] // MongoDB ID field
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public string RaterId { get; set; }
    public string RateeId { get; set; }
    public string FeedbackText { get; set; }
    public double Rating { get; set; }
    public string Timestamp { get; set; } = DateTime.UtcNow.ToString("o");
}

