using FluentValidation;
using Pondrop.Service.Store.Application.Interfaces.Services;

namespace Pondrop.Service.Store.Application.Commands;

public class CreateStoreCommandHandlerValidator : AbstractValidator<CreateStoreCommand>
{
    private readonly IAddressService _addressService;
    
    public CreateStoreCommandHandlerValidator(IAddressService addressService)
    {
        _addressService = addressService;
        
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Status).NotEmpty();
        RuleFor(x => x.ExternalReferenceId).NotEmpty();
        RuleFor(x => x.Phone).NotEmpty();
        RuleFor(x => x.Email).NotEmpty();
        RuleFor(x => x.OpenHours).NotEmpty();

        RuleFor(x => x.Address).NotNull();
        RuleFor(x => x.Address!.ExternalReferenceId).NotEmpty();
        RuleFor(x => x.Address!.AddressLine1).NotEmpty();
        RuleFor(x => x.Address!.Suburb).NotEmpty();
        RuleFor(x => x.Address!.Postcode).Must(x => _addressService.IsValidAustralianPostcode(x));
        RuleFor(x => x.Address!.State).Must(x => _addressService.IsValidAustralianState(x));
        RuleFor(x => x.Address!.Country).Must(x => _addressService.IsValidAustralianCountry(x));
        RuleFor(x => x.Address!.Latitude).Must(x => x != 0);
        RuleFor(x => x.Address!.Longitude).Must(x => x != 0);
        
        RuleFor(x => x.RetailerId).NotEmpty();
        RuleFor(x => x.StoreTypeId).NotEmpty();
    }
}