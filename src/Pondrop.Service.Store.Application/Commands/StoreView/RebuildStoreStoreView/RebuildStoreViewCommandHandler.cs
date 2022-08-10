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

public class RebuildStoreViewCommandHandler : IRequestHandler<RebuildStoreViewCommand, Result<int>>
{
    private readonly IMaterializedViewRepository<RetailerEntity> _retailerViewRepository;
    private readonly IMaterializedViewRepository<StoreTypeEntity> _storeTypeViewRepository;
    private readonly IMaterializedViewRepository<StoreEntity> _storeViewRepository;
    private readonly IViewRepository<StoreViewRecord> _viewRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly ILogger<UpdateStoreViewCommandHandler> _logger;

    public RebuildStoreViewCommandHandler(
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

    public async Task<Result<int>> Handle(RebuildStoreViewCommand command, CancellationToken cancellationToken)
    {
        var result = default(Result<int>);

        try
        {
            var retailersTask = _retailerViewRepository.GetAllAsync();
            var storeTypesTask = _storeTypeViewRepository.GetAllAsync();
            var storesTask = _storeViewRepository.GetAllAsync();

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
            _logger.LogError(ex, $"Failed to rebuild store view");
            result = Result<int>.Error(ex);
        }

        return result;
    }
}