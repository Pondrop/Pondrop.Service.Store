using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Events.ShoppingList;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class RemoveItemFromShoppingListCommandHandler : IRequestHandler<RemoveItemFromShoppingListCommand, Result<ShoppingListRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<RemoveItemFromShoppingListCommand> _validator;    
    private readonly ILogger<RemoveItemFromShoppingListCommandHandler> _logger;

    public RemoveItemFromShoppingListCommandHandler(
        IEventRepository eventRepository,
        IMapper mapper,
        IValidator<RemoveItemFromShoppingListCommand> validator,
        ILogger<RemoveItemFromShoppingListCommandHandler> logger)
    {
        _eventRepository = eventRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<ShoppingListRecord>> Handle(RemoveItemFromShoppingListCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Remove from shopping list failed, errors on validation {validation}";
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
                var expectedSeqNum = shoppingList.AtSequence;
                shoppingList.Apply(new RemoveItemFromShoppingList(command.Id, shoppingList.Id));
                var success = await _eventRepository.AppendEventsAsync(shoppingList.StreamId, expectedSeqNum, shoppingList.GetEvents().Skip(shoppingList.EventsCount - 1).Take(1));

                result = success
                    ? Result<ShoppingListRecord>.Success(_mapper.Map<ShoppingListRecord>(shoppingList))
                    : Result<ShoppingListRecord>.Error("Failed to remove item");
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