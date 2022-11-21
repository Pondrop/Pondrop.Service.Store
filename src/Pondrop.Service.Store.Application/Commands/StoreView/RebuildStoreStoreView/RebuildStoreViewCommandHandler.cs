using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Interfaces.Services;

namespace Pondrop.Service.Store.Application.Commands;

public class RebuildStoreViewCommandHandler : IRequestHandler<RebuildStoreViewCommand, Result<int>>
{
    private readonly ICheckpointRepository<RetailerEntity> _retailerCheckpointRepository;
    private readonly ICheckpointRepository<StoreTypeEntity> _storeTypeCheckpointRepository;
    private readonly ICheckpointRepository<StoreEntity> _storeCheckpointRepository;
    private readonly IContainerRepository<StoreViewRecord> _containerRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly ILogger<RebuildStoreViewCommandHandler> _logger;

    public RebuildStoreViewCommandHandler(
        ICheckpointRepository<RetailerEntity> retailerCheckpointRepository,
        ICheckpointRepository<StoreTypeEntity> storeTypeCheckpointRepository,
        ICheckpointRepository<StoreEntity> storeCheckpointRepository,
        IContainerRepository<StoreViewRecord> containerRepository,
        IMapper mapper,
        IUserService userService,
        ILogger<RebuildStoreViewCommandHandler> logger) : base()
    {
        _retailerCheckpointRepository = retailerCheckpointRepository;
        _storeTypeCheckpointRepository = storeTypeCheckpointRepository;
        _storeCheckpointRepository = storeCheckpointRepository;
        _containerRepository = containerRepository;
        _mapper = mapper;
        _userService = userService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(RebuildStoreViewCommand command, CancellationToken cancellationToken)
    {
        var result = default(Result<int>);

        try
        {
            var retailersTask = _retailerCheckpointRepository.GetAllAsync();
            var storeTypesTask = _storeTypeCheckpointRepository.GetAllAsync();
            var storesTask = _storeCheckpointRepository.GetAllAsync();

            await Task.WhenAll(retailersTask, storeTypesTask, storesTask);

            var retailerLookup = retailersTask.Result.ToDictionary(i => i.Id, i => _mapper.Map<RetailerRecord>(i));
            var storeTypeLookup = storeTypesTask.Result.ToDictionary(i => i.Id, i => _mapper.Map<StoreTypeRecord>(i));

            var tasks = storesTask.Result.Select(async i =>
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
            _logger.LogError(ex, $"Failed to rebuild store view");
            result = Result<int>.Error(ex);
        }

        return result;
    }
}