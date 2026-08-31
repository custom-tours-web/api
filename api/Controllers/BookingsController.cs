using api.DTOs;
using api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;

[ApiController]
[Route("api/v1/booking-requests")]
public class BookingRequestsController(IBookingRequestService service) : ControllerBase
{
    private readonly IBookingRequestService _service = service;

    [HttpPost]
    public async Task<IActionResult> CreateBookingRequest([FromBody] BookingRequestDTO dto)
    {
        var response = await _service.CreateBookingRequestAsync(dto);
        return Created($"/api/v1/booking-requests/{response.Id}", response);
    }
}
