using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Queries;

public class GetTodoByCategoryQueryHandler : IRequestHandler<GetTodoByCategoryQuery, Result<List<TodoItem>>>
{
    private readonly ITodoRepository _todoRepository;
    private readonly IValidator<GetTodoByCategoryQuery> _validator;    
    private readonly ILogger<GetTodoByCategoryQueryHandler> _logger;

    public GetTodoByCategoryQueryHandler(
        ITodoRepository todoRepository,
        IValidator<GetTodoByCategoryQuery> validator,
        ILogger<GetTodoByCategoryQueryHandler> logger)
    {
        _todoRepository = todoRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<List<TodoItem>>> Handle(GetTodoByCategoryQuery request, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(request);

        if (!validation.IsValid)
        {
            var errorMessage = $"Get Todo by category failed {validation}";
            _logger.LogError(errorMessage);
            return Result<List<TodoItem>>.Error(errorMessage);
        }

        var result = default(Result<List<TodoItem>>);

        try
        {
            const string categoryKey = "@category";

            var sql = $"SELECT * FROM c WHERE c.category = {categoryKey}";
            var items = await _todoRepository.QueryItemsAsync(sql, new Dictionary<string, string>()
            {
                { categoryKey, request.Category }
            });

            result = items is not null
                ? Result<List<TodoItem>>.Success(items)
                : Result<List<TodoItem>>.Error("Failed to find item");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            result = Result<List<TodoItem>>.Error(ex);
        }

        return result;
    }
}