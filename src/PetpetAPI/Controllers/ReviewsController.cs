using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetpetAPI.DTOs;
using PetpetAPI.Services;
using System.Security.Claims;

namespace PetpetAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    // POST: api/reviews
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ReviewDto>> CreateReview([FromBody] CreateReviewDto dto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not found");

            var review = await _reviewService.CreateReviewAsync(userId, dto);
            return Ok(review);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // PUT: api/reviews/{id}
    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<ReviewDto>> UpdateReview(int id, [FromBody] UpdateReviewDto dto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not found");

            var review = await _reviewService.UpdateReviewAsync(userId, id, dto);
            return Ok(review);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // DELETE: api/reviews/{id}
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult> DeleteReview(int id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not found");

            var result = await _reviewService.DeleteReviewAsync(userId, id);
            if (!result)
                return NotFound("Review not found or you don't have permission to delete it");

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // GET: api/reviews/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<ReviewDto>> GetReview(int id)
    {
        try
        {
            var review = await _reviewService.GetReviewAsync(id);
            if (review == null)
                return NotFound("Review not found");

            return Ok(review);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // GET: api/reviews/product/{productId}
    [HttpGet("product/{productId}")]
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetProductReviews(int productId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var reviews = await _reviewService.GetProductReviewsAsync(productId, userId);
            return Ok(reviews);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // GET: api/reviews/product/{productId}/summary
    [HttpGet("product/{productId}/summary")]
    public async Task<ActionResult<ProductReviewSummaryDto>> GetProductReviewSummary(int productId)
    {
        try
        {
            var summary = await _reviewService.GetProductReviewSummaryAsync(productId);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // GET: api/reviews/user/{userId}
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetUserReviews(string userId)
    {
        try
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var reviews = await _reviewService.GetUserReviewsAsync(userId, currentUserId);
            return Ok(reviews);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // GET: api/reviews/my
    [HttpGet("my")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetMyReviews()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not found");

            var reviews = await _reviewService.GetUserReviewsAsync(userId, userId);
            return Ok(reviews);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // POST: api/reviews/helpfulness
    [HttpPost("helpfulness")]
    [Authorize]
    public async Task<ActionResult> VoteReviewHelpfulness([FromBody] ReviewHelpfulnessDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not found");

            var result = await _reviewService.VoteReviewHelpfulnessAsync(userId, dto);
            if (!result)
                return BadRequest("Unable to vote on this review. You may not vote on your own review or the review may not exist.");

            return Ok(new { message = "Vote recorded successfully", reviewId = dto.ReviewId, isHelpful = dto.IsHelpful });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    // GET: api/reviews/can-review
    [HttpGet("can-review")]
    [Authorize]
    public async Task<ActionResult<bool>> CanReviewProduct([FromQuery] int productId, [FromQuery] int orderId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not found");

            var canReview = await _reviewService.CanUserReviewProductAsync(userId, productId, orderId);
            return Ok(canReview);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
