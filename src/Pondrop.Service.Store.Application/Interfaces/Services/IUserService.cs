namespace Pondrop.Service.Store.Application.Interfaces.Services;

public interface IUserService
{
    string CurrentUserName();
    string GetMaterializedViewUserName();
}