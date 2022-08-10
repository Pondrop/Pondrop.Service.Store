﻿using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Interfaces.Services;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Events.Store;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class UpdateStoreViewCommandHandler : IRequestHandler<UpdateStoreViewCommand, Result<int>>
{
    private readonly IMaterializedViewRepository<RetailerEntity> _retailerViewRepository;
    private readonly IMaterializedViewRepository<StoreTypeEntity> _storeTypeViewRepository;
    private readonly IMaterializedViewRepository<StoreEntity> _storeViewRepository;
    private readonly IViewRepository<StoreViewRecord> _viewRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly ILogger<UpdateStoreViewCommandHandler> _logger;

    public UpdateStoreViewCommandHandler(
        IMaterializedViewRepository<RetailerEntity> retailerViewRepository,
        IMaterializedViewRepository<StoreTypeEntity> storeTypeViewRepository,
        IMaterializedViewRepository<StoreEntity> storeViewRepository,
        IViewRepository<StoreViewRecord> viewRepository,
        IMapper mapper,
        IUserService userService,
        ILogger<UpdateStoreViewCommandHandler> logger) : base()
    {
        _retailerViewRepository = retailerViewRepository;
        _storeTypeViewRepository = storeTypeViewRepository;
        _storeViewRepository = storeViewRepository;
        _viewRepository = viewRepository;
        _mapper = mapper;
        _userService = userService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(UpdateStoreViewCommand command, CancellationToken cancellationToken)
    {
        if (!command.StoreId.HasValue && !command.RetailerId.HasValue && !command.StoreTypeId.HasValue)
            return Result<int>.Success(0);

        var result = default(Result<int>);

        try
        {
            var retailersTask = _retailerViewRepository.GetAllAsync();
            var storeTypesTask = _storeTypeViewRepository.GetAllAsync();
            var affectedStoresTask = GetAffectedStoresAsync(command.RetailerId, command.StoreTypeId, command.StoreId);

            await Task.WhenAll(retailersTask, storeTypesTask, affectedStoresTask);

            var retailerLookup = retailersTask.Result.ToDictionary(i => i.Id, i => _mapper.Map<RetailerRecord>(i));
            var storeTypeLookup = storeTypesTask.Result.ToDictionary(i => i.Id, i => _mapper.Map<StoreTypeRecord>(i));

            var tasks = affectedStoresTask.Result.Select(async i =>
            {
                var success = false;

                try
                {
                    var storeView = _mapper.Map<StoreViewRecord>(i) with
                    {
                        Retailer = retailerLookup[i.RetailerId],
                        StoreType = storeTypeLookup[i.StoreTypeId]
                    };

                    var result = await _viewRepository.UpsertAsync(storeView);
                    success = result != null;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to update store view for '{i.Id}'");
                }

                return success;
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

    private async Task<List<StoreEntity>> GetAffectedStoresAsync(Guid? retailerId, Guid? storeTypeId, Guid? storeId)
    {
        const string retailerIdKey = "@retailerId";
        const string storeTypeIdKey = "@storeTypeId";
        const string storeIdKey = "@storeId";

        var conditions = new List<string>();
        var parameters = new Dictionary<string, string>();

        if (retailerId.HasValue)
        {
            conditions.Add($"c.retailerId = {retailerIdKey}");
            parameters.Add(retailerIdKey, retailerId.Value.ToString());
        }

        if (storeTypeId.HasValue)
        {
            conditions.Add($"c.storeTypeId = {storeTypeIdKey}");
            parameters.Add(storeTypeIdKey, storeTypeId.Value.ToString());
        }

        if (storeId.HasValue)
        {
            conditions.Add($"c.id = {storeIdKey}");
            parameters.Add(storeIdKey, storeId.Value.ToString());
        }

        if (!conditions.Any())
            return new List<StoreEntity>(0);

        var sqlQueryText = $"SELECT * FROM c WHERE {string.Join(" AND ", conditions)}";

        var affectedStores = await _storeViewRepository.QueryAsync(sqlQueryText, parameters);
        return affectedStores;
    }

    private static string FailedToMessage(UpdateStoreViewCommand command) =>
        $"Failed to update store view '{JsonConvert.SerializeObject(command)}'";
}