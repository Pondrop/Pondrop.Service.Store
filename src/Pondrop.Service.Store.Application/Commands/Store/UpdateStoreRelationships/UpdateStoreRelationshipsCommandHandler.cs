using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Interfaces.Services;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Events.Store;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class UpdateStoreRelationshipsCommandHandler : IRequestHandler<UpdateStoreRelationshipsCommand, Result<int>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IMaterializedViewRepository<RetailerEntity> _retailerViewRepository;
    private readonly IMaterializedViewRepository<StoreTypeEntity> _storeTypeViewRepository;
    private readonly IMaterializedViewRepository<StoreEntity> _storeViewRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly ILogger<UpdateStoreRelationshipsCommandHandler> _logger;

    public UpdateStoreRelationshipsCommandHandler(
        IEventRepository eventRepository,
        IMaterializedViewRepository<RetailerEntity> retailerViewRepository,
        IMaterializedViewRepository<StoreTypeEntity> storeTypeViewRepository,
        IMaterializedViewRepository<StoreEntity> storeViewRepository,
        IMapper mapper,
        IUserService userService,
        ILogger<UpdateStoreRelationshipsCommandHandler> logger) : base()
    {
        _eventRepository = eventRepository;
        _retailerViewRepository = retailerViewRepository;
        _storeTypeViewRepository = storeTypeViewRepository;
        _storeViewRepository = storeViewRepository;
        _mapper = mapper;
        _userService = userService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(UpdateStoreRelationshipsCommand command, CancellationToken cancellationToken)
    {
        if (command.RetailerId == Guid.Empty && command.StoreTypeId == Guid.Empty)
            return Result<int>.Success(0);
        
        var result = default(Result<int>);

        try
        {
            var retailerTask = command.RetailerId != Guid.Empty
                ? _retailerViewRepository.GetByIdAsync(command.RetailerId)
                : Task.FromResult(default(RetailerEntity?));
            var storeTypeTask = command.StoreTypeId != Guid.Empty
                ? _storeTypeViewRepository.GetByIdAsync(command.StoreTypeId)
                : Task.FromResult(default(StoreTypeEntity?));

            await Task.WhenAll(retailerTask, storeTypeTask);

            if (retailerTask.Result is null && storeTypeTask.Result is null)
                return Result<int>.Success(0);

            var affectedStores = await GetAffectedStoresAsync(retailerTask.Result?.Id, storeTypeTask.Result?.Id);
            
            var retailer = retailerTask.Result is not null
                ? _mapper.Map<RetailerRecord>(retailerTask.Result)
                : default(RetailerRecord?);
            var storeType = storeTypeTask.Result is not null
                ? _mapper.Map<StoreTypeRecord>(storeTypeTask.Result)
                : default(StoreTypeRecord?);
            
            var tasks = affectedStores.Select(i =>
            {
                var updateStore = new UpdateStore(
                    null,
                    null,
                    i.Retailer.Id == retailer?.Id ? retailer : null,
                    i.StoreType.Id == storeType?.Id ? storeType : null);

                return UpdateStoreAsync(i, updateStore, _userService.GetMaterializedViewUserName());
            }).ToList();

            await Task.WhenAll(tasks);

            result = Result<int>.Success(tasks.Count(t => t.Result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToMessage(command));
            result = Result<int>.Error(ex);
        }

        return result;
    }
    
    private async Task<List<StoreEntity>> GetAffectedStoresAsync(Guid? retailerId, Guid? storeTypeId)
    {
        const string retailerIdKey = "@retailerId";
        const string storeTypeIdKey = "@storeTypeId";

        if (!retailerId.HasValue && !storeTypeId.HasValue)
            return new List<StoreEntity>(0);
        
        var conditions = new List<string>();
        var parameters = new Dictionary<string, string>();

        if (retailerId.HasValue)
        {
            conditions.Add($"c.retailer.id = {retailerIdKey}");
            parameters.Add(retailerIdKey, retailerId.Value.ToString());
        }
        
        if (storeTypeId.HasValue)
        {
            conditions.Add($"c.storeType.id = {storeTypeIdKey}");
            parameters.Add(storeTypeIdKey, storeTypeId.Value.ToString());
        }
        
        var sqlQueryText = $"SELECT * FROM c WHERE {string.Join(" AND ", conditions)}";
        
        var affectedStores = await _storeViewRepository.QueryAsync(sqlQueryText, parameters);
        return affectedStores;
    }

    private async Task<bool> UpdateStoreAsync(StoreEntity storeEntity, UpdateStore updateStore, string createdBy)
    {
        try
        {
            storeEntity.Apply(updateStore, createdBy);
            var success = await _eventRepository.AppendEventsAsync(storeEntity.StreamId, storeEntity.AtSequence - 1, storeEntity.GetEvents(storeEntity.AtSequence));
            var result = success
                ? await _storeViewRepository.UpsertAsync(storeEntity.AtSequence - 1, storeEntity)
                : default(StoreEntity?);

            return result is not null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating relationship for store {storeEntity.Id}");
        }

        return false;
    }
    
    private static string FailedToMessage(UpdateStoreRelationshipsCommand command) =>
        $"Failed to update store relationships '{JsonConvert.SerializeObject(command)}'";
}