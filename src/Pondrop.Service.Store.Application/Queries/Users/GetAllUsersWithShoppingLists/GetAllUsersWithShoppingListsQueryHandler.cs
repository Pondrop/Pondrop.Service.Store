using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Queries;

public class GetAllUsersAndShoppingListsQueryHandler : IRequestHandler<GetAllUsersWithShoppingListsQuery, Result<List<UserWithShoppingListsRecord>>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetAllUsersWithShoppingListsQuery> _validator;    
    private readonly ILogger<GetAllUsersAndShoppingListsQueryHandler> _logger;

    public GetAllUsersAndShoppingListsQueryHandler(
        IEventRepository eventRepository,
        IMapper mapper,
        IValidator<GetAllUsersWithShoppingListsQuery> validator,
        ILogger<GetAllUsersAndShoppingListsQueryHandler> logger)
    {
        _eventRepository = eventRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<List<UserWithShoppingListsRecord>>> Handle(GetAllUsersWithShoppingListsQuery request, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(request);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get all users with shopping lists failed {validation}";
            _logger.LogError(errorMessage);
            return Result<List<UserWithShoppingListsRecord>>.Error(errorMessage);
        }

        var result = default(Result<List<UserWithShoppingListsRecord>>);

        try
        {
            var allUserStreamsTask = _eventRepository.LoadStreamsByTypeAsync(request.UserStreamType);
            var allShoppingListStreamsTask = _eventRepository.LoadStreamsByTypeAsync(request.ShoppingListStreamType);

            await Task.WhenAll(allUserStreamsTask, allShoppingListStreamsTask);
            
            var users = allUserStreamsTask.Result
                .Select(i => _mapper.Map<UserRecord>(new UserEntity(i.Value.Events)));
            var shoppingLists = allShoppingListStreamsTask.Result
                .Select(i => _mapper.Map<ShoppingListRecord>(new ShoppingListEntity(i.Value.Events)));

            var records = new List<UserWithShoppingListsRecord>();

            foreach (var u in users)
            {
                var userLists = shoppingLists.Where(i => i.UserId == u.Id).ToList();
                records.Add(new UserWithShoppingListsRecord(u.Id, u.FirstName, u.LastName, u.Email, userLists));
            }
            
            result = records.Any()
                ? Result<List<UserWithShoppingListsRecord>>.Success(records) 
                : Result<List<UserWithShoppingListsRecord>>.Error("No users found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<List<UserWithShoppingListsRecord>>.Error(ex);
        }

        return result;
    }
}