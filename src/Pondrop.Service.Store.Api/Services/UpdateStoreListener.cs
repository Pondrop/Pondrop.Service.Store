
using AutoMapper;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pondrop.Service.Store.Api.Services.Interface;
using Pondrop.Service.Store.Application.Commands;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pondrop.Service.Store.Api.Services;
public class UpdateStoreListener : IServiceBusListener
{
    //private readonly IProcessData _processData;

    private readonly IMapper _mapper;
    private readonly ILogger<UpdateStoreListener> _logger;
    private readonly IMediator _mediator;
    private readonly ServiceBusConfiguration _config;
    private readonly IMaterializedViewRepository<StoreEntity> _storeViewRepository;

    private readonly ServiceBusClient _serviceBusClient;

    private ServiceBusProcessor _processor;

    public UpdateStoreListener(
        IMapper mapper,
        IOptions<ServiceBusConfiguration> config,
        IMaterializedViewRepository<StoreEntity> storeViewRepository,
        IMediator mediator,
        ILogger<UpdateStoreListener> logger)
    {
        _mapper = mapper;
        _logger = logger;
        _mediator = mediator;
        _storeViewRepository = storeViewRepository;

        if (string.IsNullOrEmpty(config.Value?.ConnectionString))
            throw new ArgumentException("Service Bus 'ConnectionString' cannot be null or empty");
        if (string.IsNullOrEmpty(config.Value?.QueueName))
            throw new ArgumentException("Service Bus 'QueueName' cannot be null or empty"); ;

        _config = config.Value;

        _serviceBusClient = new ServiceBusClient(_config.ConnectionString);
    }


    public async Task PrepareFiltersAndHandleMessages()
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

    private async Task ProcessMessagesAsync(ProcessMessageEventArgs args)
    {
        try
        {
            var payload = args.Message.Body.ToObjectFromJson<RetailerEntity>();
        }
        catch (Exception ex)
        {
            var payload = args.Message.Body.ToObjectFromJson<StoreTypeEntity>();
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

    public async Task CloseSubscriptionAsync()
    {
        await _processor.CloseAsync().ConfigureAwait(false);
    }

    private async Task UpdateStoresAsync(Guid updatedRetailerId)
    {
        const string retailerIdKey = "@retailerId";
        var affectedStores = await _storeViewRepository.QueryAsync(
            $"SELECT * FROM c WHERE c.retailer.id = {retailerIdKey}",
            new Dictionary<string, string>() { [retailerIdKey] = updatedRetailerId.ToString() });

        var updateTasks = affectedStores.Select(i =>
        {
            var command = new UpdateStoreCommand() { Id = i.Id, RetailerId = updatedRetailerId };
            return _mediator.Send(command);
        }).ToList();

        await Task.WhenAll(updateTasks);
    }
}