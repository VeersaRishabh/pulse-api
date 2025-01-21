namespace api.Dto.Feedback;

public class FeedbackDto
    {
        public string PendingFeedbackId { get; set; }
        public string RaterId { get; set; }
        public string RateeId { get; set; }
        public string FeedbackText { get; set; }
        public double Rating { get; set; }
    }
