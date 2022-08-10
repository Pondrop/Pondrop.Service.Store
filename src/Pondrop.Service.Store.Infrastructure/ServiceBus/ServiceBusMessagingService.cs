using AutoMapper;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pondrop.Service.Store.Application.Interfaces.ServiceBus;
using Pondrop.Service.Store.Application.Models;

namespace Pondrop.Service.Store.Infrastructure.ServiceBus;
public class ServiceBusMessagingService<T> : IServiceBusMessagingService<T> where T : new()
{
    private readonly IMapper _mapper;
    private readonly ILogger<ServiceBusMessagingService<T>> _logger;
    private readonly ServiceBusConfiguration _config;

    private readonly ServiceBusClient _serviceBusClient;
    private readonly ServiceBusSender _sender;

    public ServiceBusMessagingService(
        IMapper mapper,
        IOptions<ServiceBusConfiguration> config,
        ILogger<ServiceBusMessagingService<T>> logger)
    {
        _mapper = mapper;
        _logger = logger;

        if (string.IsNullOrEmpty(config.Value?.ConnectionString))
            throw new ArgumentException("Service Bus 'ConnectionString' cannot be null or empty");
        if (string.IsNullOrEmpty(config.Value?.QueueName))
            throw new ArgumentException("Service Bus 'QueueName' cannot be null or empty"); ;

        _config = config.Value;
        _serviceBusClient = new ServiceBusClient(_config.ConnectionString);
        _sender = _serviceBusClient.CreateSender(_config.QueueName);

    }

    public async Task SendMessageAsync(T message)
    {
        try
        {
            var jsonMessage = JsonConvert.SerializeObject(message);
            var serviceBusMessage = new ServiceBusMessage(jsonMessage);
            serviceBusMessage.Subject = $"{typeof(T).FullName},{typeof(T).Assembly.FullName}";
            await _sender.SendMessageAsync(serviceBusMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }
}
