using AutoMapper;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Domain.Mapper;

public class StoreProfile : Profile
{
    public StoreProfile()
    {
        CreateMap<StoreEntity, StoreRecord>();
        CreateMap<StoreEntity, StoreViewRecord>();
        CreateMap<StoreEntity, StoreSearchIndexViewRecord>();
    }
}
