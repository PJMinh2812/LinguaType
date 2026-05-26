using LinguaType.Api.Data;
using LinguaType.Api.Models;
using LinguaType.Api.Services;
using Microsoft.EntityFrameworkCore;
// translation feature removed

var builder = WebApplication.CreateBuilder(args);
// translation feature removed; no external translation endpoints

builder.Services.AddCors();
var connectionString = ResolveConnectionString(builder.Configuration);
builder.Services.AddDbContext<LinguaTypeDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<ISampleProvider, DatabaseSampleProvider>();
builder.Services.AddSingleton<IPinyinService, ToneMarkedPinyinService>();
// no external HTTP calls needed for now

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LinguaTypeDbContext>();
    await db.Database.MigrateAsync();
}

app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

app.MapGet("/", () => Results.Text("LinguaType API running"));

app.MapGet("/api/samples", (ISampleProvider sampleProvider) => Results.Ok(sampleProvider.GetAll()));

app.MapGet("/api/samples/random", (ISampleProvider sampleProvider) =>
{
    try
    {
        return Results.Ok(sampleProvider.GetRandom());
    }
    catch (InvalidOperationException)
    {
        return Results.NotFound(new { error = "no samples available" });
    }
});

var adminSamples = app.MapGroup("/api/admin/samples");

adminSamples.MapGet("", async (LinguaTypeDbContext db) =>
    Results.Ok(await db.TypingSamples.AsNoTracking().OrderBy(sample => sample.Id).ToListAsync()));

adminSamples.MapGet("/{id:int}", async (int id, LinguaTypeDbContext db) =>
{
    var sample = await db.TypingSamples.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id);
    return sample is null ? Results.NotFound(new { error = "sample not found" }) : Results.Ok(sample);
});

adminSamples.MapPost("", async (TypingSampleUpsertRequest request, LinguaTypeDbContext db) =>
{
    var text = request.Text?.Trim();
    if (string.IsNullOrWhiteSpace(text))
    {
        return Results.BadRequest(new { error = "text required" });
    }

    var nextId = await db.TypingSamples.Select(sample => (int?)sample.Id).MaxAsync() ?? 0;
    var sample = new TypingSample
    {
        Id = nextId + 1,
        Text = text,
    };

    db.TypingSamples.Add(sample);
    await db.SaveChangesAsync();

    return Results.Created($"/api/admin/samples/{sample.Id}", sample);
});

adminSamples.MapPut("/{id:int}", async (int id, TypingSampleUpsertRequest request, LinguaTypeDbContext db) =>
{
    var text = request.Text?.Trim();
    if (string.IsNullOrWhiteSpace(text))
    {
        return Results.BadRequest(new { error = "text required" });
    }

    var sample = await db.TypingSamples.FirstOrDefaultAsync(item => item.Id == id);
    if (sample is null)
    {
        return Results.NotFound(new { error = "sample not found" });
    }

    sample.Text = text;
    await db.SaveChangesAsync();

    return Results.Ok(sample);
});

adminSamples.MapDelete("/{id:int}", async (int id, LinguaTypeDbContext db) =>
{
    var sample = await db.TypingSamples.FirstOrDefaultAsync(item => item.Id == id);
    if (sample is null)
    {
        return Results.NotFound(new { error = "sample not found" });
    }

    db.TypingSamples.Remove(sample);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapGet("/api/pinyin", (string? text, IPinyinService pinyinService) =>
{
    if (string.IsNullOrWhiteSpace(text))
    {
        return Results.BadRequest(new { error = "text required" });
    }

    var pinyin = pinyinService.ConvertToPinyin(text);
    return Results.Ok(new { text, pinyin });
});

app.MapPost("/api/samples/custom", async (HttpRequest httpRequest, IPinyinService pinyinService) =>
{
    string body = string.Empty;
    try
    {
        using var sr = new System.IO.StreamReader(httpRequest.Body);
        body = await sr.ReadToEndAsync();
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Failed to read request body for custom sample");
    }

    if (string.IsNullOrWhiteSpace(body))
    {
        return Results.BadRequest(new { error = "text required" });
    }

    string? text = null;
    try
    {
        var doc = System.Text.Json.JsonDocument.Parse(body);
        if (doc.RootElement.TryGetProperty("text", out var t))
        {
            text = t.GetString();
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Failed to parse custom sample JSON body");
        return Results.BadRequest(new { error = "invalid json" });
    }

    if (string.IsNullOrWhiteSpace(text))
    {
        return Results.BadRequest(new { error = "text required" });
    }

    text = text.Trim();
    var pinyin = pinyinService.ConvertToPinyin(text);
    return Results.Ok(new { text, pinyin });
});

// translation models removed

app.Run();

static string ResolveConnectionString(ConfigurationManager configuration)
{
    var rawConnectionString = configuration.GetConnectionString("LinguaTypeDb");

    if (string.IsNullOrWhiteSpace(rawConnectionString))
    {
        throw new InvalidOperationException(
            "Missing database connection string. Set ConnectionStrings:LinguaTypeDb in appsettings.json.");
    }

    if (rawConnectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
        || rawConnectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
    {
        var uri = new Uri(rawConnectionString);
        var userInfo = uri.UserInfo.Split(':', 2);
        var database = uri.AbsolutePath.Trim('/');

        var builder = new Npgsql.NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.IsDefaultPort ? 5432 : uri.Port,
            Database = string.IsNullOrWhiteSpace(database) ? "postgres" : database,
            Username = Uri.UnescapeDataString(userInfo[0]),
            Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty,
            SslMode = Npgsql.SslMode.Require,
        };

        return builder.ConnectionString;
    }

    return rawConnectionString;
}
