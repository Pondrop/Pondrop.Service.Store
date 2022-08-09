using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class UpdateStoreTypeMaterializedViewByIdCommandHandler : UpdateMaterializedViewByIdCommandCommandHandler<UpdateStoreTypeMaterializedViewByIdCommand, StoreTypeEntity, StoreTypeRecord>
{
    public UpdateStoreTypeMaterializedViewByIdCommandHandler(
        IEventRepository eventRepository,
        IMaterializedViewRepository<StoreTypeEntity> storeTypeViewRepository,
        IMapper mapper,
        IValidator<UpdateMaterializedViewByIdCommand> validator,
        ILogger<UpdateStoreTypeMaterializedViewByIdCommandHandler> logger) : base(eventRepository, storeTypeViewRepository, mapper, validator, logger)
    {
    }
}