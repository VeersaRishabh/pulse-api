using System;
using api.ApplicationDBContext;
using api.Dto.Feedback;
using api.Dto.PendingFeedback;
using api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeedbackController : ControllerBase
    {
        private readonly MongoRepository<Feedback> _feedbackRepository;
        private readonly MongoRepository<PendingFeedback> _pendingFeedbackRepository;

        public FeedbackController(IMongoDatabase database)
        {
            _feedbackRepository = new MongoRepository<Feedback>(database, "Feedback");
            _pendingFeedbackRepository = new MongoRepository<PendingFeedback>(database, "PendingFeedback");
        }

        [HttpPost]
        public async Task<IActionResult> SubmitFeedback([FromBody] FeedbackDto feedback)
        {
            var pendingFeedback = await _pendingFeedbackRepository.GetByIdAsync(feedback.PendingFeedbackId);

            if (pendingFeedback == null)
                return BadRequest(new { Message = "Invalid pending feedback id." });

            if (pendingFeedback.RaterId != feedback.RaterId) 
                return BadRequest(new { message = "Invalid rater id"});

            if (pendingFeedback.RateeId != feedback.RateeId) 
                return BadRequest(new { message = "Invalid ratee id"});

            if(string.IsNullOrWhiteSpace(feedback.FeedbackText))
                return BadRequest(new { message = "feedback text can't be empty"});

            if(feedback.Rating <=0 || feedback.Rating > 5)
                return BadRequest(new { message = "rating must be in range 1-5 only"});

            if (pendingFeedback.Status == PendingFeedback.FeedbackStatus.Completed)
                return BadRequest(new { Message = "Feedback already submitted." });

            var newFeedback = new Feedback
            {
                RaterId = pendingFeedback.RaterId,
                RateeId = pendingFeedback.RateeId,
                FeedbackText = feedback.FeedbackText,
                Rating = feedback.Rating
            };

            await _feedbackRepository.InsertAsync(newFeedback);

            pendingFeedback.Status = PendingFeedback.FeedbackStatus.Completed;
            await _pendingFeedbackRepository.UpdateAsync(pendingFeedback.Id, pendingFeedback);

            return Ok(new { Message = "Feedback submitted successfully!" });
        }

        [HttpGet("{employeeId}")]
        public async Task<IActionResult> GetFeedback(string employeeId)
        {
            var feedbacks = await _feedbackRepository.GetAllAsync();
            var userFeedback = feedbacks.Where(f => f.RateeId == employeeId).ToList();

            if (!userFeedback.Any())
                return NotFound(new { Message = "No feedback found for this user." });

            return Ok(userFeedback);
        }

        [HttpGet]
        [Route("pendingfeedback/{employeeId}")]
        public async Task<IActionResult> GetPendingFeedback(string employeeId)
        {
            var pendingFeedbacks = (await _pendingFeedbackRepository.GetAllAsync())
                .Where(f => f.RaterId == employeeId && f.Status != PendingFeedback.FeedbackStatus.Completed)
                .ToList();

            if (!pendingFeedbacks.Any())
                return NotFound(new { Message = "No pending feedback found for this user." });

            var pendingFeedbacksListDto = pendingFeedbacks.Select(f => new PendingFeedbackDto
            {
                PendingFeedbackId = f.Id,
                RateeId = f.RateeId,
                DueDate = f.DueDate,
                Status = DateTime.UtcNow > f.DueDate
                    ? PendingFeedback.FeedbackStatus.Overdue.ToString()
                    : PendingFeedback.FeedbackStatus.Pending.ToString(),
            });

            return Ok(new
            {
                totalPendingFeedbacks = pendingFeedbacksListDto.Count(),
                pendingFeedbacks = pendingFeedbacksListDto,
            });
        }
    }

}
