using Azure;
using Azure.Messaging.ServiceBus;
using MediatR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pondrop.Service.Store.Application.Commands;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;
using System.Text;
using Azure.Messaging.EventGrid;

namespace Pondrop.Service.Store.Api.Services;

public class ServiceBusListenerService : IServiceBusListenerService
{
    private readonly ILogger<ServiceBusListenerService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMediator _mediator;
    private readonly ServiceBusConfiguration _config;

    private readonly ServiceBusClient _serviceBusClient;

    private ServiceBusProcessor _processor;
    private readonly EventGridPublisherClient _submissionViewTopic;

    public ServiceBusListenerService(
        IOptions<ServiceBusConfiguration> config,
        IOptions<SubmissionViewTopicConfiguration> submissionViewTopicConfig,
        IMediator mediator,
        IServiceProvider serviceProvider,
        ILogger<ServiceBusListenerService> logger)
    {
        _mediator = mediator;
        _logger = logger;
        _serviceProvider = serviceProvider;

        if (string.IsNullOrEmpty(config.Value?.ConnectionString))
            throw new ArgumentException("Service Bus 'ConnectionString' cannot be null or empty");
        if (string.IsNullOrEmpty(config.Value?.QueueName))
            throw new ArgumentException("Service Bus 'QueueName' cannot be null or empty"); ;

        if (string.IsNullOrEmpty(submissionViewTopicConfig.Value?.Endpoint))
            throw new ArgumentException("Event Grid Topic Endpoint cannot be null or empty");
        if (string.IsNullOrEmpty(submissionViewTopicConfig.Value?.AccessKey))
            throw new ArgumentException("Event Grid Topic AccessKey cannot be null or empty");

        _config = config.Value;

        _serviceBusClient = new ServiceBusClient(_config.ConnectionString);

        _submissionViewTopic = new EventGridPublisherClient(new Uri(submissionViewTopicConfig.Value.Endpoint),
            new AzureKeyCredential(submissionViewTopicConfig.Value.AccessKey));

    }


    public async Task StartListener()
    {
        ServiceBusProcessorOptions _serviceBusProcessorOptions = new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = 1,
            AutoCompleteMessages = false,
        };

        _processor = _serviceBusClient.CreateProcessor(_config.QueueName, _serviceBusProcessorOptions);
        _processor.ProcessMessageAsync += ProcessMessagesAsync;
        _processor.ProcessErrorAsync += ProcessErrorAsync;


        await _processor.StartProcessingAsync().ConfigureAwait(false);
    }

    public async Task StopListener()
    {
        await _processor.CloseAsync().ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        if (_processor != null)
        {
            await _processor.DisposeAsync().ConfigureAwait(false);
        }

        if (_serviceBusClient != null)
        {
            await _serviceBusClient.DisposeAsync().ConfigureAwait(false);
        }
    }

    private async Task ProcessMessagesAsync(ProcessMessageEventArgs args)
    {
        try
        {
            if (args.Message.Subject.Contains("Command"))
            {
                var commandType = typeof(UpdateCheckpointByIdCommand);
                var commandTypeName = $"{commandType.FullName!.Replace(nameof(UpdateCheckpointByIdCommand), args.Message.Subject)}, {commandType.Assembly.GetName()}";

                commandType = Type.GetType(commandTypeName);
                var payload = Encoding.UTF8.GetString(args.Message.Body);

                if (commandType is not null && !string.IsNullOrEmpty(payload))
                {
                    var command = JsonConvert.DeserializeObject<JObject>(payload)?.ToObject(commandType);
                    if (command is not null)
                    {
                        try
                        {
                            using var scoped = _serviceProvider.CreateScope();
                            var mediator = scoped.ServiceProvider.GetService<IMediator>();
                            await mediator!.Send(command);

                            var storeIds = new List<SubmissionStoreViewRecord>();
                            var result = default(Result<List<SubmissionStoreViewRecord>>);


                            switch (command)
                            {
                                case UpdateRetailerCheckpointByIdCommand retailer:
                                    result = await mediator!.Send(new UpdateStoreViewCommand() { RetailerId = retailer.Id });
                                    await mediator!.Send(new UpdateStoreSearchIndexViewCommand() { RetailerId = retailer.Id });
                                    break;
                                case UpdateStoreTypeCheckpointByIdCommand storeType:
                                    await mediator!.Send(new UpdateStoreViewCommand() { StoreTypeId = storeType.Id });
                                    await mediator!.Send(new UpdateStoreSearchIndexViewCommand() { StoreTypeId = storeType.Id });
                                    break;
                                case UpdateStoreCheckpointByIdCommand store:
                                    result = await mediator!.Send(new UpdateStoreViewCommand() { StoreId = store.Id });
                                    await mediator!.Send(new UpdateStoreSearchIndexViewCommand() { StoreId = store.Id });
                                    break;
                            }

                            if (result is { IsSuccess: true, Value: { } })
                            {
                                var events = result.Value.Select(s =>
                                    new EventGridEvent("SubmissionViewUpdate", "SubmissionViewUpdate", "1.0", s));

                                await _submissionViewTopic.SendEventsAsync(events);
                            }

                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Failed to run process event '{args.Message.Subject}'");
                        }
                    }
                }
            }
        }
        finally
        {
            await args.CompleteMessageAsync(args.Message).ConfigureAwait(false);
        }
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs arg)
    {
        _logger.LogError(arg.Exception, "Message handler encountered an exception");
        _logger.LogDebug($"- ErrorSource: {arg.ErrorSource}");
        _logger.LogDebug($"- Entity Path: {arg.EntityPath}");
        _logger.LogDebug($"- FullyQualifiedNamespace: {arg.FullyQualifiedNamespace}");

        return Task.CompletedTask;
    }
}