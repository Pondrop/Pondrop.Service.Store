using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Queries;
using Pondrop.Service.Store.Domain.Models;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace Pondrop.Service.Store.Application.Tests.Queries;

public class GetTodoByCategoryQueryHandlerTests
{
    private readonly Mock<ILogger<GetTodoByCategoryQueryHandler>> _loggerMock;
        
    public GetTodoByCategoryQueryHandlerTests()
    {
        _loggerMock = new Mock<ILogger<GetTodoByCategoryQueryHandler>>();
    }
    
    [Fact]
    public async void RequestWithNoCategory_ShouldReturn_InvalidInput()
    {
        // arrange
        var validator = new GetTodoByCategoryQueryHandlerValidator();
        var repoMock = new Mock<ITodoRepository>();

        // act
        var handler = new GetTodoByCategoryQueryHandler(
            repoMock.Object,
            validator,
            _loggerMock.Object
        );
        var result = await handler.Handle(new GetTodoByCategoryQuery(), new CancellationToken());

        // assert
        Assert.False(result.IsSuccess);
    }
    
    [Fact]
    public async void Request_ShouldCall_QueryItems()
    {
        // arrange
        var items = new List<TodoItem>(0);
        var validator = new GetTodoByCategoryQueryHandlerValidator();
        var repoMock = new Mock<ITodoRepository>();
        repoMock.Setup(i => i.QueryItemsAsync(
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, string>>())).ReturnsAsync(items);
        
        var query = new GetTodoByCategoryQuery()
        {
            Category = "shopping"
        };

        // act
        var handler = new GetTodoByCategoryQueryHandler(
            repoMock.Object,
            validator,
            _loggerMock.Object
        );
        var result = await handler.Handle(query, new CancellationToken());

        // assert
        Assert.Equal(items, result.Value);
        repoMock.Verify(i => 
            i.QueryItemsAsync(
                It.IsAny<string>(), 
                It.IsAny<Dictionary<string, string>>()), Times.Once);
    }
}