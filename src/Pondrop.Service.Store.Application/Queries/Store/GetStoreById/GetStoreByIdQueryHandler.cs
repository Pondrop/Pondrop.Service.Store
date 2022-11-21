using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Queries;

public class GetStoreByIdQueryHandler : IRequestHandler<GetStoreByIdQuery, Result<StoreViewRecord?>>
{
    private readonly IContainerRepository<StoreViewRecord> _viewRepository;
    private readonly IValidator<GetStoreByIdQuery> _validator;
    private readonly ILogger<GetStoreByIdQueryHandler> _logger;

    public GetStoreByIdQueryHandler(
        IContainerRepository<StoreViewRecord> viewRepository,
        IValidator<GetStoreByIdQuery> validator,
        ILogger<GetStoreByIdQueryHandler> logger)
    {
        _viewRepository = viewRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<StoreViewRecord?>> Handle(GetStoreByIdQuery query, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(query);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get store by id failed {validation}";
            _logger.LogError(errorMessage);
            return Result<StoreViewRecord?>.Error(errorMessage);
        }

        var result = default(Result<StoreViewRecord?>);

        try
        {
            var record = await _viewRepository.GetByIdAsync(query.Id);

            result = record is not null
                ? Result<StoreViewRecord?>.Success(record)
                : Result<StoreViewRecord?>.Success(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<StoreViewRecord?>.Error(ex);
        }

        return result;
    }
}