using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class CreateShoppingListCommandHandler : IRequestHandler<CreateShoppingListCommand, Result<ShoppingListRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateShoppingListCommand> _validator;    
    private readonly ILogger<CreateShoppingListCommandHandler> _logger;

    public CreateShoppingListCommandHandler(
        IEventRepository eventRepository,
        IMapper mapper,
        IValidator<CreateShoppingListCommand> validator,
        ILogger<CreateShoppingListCommandHandler> logger)
    {
        _eventRepository = eventRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<ShoppingListRecord>> Handle(CreateShoppingListCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Create shopping list failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<ShoppingListRecord>.Error(errorMessage);
        }

        var result = default(Result<ShoppingListRecord>);

        try
        {
            var shoppingList = new ShoppingListEntity(command.UserId, command.Name, command.Description);
            var success = await _eventRepository.AppendEventsAsync(shoppingList.StreamId, 0, shoppingList.GetEvents());

            result = success
                ? Result<ShoppingListRecord>.Success(_mapper.Map<ShoppingListRecord>(shoppingList))
                : Result<ShoppingListRecord>.Error("Failed to create shopping list");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<ShoppingListRecord>.Error(ex);
        }

        return result;
    }
}