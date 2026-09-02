using api.Datas;
using api.Models;
using api.Repositories;
using Microsoft.EntityFrameworkCore;
// Make sure to add the correct using statement for your Data namespace here
// e.g., using ut.Data;

namespace ut.Repositories;

/// <summary>
/// Unit tests for the BookingRequestRepository class.
/// </summary>
[TestFixture]
public class BookingRequestRepositoryTests
{

    /// <summary>
    /// DbContextOptions for configuring the in-memory database used in tests. Each test gets a unique database instance to ensure isolation.
    /// </summary>
    private DbContextOptions<TourismDbContext> _dbContextOptions;

    /// <summary>
    /// The in-memory database context used for testing. This allows us to test repository methods without affecting a real database.
    /// </summary>
    private TourismDbContext _dbContext;

    /// <summary>
    /// The BookingRequestRepository instance under test. This is the actual repository implementation that interacts with the in-memory database.
    /// </summary>
    private BookingRequestRepository _repository;

    /// <summary>
    /// This ensures that each test runs in isolation with a fresh database context and repository instance.
    /// </summary>
    [SetUp]
    public void Setup()
    {

        _dbContextOptions = new DbContextOptionsBuilder<TourismDbContext>()
            .UseInMemoryDatabase(databaseName: $"TourismDb_Test_{Guid.NewGuid()}")
            .Options;

        _dbContext = new TourismDbContext(_dbContextOptions);

        _repository = new BookingRequestRepository(_dbContext);
    }

    /// <summary>
    /// It ensures that the in-memory database is deleted and the DbContext is disposed
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    /// <summary>
    /// Test to verify that the AddAsync method of the BookingRequestRepository successfully saves a valid booking request to the in-memory database.
    /// </summary>
    [Test]
    public async Task AddAsync_WithValidRequest_SavesToDatabaseSuccessfully()
    {
        // Arrange
        var request = BookingTestData.GetValidBookingRequestEntity();

        // Act
        await _repository.AddAsync(request);

        // Assert
        var savedRecord = await _dbContext.BookingRequests.FirstOrDefaultAsync(b => b.FullName == request.FullName);

        Assert.That(savedRecord, Is.Not.Null);
        Assert.That(savedRecord.Destination, Is.EqualTo(request.Destination));
    }

    /// <summary>
    /// Test to verify that the AddAsync method of the BookingRequestRepository correctly handles exceptions thrown.
    /// </summary>
    [Test]
    public void AddAsync_WhenDatabaseContextThrowsException_LogsAndRethrows()
    {
        // Arrange
        BookingRequest nullRequest = null!;

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _repository.AddAsync(nullRequest));

        Assert.That(ex.ParamName, Is.EqualTo("entity"));
    }
}
