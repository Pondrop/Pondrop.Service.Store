using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Queries;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, Result<List<UserRecord>>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetAllUsersQuery> _validator;    
    private readonly ILogger<GetAllUsersQueryHandler> _logger;

    public GetAllUsersQueryHandler(
        IEventRepository eventRepository,
        IMapper mapper,
        IValidator<GetAllUsersQuery> validator,
        ILogger<GetAllUsersQueryHandler> logger)
    {
        _eventRepository = eventRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<List<UserRecord>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(request);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get all users failed {validation}";
            _logger.LogError(errorMessage);
            return Result<List<UserRecord>>.Error(errorMessage);
        }

        var result = default(Result<List<UserRecord>>);

        try
        {
            var allUserStreams = await _eventRepository.LoadStreamsByTypeAsync(request.StreamType);
            var users = allUserStreams
                .Select(i => _mapper.Map<UserRecord>(new UserEntity(i.Value.Events)))
                .ToList();
            
            result = users.Any()
                ? Result<List<UserRecord>>.Success(users) 
                : Result<List<UserRecord>>.Error("No users found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<List<UserRecord>>.Error(ex);
        }

        return result;
    }
}