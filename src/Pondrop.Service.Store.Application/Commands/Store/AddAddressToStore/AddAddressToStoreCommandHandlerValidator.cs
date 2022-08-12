using FluentValidation;
using Pondrop.Service.Store.Application.Interfaces.Services;

namespace Pondrop.Service.Store.Application.Commands;

public class AddAddressToStoreCommandHandlerValidator : AbstractValidator<AddAddressToStoreCommand>
{
    private readonly IAddressService _addressService;
    
    public AddAddressToStoreCommandHandlerValidator(IAddressService addressService)
    {
        _addressService = addressService;
        
        RuleFor(x => x.StoreId).NotEmpty();
        RuleFor(x => x.ExternalReferenceId).NotEmpty();
        RuleFor(x => x.AddressLine1).NotEmpty();
        RuleFor(x => x.Suburb).NotEmpty();
        RuleFor(x => x.Postcode).Must(x => _addressService.IsValidAustralianPostcode(x));
        RuleFor(x => x.State).Must(x => _addressService.IsValidAustralianState(x));
        RuleFor(x => x.Country).Must(x => _addressService.IsValidAustralianCountry(x));
        RuleFor(x => x.Latitude).Must(x => x != 0);
        RuleFor(x => x.Longitude).Must(x => x != 0);
    }
}