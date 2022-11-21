using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;
using Pondrop.Service.Interfaces.Services;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Store.Domain.Events.Retailer;

namespace Pondrop.Service.Store.Application.Commands;

public class UpdateRetailerCommandHandler : DirtyCommandHandler<RetailerEntity, UpdateRetailerCommand, Result<RetailerRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly ICheckpointRepository<RetailerEntity> _retailerCheckpointRepository;
    private readonly IUserService _userService;
    private readonly IMapper _mapper;
    private readonly IValidator<UpdateRetailerCommand> _validator;
    private readonly ILogger<UpdateRetailerCommandHandler> _logger;

    public UpdateRetailerCommandHandler(
        IOptions<RetailerUpdateConfiguration> retailerUpdateConfig,
        IEventRepository eventRepository,
        ICheckpointRepository<RetailerEntity> retailerCheckpointRepository,
        IUserService userService,
        IDaprService daprService,
        IMapper mapper,
        IValidator<UpdateRetailerCommand> validator,
        ILogger<UpdateRetailerCommandHandler> logger) : base(eventRepository, retailerUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _retailerCheckpointRepository = retailerCheckpointRepository;
        _userService = userService;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<RetailerRecord>> Handle(UpdateRetailerCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Update retailer failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<RetailerRecord>.Error(errorMessage);
        }

        var result = default(Result<RetailerRecord>);

        try
        {
            var retailerEntity = await _retailerCheckpointRepository.GetByIdAsync(command.Id);
            retailerEntity ??= await GetFromStreamAsync(command.Id);

            if (retailerEntity is not null)
            {
                var evtPayload = new UpdateRetailer(command.Name);
                var createdBy = _userService.CurrentUserName();

                var success = await UpdateStreamAsync(retailerEntity, evtPayload, createdBy);

                if (!success)
                {
                    await _retailerCheckpointRepository.FastForwardAsync(retailerEntity);
                    success = await UpdateStreamAsync(retailerEntity, evtPayload, createdBy);
                }

                await Task.WhenAll(
                    InvokeDaprMethods(retailerEntity.Id, retailerEntity.GetEvents()));

                result = success
                    ? Result<RetailerRecord>.Success(_mapper.Map<RetailerRecord>(retailerEntity))
                    : Result<RetailerRecord>.Error(FailedToMessage(command));
            }
            else
            {
                result = Result<RetailerRecord>.Error($"Retailer does not exist '{command.Id}'");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToMessage(command));
            result = Result<RetailerRecord>.Error(ex);
        }

        return result;
    }

    private static string FailedToMessage(UpdateRetailerCommand command) =>
        $"Failed to update retailer '{JsonConvert.SerializeObject(command)}'";
}