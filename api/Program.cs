using api.Datas;
using api.DTOs;
using api.Interfaces;
using api.Models;
using api.Repositories;
using api.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

#region Service Configuration (DI Container)

// 1. Database Configuration
// Registers the SQLite database context using the default connection string.
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

#endregion

var app = builder.Build();

#region HTTP Request Pipeline

// Configure the HTTP request pipeline for development environments
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(); // Interactive UI for OpenAPI testing
}

// Standard middleware pipeline execution order
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

#endregion

app.Run();
