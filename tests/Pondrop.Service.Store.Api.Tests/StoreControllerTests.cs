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
    public class StoreControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<IUpdateMaterializeViewQueueService> _updateMaterializeViewQueueServiceMock;
        private readonly Mock<IRebuildMaterializeViewQueueService> _rebuildMaterializeViewQueueServiceMock;
        private readonly Mock<ILogger<StoreController>> _loggerMock;
        
        public StoreControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _updateMaterializeViewQueueServiceMock = new Mock<IUpdateMaterializeViewQueueService>();
            _rebuildMaterializeViewQueueServiceMock = new Mock<IRebuildMaterializeViewQueueService>();
            _loggerMock = new Mock<ILogger<StoreController>>();
        }

        [Fact]
        public async void GetAllStores_ShouldReturnOkResult()
        {
            // arrange
            var items = StoreFaker.GetStoreRecords();
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetAllStoresQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<List<StoreRecord>>.Success(items));
            var controller = GetController();

            // act
            var response = await controller.GetAllStores();

            // assert
            Assert.IsType<OkObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, items);
            _mediatorMock.Verify(x => x.Send(It.IsAny<GetAllStoresQuery>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void GetAllStores_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<List<StoreRecord>>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetAllStoresQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();

            // act
            var response = await controller.GetAllStores();

            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<GetAllStoresQuery>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void GetStoreById_ShouldReturnOkResult()
        {
            // arrange
            var item = StoreFaker.GetStoreRecords(1).Single();
            _mediatorMock
                .Setup(x => x.Send(It.Is<GetStoreByIdQuery>(x => x.Id == item.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<StoreRecord>.Success(item));
            var controller = GetController();
        
            // act
            var response = await controller.GetStoreById(item.Id);
        
            // assert
            Assert.IsType<OkObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, item);
            _mediatorMock.Verify(x => x.Send(It.Is<GetStoreByIdQuery>(x => x.Id == item.Id), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void GetStoreById_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<StoreRecord>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetStoreByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();
        
            // act
            var response = await controller.GetStoreById(Guid.NewGuid());
        
            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<GetStoreByIdQuery>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void CreateStoreCommand_ShouldReturnOkResult()
        {
            // arrange
            var cmd = StoreFaker.GetCreateStoreCommand();
            var item = StoreFaker.GetStoreRecord(cmd);
            _mediatorMock
                .Setup(x => x.Send(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<StoreRecord>.Success(item));
            var controller = GetController();
        
            // act
            var response = await controller.CreateStore(cmd);
        
            // assert
            Assert.IsType<ObjectResult>(response);
            Assert.Equal(((ObjectResult)response).StatusCode, StatusCodes.Status201Created);
            Assert.Equal(((ObjectResult)response).Value, item);
            _mediatorMock.Verify(x => x.Send(cmd, It.IsAny<CancellationToken>()), Times.Once());
            _updateMaterializeViewQueueServiceMock.Verify(x => x.Queue(It.Is<UpdateMaterializedViewByIdCommand>(c => c.Id == item.Id)));
        }
        
        [Fact]
        public async void CreateStoreCommand_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<StoreRecord>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<CreateStoreCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();
        
            // act
            var response = await controller.CreateStore(new CreateStoreCommand());
        
            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<CreateStoreCommand>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void UpdateMaterializedView_ShouldReturnOkResult()
        {
            // arrange
            var cmd = new UpdateStoreMaterializedViewByIdCommand() { Id = Guid.NewGuid() };
            var item = StoreFaker.GetStoreRecords(1).Single() with { Id = cmd.Id };
            _mediatorMock
                .Setup(x => x.Send(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<StoreRecord>.Success(item));
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
            var failedResult = Result<StoreRecord>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<UpdateStoreMaterializedViewByIdCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();
        
            // act
            var response = await controller.UpdateMaterializedView(new UpdateStoreMaterializedViewByIdCommand());
        
            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<UpdateStoreMaterializedViewByIdCommand>(), It.IsAny<CancellationToken>()), Times.Once());
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
            _rebuildMaterializeViewQueueServiceMock.Verify(x => x.Queue(It.IsAny<RebuildStoreMaterializedViewCommand>()), Times.Once());
        }
        
        private StoreController GetController() =>
            new StoreController(
                _mediatorMock.Object,
                _updateMaterializeViewQueueServiceMock.Object,
                _rebuildMaterializeViewQueueServiceMock.Object,
                _loggerMock.Object
            );
    }
}
