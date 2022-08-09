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

public class RemoveAddressFromStoreCommandHandler : DirtyCommandHandler<RemoveAddressFromStoreCommand, Result<StoreRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<RemoveAddressFromStoreCommand> _validator;    
    private readonly ILogger<RemoveAddressFromStoreCommandHandler> _logger;

    public RemoveAddressFromStoreCommandHandler(
        IOptions<StoreUpdateConfiguration> storeUpdateConfig,
        IEventRepository eventRepository,
        IDaprService daprService,
        IUserService userService,
        IMapper mapper,
        IValidator<RemoveAddressFromStoreCommand> validator,
        ILogger<RemoveAddressFromStoreCommandHandler> logger) : base(storeUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<StoreRecord>> Handle(RemoveAddressFromStoreCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Remove store address failed, errors on validation {validation}";
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

                if (storeEntity.Addresses.Any(i => i.Id == command.Id))
                {
                    storeEntity.Apply(new RemoveAddressFromStore(command.Id, command.StoreId), _userService.CurrentUserName());
                    var success = await _eventRepository.AppendEventsAsync(storeEntity.StreamId, storeEntity.AtSequence - 1, storeEntity.GetEvents(storeEntity.AtSequence));

                    await Task.WhenAll(
                        InvokeDaprMethods(storeEntity.Id, storeEntity.GetEvents(storeEntity.AtSequence)));
            
                    result = success
                        ? Result<StoreRecord>.Success(_mapper.Map<StoreRecord>(storeEntity))
                        : Result<StoreRecord>.Error(FailedToCreateMessage(command));   
                }
                else
                {
                    result = Result<StoreRecord>.Error("Address does not exist for store");
                }
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

    private static string FailedToCreateMessage(RemoveAddressFromStoreCommand command) =>
        $"Failed to remove store address\nCommand: '{JsonConvert.SerializeObject(command)}'";
}