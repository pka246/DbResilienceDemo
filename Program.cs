using Microsoft.Data.SqlClient;
using Polly;
using Polly.Registry;
using Polly.Retry;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5081");

builder.Services.AddSingleton<ChaosSwitch>();
builder.Services.AddScoped<OrderRepository>();

builder.Services.AddResiliencePipeline("db-retry", pipeline =>
{
    pipeline.AddRetry(new RetryStrategyOptions
    {
        ShouldHandle = new PredicateBuilder()
          .Handle<SqlException>(ex => ex.IsTransient)
          .Handle<TimeoutException>(),
        MaxRetryAttempts = 3,
        Delay = TimeSpan.FromMilliseconds(200),
        BackoffType = DelayBackoffType.Exponential,
        UseJitter = true,
        OnRetry = args =>
        {
            Console.WriteLine($"[db-retry] Attempt {args.AttemptNumber + 1} failed: {args.Outcome.Exception?.Message}");
            return default;
        }
    })
    .AddTimeout(TimeSpan.FromSeconds(3));
});

var app = builder.Build();


// Our API: reads orders from SQL Server.  Try: GET http://localhost:5081/api/orders
app.MapGet("/api/orders", async (OrderRepository repo, CancellationToken ct) =>
    Results.Ok(await repo.GetOrdersAsync(ct)));

// ---------- Chaos controls for the demo ----------
app.MapGet("/chaos", (ChaosSwitch c) => Results.Ok(c));
app.MapPost("/chaos/db/{percent:int}", (int percent, ChaosSwitch c) =>
{
    c.DbFailurePercent = Math.Clamp(percent, 0, 100);
    return Results.Ok(c);
});

app.Run();
