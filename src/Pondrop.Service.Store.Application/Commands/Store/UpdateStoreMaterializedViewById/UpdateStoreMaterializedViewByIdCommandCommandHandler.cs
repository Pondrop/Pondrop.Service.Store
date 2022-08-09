using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class UpdateStoreMaterializedViewByIdCommandCommandHandler : UpdateMaterializedViewByIdCommandCommandHandler<UpdateStoreMaterializedViewByIdCommand, StoreEntity, StoreRecord>
{
    public UpdateStoreMaterializedViewByIdCommandCommandHandler(
        IEventRepository eventRepository,
        IMaterializedViewRepository<StoreEntity> storeTypeViewRepository,
        IMapper mapper,
        IValidator<UpdateMaterializedViewByIdCommand> validator,
        ILogger<UpdateStoreMaterializedViewByIdCommandCommandHandler> logger) : base(eventRepository, storeTypeViewRepository, mapper, validator, logger)
    {
    }
}