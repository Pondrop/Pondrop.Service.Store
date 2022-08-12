using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class UpdateRetailerCheckpointByIdCommandHandler : UpdateCheckpointByIdCommandHandler<UpdateRetailerCheckpointByIdCommand, RetailerEntity, RetailerRecord>
{
    public UpdateRetailerCheckpointByIdCommandHandler(
        IEventRepository eventRepository,
        ICheckpointRepository<RetailerEntity> retailerCheckpointRepository,
        IMapper mapper,
        IValidator<UpdateCheckpointByIdCommand> validator,
        ILogger<UpdateRetailerCheckpointByIdCommandHandler> logger) : base(eventRepository, retailerCheckpointRepository, mapper, validator, logger)
    {
    }
}