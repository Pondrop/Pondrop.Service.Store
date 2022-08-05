using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Queries;

public class GetAllStoresQueryHandler : IRequestHandler<GetAllStoresQuery, Result<List<StoreRecord>>>
{
    private readonly IMaterializedViewRepository<StoreEntity> _storeViewRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetAllStoresQuery> _validator;    
    private readonly ILogger<GetAllStoresQueryHandler> _logger;

    public GetAllStoresQueryHandler(
        IMaterializedViewRepository<StoreEntity> storeViewRepository,
        IMapper mapper,
        IValidator<GetAllStoresQuery> validator,
        ILogger<GetAllStoresQueryHandler> logger)
    {
        _storeViewRepository = storeViewRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<List<StoreRecord>>> Handle(GetAllStoresQuery request, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(request);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get all shopping lists failed {validation}";
            _logger.LogError(errorMessage);
            return Result<List<StoreRecord>>.Error(errorMessage);
        }

        var result = default(Result<List<StoreRecord>>);

        try
        {
            var storeEntities = await _storeViewRepository.GetAllAsync();
            var storeRecords = storeEntities.Select(i => _mapper.Map<StoreRecord>(i)).ToList();
            
            result = Result<List<StoreRecord>>.Success(storeRecords);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<List<StoreRecord>>.Error(ex);
        }

        return result;
    }
}