using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Queries;

public class GetAllStoreTypesQueryHandler : IRequestHandler<GetAllStoreTypesQuery, Result<List<StoreTypeRecord>>>
{
    private readonly ICheckpointRepository<StoreTypeEntity> _viewRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetAllStoreTypesQuery> _validator;
    private readonly ILogger<GetAllStoreTypesQueryHandler> _logger;

    public GetAllStoreTypesQueryHandler(
        ICheckpointRepository<StoreTypeEntity> viewRepository,
        IMapper mapper,
        IValidator<GetAllStoreTypesQuery> validator,
        ILogger<GetAllStoreTypesQueryHandler> logger)
    {
        _viewRepository = viewRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<List<StoreTypeRecord>>> Handle(GetAllStoreTypesQuery query, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(query);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get all stores failed {validation}";
            _logger.LogError(errorMessage);
            return Result<List<StoreTypeRecord>>.Error(errorMessage);
        }

        var result = default(Result<List<StoreTypeRecord>>);

        try
        {
            var storeTypeEntities = await _viewRepository.GetAllAsync();
            var storeTypeRecords = storeTypeEntities
                .Select(i => _mapper.Map<StoreTypeRecord>(i))
                .ToList();

            result = Result<List<StoreTypeRecord>>.Success(storeTypeRecords);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<List<StoreTypeRecord>>.Error(ex);
        }

        return result;
    }
}