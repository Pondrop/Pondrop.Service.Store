using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class UpdateRetailerMaterializedViewByIdCommandHandler : IRequestHandler<UpdateRetailerMaterializedViewByIdCommand, Result<RetailerRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IMaterializedViewRepository<RetailerEntity> _retailerMaterializedViewRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<UpdateRetailerMaterializedViewByIdCommand> _validator;    
    private readonly ILogger<UpdateRetailerMaterializedViewByIdCommandHandler> _logger;

    public UpdateRetailerMaterializedViewByIdCommandHandler(
        IEventRepository eventRepository,
        IMaterializedViewRepository<RetailerEntity> retailerMaterializedViewRepository,
        IMapper mapper,
        IValidator<UpdateRetailerMaterializedViewByIdCommand> validator,
        ILogger<UpdateRetailerMaterializedViewByIdCommandHandler> logger)
    {
        _eventRepository = eventRepository;
        _retailerMaterializedViewRepository = retailerMaterializedViewRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<RetailerRecord>> Handle(UpdateRetailerMaterializedViewByIdCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Update retailer materialized view failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<RetailerRecord>.Error(errorMessage);
        }

        var result = default(Result<RetailerRecord>);

        try
        {
            var retailerStream = await _eventRepository.LoadStreamAsync(EventEntity.GetStreamId<RetailerEntity>(command.Id));

            if (retailerStream.Version >= 0)
            {
                var retailerEntity = new RetailerEntity(retailerStream.Events);
                retailerEntity = await _retailerMaterializedViewRepository.ReplaceAsync(retailerEntity);

                result = retailerEntity is not null && retailerEntity.Id == command.Id
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
    
    private static string FailedToMessage(UpdateRetailerMaterializedViewByIdCommand command) =>
        $"Failed to update materialized retailer '{command.Id}'";
}