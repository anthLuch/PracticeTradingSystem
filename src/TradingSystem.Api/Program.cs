using Microsoft.Extensions.Options;
using TradingSystem.Api;
using TradingSystem.Api.Controllers;
using TradingSystem.Core.Interfaces;
using TradingSystem.Core.Services;
using TradingSystem.Data.Interfaces;
using TradingSystem.Data.Repositories;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddOpenApi();

builder.Services.AddSingleton<IMatchingEngineServiceV2>(sp =>
{
    var options = sp.GetRequiredService<IOptions<TradingOptions>>();
    return new MatchingEngineServiceV2(options.Value.PriceIncrement);
});
builder.Services.AddSingleton<IOrderBookService>(sp =>
{
    var options = sp.GetRequiredService<IOptions<TradingOptions>>();
    var matchingEngine = sp.GetRequiredService<IMatchingEngineServiceV2>();
    return new OrderBookServiceV2(matchingEngine, options.Value.PriceIncrement);
});

builder.Services.AddSingleton<OrderBookProcessing>();
builder.Services.AddSingleton<PositionTracker>();

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ITradeRespository, TradeRepository>();
builder.Services.Configure<TradingOptions>(builder.Configuration.GetSection("Trading"));

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

