using AutoMapper;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Domain.Mapper;

public class RetailerProfile : Profile
{
    public RetailerProfile()
    {
        CreateMap<RetailerEntity, RetailerRecord>();
    }
}
