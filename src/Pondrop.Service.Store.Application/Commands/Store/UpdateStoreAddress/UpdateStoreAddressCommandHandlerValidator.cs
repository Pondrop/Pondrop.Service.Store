using FluentValidation;
using Pondrop.Service.Interfaces.Services;

namespace Pondrop.Service.Store.Application.Commands;

public class UpdateStoreAddressCommandHandlerValidator : AbstractValidator<UpdateStoreAddressCommand>
{
    private readonly IAddressService _addressService;
    
    public UpdateStoreAddressCommandHandlerValidator(IAddressService addressService)
    {
        _addressService = addressService;
        
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.StoreId).NotEmpty();
        //RuleFor(x => x.AddressLine1).Must(x => x is null || !string.IsNullOrEmpty(x));
        //RuleFor(x => x.Suburb).Must(x => x is null || !string.IsNullOrEmpty(x));
        //RuleFor(x => x.Postcode).Must(x => x is null || _addressService.IsValidAustralianPostcode(x));
        //RuleFor(x => x.State).Must(x => x is null ||_addressService.IsValidAustralianState(x));
        //RuleFor(x => x.Country).Must(x => x is null ||_addressService.IsValidAustralianCountry(x));
        //RuleFor(x => x.Latitude).Must(x => x is null || x != 0);
        //RuleFor(x => x.Longitude).Must(x => x is null || x != 0);
    }
}