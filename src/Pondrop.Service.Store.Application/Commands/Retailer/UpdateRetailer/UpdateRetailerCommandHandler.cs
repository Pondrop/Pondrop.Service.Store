using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Interfaces.ServiceBus;
using Pondrop.Service.Store.Application.Interfaces.Services;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Events.Retailer;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class UpdateRetailerCommandHandler : DirtyCommandHandler<UpdateRetailerCommand, Result<RetailerRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IServiceBusMessagingService<UpdateRetailerCommand> _retailerMessagingService;
    private readonly IMaterializedViewRepository<RetailerEntity> _retailerViewRepository;
    private readonly IMaterializedViewRepository<StoreEntity> _storeViewRepository;
    private readonly IMediator _mediator;
    private readonly IUserService _userService;
    private readonly IMapper _mapper;
    private readonly IValidator<UpdateRetailerCommand> _validator;
    private readonly ILogger<UpdateRetailerCommandHandler> _logger;

    public UpdateRetailerCommandHandler(
        IOptions<RetailerUpdateConfiguration> retailerUpdateConfig,
        IEventRepository eventRepository,
        IMaterializedViewRepository<RetailerEntity> retailerViewRepository,
        IMaterializedViewRepository<StoreEntity> storeViewRepository,
        IServiceBusMessagingService<UpdateRetailerCommand> retailerMessagingService,
        IMediator mediator,
        IUserService userService,
        IDaprService daprService,
        IMapper mapper,
        IValidator<UpdateRetailerCommand> validator,
        ILogger<UpdateRetailerCommandHandler> logger) : base(retailerUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _retailerMessagingService = retailerMessagingService;
        _retailerViewRepository = retailerViewRepository;
        _storeViewRepository = storeViewRepository;
        _mediator = mediator;
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

            if (retailerEntity is not null)
            {
                retailerEntity.Apply(new UpdateRetailer(command.Name), _userService.CurrentUserName());

                var success = await _eventRepository.AppendEventsAsync(retailerEntity.StreamId, retailerEntity.AtSequence - 1, retailerEntity.GetEvents(retailerEntity.AtSequence));

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