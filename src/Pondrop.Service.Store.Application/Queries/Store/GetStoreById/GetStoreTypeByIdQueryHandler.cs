using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Queries;

public class GetStoreByIdQueryHandler : IRequestHandler<GetStoreByIdQuery, Result<StoreViewRecord?>>
{
    private readonly IViewRepository<StoreViewRecord> _viewRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetStoreByIdQuery> _validator;
    private readonly ILogger<GetStoreByIdQueryHandler> _logger;

    public GetStoreByIdQueryHandler(
        IViewRepository<StoreViewRecord> viewRepository,
        IMapper mapper,
        IValidator<GetStoreByIdQuery> validator,
        ILogger<GetStoreByIdQueryHandler> logger)
    {
        _viewRepository = viewRepository;
        _mapper = mapper;
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