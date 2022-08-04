using AutoMapper;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Domain.Mapper;

public class ShoppingListProfile : Profile
{
    public ShoppingListProfile()
    {
        CreateMap<ShoppingListItemEntity, ShoppingListItemRecord>().ReverseMap();
        CreateMap<ShoppingListEntity, ShoppingListRecord>().ReverseMap();
    }
}
