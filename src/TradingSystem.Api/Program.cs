using TradingSystem.Api.Controllers;
using TradingSystem.Core.Interfaces;
using TradingSystem.Core.Services;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSingleton<IMatchingEngineServiceV2, MatchingEngineServiceV2>();
builder.Services.AddSingleton<IOrderBookService, OrderBookServiceV2>();
builder.Services.AddSingleton<OrderBookProcessing>();
builder.Services.AddSingleton<PositionTracker>();

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


var app = builder.Build();

app.UseCors();

var processor = app.Services.GetRequiredService<OrderBookProcessing>();
_ = processor.StartAsync(app.Lifetime.ApplicationStopping);

app.MapControllers();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//app.UseHttpsRedirection();

app.Run();

