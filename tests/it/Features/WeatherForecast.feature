Feature: Weather Forecast API
  As an API consumer
  I want to retrieve the weather forecast
  So that I can see the upcoming temperatures

  Scenario: Retrieve the default weather forecast
    When I request the weather forecast from "/WeatherForecast"
    Then the API should return a 200 OK status
    And the response should contain exactly 5 daily forecasts
