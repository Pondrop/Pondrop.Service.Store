using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Interfaces.Services;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Events;
using Pondrop.Service.Store.Domain.Events.StoreType;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class UpdateStoreTypeCommandHandler : DirtyCommandHandler<UpdateStoreTypeCommand, Result<StoreTypeRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IMaterializedViewRepository<StoreTypeEntity> _storeTypeViewRepository;
    private readonly IUserService _userService;
    private readonly IMapper _mapper;
    private readonly IValidator<UpdateStoreTypeCommand> _validator;
    private readonly ILogger<UpdateStoreTypeCommandHandler> _logger;

    public UpdateStoreTypeCommandHandler(
        IOptions<StoreTypeUpdateConfiguration> storeTypeUpdateConfig,
        IEventRepository eventRepository,
        IMaterializedViewRepository<StoreTypeEntity> storeTypeViewRepository,
        IUserService userService,
        IDaprService daprService,
        IMapper mapper,
        IValidator<UpdateStoreTypeCommand> validator,
        ILogger<UpdateStoreTypeCommandHandler> logger) : base(storeTypeUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _storeTypeViewRepository = storeTypeViewRepository;
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
            storeTypeEntity ??= await GetFromStreamAsync(command.Id);

            if (storeTypeEntity is not null)
            {
                var evtPayload = new UpdateStoreType(command.Name);
                var createdBy = _userService.CurrentUserName();

                var success = await UpdateStreamAsync(storeTypeEntity, evtPayload, createdBy);

                if (!success)
                {
                    await _storeTypeViewRepository.FastForwardAsync(storeTypeEntity);
                    success = await UpdateStreamAsync(storeTypeEntity, evtPayload, createdBy);
                }

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

    private async Task<StoreTypeEntity?> GetFromStreamAsync(Guid id)
    {
        var stream = await _eventRepository.LoadStreamAsync(EventEntity.GetStreamId<StoreTypeEntity>(id));
        if (stream.Events.Any())
            return new StoreTypeEntity(stream.Events);

        return null;
    }

    private async Task<bool> UpdateStreamAsync(StoreTypeEntity entity, IEventPayload evtPayload, string createdBy)
    {
        var appliedEntity = entity with { };
        appliedEntity.Apply(evtPayload, createdBy);

        var success = await _eventRepository.AppendEventsAsync(appliedEntity.StreamId, appliedEntity.AtSequence - 1, appliedEntity.GetEvents(appliedEntity.AtSequence));

        if (success)
            entity.Apply(evtPayload, createdBy);

        return success;
    }

    private static string FailedToMessage(UpdateStoreTypeCommand command) =>
        $"Failed to update store type '{JsonConvert.SerializeObject(command)}'";
}