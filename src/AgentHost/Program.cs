using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Resources;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ---------------- LOGGING ----------------
builder.Host.UseSerilog((ctx, lc) =>
{
    lc.WriteTo.Console()
      .Enrich.FromLogContext()
      .Enrich.WithProperty("App", "AI-Research-Intelligence");
});

// ---------------- HEALTH ----------------
builder.Services.AddHealthChecks();

// ---------------- TELEMETRY ----------------
builder.Services.AddOpenTelemetry()
    .WithTracing(t =>
    {
        t.SetResourceBuilder(ResourceBuilder.CreateDefault()
            .AddService("AI-Research-Intelligence"));
        t.AddAspNetCoreInstrumentation();
        t.AddHttpClientInstrumentation();
    });

// ---------------- AZURE OPENAI ----------------
builder.Services.AddSingleton(sp =>
{
    var endpoint = builder.Configuration["AzureOpenAI:Endpoint"];
    var key = builder.Configuration["AzureOpenAI:ApiKey"];

    return new OpenAIClient(new Uri(endpoint!), new AzureKeyCredential(key!));
});

var app = builder.Build();

app.MapHealthChecks("/health");

app.MapGet("/", () => Results.Ok(new
{
    service = "AI Research Intelligence",
    status = "running"
}));

app.Run();