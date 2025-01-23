using MongoDB.Driver;

namespace api.Dto.PendingFeedback;

 public class PendingFeedbackDto
{
    public string PendingFeedbackId { get; set; }
    public string RateeId { get; set; }
    public string RateeName { get; set; }
    public DateTime MeetingDate { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; }
}
