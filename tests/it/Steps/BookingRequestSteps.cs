using System.Net.Http.Json;
using System.Text.Json;
using api.DTOs;

namespace it.Steps;

/// <summary>
/// This class contains step definitions for SpecFlow tests related to creating booking requests.
/// <paramref name="httpClient"/> is injected to facilitate HTTP requests to the in-memory TestHost API.
/// </summary>
[Binding]
public class CreateBookingRequestSteps(HttpClient httpClient)
{

    /// <summary>
    /// The HttpClient instance used to send HTTP requests to the in-memory TestHost API during SpecFlow tests.
    /// </summary>
    private readonly HttpClient _httpClient = httpClient;

    /// <summary>
    /// The BookingRequestDTO instance that holds the payload for creating a booking request.
    /// </summary>
    private BookingRequestDTO? _requestPayload;

    /// <summary>
    /// The HttpResponseMessage instance that holds the response from the API after submitting the booking request.
    /// </summary>
    private HttpResponseMessage? _response;

    #region Given Steps

    /// <summary>
    /// Sets up a valid booking request payload for a traveler with the specified full name and destination.
    /// </summary>
    /// <param name="fullName">The full name of the traveler.</param>
    /// <param name="destination">The destination of the booking.</param>

    [Given("I have a valid booking request payload for {string} traveling to {string}")]
    public void GivenIHaveAValidBookingRequestPayloadForTravelingTo(string fullName, string destination)
    {
        _requestPayload = new BookingRequestDTO(
            fullName,
            "+91-8428-558-275",
            "New York",
            destination,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            2,
            "Aisle seat preferred"
        );
    }

    /// <summary>
    /// This step is used to test the API's validation behavior when required fields are not provided in the request payload.
    /// </summary>
    /// <param name="field">The name of the missing required field.</param>
    [Given("I have a booking request payload missing name")]
    public void GivenIHaveABookingRequestPayloadMissingThe()
    {
        _requestPayload = new BookingRequestDTO(
            string.Empty,
            "+91-8428-558-275",
            "New York",
            "Coimbatore",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            2,
            null
        );
    }

    /// <summary>
    /// Sets up a booking request payload where the current location and destination are both specified by the {string} parameter.
    /// </summary>
    /// <param name="location">The location of the traveler.</param>
    [Given("I have a booking request payload where the current location and destination are both {string}")]
    public void GivenIHaveABookingRequestPayloadWhereLocationsMatch(string location)
    {
        _requestPayload = new BookingRequestDTO(
            "Prasad",
            "+91-8428-558-275",
            location,
            location,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            2,
            null
        );
    }

    /// <summary>
    /// Sets up a booking request payload with a tour date in the past.
    /// </summary>
    [Given("I have a booking request payload with a tour date in the past")]
    public void GivenIHaveABookingRequestPayloadWithAPastDate()
    {
        _requestPayload = new BookingRequestDTO(
            "Prasad",
            "+91-8428-558-275",
            "Tiruppur",
            "Coimbatore",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5)),
            2,
            null
        );
    }

    #endregion

    #region When Steps

    /// <summary>
    /// Submits the POST request to the specified API endpoint with the prepared booking request payload.
    /// </summary>
    /// <param name="endpoint">The API endpoint to submit the request to.</param>
    [When("I submit the POST request to {string}")]
    public async Task WhenISubmitThePOSTRequestTo(string endpoint)
    {
        ArgumentNullException.ThrowIfNull(_requestPayload);

        _response = await _httpClient.PostAsJsonAsync(endpoint, _requestPayload);
    }

    #endregion

    #region Then Steps

    /// <summary>
    /// The API response returns a Created status code (201) when the booking request is successfully created.
    /// </summary>
    /// <param name="expectedStatusCode">The expected status code.</param>
    [Then("the API should return a {int} Created status code")]
    public void ThenTheAPIShouldReturnACreatedStatusCode(int expectedStatusCode)
    {
        ArgumentNullException.ThrowIfNull(_response);
        Assert.That((int)_response.StatusCode, Is.EqualTo(expectedStatusCode));
    }

    /// <summary>
    /// The API response returns a Bad Request status code (400) when the request payload is invalid.
    /// </summary>
    /// <param name="expectedStatusCode">The expected status code.</param>
    [Then("the API should return a {int} Bad Request status code")]
    public void ThenTheAPIShouldReturnABadRequestStatusCode(int expectedStatusCode)
    {
        ArgumentNullException.ThrowIfNull(_response);
        Assert.That((int)_response.StatusCode, Is.EqualTo(expectedStatusCode));
    }

    /// <summary>
    /// The response body from the API contains a booking status of "Pending" and a valid booking ID greater than 0.
    /// </summary>
    [Then("the response body should contain the pending booking status")]
    public async Task ThenTheResponseBodyShouldContainThePendingBookingStatus()
    {
        ArgumentNullException.ThrowIfNull(_response);

        var responseContent = await _response.Content.ReadFromJsonAsync<BookingResponseDTO>(
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.That(responseContent, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(responseContent!.Status, Is.EqualTo("Pending"));
            Assert.That(responseContent.Id, Is.GreaterThan(0));
        }

    }

    #endregion
}
