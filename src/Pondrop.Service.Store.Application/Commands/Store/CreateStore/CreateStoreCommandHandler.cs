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

public class CreateStoreCommandHandler : DirtyCommandHandler<CreateStoreCommand, Result<StoreRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IMaterializedViewRepository<RetailerEntity> _retailerViewRepository;
    private readonly IMaterializedViewRepository<StoreTypeEntity> _storeTypeViewRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<CreateStoreCommand> _validator;    
    private readonly ILogger<CreateStoreCommandHandler> _logger;

    public CreateStoreCommandHandler(
        IOptions<StoreUpdateConfiguration> storeUpdateConfig,
        IEventRepository eventRepository,
        IMaterializedViewRepository<RetailerEntity> retailerViewRepository,
        IMaterializedViewRepository<StoreTypeEntity> storeTypeViewRepository,
        IDaprService daprService,
        IUserService userService,
        IMapper mapper,
        IValidator<CreateStoreCommand> validator,
        ILogger<CreateStoreCommandHandler> logger) : base(storeUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _retailerViewRepository = retailerViewRepository;
        _storeTypeViewRepository = storeTypeViewRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<StoreRecord>> Handle(CreateStoreCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Create store failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<StoreRecord>.Error(errorMessage);
        }

        var result = default(Result<StoreRecord>);

        try
        {
            var retailerTask = _retailerViewRepository.GetByIdAsync(command.RetailerId);
            var storeTypeTask = _storeTypeViewRepository.GetByIdAsync(command.StoreTypeId);

            await Task.WhenAll(retailerTask, storeTypeTask);

            if (retailerTask.Result is null)
                return Result<StoreRecord>.Error($"Could not find retailer with id '{command.RetailerId}'");
            if (storeTypeTask.Result is null)
                return Result<StoreRecord>.Error($"Could not find store type with id '{command.StoreTypeId}'");

            var retailerRecord = _mapper.Map<RetailerRecord>(retailerTask.Result);
            var storeTypeRecord = _mapper.Map<StoreTypeRecord>(storeTypeTask.Result);
            
            var storeEntity = new StoreEntity(command.Name, command.Status, command.ExternalReferenceId, retailerRecord, storeTypeRecord, _userService.CurrentUserName());
            storeEntity.Apply(new AddStoreAddress(
                Guid.NewGuid(),
                storeEntity.Id,
                command.Address!.ExternalReferenceId,
                command.Address!.AddressLine1,
                command.Address?.AddressLine2 ?? string.Empty,
                command.Address!.Suburb,
                command.Address!.State,
                command.Address!.Postcode,
                command.Address!.Country,
                command.Address!.Latitude,
                command.Address!.Longitude), _userService.CurrentUserName());
            var success = await _eventRepository.AppendEventsAsync(storeEntity.StreamId, 0, storeEntity.GetEvents());

            await Task.WhenAll(
                InvokeDaprMethods(storeEntity.Id, storeEntity.GetEvents()));
            
            result = success
                ? Result<StoreRecord>.Success(_mapper.Map<StoreRecord>(storeEntity))
                : Result<StoreRecord>.Error(FailedToCreateMessage(command));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToCreateMessage(command));
            result = Result<StoreRecord>.Error(ex);
        }

        return result;
    }

    private static string FailedToCreateMessage(CreateStoreCommand command) =>
        $"Failed to create store\nCommand: '{JsonConvert.SerializeObject(command)}'";
}