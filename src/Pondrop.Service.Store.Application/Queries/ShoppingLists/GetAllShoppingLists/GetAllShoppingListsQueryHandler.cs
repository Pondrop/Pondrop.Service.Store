using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Queries;

public class GetAllShoppingListsQueryHandler : IRequestHandler<GetAllShoppingListsQuery, Result<List<ShoppingListRecord>>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<GetAllShoppingListsQuery> _validator;    
    private readonly ILogger<GetAllUsersQueryHandler> _logger;

    public GetAllShoppingListsQueryHandler(
        IEventRepository eventRepository,
        IMapper mapper,
        IValidator<GetAllShoppingListsQuery> validator,
        ILogger<GetAllUsersQueryHandler> logger)
    {
        _eventRepository = eventRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<List<ShoppingListRecord>>> Handle(GetAllShoppingListsQuery request, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(request);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get all shopping lists failed {validation}";
            _logger.LogError(errorMessage);
            return Result<List<ShoppingListRecord>>.Error(errorMessage);
        }

        var result = default(Result<List<ShoppingListRecord>>);

        try
        {
            var allShoppingListStreams = await _eventRepository.LoadStreamsByTypeAsync(request.StreamType);
            var lists = allShoppingListStreams
                .Select(i => _mapper.Map<ShoppingListRecord>(new ShoppingListEntity(i.Value.Events)))
                .ToList();
            
            result = lists.Any()
                ? Result<List<ShoppingListRecord>>.Success(lists) 
                : Result<List<ShoppingListRecord>>.Error("No users found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<List<ShoppingListRecord>>.Error(ex);
        }

        return result;
    }
}