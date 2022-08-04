using AutoMapper;
using Pondrop.Service.Store.Application.Commands;
using Pondrop.Service.Store.Domain.Models;
using Pondrop.Service.Store.Infrastructure.Models;

namespace Pondrop.Service.Store.Infrastructure.Mapper;

public class ExampleProfile : Profile
{
    public ExampleProfile()
    {
        CreateMap<TodoItemEntity, TodoItem>().ReverseMap();
        CreateMap<TodoItemEntity, CreateTodoCommand>().ReverseMap();
    }
}
