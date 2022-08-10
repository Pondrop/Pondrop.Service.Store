using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Interfaces.Services;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Events;
using Pondrop.Service.Store.Domain.Events.Retailer;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class UpdateRetailerCommandHandler : DirtyCommandHandler<UpdateRetailerCommand, Result<RetailerRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IMaterializedViewRepository<RetailerEntity> _retailerViewRepository;
    private readonly IUserService _userService;
    private readonly IMapper _mapper;
    private readonly IValidator<UpdateRetailerCommand> _validator;
    private readonly ILogger<UpdateRetailerCommandHandler> _logger;

    public UpdateRetailerCommandHandler(
        IOptions<RetailerUpdateConfiguration> retailerUpdateConfig,
        IEventRepository eventRepository,
        IMaterializedViewRepository<RetailerEntity> retailerViewRepository,
        IUserService userService,
        IDaprService daprService,
        IMapper mapper,
        IValidator<UpdateRetailerCommand> validator,
        ILogger<UpdateRetailerCommandHandler> logger) : base(retailerUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _retailerViewRepository = retailerViewRepository;
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
            var retailerEntity = await _retailerViewRepository.GetByIdAsync(command.Id);
            retailerEntity ??= await GetFromStreamAsync(command.Id);

            if (retailerEntity is not null)
            {
                var evtPayload = new UpdateRetailer(command.Name);
                var createdBy = _userService.CurrentUserName();

                var success = await UpdateStreamAsync(retailerEntity, evtPayload, createdBy);

                if (!success)
                {
                    await _retailerViewRepository.FastForwardAsync(retailerEntity);
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

    private async Task<RetailerEntity?> GetFromStreamAsync(Guid id)
    {
        var stream = await _eventRepository.LoadStreamAsync(EventEntity.GetStreamId<RetailerEntity>(id));
        if (stream.Events.Any())
            return new RetailerEntity(stream.Events);

        return null;
    }

    private async Task<bool> UpdateStreamAsync(RetailerEntity entity, IEventPayload evtPayload, string createdBy)
    {
        var appliedEntity = entity with { };
        appliedEntity.Apply(evtPayload, createdBy);

        var success = await _eventRepository.AppendEventsAsync(appliedEntity.StreamId, appliedEntity.AtSequence - 1, appliedEntity.GetEvents(appliedEntity.AtSequence));

        if (success)
            entity.Apply(evtPayload, createdBy);

        return success;
    }

    private static string FailedToMessage(UpdateRetailerCommand command) =>
        $"Failed to update retailer '{JsonConvert.SerializeObject(command)}'";
}