using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class UpdateRetailerMaterializedViewByIdCommandHandler : UpdateMaterializedViewByIdCommandCommandHandler<UpdateRetailerMaterializedViewByIdCommand, RetailerEntity, RetailerRecord>
{
    public UpdateRetailerMaterializedViewByIdCommandHandler(
        IEventRepository eventRepository,
        IMaterializedViewRepository<RetailerEntity> retailerTypeViewRepository,
        IMapper mapper,
        IValidator<UpdateMaterializedViewByIdCommand> validator,
        ILogger<UpdateRetailerMaterializedViewByIdCommandHandler> logger) : base(eventRepository, retailerTypeViewRepository, mapper, validator, logger)
    {
    }
}