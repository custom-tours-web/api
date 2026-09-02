using api.Controllers;
using api.DTOs;
using api.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace ut.Controllers;

/// <summary>
/// Unit tests for the BookingRequestsController class.
/// </summary>
[TestFixture]
public class BookingRequestsControllerTests
{

    /// <summary>
    /// Mocked ILogger for the BookingRequestsController to verify logging behavior during tests.
    /// </summary>
    private Mock<ILogger<BookingRequestsController>> _mockLogger;

    /// <summary>
    /// Mocked IBookingRequestService for the BookingRequestsController to verify service interactions during tests.
    /// </summary>
    private Mock<IBookingRequestService> _mockService;

    /// <summary>
    /// This is the actual controller implementation that handles HTTP requests related to booking requests.
    /// </summary>
    private BookingRequestsController _controller;

    /// <summary>
    /// It initializes the mocked dependencies and creates a new instance of the BookingRequestsController for testing.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        // Arrange
        _mockLogger = new Mock<ILogger<BookingRequestsController>>();
        _mockService = new Mock<IBookingRequestService>();

        _controller = new BookingRequestsController(_mockLogger.Object, _mockService.Object);
    }

    /// <summary>
    /// It tests the CreateBookingRequest action of the BookingRequestsController when the model state is invalid.
    /// </summary>
    [Test]
    public async Task CreateBookingRequest_WithInvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var dto = BookingTestData.GetValidBookingRequestDTO();
        _controller.ModelState.AddModelError("FullName", "Required");

        // Act
        var result = await _controller.CreateBookingRequest(dto);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequestResult = (BadRequestObjectResult)result;
        Assert.That(badRequestResult.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
    }

    /// <summary>
    /// It tests the CreateBookingRequest action of the BookingRequestsController when a valid booking request DTO is provided.
    /// </summary>
    /// <returns></returns>
    [Test]
    public async Task CreateBookingRequest_WithValidData_ReturnsCreatedResult()
    {
        // Arrange
        var dto = BookingTestData.GetValidBookingRequestDTO();
        var expectedResponse = BookingTestData.GetValidBookingResponseDTO();

        _mockService
            .Setup(s => s.CreateBookingRequestAsync(It.IsAny<BookingRequestDTO>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.CreateBookingRequest(dto);

        // Assert
        Assert.That(result, Is.InstanceOf<CreatedResult>());
        var createdResult = (CreatedResult)result;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(createdResult.StatusCode, Is.EqualTo(StatusCodes.Status201Created));
            Assert.That(createdResult.Location, Is.EqualTo($"/api/v1/booking-requests/{expectedResponse.Id}"));
            Assert.That(createdResult.Value, Is.EqualTo(expectedResponse));
        }
    }

    /// <summary>
    /// It tests the CreateBookingRequest action of the BookingRequestsController when the service layer throws an exception.
    /// </summary>
    /// <returns></returns>
    [Test]
    public async Task CreateBookingRequest_WhenServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var dto = BookingTestData.GetValidBookingRequestDTO();

        _mockService
            .Setup(s => s.CreateBookingRequestAsync(It.IsAny<BookingRequestDTO>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed."));

        // Act
        var result = await _controller.CreateBookingRequest(dto);

        // Assert
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        var objectResult = (ObjectResult)result;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(objectResult.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
            Assert.That(objectResult.Value, Is.EqualTo("An unexpected error occurred while processing your request. Please try again later."));
        }
    }
}
