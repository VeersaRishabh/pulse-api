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
        public async Task<IActionResult> SubmitFeedback([FromBody] SubmitFeedbackDto feedback)
        {
            var pendingFeedback = await _pendingFeedbackRepository.GetByIdAsync(feedback.PendingFeedbackId);

            if (pendingFeedback == null)
                return BadRequest(new { Message = "Invalid pending feedback id." });

            if (pendingFeedback.RaterId != feedback.RaterId.ToLower()) 
                return BadRequest(new { message = "Invalid rater id"});

            if (pendingFeedback.RateeId != feedback.RateeId.ToLower()) 
                return BadRequest(new { message = "Invalid ratee id"});

            if(pendingFeedback.RaterName.ToLower() != feedback.RaterName.ToLower())
                return BadRequest( new { message = "Invalid rater name"});
            
            if(pendingFeedback.RateeName.ToLower() != feedback.RateeName.ToLower())
                return BadRequest( new { message = "Invalid ratee name"});

            if(string.IsNullOrWhiteSpace(feedback.FeedbackText))
                return BadRequest(new { message = "feedback text can't be empty"});

            if(feedback.Rating <=0 || feedback.Rating > 4)
                return BadRequest(new { message = "rating must be in range 1-4 only"});

            if (pendingFeedback.Status == PendingFeedback.FeedbackStatus.Completed)
                return BadRequest(new { Message = "Feedback already submitted." });

            var newFeedback = new Feedback
            {
                RaterId = pendingFeedback.RaterId,
                RateeId = pendingFeedback.RateeId,
                RaterName = pendingFeedback.RaterName,
                RateeName = pendingFeedback.RateeName,
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
            employeeId = employeeId.ToLower();
            var feedbacks = await _feedbackRepository.GetAllAsync();
            var userFeedback = feedbacks.Where(f => f.RateeId == employeeId).ToList();

            if (!userFeedback.Any())
                return NotFound(new { Message = "No feedback found for this user." });
            
            var userFeedbackListDto = userFeedback.Select(f => new GetFeedbackDto {
                FeedbackId = f.Id,
                FeedbackText = f.FeedbackText,
                Rating = f.Rating,
                Timestamp = f.Timestamp,
            });

            return Ok(userFeedbackListDto);
        }

        [HttpGet]
        [Route("pendingfeedback/{employeeId}")]
        public async Task<IActionResult> GetPendingFeedback(string employeeId)
        {
            employeeId = employeeId.ToLower();
            var pendingFeedbacks = (await _pendingFeedbackRepository.GetAllAsync())
                .Where(f => f.RaterId == employeeId && f.Status != PendingFeedback.FeedbackStatus.Completed && f.MeetingDate <= DateTime.Now)
                .OrderBy(f => f.DueDate)
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
                MeetingDate = f.MeetingDate,
                RateeName = f.RateeName,
            });

            return Ok(new
            {
                totalPendingFeedbacks = pendingFeedbacksListDto.Count(),
                pendingFeedbacks = pendingFeedbacksListDto,
            });
        }
    }

}
