using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Queries;

public class GetStoreByIdQueryHandler : IRequestHandler<GetStoreByIdQuery, Result<StoreRecord?>>
{
    private readonly IMaterializedViewRepository<StoreEntity> _viewRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetStoreByIdQuery> _validator;    
    private readonly ILogger<GetStoreByIdQueryHandler> _logger;

    public GetStoreByIdQueryHandler(
        IMaterializedViewRepository<StoreEntity> viewRepository,
        IMapper mapper,
        IValidator<GetStoreByIdQuery> validator,
        ILogger<GetStoreByIdQueryHandler> logger)
    {
        _viewRepository = viewRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<StoreRecord?>> Handle(GetStoreByIdQuery query, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(query);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get store by id failed {validation}";
            _logger.LogError(errorMessage);
            return Result<StoreRecord?>.Error(errorMessage);
        }

        var result = default(Result<StoreRecord?>);

        try
        {
            var storeEntity = await _viewRepository.GetByIdAsync(query.Id);
            result = storeEntity is not null
                ? Result<StoreRecord?>.Success(_mapper.Map<StoreRecord>(storeEntity)) 
                : Result<StoreRecord?>.Success(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<StoreRecord?>.Error(ex);
        }

        return result;
    }
}