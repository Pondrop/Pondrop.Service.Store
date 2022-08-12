using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class UpdateStoreCheckpointByIdCommandHandler : UpdateCheckpointByIdCommandHandler<UpdateStoreCheckpointByIdCommand, StoreEntity, StoreRecord>
{
    public UpdateStoreCheckpointByIdCommandHandler(
        IEventRepository eventRepository,
        ICheckpointRepository<StoreEntity> storeCheckpointRepository,
        IMapper mapper,
        IValidator<UpdateCheckpointByIdCommand> validator,
        ILogger<UpdateStoreCheckpointByIdCommandHandler> logger) : base(eventRepository, storeCheckpointRepository, mapper, validator, logger)
    {
    }
}