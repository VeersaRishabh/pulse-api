namespace api.Dto.Feedback;

public class GetFeedbackDto
{
    public string FeedbackId { get; set; } 
    public string FeedbackText { get; set; }
    public double Rating { get; set; }
    public string Timestamp { get; set; } 
}
