using AspNetCore.Proxy;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Pondrop.Service.Store.Api.Configurations.Extensions;
using Pondrop.Service.Store.Api.Middleware;
using Pondrop.Service.Store.Api.Models;
using Pondrop.Service.Store.Api.Services;
using Pondrop.Service.Store.Api.Services.Interfaces;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;
using Pondrop.Service.Infrastructure.CosmosDb;
using Pondrop.Service.Infrastructure.Dapr;
using Pondrop.Service.Infrastructure.ServiceBus;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Models;
using Pondrop.Service.Interfaces.Services;
using Pondrop.Service.Product.Application.Models;
using Pondrop.Service.Events;
using Pondrop.Service.Store.Domain.Events.Retailer;

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

var AllowedOrigins = "allowedOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowedOrigins,
        policy =>
        {
            policy.WithOrigins("https://admin-portal.ashyocean-bde16918.australiaeast.azurecontainerapps.io",
                "http://localhost:3000")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});


builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName.ToLowerInvariant()}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

services.Configure<JsonOptions>(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = false;
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

services.AddProxies();

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

services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
services.AddAutoMapper(
    typeof(Result<>),
    typeof(EventEntity),
    typeof(EventRepository),
    typeof(CreateRetailer));
services.AddMediatR(
    typeof(Result<>));
services.AddFluentValidation(config =>
    {
        config.RegisterValidatorsFromAssemblyContaining(typeof(Result<>));
    });


services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    var Key = Encoding.UTF8.GetBytes(configuration["JWT:Key"]);
    o.SaveToken = true;
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = configuration["JWT:Issuer"],
        ValidAudience = configuration["JWT:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Key)
    };
});

services.Configure<CosmosConfiguration>(configuration.GetSection(CosmosConfiguration.Key));
services.Configure<ServiceBusConfiguration>(configuration.GetSection(ServiceBusConfiguration.Key));
services.Configure<StoreSearchIndexConfiguration>(configuration.GetSection(StoreSearchIndexConfiguration.Key));
services.Configure<SubmissionViewTopicConfiguration>(configuration.GetSection(EventGridTopicConfiguration.Key).GetSection(SubmissionViewTopicConfiguration.Key));
services.Configure<RetailerUpdateConfiguration>(configuration.GetSection(DaprEventTopicConfiguration.Key).GetSection(RetailerUpdateConfiguration.Key));
services.Configure<StoreTypeUpdateConfiguration>(configuration.GetSection(DaprEventTopicConfiguration.Key).GetSection(StoreTypeUpdateConfiguration.Key));
services.Configure<StoreUpdateConfiguration>(configuration.GetSection(DaprEventTopicConfiguration.Key).GetSection(StoreUpdateConfiguration.Key));

services.AddHostedService<ServiceBusHostedService>();
services.AddSingleton<IServiceBusListenerService, ServiceBusListenerService>();

services.AddHostedService<RebuildMaterializeViewHostedService>();
services.AddSingleton<IRebuildCheckpointQueueService, RebuildCheckpointQueueService>();

services.AddSingleton<IAddressService, AddressService>();
var serviceCollection = services.AddSingleton<IUserService, UserService>();
services.AddSingleton<IEventRepository, EventRepository>();
services.AddSingleton<ICheckpointRepository<RetailerEntity>, CheckpointRepository<RetailerEntity>>();
services.AddSingleton<ICheckpointRepository<StoreTypeEntity>, CheckpointRepository<StoreTypeEntity>>();
services.AddSingleton<ICheckpointRepository<StoreEntity>, CheckpointRepository<StoreEntity>>();
services.AddSingleton<IContainerRepository<StoreViewRecord>, ContainerRepository<StoreViewRecord>>();
services.AddSingleton<IContainerRepository<StoreSearchIndexViewRecord>, ContainerRepository<StoreSearchIndexViewRecord>>();
services.AddSingleton<IDaprService, DaprService>();
services.AddSingleton<IServiceBusService, ServiceBusService>();
services.AddSingleton<ITokenProvider, JWTTokenProvider>();

DefaultEventTypePayloadResolver.Init(typeof(CreateRetailer).Assembly);

var app = builder.Build();
var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

// Configure the HTTP request pipeline.

app.UseCors(AllowedOrigins);
app.UseMiddleware<ExceptionMiddleware>();
app.UseSwaggerDocumentation(provider);

app.UseAuthentication();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
