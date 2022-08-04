using System.Threading;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;
using Moq;
using Pondrop.Service.Store.ApiControllers;
using Pondrop.Service.Store.Application.Commands;
using Pondrop.Service.Store.Application.Queries;
using System;
using System.Collections.Generic;
using Xunit;

namespace Pondrop.Service.Store.Api.Tests
{
    public class TodoControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ILogger<TodoController>> _loggerMock;
        
        public TodoControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _loggerMock = new Mock<ILogger<TodoController>>();
        }

        [Fact]
        public async void GetItemsByCategory_ShouldReturnOkResult()
        {
            // arrange
            var items = new List<TodoItem>(0);
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetTodoByCategoryQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<List<TodoItem>>.Success(items));
            var controller = new TodoController(
                _mediatorMock.Object,
                _loggerMock.Object
            );

            // act
            var response = await controller.GetItemsByCategory("shopping");

            // assert
            Assert.IsType<OkObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, items);
            _mediatorMock.Verify(x => x.Send(It.IsAny<GetTodoByCategoryQuery>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void GetItemsByCategory_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<List<TodoItem>>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetTodoByCategoryQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = new TodoController(
                _mediatorMock.Object,
                _loggerMock.Object
            );

            // act
            var response = await controller.GetItemsByCategory(string.Empty);

            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<GetTodoByCategoryQuery>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void GetTodoByIdQuery_ShouldReturnOkResult()
        {
            // arrange
            var id = Guid.NewGuid();
            var todo = new TodoItem()
            {
                Id = id.ToString(),
                Category = "shopping"
            };
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetTodoByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<TodoItem>.Success(todo));
            var controller = new TodoController(
                _mediatorMock.Object,
                _loggerMock.Object
            );

            // act
            var response = await controller.GetSingleItem(todo.Category, id);

            // assert
            Assert.IsType<OkObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, todo);
            _mediatorMock.Verify(x => x.Send(It.IsAny<GetTodoByIdQuery>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void GetTodoByIdQuery_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<TodoItem>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetTodoByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = new TodoController(
                _mediatorMock.Object,
                _loggerMock.Object
            );

            // act
            var response = await controller.GetSingleItem(string.Empty, Guid.Empty);

            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<GetTodoByIdQuery>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void CreateTodoCommand_ShouldReturnOkResult()
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
            _mediatorMock
                .Setup(x => x.Send(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<TodoItem>.Success(todo));
            var controller = new TodoController(
                _mediatorMock.Object,
                _loggerMock.Object
            );

            // act
            var response = await controller.CreateTodoCommand(cmd);

            // assert
            Assert.IsType<OkObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, todo);
            _mediatorMock.Verify(x => x.Send(cmd, It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void CreateTodoCommand_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<TodoItem>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<CreateTodoCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = new TodoController(
                _mediatorMock.Object,
                _loggerMock.Object
            );

            // act
            var response = await controller.CreateTodoCommand(new CreateTodoCommand());

            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<CreateTodoCommand>(), It.IsAny<CancellationToken>()), Times.Once());
        }
    }
}
