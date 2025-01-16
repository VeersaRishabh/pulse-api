using System;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeedbackController : ControllerBase
    {
        private static List<Feedback> FeedbackList = new List<Feedback>();
        private static List<PendingFeedback> PendingFeedbackList = new List<PendingFeedback>
        {
            new PendingFeedback { RaterId = "user1", RateeId = "user2", DueDate = DateTime.UtcNow.AddDays(5), CreatedAt = DateTime.UtcNow },
            new PendingFeedback { RaterId = "user3", RateeId = "user4", DueDate = DateTime.UtcNow.AddDays(2), CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new PendingFeedback { RaterId = "user5", RateeId = "user6", DueDate = DateTime.UtcNow.AddDays(-3), CreatedAt = DateTime.UtcNow.AddDays(-7) },
            new PendingFeedback { RaterId = "user7", RateeId = "user8", DueDate = DateTime.UtcNow.AddDays(7), CreatedAt = DateTime.UtcNow },
            new PendingFeedback { RaterId = "user9", RateeId = "user10", DueDate = DateTime.UtcNow.AddDays(1), CreatedAt = DateTime.UtcNow },
            new PendingFeedback { RaterId = "user11", RateeId = "user12", DueDate = DateTime.UtcNow.AddDays(-1), CreatedAt = DateTime.UtcNow.AddDays(-2) },
            new PendingFeedback { RaterId = "user13", RateeId = "user14", DueDate = DateTime.UtcNow.AddDays(10), CreatedAt = DateTime.UtcNow },
            new PendingFeedback { RaterId = "user15", RateeId = "user16", DueDate = DateTime.UtcNow.AddDays(-5), CreatedAt = DateTime.UtcNow.AddDays(-10) }
        };

        [HttpPost]
        public IActionResult SubmitFeedback([FromBody] FeedbackDto feedback)
        {
            //check if pendingFeedbackId is valid or not 
            if(string.IsNullOrWhiteSpace(feedback.PendingFeedbackId))
                return BadRequest(new { Message = "pending feedback id cannot be empty" });

            var pendingFeedback = PendingFeedbackList.FirstOrDefault(f => f.PendingFeedbackId == feedback.PendingFeedbackId);
            if (pendingFeedback == null)
                return BadRequest(new { Message = "Invalid pending feedback id." });
            
            if(pendingFeedback.Status == PendingFeedback.FeedbackStatus.Completed)
                return BadRequest(new { Message = "Feedback already submitted." });

            //check if rater and ratee both valid or not
            if(string.IsNullOrWhiteSpace(feedback.RaterId) || pendingFeedback.RaterId != feedback.RaterId)
                return BadRequest(new { Message = "Invalid rater id." });
            
            if(string.IsNullOrWhiteSpace(feedback.RateeId) || pendingFeedback.RateeId != feedback.RateeId)
                return BadRequest(new { Message = "Invalid ratee id." });

            if (string.IsNullOrWhiteSpace(feedback.FeedbackText))
                return BadRequest(new { Message = "FeedbackText cannot be empty." });

            if (feedback.Rating <= 0 || feedback.Rating > 5)
                return BadRequest(new { Message = "Rating must be in the range 1-5." });

            FeedbackList.Add(new Feedback
            {
                FeedbackId = Guid.NewGuid().ToString(),
                RaterId = pendingFeedback.RaterId,
                RateeId = pendingFeedback.RateeId,
                FeedbackText = feedback.FeedbackText,
                Rating = feedback.Rating,
                Timestamp = DateTime.UtcNow.ToString("o")
            });

            //update the status of pending to completed 
            pendingFeedback.Status = PendingFeedback.FeedbackStatus.Completed;

            return Ok(new { Message = "Feedback submitted successfully!" });
        }

        [HttpGet("{employeeId}")]
        public IActionResult GetFeedback([FromRoute] string employeeId)
        {
            var userFeedback = FeedbackList.Where(f => f.RateeId == employeeId).ToList();

            if (!userFeedback.Any())
                return NotFound(new { Message = "No feedback found for this user." });

            return Ok(userFeedback);
        }

        [HttpGet]
        [Route("pendingfeedback/{employeeId}")]
        public IActionResult GetPendingFeedback([FromRoute] string employeeId)
        {
            var pendingFeedbacks = PendingFeedbackList.Where(f => f.RaterId == employeeId).ToList();

            if (!pendingFeedbacks.Any())
                return NotFound(new { Message = "No pending feedback found for this user/employee." });

            var pendingFeedbacksListDto = pendingFeedbacks.Select(f => new PendingFeedbackDto
            {
                PendingFeedbackId = f.PendingFeedbackId,
                RateeId = f.RateeId,
                DueDate = f.DueDate,
                Status = f.Status.ToString()
            });

            return Ok(pendingFeedbacksListDto);
        }
    }

    public class Feedback
    {
        public string FeedbackId { get; set; } = Guid.NewGuid().ToString();
        public string RaterId { get; set; }
        public string RateeId { get; set; }
        public string FeedbackText { get; set; }
        public double Rating { get; set; }
        public string Timestamp { get; set; } = DateTime.UtcNow.ToString("o");
    }

    public class FeedbackDto
    {
        public string PendingFeedbackId { get; set; }
        public string RaterId { get; set; }
        public string RateeId { get; set; }
        public string FeedbackText { get; set; }
        public double Rating { get; set; }
    }

    public class PendingFeedback
    {
        public string PendingFeedbackId { get; set; } = Guid.NewGuid().ToString();
        public string RaterId { get; set; }
        public string RateeId { get; set; }
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

    public class PendingFeedbackDto
    {
        public string PendingFeedbackId { get; set; }
        public string RateeId { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; }
    }
}
