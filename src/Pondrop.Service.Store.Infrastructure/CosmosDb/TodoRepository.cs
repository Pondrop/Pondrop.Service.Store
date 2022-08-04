using AutoMapper;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pondrop.Service.Store.Application.Commands;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;
using Pondrop.Service.Store.Infrastructure.Models;

namespace Pondrop.Service.Store.Infrastructure.CosmosDb;

public class TodoRepository : ITodoRepository
{
    private const string ContainerName = "items";
    
    private readonly IMapper _mapper;
    private readonly ILogger<TodoRepository> _logger;
    private readonly CosmosConfiguration _config;

    private readonly CosmosClient _cosmosClient;
    private readonly SemaphoreSlim _connectSemaphore = new SemaphoreSlim(1, 1);

    private Database? _database;
    private Container? _container;

    public TodoRepository(
        IMapper mapper,
        IOptions<CosmosConfiguration> config,
        ILogger<TodoRepository> logger)
    {
        _mapper = mapper;
        _logger = logger;
        
        if (string.IsNullOrEmpty(config.Value?.ApplicationName))
            throw new ArgumentException("CosmosDB 'ApplicationName' cannot be null or empty");
        if (string.IsNullOrEmpty(config.Value?.DatabaseName))
            throw new ArgumentException("CosmosDB 'DatabaseName' cannot be null or empty");
        if (string.IsNullOrEmpty(config.Value?.ConnectionString))
            throw new ArgumentException("CosmosDB 'ConnectionString' cannot be null or empty");

        _config = config.Value;

        _cosmosClient =
            new CosmosClient(
                _config.ConnectionString,
                new CosmosClientOptions()
                {
                    ApplicationName = _config.ApplicationName
                });
    }

    public async Task<bool> IsConnectedAsync()
    {
        if (_container is not null)
            return true;

        await _connectSemaphore.WaitAsync();

        try
        {
            if (_container is null)
            {
                _database = await _cosmosClient.CreateDatabaseIfNotExistsAsync(_config.DatabaseName);
            }

            if (_database is not null && _container is null)
            {
                _container = await _database.CreateContainerIfNotExistsAsync(ContainerName, "/category");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
        finally
        {
            _connectSemaphore.Release();
        }

        return _container is not null;
    }

    public async Task<TodoItem?> AddItemAsync(CreateTodoCommand item)
    {
        if (await IsConnectedAsync())
        {
            var entity = _mapper.Map<TodoItemEntity>(item);
            var response = await _container!.CreateItemAsync(entity);
            return _mapper.Map<TodoItem>(response.Resource);
        }

        return default;
    }

    public async Task<TodoItem?> ReplaceItemAsync(TodoItem item)
    {
        if (await IsConnectedAsync())
        {
            var entity = _mapper.Map<TodoItemEntity>(item);
            var response = await _container!.ReplaceItemAsync(entity, entity.Id);
            return _mapper.Map<TodoItem>(response.Resource);
        }

        return default;
    }

    public async Task<bool> DeleteItemAsync(TodoItem item)
    {
        if (await IsConnectedAsync())
        {
            var entity = _mapper.Map<TodoItemEntity>(item);
            await _container!.DeleteItemAsync<TodoItemEntity>(entity.Id, new PartitionKey(entity.Category));
            return true;
        }

        return false;
    }

    public Task<List<TodoItem>> QueryItemsAsync(string sqlQueryText)
        => QueryItemsAsync(sqlQueryText, new Dictionary<string, string>(0));

    public async Task<List<TodoItem>> QueryItemsAsync(string sqlQueryText, Dictionary<string, string> parameters)
    {
        var items = default(List<TodoItem>);

        if (await IsConnectedAsync())
        {
            var queryDefinition = new QueryDefinition(sqlQueryText);

            foreach (var kv in parameters)
                queryDefinition = queryDefinition.WithParameter(kv.Key, kv.Value);

            var queryResultSetIterator = _container!.GetItemQueryIterator<TodoItemEntity>(queryDefinition);

            items = new List<TodoItem>();

            while (queryResultSetIterator.HasMoreResults)
            {
                var currentItems = await queryResultSetIterator.ReadNextAsync();
                items.AddRange(currentItems.Select(i => _mapper.Map<TodoItem>(i)));
            }
        }

        return items ?? new List<TodoItem>(0);
    }
}
