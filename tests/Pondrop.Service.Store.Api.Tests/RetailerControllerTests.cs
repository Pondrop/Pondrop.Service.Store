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
    public class RetailerControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<IUpdateMaterializeViewQueueService> _updateMaterializeViewQueueServiceMock;
        private readonly Mock<IRebuildMaterializeViewQueueService> _rebuildMaterializeViewQueueServiceMock;
        private readonly Mock<ILogger<RetailerController>> _loggerMock;
        
        public RetailerControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _updateMaterializeViewQueueServiceMock = new Mock<IUpdateMaterializeViewQueueService>();
            _rebuildMaterializeViewQueueServiceMock = new Mock<IRebuildMaterializeViewQueueService>();
            _loggerMock = new Mock<ILogger<RetailerController>>();
        }

        [Fact]
        public async void GetAllRetailers_ShouldReturnOkResult()
        {
            // arrange
            var items = RetailerFaker.GetRetailerRecords();
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetAllRetailersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<List<RetailerRecord>>.Success(items));
            var controller = GetController();

            // act
            var response = await controller.GetAllRetailers();

            // assert
            Assert.IsType<OkObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, items);
            _mediatorMock.Verify(x => x.Send(It.IsAny<GetAllRetailersQuery>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void GetAllRetailers_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<List<RetailerRecord>>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetAllRetailersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();

            // act
            var response = await controller.GetAllRetailers();

            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<GetAllRetailersQuery>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void GetRetailerById_ShouldReturnOkResult()
        {
            // arrange
            var item = RetailerFaker.GetRetailerRecords(1).Single();
            _mediatorMock
                .Setup(x => x.Send(It.Is<GetRetailerByIdQuery>(x => x.Id == item.Id), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<RetailerRecord>.Success(item));
            var controller = GetController();
        
            // act
            var response = await controller.GetRetailerById(item.Id);
        
            // assert
            Assert.IsType<OkObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, item);
            _mediatorMock.Verify(x => x.Send(It.Is<GetRetailerByIdQuery>(x => x.Id == item.Id), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void GetRetailerById_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<RetailerRecord>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetRetailerByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();
        
            // act
            var response = await controller.GetRetailerById(Guid.NewGuid());
        
            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<GetRetailerByIdQuery>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void CreateRetailerCommand_ShouldReturnOkResult()
        {
            // arrange
            var cmd = RetailerFaker.GetCreateRetailerCommand();
            var item = RetailerFaker.GetRetailerRecord(cmd);
            _mediatorMock
                .Setup(x => x.Send(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<RetailerRecord>.Success(item));
            var controller = GetController();
        
            // act
            var response = await controller.CreateRetailer(cmd);
        
            // assert
            Assert.IsType<ObjectResult>(response);
            Assert.Equal(((ObjectResult)response).StatusCode, StatusCodes.Status201Created);
            Assert.Equal(((ObjectResult)response).Value, item);
            _mediatorMock.Verify(x => x.Send(cmd, It.IsAny<CancellationToken>()), Times.Once());
            _updateMaterializeViewQueueServiceMock.Verify(x => x.Queue(It.Is<UpdateMaterializedViewByIdCommand>(c => c.Id == item.Id)));
        }
        
        [Fact]
        public async void CreateRetailerCommand_ShouldReturnBadResult_WhenFailedResult()
        {
            // arrange
            var failedResult = Result<RetailerRecord>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<CreateRetailerCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();
        
            // act
            var response = await controller.CreateRetailer(new CreateRetailerCommand());
        
            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<CreateRetailerCommand>(), It.IsAny<CancellationToken>()), Times.Once());
        }
        
        [Fact]
        public async void UpdateMaterializedView_ShouldReturnOkResult()
        {
            // arrange
            var cmd = new UpdateRetailerMaterializedViewByIdCommand() { Id = Guid.NewGuid() };
            var item = RetailerFaker.GetRetailerRecords(1).Single() with { Id = cmd.Id };
            _mediatorMock
                .Setup(x => x.Send(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<RetailerRecord>.Success(item));
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
            var failedResult = Result<RetailerRecord>.Error("Invalid result!");
            _mediatorMock
                .Setup(x => x.Send(It.IsAny<UpdateRetailerMaterializedViewByIdCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(failedResult);
            var controller = GetController();
        
            // act
            var response = await controller.UpdateMaterializedView(new UpdateRetailerMaterializedViewByIdCommand());
        
            // assert
            Assert.IsType<BadRequestObjectResult>(response);
            Assert.Equal(((ObjectResult)response).Value, failedResult.ErrorMessage);
            _mediatorMock.Verify(x => x.Send(It.IsAny<UpdateRetailerMaterializedViewByIdCommand>(), It.IsAny<CancellationToken>()), Times.Once());
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
            _rebuildMaterializeViewQueueServiceMock.Verify(x => x.Queue(It.IsAny<RebuildRetailerMaterializedViewCommand>()), Times.Once());
        }
        
        private RetailerController GetController() =>
            new RetailerController(
                _mediatorMock.Object,
                _updateMaterializeViewQueueServiceMock.Object,
                _rebuildMaterializeViewQueueServiceMock.Object,
                _loggerMock.Object
            );
    }
}
