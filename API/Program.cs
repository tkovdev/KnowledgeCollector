using KnowledgeCollector.API.TokenService;
using KnowledgeCollector.API.TokenService.Interfaces;
using KnowledgeCollector.Logic.Commands;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.AddScoped<ITokenService, AzureTokenService>();
builder.Services.AddScoped<OpenAIChat>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
