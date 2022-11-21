using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Interfaces.Services;
using Pondrop.Service.Store.Domain.Events.StoreType;

namespace Pondrop.Service.Store.Application.Commands;

public class UpdateStoreTypeCommandHandler : DirtyCommandHandler<StoreTypeEntity, UpdateStoreTypeCommand, Result<StoreTypeRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly ICheckpointRepository<StoreTypeEntity> _storeTypeCheckpointRepository;
    private readonly IUserService _userService;
    private readonly IMapper _mapper;
    private readonly IValidator<UpdateStoreTypeCommand> _validator;
    private readonly ILogger<UpdateStoreTypeCommandHandler> _logger;

    public UpdateStoreTypeCommandHandler(
        IOptions<StoreTypeUpdateConfiguration> storeTypeUpdateConfig,
        IEventRepository eventRepository,
        ICheckpointRepository<StoreTypeEntity> storeTypeCheckpointRepository,
        IUserService userService,
        IDaprService daprService,
        IMapper mapper,
        IValidator<UpdateStoreTypeCommand> validator,
        ILogger<UpdateStoreTypeCommandHandler> logger) : base(eventRepository, storeTypeUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _storeTypeCheckpointRepository = storeTypeCheckpointRepository;
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
            var storeTypeEntity = await _storeTypeCheckpointRepository.GetByIdAsync(command.Id);
            storeTypeEntity ??= await GetFromStreamAsync(command.Id);

            if (storeTypeEntity is not null)
            {
                var evtPayload = new UpdateStoreType(command.Name);
                var createdBy = _userService.CurrentUserName();

                var success = await UpdateStreamAsync(storeTypeEntity, evtPayload, createdBy);

                if (!success)
                {
                    await _storeTypeCheckpointRepository.FastForwardAsync(storeTypeEntity);
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

    private static string FailedToMessage(UpdateStoreTypeCommand command) =>
        $"Failed to update store type '{JsonConvert.SerializeObject(command)}'";
}