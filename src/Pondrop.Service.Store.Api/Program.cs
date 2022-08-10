using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Pondrop.Service.Store.Api.Configurations.Extensions;
using Pondrop.Service.Store.Api.Middleware;
using Pondrop.Service.Store.Api.Services;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Interfaces.Services;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;
using Pondrop.Service.Store.Infrastructure.CosmosDb;
using Pondrop.Service.Store.Infrastructure.Dapr;
using System.Text.Json;
using System.Text.Json.Serialization;

JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
{
    ContractResolver = new DefaultContractResolver()
    {
        NamingStrategy = new CamelCaseNamingStrategy()
    },
    DateTimeZoneHandling = DateTimeZoneHandling.Utc
};

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName.ToLowerInvariant()}.json", optional: true, reloadOnChange: true);

services.Configure<JsonOptions>(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = false;
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

// Add services to the container.
services.AddVersionedApiExplorer(options => options.GroupNameFormat = "'v'VVV");
services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
});

services.AddLogging(config =>
{
    config.AddDebug();
    config.AddConsole();
});
services
    .AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

services.AddAutoMapper(
    typeof(Result<>),
    typeof(EventEntity),
    typeof(EventRepository));
services.AddMediatR(
    typeof(Result<>));
services.AddFluentValidation(config =>
    {
        config.RegisterValidatorsFromAssemblyContaining(typeof(Result<>));
    });

services.Configure<CosmosConfiguration>(configuration.GetSection(CosmosConfiguration.Key));
services.Configure<RetailerUpdateConfiguration>(configuration.GetSection(DaprEventTopicConfiguration.Key).GetSection(RetailerUpdateConfiguration.Key));
services.Configure<StoreTypeUpdateConfiguration>(configuration.GetSection(DaprEventTopicConfiguration.Key).GetSection(StoreTypeUpdateConfiguration.Key));
services.Configure<StoreUpdateConfiguration>(configuration.GetSection(DaprEventTopicConfiguration.Key).GetSection(StoreUpdateConfiguration.Key));

services.AddHostedService<UpdateMaterializeViewHostedService>();
services.AddSingleton<IUpdateMaterializeViewQueueService, UpdateMaterializeViewQueueService>();

services.AddHostedService<RebuildMaterializeViewHostedService>();
services.AddSingleton<IRebuildMaterializeViewQueueService, RebuildMaterializeViewQueueService>();

services.AddSingleton<IAddressService, AddressService>();
services.AddSingleton<IUserService, UserService>();
services.AddSingleton<IEventRepository, EventRepository>();
services.AddSingleton<IMaterializedViewRepository<RetailerEntity>, MaterializedViewRepository<RetailerEntity>>();
services.AddSingleton<IMaterializedViewRepository<StoreTypeEntity>, MaterializedViewRepository<StoreTypeEntity>>();
services.AddSingleton<IMaterializedViewRepository<StoreEntity>, MaterializedViewRepository<StoreEntity>>();
services.AddSingleton<IViewRepository<StoreViewRecord>, ViewRepository<StoreViewRecord>>();
services.AddSingleton<IDaprService, DaprService>();

var app = builder.Build();
var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

// Configure the HTTP request pipeline.

app.UseMiddleware<ExceptionMiddleware>();
app.UseSwaggerDocumentation(provider);

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
