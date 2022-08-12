using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Queries;

public class GetAllStoresQueryHandler : IRequestHandler<GetAllStoresQuery, Result<List<StoreViewRecord>>>
{
    private readonly IContainerRepository<StoreViewRecord> _containerRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetAllStoresQuery> _validator;
    private readonly ILogger<GetAllStoresQueryHandler> _logger;

    public GetAllStoresQueryHandler(
        IContainerRepository<StoreViewRecord> storeRepository,
        IMapper mapper,
        IValidator<GetAllStoresQuery> validator,
        ILogger<GetAllStoresQueryHandler> logger)
    {
        _containerRepository = storeRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<List<StoreViewRecord>>> Handle(GetAllStoresQuery request, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(request);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get all stores failed {validation}";
            _logger.LogError(errorMessage);
            return Result<List<StoreViewRecord>>.Error(errorMessage);
        }

        var result = default(Result<List<StoreViewRecord>>);

        try
        {
            var records = await _containerRepository.GetAllAsync();
            result = Result<List<StoreViewRecord>>.Success(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<List<StoreViewRecord>>.Error(ex);
        }

        return result;
    }
}