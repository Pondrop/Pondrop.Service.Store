using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Interfaces.Services;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class CreateStoreTypeCommandHandler : DirtyCommandHandler<StoreTypeEntity, CreateStoreTypeCommand, Result<StoreTypeRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<CreateStoreTypeCommand> _validator;
    private readonly ILogger<CreateStoreTypeCommandHandler> _logger;

    public CreateStoreTypeCommandHandler(
        IOptions<StoreTypeUpdateConfiguration> storeTypeUpdateConfig,
        IEventRepository eventRepository,
        IDaprService daprService,
        IUserService userService,
        IMapper mapper,
        IValidator<CreateStoreTypeCommand> validator,
        ILogger<CreateStoreTypeCommandHandler> logger) : base(eventRepository, storeTypeUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<StoreTypeRecord>> Handle(CreateStoreTypeCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Create store type failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<StoreTypeRecord>.Error(errorMessage);
        }

        var result = default(Result<StoreTypeRecord>);

        try
        {
            var storeType = new StoreTypeEntity(command.Name, command.ExternalReferenceId, command.Sector, _userService.CurrentUserName());
            var success = await _eventRepository.AppendEventsAsync(storeType.StreamId, 0, storeType.GetEvents());

            await Task.WhenAll(
                InvokeDaprMethods(storeType.Id, storeType.GetEvents()));

            result = success
                ? Result<StoreTypeRecord>.Success(_mapper.Map<StoreTypeRecord>(storeType))
                : Result<StoreTypeRecord>.Error(FailedToCreateMessage(command));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToCreateMessage(command));
            result = Result<StoreTypeRecord>.Error(ex);
        }

        return result;
    }

    private static string FailedToCreateMessage(CreateStoreTypeCommand command) =>
        $"Failed to create store type\nCommand: '{JsonConvert.SerializeObject(command)}'";
}