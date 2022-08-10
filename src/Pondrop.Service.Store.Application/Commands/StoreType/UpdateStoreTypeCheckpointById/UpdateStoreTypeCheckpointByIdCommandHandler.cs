using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Models;
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