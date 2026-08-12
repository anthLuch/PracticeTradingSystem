using TradingSystem.Core.Interfaces;
using TradingSystem.Core.Services;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSingleton<IMatchingEngineServiceV2, MatchingEngineServiceV2>();
builder.Services.AddSingleton<IOrderBookService, OrderBookServiceV2>();
builder.Services.AddSingleton<PositionTracker>();

builder.Services.AddControllers();


var app = builder.Build();

app.MapControllers();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();

