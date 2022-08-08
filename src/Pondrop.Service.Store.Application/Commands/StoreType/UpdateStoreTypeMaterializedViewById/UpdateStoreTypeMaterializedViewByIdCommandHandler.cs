using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class UpdateStoreTypeMaterializedViewByIdCommandHandler : IRequestHandler<UpdateStoreTypeMaterializedViewByIdCommand, Result<StoreTypeRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IMaterializedViewRepository<StoreTypeEntity> _storeTypeMaterializedViewRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<UpdateStoreTypeMaterializedViewByIdCommand> _validator;    
    private readonly ILogger<UpdateStoreTypeMaterializedViewByIdCommandHandler> _logger;

    public UpdateStoreTypeMaterializedViewByIdCommandHandler(
        IEventRepository eventRepository,
        IMaterializedViewRepository<StoreTypeEntity> storeTypeMaterializedViewRepository,
        IMapper mapper,
        IValidator<UpdateStoreTypeMaterializedViewByIdCommand> validator,
        ILogger<UpdateStoreTypeMaterializedViewByIdCommandHandler> logger)
    {
        _eventRepository = eventRepository;
        _storeTypeMaterializedViewRepository = storeTypeMaterializedViewRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<StoreTypeRecord>> Handle(UpdateStoreTypeMaterializedViewByIdCommand byIdCommand, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(byIdCommand);

        if (!validation.IsValid)
        {
            var errorMessage = $"Update store type materialized view failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<StoreTypeRecord>.Error(errorMessage);
        }

        var result = default(Result<StoreTypeRecord>);

        try
        {
            var storeTypeStream = await _eventRepository.LoadStreamAsync(EventEntity.GetStreamId<StoreTypeEntity>(byIdCommand.Id));

            if (storeTypeStream.Version >= 0)
            {
                var storeTypeEntity = new StoreTypeEntity(storeTypeStream.Events);
                storeTypeEntity = await _storeTypeMaterializedViewRepository.ReplaceAsync(storeTypeEntity);

                result = storeTypeEntity is not null && storeTypeEntity.Id == byIdCommand.Id
                    ? Result<StoreTypeRecord>.Success(_mapper.Map<StoreTypeRecord>(storeTypeEntity))
                    : Result<StoreTypeRecord>.Error(FailedToMessage(byIdCommand));
            }
            else
            {
                result = Result<StoreTypeRecord>.Error($"Store type does not exist '{byIdCommand.Id}'");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToMessage(byIdCommand));
            result = Result<StoreTypeRecord>.Error(ex);
        }

        return result;
    }
    
    private static string FailedToMessage(UpdateStoreTypeMaterializedViewByIdCommand byIdCommand) =>
        $"Failed to update materialized store type '{byIdCommand.Id}'";
}