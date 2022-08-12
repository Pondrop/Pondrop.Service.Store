﻿using FluentValidation;
using Pondrop.Service.Store.Application.Interfaces.Services;

namespace Pondrop.Service.Store.Application.Commands;

public class RemoveAddressFromStoreCommandHandlerValidator : AbstractValidator<RemoveAddressFromStoreCommand>
{
    public RemoveAddressFromStoreCommandHandlerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.StoreId).NotEmpty();
    }
}