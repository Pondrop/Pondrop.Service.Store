using System.Threading;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pondrop.Service.Store.Application.Models;
using Pondrop.Service.Store.Domain.Models;
using Moq;
using Pondrop.Service.Store.Api.Services;
using Pondrop.Service.Store.ApiControllers;
using Pondrop.Service.Store.Application.Commands;
using Pondrop.Service.Store.Application.Queries;
using Pondrop.Service.Store.Tests.Faker;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Pondrop.Service.Store.Api.Tests
{
    public class StoreTypeControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<IUpdateMaterializeViewQueueService> _updateMaterializeViewQueueServiceMock;
        private readonly Mock<IRebuildMaterializeViewQueueService> _rebuildMaterializeViewQueueServiceMock;
        private readonly Mock<ILogger<StoreTypeController>> _loggerMock;
        
        public StoreTypeControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _updateMaterializeViewQueueServiceMock = new Mock<IUpdateMaterializeViewQueueService>();
            _rebuildMaterializeViewQueueServiceMock = new Mock<IRebuildMaterializeViewQueueService>();
            _loggerMock = new Mock<ILogger<StoreTypeController>>();
        }

        [Fact]
        public async void GetAllStoreTypes_ShouldReturnOkResult()
        {
            // arrange
            var items = StoreTypeFaker.GetStoreTypeRecords();
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetAllStoreTypesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<List<StoreTypeRecord>>.Success(items));
            var controller = GetController();

            // act
            var response = await controller.GetAllStoreTypes();

            // assert
            Assert.IsType<OkObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, items);
            _mediatorMock.Verify(x => x.Send(It.IsAny<GetAllStoreTypesQuery>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void GetAllStoreTypes_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<List<StoreTypeRecord>>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetAllStoreTypesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();

            // act
            var response = await controller.GetAllStoreTypes();

            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<GetAllStoreTypesQuery>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void GetStoreTypeById_ShouldReturnOkResult()
        {
            // arrange
            var item = StoreTypeFaker.GetStoreTypeRecords(1).Single();
            _mediatorMock
                .Setup(x => x.Send(It.Is<GetStoreTypeByIdQuery>(x => x.Id == item.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<StoreTypeRecord>.Success(item));
            var controller = GetController();
        
            // act
            var response = await controller.GetStoreTypeById(item.Id);
        
            // assert
            Assert.IsType<OkObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, item);
            _mediatorMock.Verify(x => x.Send(It.Is<GetStoreTypeByIdQuery>(x => x.Id == item.Id), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void GetStoreTypeById_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<StoreTypeRecord>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetStoreTypeByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();
        
            // act
            var response = await controller.GetStoreTypeById(Guid.NewGuid());
        
            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<GetStoreTypeByIdQuery>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void CreateStoreTypeCommand_ShouldReturnOkResult()
        {
            // arrange
            var cmd = StoreTypeFaker.GetCreateStoreTypeCommand();
            var item = StoreTypeFaker.GetStoreTypeRecord(cmd);
            _mediatorMock
                .Setup(x => x.Send(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<StoreTypeRecord>.Success(item));
            var controller = GetController();
        
            // act
            var response = await controller.CreateStoreType(cmd);
        
            // assert
            Assert.IsType<ObjectResult>(response);
            Assert.Equal(((ObjectResult)response).StatusCode, StatusCodes.Status201Created);
            Assert.Equal(((ObjectResult)response).Value, item);
            _mediatorMock.Verify(x => x.Send(cmd, It.IsAny<CancellationToken>()), Times.Once());
            _updateMaterializeViewQueueServiceMock.Verify(x => x.Queue(It.Is<UpdateMaterializedViewByIdCommand>(c => c.Id == item.Id)));
        }
        
        [Fact]
        public async void CreateStoreTypeCommand_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<StoreTypeRecord>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<CreateStoreTypeCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();
        
            // act
            var response = await controller.CreateStoreType(new CreateStoreTypeCommand());
        
            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<CreateStoreTypeCommand>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void UpdateStoreTypeCommand_ShouldReturnOkResult()
        {
            // arrange
            var cmd = StoreTypeFaker.GetUpdateStoreTypeCommand();
            var item = StoreTypeFaker.GetStoreTypeRecord(cmd);
            _mediatorMock
                .Setup(x => x.Send(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<StoreTypeRecord>.Success(item));
            var controller = GetController();
        
            // act
            var response = await controller.UpdateStoreType(cmd);
        
            // assert
            Assert.IsType<OkObjectResult>(response);
            Assert.Equal(((OkObjectResult)response).StatusCode, StatusCodes.Status200OK);
            Assert.Equal(((OkObjectResult)response).Value, item);
            _mediatorMock.Verify(x => x.Send(cmd, It.IsAny<CancellationToken>()), Times.Once());
            _updateMaterializeViewQueueServiceMock.Verify(x => x.Queue(It.Is<UpdateMaterializedViewByIdCommand>(c => c.Id == item.Id)));
        }
        
        [Fact]
        public async void UpdateStoreTypeCommand_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<StoreTypeRecord>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<UpdateStoreTypeCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();
        
            // act
            var response = await controller.UpdateStoreType(new UpdateStoreTypeCommand());
        
            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<UpdateStoreTypeCommand>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void UpdateMaterializedView_ShouldReturnOkResult()
        {
            // arrange
            var cmd = new UpdateStoreTypeMaterializedViewByIdCommand() { Id = Guid.NewGuid() };
            var item = StoreTypeFaker.GetStoreTypeRecords(1).Single() with { Id = cmd.Id };
            _mediatorMock
                .Setup(x => x.Send(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<StoreTypeRecord>.Success(item));
            var controller = GetController();
        
            // act
            var response = await controller.UpdateMaterializedView(cmd);
        
            // assert
            Assert.IsType<OkObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, item);
            _mediatorMock.Verify(x => x.Send(cmd, It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void UpdateMaterializedView_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<StoreTypeRecord>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<UpdateStoreTypeMaterializedViewByIdCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();
        
            // act
            var response = await controller.UpdateMaterializedView(new UpdateStoreTypeMaterializedViewByIdCommand());
        
            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<UpdateStoreTypeMaterializedViewByIdCommand>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void RebuildMaterializedView_ShouldReturnAcceptedResult()
        {
            // arrange
            var controller = GetController();
        
            // act
            var response = controller.RebuildMaterializedView();
        
            // assert
            Assert.IsType<AcceptedResult>(response);
            _rebuildMaterializeViewQueueServiceMock.Verify(x => x.Queue(It.IsAny<RebuildStoreTypeMaterializedViewCommand>()), Times.Once());
        }

        private StoreTypeController GetController() =>
            new StoreTypeController(
                _mediatorMock.Object,
                _updateMaterializeViewQueueServiceMock.Object,
                _rebuildMaterializeViewQueueServiceMock.Object,
                _loggerMock.Object
            );
    }
}
