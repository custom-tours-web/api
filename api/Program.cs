using api.Datas;
using api.DTOs;
using api.Interfaces;
using api.Models;
using api.Repositories;
using api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Scalar.AspNetCore;
using api.Middlewares;

var builder = WebApplication.CreateBuilder(args);

#region Service Configuration (DI Container)

// Only register the production/development database if we are NOT running integration tests
builder.Services.AddHealthChecks().AddDbContextCheck<TourismDbContext>();
if (!builder.Environment.IsEnvironment("Testing"))
    builder.Services.AddDbContext<TourismDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

// 2. Application Services & Repositories
// Scoped lifecycle ensures a new instance is created per HTTP request.
builder.Services.AddScoped<IBookingRequestRepository, BookingRequestRepository>();
builder.Services.AddScoped<IBookingRequestService, BookingRequestService>();

// 3. Third-Party Libraries (AutoMapper)
// Configures object-to-object mapping profiles for DTOs and Data Models.
builder.Services.AddAutoMapper(cfg =>
{
    cfg.CreateMap<BookingRequestDTO, BookingRequest>();
});

// 4. API & Controller Setup
builder.Services.AddControllers();
builder.Services.AddOpenApi(); // Generates OpenAPI specifications

builder.Services.AddCors(options =>
{
    options.AddPolicy("UiCorsPolicy", policy =>
    {
        policy
            .WithOrigins("http://127.0.0.1:5500")
            .WithMethods("POST")
            .WithHeaders("Content-Type");
    });
});

#endregion

var app = builder.Build();

#region HTTP Request Pipeline

// Configure the HTTP request pipeline for development environments
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // Interactive UI for OpenAPI testing
}
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
// Standard middleware pipeline execution order
app.UseCors("UiCorsPolicy");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapGet("/health", async (HealthCheckService healthChecks) =>
{
    var report = await healthChecks.CheckHealthAsync();
    var statusCode = report.Status == HealthStatus.Healthy
        ? StatusCodes.Status200OK
        : StatusCodes.Status503ServiceUnavailable;

    return Results.Json(
        new { status = report.Status.ToString() },
        statusCode: statusCode);
})
.Produces(StatusCodes.Status200OK)
.Produces(StatusCodes.Status503ServiceUnavailable)
.WithTags("System Health");
app.MapControllers();

#endregion

app.Run();
