using api.Interfaces;
using api.Repositories;
using api.Services;
using Scalar.AspNetCore;
// using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<IBookingRequestRepository, BookingRequestRepository>();

builder.Services.AddSingleton<IBookingRequestService, BookingRequestService>();

// builder.Services.AddDbContext<>(options => options.UseSqlite(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
