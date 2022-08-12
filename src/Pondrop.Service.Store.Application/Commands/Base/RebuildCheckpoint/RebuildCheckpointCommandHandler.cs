using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class RebuildCheckpointCommandHandler<TCommand, TEntity> : IRequestHandler<TCommand, Result<int>>
    where TCommand : RebuildCheckpointCommand
    where TEntity : EventEntity
{
    private readonly ICheckpointRepository<TEntity> _checkpointRepository;
    private readonly ILogger _logger;

    public RebuildCheckpointCommandHandler(
        ICheckpointRepository<TEntity> checkpointRepository,
        ILogger logger)
    {
        _checkpointRepository = checkpointRepository;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(TCommand command, CancellationToken cancellationToken)
    {
        var result = default(Result<int>);

        try
        {
            var count = await _checkpointRepository.RebuildAsync();
            result = count >= 0
                ? Result<int>.Success(count)
                : Result<int>.Error(FailedToMessage());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToMessage());
            result = Result<int>.Error(ex);
        }

        return result;
    }
    
    private static string FailedToMessage() =>
        $"Failed to rebuild {typeof(TEntity).Name} checkpoint";
}