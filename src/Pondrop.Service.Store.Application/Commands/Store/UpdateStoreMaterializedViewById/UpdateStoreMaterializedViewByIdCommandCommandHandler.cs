using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class UpdateStoreMaterializedViewByIdCommandCommandHandler : IRequestHandler<UpdateStoreMaterializedViewByIdCommand, Result<StoreRecord>>
{
    private readonly IEventRepository _eventRepository;
    private readonly IMaterializedViewRepository<StoreEntity> _storeMaterializedViewRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<UpdateStoreMaterializedViewByIdCommand> _validator;    
    private readonly ILogger<UpdateStoreMaterializedViewByIdCommandCommandHandler> _logger;

    public UpdateStoreMaterializedViewByIdCommandCommandHandler(
        IEventRepository eventRepository,
        IMaterializedViewRepository<StoreEntity> storeMaterializedViewRepository,
        IMapper mapper,
        IValidator<UpdateStoreMaterializedViewByIdCommand> validator,
        ILogger<UpdateStoreMaterializedViewByIdCommandCommandHandler> logger)
    {
        _eventRepository = eventRepository;
        _storeMaterializedViewRepository = storeMaterializedViewRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<StoreRecord>> Handle(UpdateStoreMaterializedViewByIdCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Update store materialized view failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<StoreRecord>.Error(errorMessage);
        }

        var result = default(Result<StoreRecord>);

        try
        {
            var storeStream = await _eventRepository.LoadStreamAsync(EventEntity.GetStreamId<StoreEntity>(command.Id));

            if (storeStream.Version >= 0)
            {
                var storeEntity = new StoreEntity(storeStream.Events);
                storeEntity = await _storeMaterializedViewRepository.ReplaceAsync(storeEntity);

                result = storeEntity is not null && storeEntity.Id == command.Id
                    ? Result<StoreRecord>.Success(_mapper.Map<StoreRecord>(storeEntity))
                    : Result<StoreRecord>.Error(FailedToMessage(command));
            }
            else
            {
                result = Result<StoreRecord>.Error($"Store does not exist '{command.Id}'");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToMessage(command));
            result = Result<StoreRecord>.Error(ex);
        }

        return result;
    }
    
    private static string FailedToMessage(UpdateStoreMaterializedViewByIdCommand byIdCommand) =>
        $"Failed to update materialized store '{byIdCommand.Id}'";
}