using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using PaymentSimulation.Api.Application.Interfaces;
using PaymentSimulation.Api.Application.Payments;
using PaymentSimulation.Api.Infra.Persistence;
using PaymentSimulation.Api.Infra.Queue;
using PaymentSimulation.Api.Middleware;
using PaymentSimulation.Api.Workers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder
    .Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            //return simple validation error response
            return new BadRequestObjectResult(
                new
                {
                    error = "Invalid request",
                    details = context
                        .ModelState.Where(x => x.Value?.Errors.Count > 0)
                        .ToDictionary(x => x.Key, x => x.Value?.Errors.Select(e => e.ErrorMessage)),
                }
            );
        };
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddRouting(options => options.LowercaseUrls = true);

// Dependency injection
builder.Services.AddSingleton<IPaymentRepository, InMemoryPaymentRepository>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddSingleton<InMemoryQueue>();
builder.Services.AddHostedService<PaymentProcessWorker>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<IdempotencyMiddleware>();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
