using BusTicketReservationSystem.Application.Contracts.Interfaces;
using BusTicketReservationSystem.Application.Services;
using BusTicketReservationSystem.Infrastructure.Data;
using BusTicketReservationSystem.Infrastructure.Repositories;
using BusTicketReservationSystem.Infrastructure.Unit_of_Work;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        if (context.ProblemDetails.Status == StatusCodes.Status500InternalServerError)
        {
            context.ProblemDetails.Title = "An unexpected server error occurred.";
            context.ProblemDetails.Detail = null;
        }
    };
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "The PostgreSQL connection string is not configured. Use User Secrets or the ConnectionStrings__DefaultConnection environment variable.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IBusScheduleRepository, BusScheduleRepository>();
builder.Services.AddScoped<ISeatRepository, SeatRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<IPassengerRepository, PassengerRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<SearchService>();
builder.Services.AddScoped<BookingService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Bus Ticket Reservation System API",
        Version = "v1",
        Description = "Reservation, seat availability, booking, purchase, and cancellation API. Developed by Muhammad Ali Nawaz.",
        Contact = new OpenApiContact { Name = "Muhammad Ali Nawaz" }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy => policy
        .SetIsOriginAllowed(origin => Uri.TryCreate(origin, UriKind.Absolute, out var uri) && uri.Host == "localhost")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAngular");
app.MapControllers();

app.Run();
