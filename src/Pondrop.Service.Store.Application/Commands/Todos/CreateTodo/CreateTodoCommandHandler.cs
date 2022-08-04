using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class CreateTodoCommandHandler : IRequestHandler<CreateTodoCommand, Result<TodoItem>>
{
    private readonly ITodoRepository _todoRepository;
    private readonly IValidator<CreateTodoCommand> _validator;    
    private readonly ILogger<CreateTodoCommandHandler> _logger;

    public CreateTodoCommandHandler(
        ITodoRepository todoRepository,
        IValidator<CreateTodoCommand> validator,
        ILogger<CreateTodoCommandHandler> logger)
    {
        _todoRepository = todoRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<TodoItem>> Handle(CreateTodoCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Create Todo item failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<TodoItem>.Error(errorMessage);
        }

        var result = default(Result<TodoItem>);

        try
        {
            var newItem = await _todoRepository.AddItemAsync(command);

            result = !string.IsNullOrEmpty(newItem?.Id)
                ? Result<TodoItem>.Success(newItem)
                : Result<TodoItem>.Error("Failed to create item");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<TodoItem>.Error(ex);
        }

        return result;
    }
}