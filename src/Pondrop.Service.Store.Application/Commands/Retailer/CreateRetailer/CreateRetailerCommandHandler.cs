using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Interfaces.Services;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class CreateRetailerCommandHandler : DirtyCommandHandler<RetailerEntity, CreateRetailerCommand, Result<RetailerRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IValidator<CreateRetailerCommand> _validator;
    private readonly ILogger<CreateRetailerCommandHandler> _logger;

    public CreateRetailerCommandHandler(
        IOptions<RetailerUpdateConfiguration> retailerUpdateConfig,
        IEventRepository eventRepository,
        IDaprService daprService,
        IUserService userService,
        IMapper mapper,
        IValidator<CreateRetailerCommand> validator,
        ILogger<CreateRetailerCommandHandler> logger) : base(eventRepository, retailerUpdateConfig.Value, daprService, logger)
    {
        _eventRepository = eventRepository;
        _mapper = mapper;
        _userService = userService;
        _validator = validator;
        _logger = logger;
    }

    public override async Task<Result<RetailerRecord>> Handle(CreateRetailerCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Create retailer failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<RetailerRecord>.Error(errorMessage);
        }

        var result = default(Result<RetailerRecord>);

        try
        {
            var retailerEntity = new RetailerEntity(command.Name, command.ExternalReferenceId, _userService.CurrentUserName());
            var success = await _eventRepository.AppendEventsAsync(retailerEntity.StreamId, 0, retailerEntity.GetEvents());

            await Task.WhenAll(
                InvokeDaprMethods(retailerEntity.Id, retailerEntity.GetEvents()));

            result = success
                ? Result<RetailerRecord>.Success(_mapper.Map<RetailerRecord>(retailerEntity))
                : Result<RetailerRecord>.Error(FailedToCreateMessage(command));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToCreateMessage(command));
            result = Result<RetailerRecord>.Error(ex);
        }

        return result;
    }

    private static string FailedToCreateMessage(CreateRetailerCommand command) =>
        $"Failed to create retailer\nCommand: '{JsonConvert.SerializeObject(command)}'";
}