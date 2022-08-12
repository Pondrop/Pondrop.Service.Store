using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Queries;

public class GetRetailerByIdQueryHandler : IRequestHandler<GetRetailerByIdQuery, Result<RetailerRecord?>>
{
    private readonly ICheckpointRepository<RetailerEntity> _viewRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetRetailerByIdQuery> _validator;    
    private readonly ILogger<GetRetailerByIdQueryHandler> _logger;

    public GetRetailerByIdQueryHandler(
        ICheckpointRepository<RetailerEntity> viewRepository,
        IMapper mapper,
        IValidator<GetRetailerByIdQuery> validator,
        ILogger<GetRetailerByIdQueryHandler> logger)
    {
        _viewRepository = viewRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<RetailerRecord?>> Handle(GetRetailerByIdQuery query, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(query);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get retailer by id failed {validation}";
            _logger.LogError(errorMessage);
            return Result<RetailerRecord?>.Error(errorMessage);
        }

        var result = default(Result<RetailerRecord?>);

        try
        {
            var retailerEntity = await _viewRepository.GetByIdAsync(query.Id);
            result = retailerEntity is not null
                ? Result<RetailerRecord?>.Success(_mapper.Map<RetailerRecord>(retailerEntity)) 
                : Result<RetailerRecord?>.Success(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<RetailerRecord?>.Error(ex);
        }

        return result;
    }
}