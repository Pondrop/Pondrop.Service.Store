using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Queries;

public class GetTodoByIdQueryHandler : IRequestHandler<GetTodoByIdQuery, Result<TodoItem>>
{
    private readonly ITodoRepository _todoRepository;
    private readonly IValidator<GetTodoByIdQuery> _validator;    
    private readonly ILogger<GetTodoByIdQueryHandler> _logger;

    public GetTodoByIdQueryHandler(
        ITodoRepository todoRepository,
        IValidator<GetTodoByIdQuery> validator,
        ILogger<GetTodoByIdQueryHandler> logger)
    {
        _todoRepository = todoRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<TodoItem>> Handle(GetTodoByIdQuery request, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(request);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get Todo by ID failed {validation}";
            _logger.LogError(errorMessage);
            return Result<TodoItem>.Error(errorMessage);
        }

        var result = default(Result<TodoItem>);

        try
        {
            const string categoryKey = "@category";
            const string idKey = "@id";

            var sql = $"SELECT * FROM c WHERE c.category = {categoryKey} AND c.id = {idKey}";
            var items = await _todoRepository.QueryItemsAsync(sql, new Dictionary<string, string>()
            {
                { categoryKey, request.Category },
                { idKey, request.Id.ToString() },
            });

            var item = items.Single();

            result = item is not null
                ? Result<TodoItem>.Success(item)
                : Result<TodoItem>.Error("Failed to find item");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<TodoItem>.Error(ex);
        }

        return result;
    }
}