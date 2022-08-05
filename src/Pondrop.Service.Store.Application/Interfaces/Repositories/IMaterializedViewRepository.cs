using Pondrop.Service.Store.Application.Commands;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Interfaces;

public interface IMaterializedViewRepository<T> where T : EventEntity
{
    Task<bool> IsConnectedAsync();
    
    Task<int> RebuildAsync();
    Task<T?> UpsertAsync(T item);
    
    Task<List<T>> GetAllAsync();
    Task<T?> GetByIdAsync(Guid id);
}