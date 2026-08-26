using System.Net.Http.Json;
using api.Models;

namespace it.Steps;

[Binding]
public class WeatherForecastStepDefinitions(HttpClient client)
{
    private readonly HttpClient _client = client;
    private HttpResponseMessage? _response;
    private IEnumerable<WeatherForecast>? _forecastData;

    [When("I request the weather forecast from {string}")]
    public async Task WhenIRequestTheWeatherForecastFrom(string endpoint)
    {
        // Executes the GET request against the TestServer
        _response = await _client.GetAsync(endpoint);
    }

    [Then("the API should return a {int} OK status")]
    public void ThenTheApiShouldReturnAOkStatus(int expectedStatusCode)
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_response, Is.Not.Null);
            Assert.That((int)_response!.StatusCode, Is.EqualTo(expectedStatusCode));
        }

    }

    [Then("the response should contain exactly {int} daily forecasts")]
    public async Task ThenTheResponseShouldContainExactlyDailyForecasts(int expectedCount)
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(_response, Is.Not.Null);

            // Deserialize the response body into our expected model
            _forecastData = await _response!.Content.ReadFromJsonAsync<IEnumerable<WeatherForecast>>();

            Assert.That(_forecastData, Is.Not.Null);
        }
        // Validates the logic in the controller that explicitly generates 5 items
        Assert.That(_forecastData!.Count(), Is.EqualTo(expectedCount));
    }
}
