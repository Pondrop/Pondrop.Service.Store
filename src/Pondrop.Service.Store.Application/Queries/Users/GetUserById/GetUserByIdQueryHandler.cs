using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Queries;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<UserRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetUserByIdQuery> _validator;    
    private readonly ILogger<GetUserByIdQueryHandler> _logger;

    public GetUserByIdQueryHandler(
        IEventRepository eventRepository,
        IMapper mapper,
        IValidator<GetUserByIdQuery> validator,
        ILogger<GetUserByIdQueryHandler> logger)
    {
        _eventRepository = eventRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<UserRecord>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(request);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get User by ID failed {validation}";
            _logger.LogError(errorMessage);
            return Result<UserRecord>.Error(errorMessage);
        }

        var result = default(Result<UserRecord>);

        try
        {
            var userStream = await _eventRepository.LoadStreamAsync(EventEntity.GetStreamId<UserEntity>(request.Id));

            result = userStream.Version >= 0 
                ? Result<UserRecord>.Success(_mapper.Map<UserRecord>(new UserEntity(userStream.Events))) 
                : Result<UserRecord>.Error("User does not exist");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<UserRecord>.Error(ex);
        }

        return result;
    }
}