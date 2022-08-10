
using AutoMapper;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pondrop.Service.Store.Api.Services.Interface;
using Pondrop.Service.Store.Application.Commands;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pondrop.Service.Store.Api.Services;
public class ServiceBusListener : IServiceBusListener
{
    private readonly IMapper _mapper;
    private readonly ILogger<ServiceBusListener> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMediator _mediator;
    private readonly ServiceBusConfiguration _config;
    private readonly IMaterializedViewRepository<StoreEntity> _storeViewRepository;

    private readonly ServiceBusClient _serviceBusClient;

    private ServiceBusProcessor _processor;

    public ServiceBusListener(
        IMapper mapper,
        IOptions<ServiceBusConfiguration> config,
        IMaterializedViewRepository<StoreEntity> storeViewRepository,
        IMediator mediator,
        IServiceProvider serviceProvider,
        ILogger<ServiceBusListener> logger)
    {
        _mapper = mapper;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _storeViewRepository = storeViewRepository;

        if (string.IsNullOrEmpty(config.Value?.ConnectionString))
            throw new ArgumentException("Service Bus 'ConnectionString' cannot be null or empty");
        if (string.IsNullOrEmpty(config.Value?.QueueName))
            throw new ArgumentException("Service Bus 'QueueName' cannot be null or empty"); ;

        _config = config.Value;

        _serviceBusClient = new ServiceBusClient(_config.ConnectionString);
    }


    public async Task HandleMessages()
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
            var payload = Encoding.UTF8.GetString(args.Message.Body);

            var commandType = Type.GetType(args.Message.Subject);
            //if (commandType == typeof()
            //{
            //    var command = JsonConvert.DeserializeObject<UpdateRetailerCommand>(payload);
            //    await UpdateStoresByRetailerAsync(command);
            //}
            //else if (args.Message.Subject == "UpdateStoreTypeCommand")
            //{
            //    var command = JsonConvert.DeserializeObject<UpdateStoreTypeCommand>(payload);
            //    await UpdateStoresByStoreTypeAsync(command);
            //}
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

    public async Task CloseListener()
    {
        await _processor.CloseAsync().ConfigureAwait(false);
    }

    private async Task UpdateStoresByRetailerAsync(UpdateRetailerCommand command)
    {
        const string retailerIdKey = "@retailerId";
        var affectedStores = await _storeViewRepository.QueryAsync(
            $"SELECT * FROM c WHERE c.retailer.id = {retailerIdKey}",
            new Dictionary<string, string>() { [retailerIdKey] = command.Id.ToString() });
        try
        {
            using var scoped = _serviceProvider.CreateScope();
            var mediator = scoped.ServiceProvider.GetService<IMediator>();

            var updateTasks = affectedStores.Select(i =>
            {
                var updateStoreCommand = new UpdateStoreCommand() { Id = i.Id, RetailerId = command.Id };

                return mediator.Send(updateStoreCommand);
            }).ToList();


            await Task.WhenAll(updateTasks);
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to run rebuild materialize view {command.GetType().Name}");
        }
    }

    private async Task UpdateStoresByStoreTypeAsync(UpdateStoreTypeCommand command)
    {
        const string storeTypeIdKey = "@storeTypeId";
        var affectedStores = await _storeViewRepository.QueryAsync(
            $"SELECT * FROM c WHERE c.storeType.id = {storeTypeIdKey}",
            new Dictionary<string, string>() { [storeTypeIdKey] = command.Id.ToString() });
        try
        {
            using var scoped = _serviceProvider.CreateScope();
            var mediator = scoped.ServiceProvider.GetService<IMediator>();

            var updateTasks = affectedStores.Select(i =>
            {
                var updateStoreCommand = new UpdateStoreCommand() { Id = i.Id, StoreTypeId = command.Id };

                return mediator.Send(updateStoreCommand);
            }).ToList();


            await Task.WhenAll(updateTasks);
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to run rebuild materialize view {command.GetType().Name}");
        }
    }
}