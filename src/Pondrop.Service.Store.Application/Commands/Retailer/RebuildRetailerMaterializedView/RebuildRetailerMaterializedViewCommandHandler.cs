using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class RebuildRetailerMaterializedViewCommandHandler : IRequestHandler<RebuildRetailerMaterializedViewCommand, Result<int>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IMaterializedViewRepository<RetailerEntity> _retailerMaterializedViewRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<RebuildRetailerMaterializedViewCommand> _validator;    
    private readonly ILogger<RebuildRetailerMaterializedViewCommandHandler> _logger;

    public RebuildRetailerMaterializedViewCommandHandler(
        IEventRepository eventRepository,
        IMaterializedViewRepository<RetailerEntity> retailerMaterializedViewRepository,
        IMapper mapper,
        IValidator<RebuildRetailerMaterializedViewCommand> validator,
        ILogger<RebuildRetailerMaterializedViewCommandHandler> logger)
    {
        _eventRepository = eventRepository;
        _retailerMaterializedViewRepository = retailerMaterializedViewRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(RebuildRetailerMaterializedViewCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Update retailer materialized view failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<int>.Error(errorMessage);
        }

        var result = default(Result<int>);

        try
        {
            var count = await _retailerMaterializedViewRepository.RebuildAsync();
            result = count >= 0
                ? Result<int>.Success(count)
                : Result<int>.Error(FailedToMessage());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToMessage());
            result = Result<int>.Error(ex);
        }

        return result;
    }
    
    private static string FailedToMessage() =>
        $"Failed to rebuild materialized retailer view";
}