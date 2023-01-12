using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Interfaces.Services;
using Pondrop.Service.Store.Domain.Events.Store;

namespace Pondrop.Service.Store.Application.Commands;

public class UpdateStoreCommandHandler : DirtyCommandHandler<StoreEntity, UpdateStoreCommand, Result<StoreRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly ICheckpointRepository<RetailerEntity> _retailerCheckpointRepository;
    private readonly ICheckpointRepository<StoreTypeEntity> _storeTypeCheckpointRepository;
    private readonly ICheckpointRepository<StoreEntity> _storeCheckpointRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<UpdateStoreCommand> _validator;
    private readonly ILogger<UpdateStoreCommandHandler> _logger;

    public UpdateStoreCommandHandler(
        IOptions<StoreUpdateConfiguration> storeUpdateConfig,
        IEventRepository eventRepository,
        ICheckpointRepository<RetailerEntity> retailerCheckpointRepository,
        ICheckpointRepository<StoreTypeEntity> storeTypeCheckpointRepository,
        ICheckpointRepository<StoreEntity> storeCheckpointRepository,
        IDaprService daprService,
        IUserService userService,
        IMapper mapper,
        IValidator<UpdateStoreCommand> validator,
        ILogger<UpdateStoreCommandHandler> logger) : base(eventRepository, storeUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _retailerCheckpointRepository = retailerCheckpointRepository;
        _storeTypeCheckpointRepository = storeTypeCheckpointRepository;
        _storeCheckpointRepository = storeCheckpointRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<StoreRecord>> Handle(UpdateStoreCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Update store failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<StoreRecord>.Error(errorMessage);
        }

        var result = default(Result<StoreRecord>);

        try
        {
            var retailerTask = command.RetailerId.HasValue
                ? _retailerCheckpointRepository.GetByIdAsync(command.RetailerId.Value)
                : Task.FromResult(default(RetailerEntity?));
            var storeTypeTask = command.StoreTypeId.HasValue
                ? _storeTypeCheckpointRepository.GetByIdAsync(command.StoreTypeId.Value)
                : Task.FromResult(default(StoreTypeEntity?));

            await Task.WhenAll(retailerTask, storeTypeTask);

            if (command.RetailerId.HasValue && retailerTask.Result is null)
                return Result<StoreRecord>.Error($"Could not find retailer with id '{command.RetailerId}'");
            if (command.StoreTypeId.HasValue && storeTypeTask.Result is null)
                return Result<StoreRecord>.Error($"Could not find store type with id '{command.StoreTypeId}'");

            var storeEntity = await _storeCheckpointRepository.GetByIdAsync(command.Id);
            storeEntity ??= await GetFromStreamAsync(command.Id);

            if (storeEntity is not null)
            {
                var evtPayload = new UpdateStore(
                    command.Name,
                    command.Status,
                    command.Phone,
                    command.Email,
                    command.OpenHours,
                    command.IsCommunityStore,
                    retailerTask.Result is not null ? retailerTask.Result.Id : null,
                    storeTypeTask.Result is not null ? storeTypeTask.Result.Id : null);
                var createdBy = _userService.CurrentUserName();

                var success = await UpdateStreamAsync(storeEntity, evtPayload, createdBy);

                if (!success)
                {
                    await _storeCheckpointRepository.FastForwardAsync(storeEntity);
                    success = await UpdateStreamAsync(storeEntity, evtPayload, createdBy);
                }

                await Task.WhenAll(
                    InvokeDaprMethods(storeEntity.Id, storeEntity.GetEvents(storeEntity.AtSequence)));

                result = success
                    ? Result<StoreRecord>.Success(_mapper.Map<StoreRecord>(storeEntity))
                    : Result<StoreRecord>.Error(FailedToCreateMessage(command));
            }
            else
            {
                result = Result<StoreRecord>.Error($"Store does not exist '{command.Id}'");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToCreateMessage(command));
            result = Result<StoreRecord>.Error(ex);
        }

        return result;
    }

    private static string FailedToCreateMessage(UpdateStoreCommand command) =>
        $"Failed to update store\nCommand: '{JsonConvert.SerializeObject(command)}'";
}