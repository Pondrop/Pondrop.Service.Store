using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class UpdateMaterializedViewByIdCommandCommandHandler<TCommand, TEntity, TRecord> : IRequestHandler<TCommand, Result<TRecord>>
    where TCommand : UpdateMaterializedViewByIdCommand, IRequest<Result<TRecord>>
    where TEntity : EventEntity, new()
{
    private readonly IEventRepository _eventRepository;
    private readonly IMaterializedViewRepository<TEntity> _typeViewRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<UpdateMaterializedViewByIdCommand> _validator;
    private readonly ILogger _logger;

    public UpdateMaterializedViewByIdCommandCommandHandler(
        IEventRepository eventRepository,
        IMaterializedViewRepository<TEntity> typeViewRepository,
        IMapper mapper,
        IValidator<UpdateMaterializedViewByIdCommand> validator,
        ILogger logger)
    {
        _eventRepository = eventRepository;
        _typeViewRepository = typeViewRepository;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<TRecord>> Handle(TCommand command, CancellationToken cancellationToken)
    {
        var validation = _validator.Validate(command);

        if (!validation.IsValid)
        {
            var errorMessage = $"Update {typeof(TEntity).Name} materialized view failed, errors on validation {validation}";
            _logger.LogError(errorMessage);
            return Result<TRecord>.Error(errorMessage);
        }

        var result = default(Result<TRecord>);

        try
        {
            var entity = await _typeViewRepository.GetByIdAsync(command.Id);
            var expectedVersion = entity?.AtSequence ?? -1;
            var stream = await _eventRepository.LoadStreamAsync(EventEntity.GetStreamId<TEntity>(command.Id), expectedVersion + 1);

            if (stream.Events.Any())
            {
                entity ??= new TEntity();
                entity.Apply(stream.Events);
                
                entity = await _typeViewRepository.UpsertAsync(Math.Max(0, expectedVersion), entity);

                result = entity is not null
                    ? Result<TRecord>.Success(_mapper.Map<TRecord>(entity))
                    : Result<TRecord>.Error(FailedToMessage(command));
            }
            else if (entity is not null)
            {
                result = Result<TRecord>.Success(_mapper.Map<TRecord>(entity));
            }
            else
            {
                result = Result<TRecord>.Error($"{typeof(TEntity).Name} does not exist '{command.Id}'");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToMessage(command));
            result = Result<TRecord>.Error(ex);
        }

        return result;
    }
    
    private static string FailedToMessage(TCommand byIdCommand) =>
        $"Failed to update materialized {typeof(TEntity).Name} '{byIdCommand.Id}'";
}