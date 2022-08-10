using MediatR;
using Pondrop.Service.Store.Application.Models;

namespace Pondrop.Service.Store.Application.Commands;

public abstract class RebuildCheckpointCommand : IRequest<Result<int>> 
{
}