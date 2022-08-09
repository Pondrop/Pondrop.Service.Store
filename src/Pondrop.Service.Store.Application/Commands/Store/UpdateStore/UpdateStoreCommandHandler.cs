using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Interfaces.Services;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Events.Store;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class UpdateStoreCommandHandler : DirtyCommandHandler<UpdateStoreCommand, Result<StoreRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IMaterializedViewRepository<RetailerEntity> _retailerViewRepository;
    private readonly IMaterializedViewRepository<StoreTypeEntity> _storeTypeViewRepository;
    private readonly IMaterializedViewRepository<StoreEntity> _storeViewRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<UpdateStoreCommand> _validator;    
    private readonly ILogger<UpdateStoreCommandHandler> _logger;

    public UpdateStoreCommandHandler(
        IOptions<StoreUpdateConfiguration> storeUpdateConfig,
        IEventRepository eventRepository,
        IMaterializedViewRepository<RetailerEntity> retailerViewRepository,
        IMaterializedViewRepository<StoreTypeEntity> storeTypeViewRepository,
        IMaterializedViewRepository<StoreEntity> storeViewRepository,
        IDaprService daprService,
        IUserService userService,
        IMapper mapper,
        IValidator<UpdateStoreCommand> validator,
        ILogger<UpdateStoreCommandHandler> logger) : base(storeUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _retailerViewRepository = retailerViewRepository;
        _storeTypeViewRepository = storeTypeViewRepository;
        _storeViewRepository = storeViewRepository;
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
                ? _retailerViewRepository.GetByIdAsync(command.RetailerId.Value)
                : Task.FromResult(default(RetailerEntity?));
            var storeTypeTask = command.StoreTypeId.HasValue
                ? _storeTypeViewRepository.GetByIdAsync(command.StoreTypeId.Value)
                : Task.FromResult(default(StoreTypeEntity?));

            await Task.WhenAll(retailerTask, storeTypeTask);

            if (command.RetailerId.HasValue && retailerTask.Result is null)
                return Result<StoreRecord>.Error($"Could not find retailer with id '{command.RetailerId}'");
            if (command.StoreTypeId.HasValue && storeTypeTask.Result is null)
                return Result<StoreRecord>.Error($"Could not find store type with id '{command.StoreTypeId}'");

            var storeEntity = await _storeViewRepository.GetByIdAsync(command.Id);
            if (storeEntity is not null)
            {
                storeEntity.Apply(new UpdateStore(
                    command.Name,
                    command.Status,
                    retailerTask.Result is not null ? _mapper.Map<RetailerRecord>(retailerTask.Result) : null,
                    storeTypeTask.Result is not null ? _mapper.Map<StoreTypeRecord>(storeTypeTask.Result) : null), _userService.CurrentUserName());
                var success = await _eventRepository.AppendEventsAsync(storeEntity.StreamId, storeEntity.AtSequence - 1, storeEntity.GetEvents(storeEntity.AtSequence));

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