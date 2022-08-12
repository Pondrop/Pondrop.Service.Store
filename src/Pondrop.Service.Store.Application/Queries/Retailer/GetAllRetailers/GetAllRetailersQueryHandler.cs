using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Queries;

public class GetAllRetailersQueryHandler : IRequestHandler<GetAllRetailersQuery, Result<List<RetailerRecord>>>
{
    private readonly ICheckpointRepository<RetailerEntity> _viewRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetAllRetailersQuery> _validator;    
    private readonly ILogger<GetAllRetailersQueryHandler> _logger;

    public GetAllRetailersQueryHandler(
        ICheckpointRepository<RetailerEntity> viewRepository,
        IMapper mapper,
        IValidator<GetAllRetailersQuery> validator,
        ILogger<GetAllRetailersQueryHandler> logger)
    {
        _viewRepository = viewRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<List<RetailerRecord>>> Handle(GetAllRetailersQuery query, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(query);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get all retailers failed {validation}";
            _logger.LogError(errorMessage);
            return Result<List<RetailerRecord>>.Error(errorMessage);
        }

        var result = default(Result<List<RetailerRecord>>);

        try
        {
            var retailerEntities = await _viewRepository.GetAllAsync();
            var retailerRecords = retailerEntities
                .Select(i => _mapper.Map<RetailerRecord>(i))
                .ToList();
            
            result = Result<List<RetailerRecord>>.Success(retailerRecords);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<List<RetailerRecord>>.Error(ex);
        }

        return result;
    }
}