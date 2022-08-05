using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class RebuildStoreMaterializedViewCommandHandler : IRequestHandler<RebuildStoreMaterializedViewCommand, Result<int>>
{
    private readonly IMaterializedViewRepository<StoreEntity> _storeMaterializedViewRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<RebuildStoreMaterializedViewCommand> _validator;    
    private readonly ILogger<RebuildStoreMaterializedViewCommandHandler> _logger;

    public RebuildStoreMaterializedViewCommandHandler(
        IMaterializedViewRepository<StoreEntity> storeMaterializedViewRepository,
        IMapper mapper,
        IValidator<RebuildStoreMaterializedViewCommand> validator,
        ILogger<RebuildStoreMaterializedViewCommandHandler> logger)
    {
        _storeMaterializedViewRepository = storeMaterializedViewRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(RebuildStoreMaterializedViewCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Update store materialized view failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<int>.Error(errorMessage);
        }

        var result = default(Result<int>);

        try
        {
            var count = await _storeMaterializedViewRepository.RebuildAsync();
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
        $"Failed to rebuild materialized store view";
}