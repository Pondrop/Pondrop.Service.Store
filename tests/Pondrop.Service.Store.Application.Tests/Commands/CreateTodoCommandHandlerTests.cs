using Microsoft.Extensions.Logging;
using Moq;
using Pondrop.Service.Store.Application.Commands;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Queries;
using Pondrop.Service.Store.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace Pondrop.Service.Store.Application.Tests.Commands;

public class CreateTodoCommandHandlerTests
{
    private readonly Mock<ILogger<CreateTodoCommandHandler>> _loggerMock;
        
    public CreateTodoCommandHandlerTests()
    {
        _loggerMock = new Mock<ILogger<CreateTodoCommandHandler>>();
    }
    
    [Fact]
    public async void EmptyCommand_ShouldReturn_InvalidInput()
    {
        // arrange
        var validator = new CreateTodoCommandHandlerValidator();
        var repoMock = new Mock<ITodoRepository>();

        // act
        var handler = new CreateTodoCommandHandler(
            repoMock.Object,
            validator,
            _loggerMock.Object
        );
        var result = await handler.Handle(new CreateTodoCommand(), new CancellationToken());

        // assert
        Assert.False(result.IsSuccess);
    }
    
    [Fact]
    public async void ValidCommand_ShouldCall_AddItem()
    {
        // arrange
        var cmd = new CreateTodoCommand()
        {
            Name = "First item to complete",
            Description = "My new awesome todo item!",
            Category = "shopping"
        };
        var todo = new TodoItem()
        {
            Id = Guid.NewGuid().ToString(),
            Name = cmd.Name,
            Description = cmd.Description,
            Category = cmd.Category
        };
        var validator = new CreateTodoCommandHandlerValidator();
        var repoMock = new Mock<ITodoRepository>();
        repoMock.Setup(i => i.AddItemAsync(cmd)).ReturnsAsync(todo);
        
        // act
        var handler = new CreateTodoCommandHandler(
            repoMock.Object,
            validator,
            _loggerMock.Object
        );
        var result = await handler.Handle(cmd, new CancellationToken());

        // assert
        Assert.Equal(todo, result.Value);
        repoMock.Verify(i => i.AddItemAsync(It.IsAny<CreateTodoCommand>()), Times.Once);
    }
}