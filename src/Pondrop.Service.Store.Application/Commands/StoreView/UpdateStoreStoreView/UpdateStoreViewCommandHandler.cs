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

public class UpdateStoreViewCommandHandler : IRequestHandler<UpdateStoreViewCommand, Result<List<SubmissionStoreViewRecord>>>
{
    private readonly ICheckpointRepository<RetailerEntity> _retailerCheckpointRepository;
    private readonly ICheckpointRepository<StoreTypeEntity> _storeTypeCheckpointRepository;
    private readonly ICheckpointRepository<StoreEntity> _storeCheckpointRepository;
    private readonly IContainerRepository<StoreViewRecord> _containerRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly ILogger<UpdateStoreViewCommandHandler> _logger;

    public UpdateStoreViewCommandHandler(
        ICheckpointRepository<RetailerEntity> retailerCheckpointRepository,
        ICheckpointRepository<StoreTypeEntity> storeTypeCheckpointRepository,
        ICheckpointRepository<StoreEntity> storeCheckpointRepository,
        IContainerRepository<StoreViewRecord> containerRepository,
        IMapper mapper,
        IUserService userService,
        ILogger<UpdateStoreViewCommandHandler> logger) : base()
    {
        _retailerCheckpointRepository = retailerCheckpointRepository;
        _storeTypeCheckpointRepository = storeTypeCheckpointRepository;
        _storeCheckpointRepository = storeCheckpointRepository;
        _containerRepository = containerRepository;
        _mapper = mapper;
        _userService = userService;
        _logger = logger;
    }

    public async Task<Result<List<SubmissionStoreViewRecord>>> Handle(UpdateStoreViewCommand command, CancellationToken cancellationToken)
    {
        if (!command.StoreId.HasValue && !command.RetailerId.HasValue && !command.StoreTypeId.HasValue)
            return Result<List<SubmissionStoreViewRecord>>.Success(new List<SubmissionStoreViewRecord>());

        var result = default(Result<List<SubmissionStoreViewRecord>>);

        try
        {
            var retailersTask = _retailerCheckpointRepository.GetAllAsync();
            var storeTypesTask = _storeTypeCheckpointRepository.GetAllAsync();
            var affectedStoresTask = GetAffectedStoresAsync(command.RetailerId, command.StoreTypeId, command.StoreId);

            await Task.WhenAll(retailersTask, storeTypesTask, affectedStoresTask);

            var retailerLookup = retailersTask.Result.ToDictionary(i => i.Id, i => _mapper.Map<RetailerRecord>(i));
            var storeTypeLookup = storeTypesTask.Result.ToDictionary(i => i.Id, i => _mapper.Map<StoreTypeRecord>(i));

            var stores = new List<SubmissionStoreViewRecord>();

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

                    var result = await _containerRepository.UpsertAsync(storeView);
                    success = result != null;

                    if (success)
                        stores.Add(new SubmissionStoreViewRecord()
                        {
                            StoreId = i.Id,
                            Name = i.Name,
                            RetailerName = storeView.Retailer.Name
                        });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to update store view for '{i.Id}'");
                }

                return success;
            }).ToList();

            await Task.WhenAll(tasks);

            result = Result<List<SubmissionStoreViewRecord>>.Success(stores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToMessage(command));
            result = Result<List<SubmissionStoreViewRecord>>.Error(ex);
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

        var affectedStores = await _storeCheckpointRepository.QueryAsync(sqlQueryText, parameters);
        return affectedStores;
    }

    private static string FailedToMessage(UpdateStoreViewCommand command) =>
        $"Failed to update store view '{JsonConvert.SerializeObject(command)}'";
}