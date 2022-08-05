using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class RebuildStoreTypeMaterializedViewCommandHandler : IRequestHandler<RebuildStoreTypeMaterializedViewCommand, Result<int>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IMaterializedViewRepository<StoreTypeEntity> _storeTypeMaterializedViewRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<RebuildStoreTypeMaterializedViewCommand> _validator;    
    private readonly ILogger<RebuildStoreTypeMaterializedViewCommandHandler> _logger;

    public RebuildStoreTypeMaterializedViewCommandHandler(
        IEventRepository eventRepository,
        IMaterializedViewRepository<StoreTypeEntity> storeTypeMaterializedViewRepository,
        IMapper mapper,
        IValidator<RebuildStoreTypeMaterializedViewCommand> validator,
        ILogger<RebuildStoreTypeMaterializedViewCommandHandler> logger)
    {
        _eventRepository = eventRepository;
        _storeTypeMaterializedViewRepository = storeTypeMaterializedViewRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(RebuildStoreTypeMaterializedViewCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Update store type materialized view failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<int>.Error(errorMessage);
        }

        var result = default(Result<int>);

        try
        {
            var count = await _storeTypeMaterializedViewRepository.RebuildAsync();
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
        $"Failed to rebuild materialized store type view";
}