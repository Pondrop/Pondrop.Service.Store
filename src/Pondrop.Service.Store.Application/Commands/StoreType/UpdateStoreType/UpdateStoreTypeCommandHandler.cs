﻿using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Interfaces.ServiceBus;
using Pondrop.Service.Store.Application.Interfaces.Services;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Events.StoreType;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class UpdateStoreTypeCommandHandler : DirtyCommandHandler<UpdateStoreTypeCommand, Result<StoreTypeRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IMaterializedViewRepository<StoreTypeEntity> _storeTypeViewRepository;
    private readonly IMaterializedViewRepository<StoreEntity> _storeViewRepository;
    private readonly IMediator _mediator;
    private readonly IUserService _userService;
    private readonly IMapper _mapper;
    private readonly IValidator<UpdateStoreTypeCommand> _validator;    
    private readonly ILogger<UpdateStoreTypeCommandHandler> _logger;

    public UpdateStoreTypeCommandHandler(
        IOptions<StoreTypeUpdateConfiguration> storeTypeUpdateConfig,
        IEventRepository eventRepository,
        IMaterializedViewRepository<StoreTypeEntity> storeTypeViewRepository,
        IMaterializedViewRepository<StoreEntity> storeViewRepository,
        IMediator mediator,
        IUserService userService,
        IDaprService daprService,
        IMapper mapper,
        IValidator<UpdateStoreTypeCommand> validator,
        ILogger<UpdateStoreTypeCommandHandler> logger) : base(storeTypeUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _storeTypeViewRepository = storeTypeViewRepository;
        _storeViewRepository = storeViewRepository;
        _mediator = mediator;
        _userService = userService;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<StoreTypeRecord>> Handle(UpdateStoreTypeCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Update store type failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<StoreTypeRecord>.Error(errorMessage);
        }

        var result = default(Result<StoreTypeRecord>);

        try
        {
            var storeTypeEntity = await _storeTypeViewRepository.GetByIdAsync(command.Id);

            if (storeTypeEntity is not null)
            {
                storeTypeEntity.Apply(new UpdateStoreType(command.Name), _userService.CurrentUserName());
                
                var success = await _eventRepository.AppendEventsAsync(storeTypeEntity.StreamId, storeTypeEntity.AtSequence - 1, storeTypeEntity.GetEvents(storeTypeEntity.AtSequence));

                await Task.WhenAll(
                    InvokeDaprMethods(storeTypeEntity.Id, storeTypeEntity.GetEvents(storeTypeEntity.AtSequence)));

                result = success
                    ? Result<StoreTypeRecord>.Success(_mapper.Map<StoreTypeRecord>(storeTypeEntity))
                    : Result<StoreTypeRecord>.Error(FailedToMessage(command));
            }
            else
            {
                result = Result<StoreTypeRecord>.Error($"Retailer does not exist '{command.Id}'");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToMessage(command));
            result = Result<StoreTypeRecord>.Error(ex);
        }

        return result;
    }
    
    private static string FailedToMessage(UpdateStoreTypeCommand command) =>
        $"Failed to update store type '{JsonConvert.SerializeObject(command)}'";
}