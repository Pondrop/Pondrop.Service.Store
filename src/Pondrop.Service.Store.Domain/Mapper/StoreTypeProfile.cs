using AutoMapper;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Domain.Mapper;

public class StoreTypeProfile : Profile
{
    public StoreTypeProfile()
    {
        CreateMap<StoreTypeEntity, StoreTypeRecord>();
    }
}
