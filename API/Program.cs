using KnowledgeCollector.Intelligence.Services;
using KnowledgeCollector.Intelligence.Services.Interfaces;
using KnowledgeCollector.Intelligence.Commands;
using KnowledgeCollector.Intelligence.Tools;
using API.Configurations;
using MongoDB.Driver;
using Mediator;
using Mediator.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

var mongoSettings = builder.Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();
if (mongoSettings is null) throw new Exception("MongoDbSettings is empty or does not exist");

builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));

builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoSettings.ConnectionString));

builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(mongoSettings.DatabaseName);
});

builder.Services.AddScoped<ITokenService, AzureTokenService>();
builder.Services.AddScoped(sp => new Microsoft.Graph.GraphServiceClient(sp.GetRequiredService<ITokenService>().GetTokenCredential()));
builder.Services.AddScoped<HttpClient>();
builder.Services.AddScoped<ITool, FileReader>();
builder.Services.AddScoped<IAIChatService, OpenAIChatService>();

builder.Services.AddScoped<IMediator, Mediator.Mediator>();
MediatorHandler.RegisterHandlers(builder.Services);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
