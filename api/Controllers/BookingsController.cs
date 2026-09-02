using api.DTOs;
using api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

/// <summary>
/// Handles HTTP requests related to booking request operations.
/// <paramref name="logger"/> is used for logging request processing details and errors.
/// <paramref name="service"/> is used to execute the business logic for handling booking requests.
/// </summary>
[ApiController]
[Route("api/v1/booking-requests")]
public class BookingRequestsController(
    ILogger<BookingRequestsController> logger,
    IBookingRequestService service) : ControllerBase
{
    #region Dependencies

    /// <summary>
    /// The logger instance for logging controller-level operations and errors.
    /// </summary>
    private readonly ILogger<BookingRequestsController> _logger = logger;

    /// <summary>
    /// The service instance for executing business logic related to booking requests.
    /// </summary>
    private readonly IBookingRequestService _service = service;

    #endregion

    #region Endpoints

    /// <summary>
    /// Creates a new booking request.
    /// </summary>
    /// <param name="dto">The booking request data transfer object containing the necessary details.</param>
    /// <returns>An IActionResult containing the created booking request details.</returns>
    /// <response code="201">Returns the newly created booking request.</response>
    /// <response code="400">If the provided data is invalid or missing.</response>
    /// <response code="500">If an unexpected server error occurs during processing.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateBookingRequest([FromBody] BookingRequestDTO dto)
    {
        _logger.LogInformation("Received request to create a new booking request.");

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for booking request creation.");
            return BadRequest(ModelState);
        }

        try
        {
            var response = await _service.CreateBookingRequestAsync(dto);

            _logger.LogInformation("Successfully created booking request with ID: {BookingId}", response?.Id);

            return Created($"/api/v1/booking-requests/{response?.Id}", response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating the booking request.");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred while processing your request. Please try again later.");
        }
    }

    #endregion
}
