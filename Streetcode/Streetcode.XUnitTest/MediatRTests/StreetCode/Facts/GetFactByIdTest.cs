﻿using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Localization;
using Moq;
using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;
using Streetcode.BLL.MediatR.Streetcode.Fact.GetById;
using Streetcode.BLL.SharedResource;
using Streetcode.DAL.Entities.Streetcode.TextContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;

namespace Streetcode.XUnitTest.MediatRTests.StreetCode.Facts;

public class GetFactByIdTest
{
    private Mock<IRepositoryWrapper> _mockRepository;
    private Mock<IMapper> _mockMapper;
    private readonly Mock<IStringLocalizer<CannotFindSharedResource>> _mockLocalizerCannotFind;

    public GetFactByIdTest() {
        _mockMapper = new Mock<IMapper>();
        _mockRepository = new Mock<IRepositoryWrapper>();
        _mockLocalizerCannotFind = new Mock<IStringLocalizer<CannotFindSharedResource>>();
    }

    [Theory]
    [InlineData(1)]
    public async Task ShouldReturnSuccessfully_ExistingId(int id)
    {
        //Arrange
        _mockRepository.Setup(x => x.FactRepository
            .GetFirstOrDefaultAsync(
               It.IsAny<Expression<Func<Fact, bool>>>(),
                It.IsAny<Func<IQueryable<Fact>,
                IIncludableQueryable<Fact, object>>>()))
            .ReturnsAsync(GetFact(id));

        _mockMapper
            .Setup(x => x
            .Map<FactDto>(It.IsAny<Fact>()))
            .Returns(GetFactDTO(id));

        var handler = new GetFactByIdHandler(_mockRepository.Object, _mockMapper.Object, _mockLocalizerCannotFind.Object);

        //Act
        var result = await handler.Handle(new GetFactByIdQuery(id), CancellationToken.None);

        //Assert
        Assert.Multiple(
            () => Assert.NotNull(result),
            () => Assert.True(result.IsSuccess),
            () => Assert.Equal(result.Value.Id, id)
        );
    }

    [Theory]
    [InlineData(1)]
    public async Task ShouldReturnSuccessfully_NotExistingId(int id)
    {
        //Arrange
        _mockRepository.Setup(x => x.FactRepository
            .GetFirstOrDefaultAsync(
               It.IsAny<Expression<Func<Fact, bool>>>(),
                It.IsAny<Func<IQueryable<Fact>,
                IIncludableQueryable<Fact, object>>>()))
            .ReturnsAsync(GetFactWithNotExistingId());

        _mockMapper
            .Setup(x => x
            .Map<FactDto>(It.IsAny<Fact>()))
            .Returns(GetFactDTOWithNotExistingId());

        var expectedError = $"Cannot find any fact with corresponding id: {id}";

        var handler = new GetFactByIdHandler(_mockRepository.Object, _mockMapper.Object, _mockLocalizerCannotFind.Object);

        //Act
        var result = await handler.Handle(new GetFactByIdQuery(id), CancellationToken.None);

        //Assert
        Assert.Multiple(
            () => Assert.NotNull(result),
            () => Assert.True(result.IsFailed),
            () => Assert.Equal(expectedError, result.Errors.First().Message)
        );
    }

    [Theory]
    [InlineData(1)]
    public async Task ShouldReturnSuccessfully_CorrectType(int id)
    {
        //Arrange
        _mockRepository.Setup(x => x.FactRepository
            .GetFirstOrDefaultAsync(
               It.IsAny<Expression<Func<Fact, bool>>>(),
                It.IsAny<Func<IQueryable<Fact>,
                IIncludableQueryable<Fact, object>>>()))
            .ReturnsAsync(GetFact(id));

        _mockMapper
            .Setup(x => x
            .Map<FactDto>(It.IsAny<Fact>()))
            .Returns(GetFactDTO(id));

        var handler = new GetFactByIdHandler(_mockRepository.Object, _mockMapper.Object, _mockLocalizerCannotFind.Object);

        //Act
        var result = await handler.Handle(new GetFactByIdQuery(id), CancellationToken.None);

        //Assert
        Assert.Multiple(
            () => Assert.NotNull(result.ValueOrDefault),
            () => Assert.IsType<FactDto>(result.ValueOrDefault)
        );
    }

    private static Fact GetFact(int id)
    {
        return new Fact
        {
            Id = id
        };
    }
    private static Fact? GetFactWithNotExistingId()
    {
        return null;
    }

    private static FactDto GetFactDTO(int id)
    {
        return new FactDto
        {
            Id = id
        };
    }
    private static FactDto? GetFactDTOWithNotExistingId()
    {
        return null;
    }
}
