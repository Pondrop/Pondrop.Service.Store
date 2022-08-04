using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Events.ShoppingList;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class AddItemToShoppingListCommandHandler : IRequestHandler<AddItemToShoppingListCommand, Result<ShoppingListRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<AddItemToShoppingListCommand> _validator;    
    private readonly ILogger<AddItemToShoppingListCommandHandler> _logger;

    public AddItemToShoppingListCommandHandler(
        IEventRepository eventRepository,
        IMapper mapper,
        IValidator<AddItemToShoppingListCommand> validator,
        ILogger<AddItemToShoppingListCommandHandler> logger)
    {
        _eventRepository = eventRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<ShoppingListRecord>> Handle(AddItemToShoppingListCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Add to shopping list failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<ShoppingListRecord>.Error(errorMessage);
        }

        var result = default(Result<ShoppingListRecord>);

        try
        {
            var listStream = await _eventRepository.LoadStreamAsync(EventEntity.GetStreamId<ShoppingListEntity>(command.ShoppingListId));

            if (listStream.Version >= 0)
            {
                var shoppingList = new ShoppingListEntity(listStream.Events);
                shoppingList.Apply(new AddItemToShoppingList(Guid.NewGuid(), shoppingList.Id, command.Name));
                var success = await _eventRepository.AppendEventsAsync(shoppingList.StreamId, shoppingList.AtSequence - 1, shoppingList.GetEvents(shoppingList.AtSequence));

                result = success
                    ? Result<ShoppingListRecord>.Success(_mapper.Map<ShoppingListRecord>(shoppingList))
                    : Result<ShoppingListRecord>.Error("Failed to add item");
            }
            else
            {
                result = Result<ShoppingListRecord>.Error("Shopping list does not exist");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<ShoppingListRecord>.Error(ex);
        }

        return result;
    }
}