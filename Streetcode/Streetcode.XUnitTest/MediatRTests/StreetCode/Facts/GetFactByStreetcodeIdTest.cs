﻿using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Localization;
using Moq;
using Streetcode.BLL.DTO.Streetcode.TextContent.Fact;
using Streetcode.BLL.MediatR.Streetcode.Fact.GetByStreetcodeId;
using Streetcode.BLL.SharedResource;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Entities.Streetcode.TextContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;

namespace Streetcode.XUnitTest.MediatRTests.StreetCode.Facts;

public class GetFactByStreetcodeIdTest
{
    private Mock<IRepositoryWrapper> _mockRepository;
    private Mock<IMapper> _mockMapper;
    private readonly Mock<IStringLocalizer<CannotFindSharedResource>> _mockLocalizerCannotFind;

    public GetFactByStreetcodeIdTest()
    {
        _mockRepository = new Mock<IRepositoryWrapper>();
        _mockMapper = new Mock<IMapper>();
        _mockLocalizerCannotFind = new Mock<IStringLocalizer<CannotFindSharedResource>>();
    }

    [Theory]
    [InlineData(1)]
    public async Task ShouldReturnSuccessfully_ExistingId(int streetCodeId)
    {
        //Arrange
        _mockRepository.Setup(x => x.FactRepository
              .GetAllAsync(
                  It.IsAny<Expression<Func<Fact, bool>>>(),
                    It.IsAny<Func<IQueryable<Fact>,
              IIncludableQueryable<Fact, object>>>()))
              .ReturnsAsync(GetListFacts());

        _mockMapper
            .Setup(x => x
            .Map<IEnumerable<FactDto>>(It.IsAny<IEnumerable<Fact>>()))
            .Returns(GetListFactDTO());

        var handler = new GetFactByStreetcodeIdHandler(_mockRepository.Object, _mockMapper.Object, _mockLocalizerCannotFind.Object);

        //Act
        var result = await handler.Handle(new GetFactByStreetcodeIdQuery(streetCodeId), CancellationToken.None);

        //Assert
        Assert.Multiple(
            () => Assert.NotNull(result),
            () => Assert.True(result.IsSuccess),
            () => Assert.NotEmpty(result.Value)
        );
    }

    [Theory]
    [InlineData(2)]
    public async Task ShouldReturnSuccessfully_CorrectType(int streetCodeId)
    {
        //Arrange
        _mockRepository.Setup(x => x.FactRepository
              .GetAllAsync(
                  It.IsAny<Expression<Func<Fact, bool>>>(),
                    It.IsAny<Func<IQueryable<Fact>,
              IIncludableQueryable<Fact, object>>>()))
              .ReturnsAsync(GetListFacts());

        _mockMapper
            .Setup(x => x
            .Map<IEnumerable<FactDto>>(It.IsAny<IEnumerable<Fact>>()))
            .Returns(GetListFactDTO());

        var handler = new GetFactByStreetcodeIdHandler(_mockRepository.Object, _mockMapper.Object, _mockLocalizerCannotFind.Object);

        //Act
        var result = await handler.Handle(new GetFactByStreetcodeIdQuery(streetCodeId), CancellationToken.None);

        //Assert
        Assert.Multiple(
            () => Assert.NotNull(result),
            () => Assert.True(result.IsSuccess),
            () => Assert.IsType<List<FactDto>>(result.ValueOrDefault)
        );
    }

    [Theory]
    [InlineData(1)]
    public async Task ShouldThrowError_IdNotExist(int streetCodeId)
    {
        //Arrange
        _mockRepository.Setup(x => x.FactRepository
              .GetAllAsync(
                  It.IsAny<Expression<Func<Fact, bool>>>(),
                    It.IsAny<Func<IQueryable<Fact>,
              IIncludableQueryable<Fact, object>>>()))
              .ReturnsAsync(GetListFactsWithNotExistingStreetcodeId());

        _mockMapper
            .Setup(x => x
            .Map<IEnumerable<FactDto>>(It.IsAny<IEnumerable<Fact>>()))
            .Returns(GetListFactsDTOWithNotExistingId());

        var expectedError = $"Cannot find any fact by the streetcode id: {streetCodeId}";

        var handler = new GetFactByStreetcodeIdHandler(_mockRepository.Object, _mockMapper.Object, _mockLocalizerCannotFind.Object);

        //Act
        var result = await handler.Handle(new GetFactByStreetcodeIdQuery(streetCodeId), CancellationToken.None);

        //Assert
        Assert.Multiple(
            () => Assert.True(result.IsFailed),
            () => Assert.Equal(expectedError, result.Errors.First().Message)
        );
    }

    private static IQueryable<Fact> GetListFacts()
    {
        var facts = new List<Fact>
        {
            new Fact
            {
                Id = 1,
                Title = "Викуп з кріпацтва",
                ImageId = null,
                Streetcode = new StreetcodeContent
            {
                Id = 1
            },
                StreetcodeId = 1,
                FactContent = "Навесні 1838-го Карл Брюллов..."
            },
        };

        return facts.AsQueryable();
    }

    private static List<StreetcodeContent> GetStreetcodes()
    {
        var streetCodes = new List<StreetcodeContent>
        {
            new StreetcodeContent
            {
                Id = 1
            },
        };

        return streetCodes;
    }
    private static List<Fact>? GetListFactsWithNotExistingStreetcodeId()
    {
        return null;
    }
    private static List<FactDto>? GetListFactsDTOWithNotExistingId()
    {
        return null;
    }
    private static List<FactDto> GetListFactDTO()
    {
        var facts = new List<FactDto>
        {
            new FactDto
            {
                Id = 1
            },
        };

        return facts;
    }
}