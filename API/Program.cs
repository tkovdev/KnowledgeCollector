using KnowledgeCollector.Logic.Services;
using KnowledgeCollector.Logic.Services.Interfaces;
using KnowledgeCollector.Logic.Commands;
using KnowledgeCollector.Logic.Tools;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddScoped<ITokenService, AzureTokenService>();
builder.Services.AddScoped(sp => new Microsoft.Graph.GraphServiceClient(sp.GetRequiredService<ITokenService>().GetTokenCredential()));
builder.Services.AddScoped<HttpClient>();
builder.Services.AddScoped<ITool, FileReader>();
builder.Services.AddScoped<IAIChatService, OpenAIChatService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
