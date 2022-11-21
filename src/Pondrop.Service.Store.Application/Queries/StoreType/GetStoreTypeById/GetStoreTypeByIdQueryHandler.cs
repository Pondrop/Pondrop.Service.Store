using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Queries;

public class GetStoreTypeByIdQueryHandler : IRequestHandler<GetStoreTypeByIdQuery, Result<StoreTypeRecord?>>
{
    private readonly ICheckpointRepository<StoreTypeEntity> _viewRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetStoreTypeByIdQuery> _validator;    
    private readonly ILogger<GetStoreTypeByIdQueryHandler> _logger;

    public GetStoreTypeByIdQueryHandler(
        ICheckpointRepository<StoreTypeEntity> viewRepository,
        IMapper mapper,
        IValidator<GetStoreTypeByIdQuery> validator,
        ILogger<GetStoreTypeByIdQueryHandler> logger)
    {
        _viewRepository = viewRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<StoreTypeRecord?>> Handle(GetStoreTypeByIdQuery query, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(query);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get store type by id failed {validation}";
            _logger.LogError(errorMessage);
            return Result<StoreTypeRecord?>.Error(errorMessage);
        }

        var result = default(Result<StoreTypeRecord?>);

        try
        {
            var storeTypeEntity = await _viewRepository.GetByIdAsync(query.Id);
            result = storeTypeEntity is not null
                ? Result<StoreTypeRecord?>.Success(_mapper.Map<StoreTypeRecord>(storeTypeEntity)) 
                : Result<StoreTypeRecord?>.Success(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<StoreTypeRecord?>.Error(ex);
        }

        return result;
    }
}