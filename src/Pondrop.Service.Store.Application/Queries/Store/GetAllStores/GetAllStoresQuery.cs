﻿using MediatR;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Application.Queries;

public class GetAllStoresQuery : IRequest<Result<List<StoreViewRecord>>>
{
    public int Offset { get; set; }

    public int Limit { get; set; }
}