using AutoMapper;
using Pondrop.Service.Store.Domain.Models;

namespace Pondrop.Service.Store.Domain.Mapper;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<UserEntity, UserRecord>().ReverseMap();
    }
}
