using MediatR;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Commands;

public class UpdateRetailerMaterializedViewByIdCommand : UpdateMaterializedViewByIdCommand<Result<RetailerRecord>>
{
}