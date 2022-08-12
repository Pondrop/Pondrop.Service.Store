using Pondrop.Service.Store.Application.Commands;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Interfaces;

public interface ICheckpointRepository<T> : IContainerRepository<T> where T : EventEntity
{
    Task<int> RebuildAsync();
    Task<T?> UpsertAsync(long expectedVersion, T item);

    Task FastForwardAsync(T item);
}