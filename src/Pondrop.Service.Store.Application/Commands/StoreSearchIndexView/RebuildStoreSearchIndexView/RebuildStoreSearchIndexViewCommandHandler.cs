using AutoMapper;
using MediatR;
using Microsoft.Azure.Cosmos.Spatial;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Interfaces.Services;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class RebuildStoreSearchIndexViewCommandHandler : IRequestHandler<RebuildStoreSearchIndexViewCommand, Result<int>>
{
    private readonly ICheckpointRepository<RetailerEntity> _retailerCheckpointRepository;
    private readonly ICheckpointRepository<StoreTypeEntity> _storeTypeCheckpointRepository;
    private readonly ICheckpointRepository<StoreEntity> _storeCheckpointRepository;
    private readonly IContainerRepository<StoreSearchIndexViewRecord> _containerRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly ILogger<RebuildStoreSearchIndexViewCommandHandler> _logger;

    public RebuildStoreSearchIndexViewCommandHandler(
        ICheckpointRepository<RetailerEntity> retailerCheckpointRepository,
        ICheckpointRepository<StoreTypeEntity> storeTypeCheckpointRepository,
        ICheckpointRepository<StoreEntity> storeCheckpointRepository,
        IContainerRepository<StoreSearchIndexViewRecord> containerRepository,
        IMapper mapper,
        IUserService userService,
        ILogger<RebuildStoreSearchIndexViewCommandHandler> logger) : base()
    {
        _retailerCheckpointRepository = retailerCheckpointRepository;
        _storeTypeCheckpointRepository = storeTypeCheckpointRepository;
        _storeCheckpointRepository = storeCheckpointRepository;
        _containerRepository = containerRepository;
        _mapper = mapper;
        _userService = userService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(RebuildStoreSearchIndexViewCommand command, CancellationToken cancellationToken)
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
                    var storeView = _mapper.Map<StoreSearchIndexViewRecord>(i) with
                    {
                        Retailer = retailerLookup[i.RetailerId],
                        StoreType = storeTypeLookup[i.StoreTypeId],
                        AddressId = i.Addresses.FirstOrDefault()?.Id ?? Guid.Empty,
                        AddressExternalReferenceId = i.Addresses.FirstOrDefault()?.ExternalReferenceId ?? string.Empty,
                        AddressLine1 = i.Addresses.FirstOrDefault()?.AddressLine1 ?? string.Empty,
                        AddressLine2 = i.Addresses.FirstOrDefault()?.AddressLine2 ?? string.Empty,
                        Suburb = i.Addresses.FirstOrDefault()?.Suburb ?? string.Empty,
                        IsCommunityStore = i.IsCommunityStore,
                        State = i.Addresses.FirstOrDefault()?.State ?? string.Empty,
                        Postcode = i.Addresses.FirstOrDefault()?.Postcode ?? string.Empty,
                        Country = i.Addresses.FirstOrDefault()?.Country ?? string.Empty,
                        Latitude = i.Addresses.FirstOrDefault()?.Latitude ?? 0,
                        Longitude = i.Addresses.FirstOrDefault()?.Longitude ?? 0,
                        LocationSort = i.Addresses.FirstOrDefault()?.LocationSort ?? new Point(0,0),
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