var app = WebApplication.CreateBuilder(args).Build();

app.MapPost("/fake-provider/pay", async () =>
{
    var rnd = Random.Shared;
    await Task.Delay(rnd.Next(200, 1500));
    var ok = rnd.NextDouble() >= 0.35;
    return Results.Ok(ok
        ? new { success = true, providerRef = Guid.NewGuid().ToString("N") }
        : new { success = false, providerRef = (string?)null, error = "Simulated failure" });
});

app.Run();
