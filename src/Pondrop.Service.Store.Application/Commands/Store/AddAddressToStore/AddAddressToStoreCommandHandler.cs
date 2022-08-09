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

public class AddAddressToStoreCommandHandler : DirtyCommandHandler<AddAddressToStoreCommand, Result<StoreRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<AddAddressToStoreCommand> _validator;    
    private readonly ILogger<AddAddressToStoreCommandHandler> _logger;

    public AddAddressToStoreCommandHandler(
        IOptions<StoreUpdateConfiguration> storeUpdateConfig,
        IEventRepository eventRepository,
        IDaprService daprService,
        IUserService userService,
        IMapper mapper,
        IValidator<AddAddressToStoreCommand> validator,
        ILogger<AddAddressToStoreCommandHandler> logger) : base(storeUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<StoreRecord>> Handle(AddAddressToStoreCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Create store address failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<StoreRecord>.Error(errorMessage);
        }

        var result = default(Result<StoreRecord>);

        try
        {
            var storeStream = await _eventRepository.LoadStreamAsync(EventEntity.GetStreamId<StoreEntity>(command.StoreId));

            if (storeStream.Version >= 0)
            {
                var storeEntity = new StoreEntity(storeStream.Events);
                storeEntity.Apply(new AddStoreAddress(
                    Guid.NewGuid(),
                    storeEntity.Id,
                    command.ExternalReferenceId,
                    command.AddressLine1,
                    command.AddressLine2,
                    command.Suburb,
                    command.State,
                    command.Postcode,
                    command.Country,
                    command.Latitude,
                    command.Longitude), _userService.CurrentUserName());
                var success = await _eventRepository.AppendEventsAsync(storeEntity.StreamId, storeEntity.AtSequence - 1, storeEntity.GetEvents(storeEntity.AtSequence));

                await Task.WhenAll(
                    InvokeDaprMethods(storeEntity.Id, storeEntity.GetEvents(storeEntity.AtSequence)));
            
                result = success
                    ? Result<StoreRecord>.Success(_mapper.Map<StoreRecord>(storeEntity))
                    : Result<StoreRecord>.Error(FailedToCreateMessage(command));
            }
            else
            {
                result = Result<StoreRecord>.Error("Store does not exist");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToCreateMessage(command));
            result = Result<StoreRecord>.Error(ex);
        }

        return result;
    }

    private static string FailedToCreateMessage(AddAddressToStoreCommand command) =>
        $"Failed to create store address\nCommand: '{JsonConvert.SerializeObject(command)}'";
}