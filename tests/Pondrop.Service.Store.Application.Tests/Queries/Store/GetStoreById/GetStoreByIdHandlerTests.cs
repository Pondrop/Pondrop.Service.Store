using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Pondrop.Service.Store.Application.Commands;
using Pondrop.Service.Store.Application.Interfaces;
using Pondrop.Service.Store.Application.Queries;
using Pondrop.Service.Store.Domain.Events;
using Pondrop.Service.Store.Domain.Models;
using Pondrop.Service.Store.Tests.Faker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pondrop.Service.Store.Application.Tests.Commands.Store.CreateStore;

public class GetStoreByIdHandlerTests
{
    private readonly Mock<IMaterializedViewRepository<StoreEntity>> _viewRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IValidator<GetStoreByIdQuery>> _validatorMock;
    private readonly Mock<ILogger<GetStoreByIdQueryHandler>> _loggerMock;
    
    public GetStoreByIdHandlerTests()
    {
        _viewRepositoryMock = new Mock<IMaterializedViewRepository<StoreEntity>>();
        _mapperMock = new Mock<IMapper>();
        _validatorMock = new Mock<IValidator<GetStoreByIdQuery>>();
        _loggerMock = new Mock<ILogger<GetStoreByIdQueryHandler>>();
    }
    
    [Fact]
    public async void GetStoreByIdQuery_ShouldSucceed()
    {
        // arrange
        var query = new GetStoreByIdQuery() { Id = Guid.NewGuid() };
        var item = StoreFaker.GetStoreRecords(1).Single();
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _viewRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Returns(Task.FromResult<StoreEntity?>(new StoreEntity()));
        _mapperMock
            .Setup(x => x.Map<StoreRecord>(It.IsAny<StoreEntity>()))
            .Returns(item);
        var handler = GetQueryHandler();
        
        // act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // assert
        Assert.True(result.IsSuccess);
        Assert.Equal(item, result.Value);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _viewRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Once());
        _mapperMock.Verify(
            x => x.Map<StoreRecord>(It.IsAny<StoreEntity>()),
            Times.Once());
    }
    
    [Fact]
    public async void GetStoreByIdQuery_WhenInvalid_ShouldFail()
    {
        // arrange
        var query = new GetStoreByIdQuery() { Id = Guid.NewGuid() };
        var item = StoreFaker.GetStoreRecords(1).Single();
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult(new [] { new ValidationFailure() }));
        _viewRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Returns(Task.FromResult<StoreEntity?>(new StoreEntity()));
        _mapperMock
            .Setup(x => x.Map<StoreRecord>(It.IsAny<StoreEntity>()))
            .Returns(item);
        var handler = GetQueryHandler();
        
        // act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _viewRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Never());
        _mapperMock.Verify(
            x => x.Map<StoreRecord>(It.IsAny<StoreEntity>()),
            Times.Never());
    }
    
    [Fact]
    public async void GetStoreByIdQuery_WhenNotFound_ShouldSucceedWithNull()
    {
        // arrange
        var query = new GetStoreByIdQuery() { Id = Guid.NewGuid() };
        var item = StoreFaker.GetStoreRecords(1).Single();
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _viewRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Returns(Task.FromResult<StoreEntity?>(null));
        _mapperMock
            .Setup(x => x.Map<StoreRecord>(It.IsAny<StoreEntity>()))
            .Returns(item);
        var handler = GetQueryHandler();
        
        // act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _viewRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Once());
        _mapperMock.Verify(
            x => x.Map<StoreRecord>(It.IsAny<StoreEntity>()),
            Times.Never());
    }
    
    [Fact]
    public async void GetStoreByIdQuery_WhenThrows_ShouldFail()
    {
        // arrange
        var query = new GetStoreByIdQuery() { Id = Guid.NewGuid() };
        var item = StoreFaker.GetStoreRecords(1).Single();
        _validatorMock
            .Setup(x => x.Validate(query))
            .Returns(new ValidationResult());
        _viewRepositoryMock
            .Setup(x => x.GetByIdAsync(query.Id))
            .Throws(new Exception());
        _mapperMock
            .Setup(x => x.Map<StoreRecord>(It.IsAny<StoreEntity>()))
            .Returns(item);
        var handler = GetQueryHandler();
        
        // act
        var result = await handler.Handle(query, CancellationToken.None);
        
        // assert
        Assert.False(result.IsSuccess);
        _validatorMock.Verify(
            x => x.Validate(query),
            Times.Once());
        _viewRepositoryMock.Verify(
            x => x.GetByIdAsync(query.Id),
            Times.Once());
        _mapperMock.Verify(
            x => x.Map<StoreRecord>(It.IsAny<StoreEntity>()),
            Times.Never());
    }
    
    private GetStoreByIdQueryHandler GetQueryHandler() =>
        new GetStoreByIdQueryHandler(
            _viewRepositoryMock.Object,
            _mapperMock.Object,
            _validatorMock.Object,
            _loggerMock.Object);
}