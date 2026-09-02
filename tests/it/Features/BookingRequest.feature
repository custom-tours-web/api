Feature: Create Booking Request
  As an API consumer
  I want to submit booking requests
  So that the system can process customer tour inquiries

  @database
  Scenario: Successfully creating a valid booking request
    Given I have a valid booking request payload for "Jane Doe" traveling to "Tokyo"
    When I submit the POST request to "/api/v1/booking-requests"
    Then the API should return a 201 Created status code
    And the response body should contain the pending booking status

  @database
  Scenario: Failing validation due to missing required fields
    Given I have a booking request payload missing name
    When I submit the POST request to "/api/v1/booking-requests"
    Then the API should return a 400 Bad Request status code

  @database
  Scenario: Failing validation due to invalid business rules (Same Location)
    Given I have a booking request payload where the current location and destination are both "London"
    When I submit the POST request to "/api/v1/booking-requests"
    Then the API should return a 400 Bad Request status code

  @database
  Scenario: Failing validation due to a past tour date
    Given I have a booking request payload with a tour date in the past
    When I submit the POST request to "/api/v1/booking-requests"
    Then the API should return a 400 Bad Request status code
