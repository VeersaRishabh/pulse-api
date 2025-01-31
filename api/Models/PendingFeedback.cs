using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace api.Models;

public class PendingFeedback
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    public string RaterId { get; set; }
    public string RateeId { get; set; }
    public string RaterName { get; set; }
    public string RateeName { get; set; }
    public DateTime MeetingDate { get; set; }
    public DateTime DueDate { get; set; }
    public FeedbackStatus Status { get; set; } = FeedbackStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public enum FeedbackStatus
    {
        Completed,
        Pending,
        Overdue
    }
}