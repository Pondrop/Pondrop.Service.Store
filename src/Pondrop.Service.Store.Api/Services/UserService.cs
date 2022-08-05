using Pondrop.Service.Store.Application.Interfaces.Services;

namespace Pondrop.Service.Store.Api.Services;

public class UserService : IUserService
{
    public string CurrentUserName() => "admin";
}