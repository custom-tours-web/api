using api.Controllers;

namespace ut;

[TestFixture]
public class WeatherForecastControllerTests
{
    private WeatherForecastController _controller;
    private readonly string[] _expectedSummaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    [SetUp]
    public void Setup()
    {
        // Initialize the controller before each test
        _controller = new WeatherForecastController();
    }

    [Test]
    public void Get_ReturnsNotNull()
    {
        // Act
        var result = _controller.Get();

        // Assert
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void Get_ReturnsExactlyFiveItems()
    {
        // Act
        var result = _controller.Get();

        // Assert
        Assert.That(result.Count(), Is.EqualTo(5));
    }

    [Test]
    public void Get_PropertiesAreWithinExpectedRanges()
    {
        // Act
        var result = _controller.Get().ToList();

        // Assert
        for (int i = 0; i < result.Count; i++)
        {
            var forecast = result[i];

            using (Assert.EnterMultipleScope())
            {
                // Check that temperatures are within the -20 to 55 range
                Assert.That(forecast.TemperatureC, Is.GreaterThanOrEqualTo(-20).And.LessThanOrEqualTo(55));

                // Check that the summary is one of the predefined strings
                Assert.That(_expectedSummaries, Does.Contain(forecast.Summary));
            }


            // Check that dates are sequential starting from tomorrow
            var expectedDate = DateOnly.FromDateTime(DateTime.Now.AddDays(i + 1));
            Assert.That(forecast.Date, Is.EqualTo(expectedDate));
        }
    }
}
