namespace api.Dto.Feedback;

public class SubmitFeedbackDto
    {
        public string PendingFeedbackId { get; set; }
        public string RaterId { get; set; }
        public string RateeId { get; set; }
        public string RaterName { get; set; }
        public string RateeName { get; set; }
        public string FeedbackText { get; set; }
        public double Rating { get; set; }
    }
