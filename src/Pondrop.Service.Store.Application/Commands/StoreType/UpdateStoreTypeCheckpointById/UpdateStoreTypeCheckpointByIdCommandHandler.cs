using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Interfaces;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class UpdateStoreTypeCheckpointByIdCommandHandler : UpdateCheckpointByIdCommandHandler<UpdateStoreTypeCheckpointByIdCommand, StoreTypeEntity, StoreTypeRecord>
{
    public UpdateStoreTypeCheckpointByIdCommandHandler(
        IEventRepository eventRepository,
        ICheckpointRepository<StoreTypeEntity> storeCheckpointRepository,
        IMapper mapper,
        IValidator<UpdateCheckpointByIdCommand> validator,
        ILogger<UpdateStoreTypeCheckpointByIdCommandHandler> logger) : base(eventRepository, storeCheckpointRepository, mapper, validator, logger)
    {
    }
}