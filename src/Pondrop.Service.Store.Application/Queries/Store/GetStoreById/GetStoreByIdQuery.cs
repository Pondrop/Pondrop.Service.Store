﻿using MediatR;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Queries;

public class GetStoreByIdQuery : IRequest<Result<StoreViewRecord?>>
{
    public Guid Id { get; init; } = Guid.Empty;
}