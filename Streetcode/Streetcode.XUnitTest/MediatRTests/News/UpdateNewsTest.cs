﻿using AutoMapper;
using MediatR;
using Moq;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.MediatR.Newss.Update;
using Streetcode.DAL.Entities.News;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;
using Streetcode.DAL.Entities.Media.Images;
using Microsoft.EntityFrameworkCore.Query;

namespace Streetcode.XUnitTest.MediatRTests.News
{
    public class UpdateNewsTests
    {
        private Mock<IRepositoryWrapper> _mockRepository;
        private Mock<IMapper> _mockMapper;
        private readonly Mock<IBlobService> _blobService;

        public UpdateNewsTests()
        {
            _mockRepository = new();
            _mockMapper = new();
            _blobService = new Mock<IBlobService>();
        }


        [Theory]
        [InlineData(1)]
        public async Task ShouldReturnSuccessfully_WhenUpdated(int returnNumber)
        {
            // Arrange
            var testNews = GetNews();
            var testNewsDTO = GetNewsDTO();

            _mockRepository.Setup(x => x.NewsRepository.Update(It.IsAny<DAL.Entities.News.News>()));
            _mockRepository.Setup(x => x.SaveChangesAsync()).ReturnsAsync(returnNumber);

            _mockMapper.Setup(x => x.Map<DAL.Entities.News.News>(It.IsAny<NewsDTO>()))
                .Returns(testNews);
            _mockMapper.Setup(x => x.Map<NewsDTO>(It.IsAny<DAL.Entities.News.News>()))
                .Returns(testNewsDTO);

            _mockRepository.Setup(x => x.ImageRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<Image, bool>>>(),
                It.IsAny<Func<IQueryable<Image>, IIncludableQueryable<Image, object>>>()
            )).ReturnsAsync((Image)null);

            _blobService.Setup(x => x.FindFileInStorageAsBase64(It.IsAny<string>()))
                .Returns("base64Image");

            var handler = new UpdateNewsHandler(_mockRepository.Object, _mockMapper.Object, _blobService.Object);

            // Act
            var result = await handler.Handle(new UpdateNewsCommand(testNewsDTO), CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.True(result.IsSuccess);
                Assert.IsType<NewsDTO>(result.Value);
            });
        }



        [Theory]
        [InlineData(1)]
        public async Task ShouldThrowExeption_TryMapNullRequest(int returnNuber)
        {
            //Arrange
            _mockRepository.Setup(x => x.NewsRepository.Update(GetNewsWithNotExistId()!));
            _mockRepository.Setup(x => x.SaveChangesAsync()).ReturnsAsync(returnNuber);

            _mockMapper.Setup(x => x.Map<DAL.Entities.News.News>(It.IsAny<NewsDTO>()))
                .Returns(GetNewsWithNotExistId()!);

            var expectedError = "Cannot convert null to news";

            var handler = new UpdateNewsHandler( _mockRepository.Object, _mockMapper.Object, _blobService.Object);

            //Act
            var result = await handler.Handle(new UpdateNewsCommand(GetNewsDTOWithNotExistId()!), CancellationToken.None);

            //Assert
            Assert.Multiple(
                () => Assert.False(result.IsSuccess),
                () => Assert.Equal(expectedError, result.Errors.First().Message)
            );
        }

        [Theory]
        [InlineData(-1)]
        public async Task ShouldThrowException_SaveChangesAsyncIsNotSuccessful(int returnNumber)
        {
            // Arrange
            var testNews = GetNews();
            var testNewsDTO = GetNewsDTO();

            _mockRepository.Setup(x => x.NewsRepository.Update(It.IsAny<DAL.Entities.News.News>()));
            _mockRepository.Setup(x => x.SaveChangesAsync()).ReturnsAsync(returnNumber);

            _mockMapper.Setup(x => x.Map<DAL.Entities.News.News>(It.IsAny<NewsDTO>()))
                .Returns(testNews);
            _mockMapper.Setup(x => x.Map<NewsDTO>(It.IsAny<DAL.Entities.News.News>()))
                .Returns(testNewsDTO);
            _mockRepository.Setup(x => x.ImageRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Image, bool>>>(),
                    It.IsAny<Func<IQueryable<Image>, IIncludableQueryable<Image, object>>>()
                )).ReturnsAsync((Image)null);

            _blobService.Setup(x => x.FindFileInStorageAsBase64(It.IsAny<string>()))
                .Returns("base64Image");

            var expectedError = "Failed to update news";
            var handler = new UpdateNewsHandler(_mockRepository.Object, _mockMapper.Object, _blobService.Object);

            // Act
            var result = await handler.Handle(new UpdateNewsCommand(testNewsDTO), CancellationToken.None);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.True(result.IsFailed);
                Assert.Equal(expectedError, result.Errors.First().Message);
            });
        }


        [Theory]
        [InlineData(1)]
        public async Task ShouldReturnSuccessfully_TypeIsCorrect(int returnNuber)
        {
            //Arrange
            _mockRepository.Setup(x => x.NewsRepository.Create(GetNews()));
            _mockRepository.Setup(x => x.SaveChangesAsync()).ReturnsAsync(returnNuber);

            _mockMapper.Setup(x => x.Map<DAL.Entities.News.News>(It.IsAny<NewsDTO>()))
                .Returns(GetNews());
            _mockRepository.Setup(x => x.ImageRepository.GetFirstOrDefaultAsync(
                    It.IsAny<Expression<Func<Image, bool>>>(),
                    It.IsAny<Func<IQueryable<Image>, IIncludableQueryable<Image, object>>>()
                )).ReturnsAsync((Image)null);

            _blobService.Setup(x => x.FindFileInStorageAsBase64(It.IsAny<string>()))
                .Returns("base64Image");

            var handler = new UpdateNewsHandler(_mockRepository.Object, _mockMapper.Object, _blobService.Object);

            //Act
            var result = await handler.Handle(new UpdateNewsCommand(GetNewsDTO()), CancellationToken.None);

            //Assert
            Assert.True(result.IsSuccess);
        }

        private static DAL.Entities.News.News GetNews()
        {
            return new DAL.Entities.News.News()
            {
                Id = 1,
                ImageId =1
            };
        }

        private static NewsDTO GetNewsDTO()
        {
            return new NewsDTO();
        }
        private static DAL.Entities.News.News? GetNewsWithNotExistId() => null;
        private static NewsDTO? GetNewsDTOWithNotExistId() => null;
    }
}
