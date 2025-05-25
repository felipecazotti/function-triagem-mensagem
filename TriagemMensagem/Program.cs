using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using TriagemMensagem.Domain.IRepositories;
using TriagemMensagem.Domain.IServices;
using TriagemMensagem.Repository;
using TriagemMensagem.Service;

var builder = FunctionsApplication.CreateBuilder(args);

builder.Services.AddSingleton(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var mongoClient = new MongoClient(configuration["MongoDbConfiguration:ConnectionString"]);
    var mongoDatabase = mongoClient.GetDatabase(configuration["MongoDbConfiguration:DatabaseName"]);
    return mongoDatabase;
});
builder.Services.AddScoped<IRegistroRepository, RegistroRepository>();
builder.Services.AddScoped<ITriagemMensagemService, TriagemMensagemService>();


builder.ConfigureFunctionsWebApplication();

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
// builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
