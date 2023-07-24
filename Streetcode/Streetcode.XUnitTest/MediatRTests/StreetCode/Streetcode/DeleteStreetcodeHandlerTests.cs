﻿using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Localization;
using Moq;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.Streetcode.Delete;
using Streetcode.BLL.SharedResource;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;

namespace Streetcode.XUnitTest.MediatRTests.StreetCode.Streetcode
{
    public class DeleteStreetcodeHandlerTests
    {   
        private readonly Mock<IRepositoryWrapper> _repository;
        private readonly Mock<ILoggerService> _mockLogger;
        private readonly Mock<IStringLocalizer<CannotFindSharedResource>> _mockLocalizerCannotFind;
        private readonly Mock<IStringLocalizer<FailedToDeleteSharedResource>> _mockLocalizerFailedToDelete;




        public DeleteStreetcodeHandlerTests()
        {
            _repository = new Mock<IRepositoryWrapper>();
            _mockLogger = new Mock<ILoggerService>();
            _mockLocalizerCannotFind = new Mock<IStringLocalizer<CannotFindSharedResource>>();
            _mockLocalizerFailedToDelete = new Mock<IStringLocalizer<FailedToDeleteSharedResource>>();
        }

        [Theory]
        [InlineData(1)]
        public async Task Handle_ReturnsSuccess(int id)
        {   
            // arrange
            var testStreetcode = new StreetcodeContent();
            int testSaveChangesSuccess = 1;

            RepositorySetup(testStreetcode, testSaveChangesSuccess);

            var handler = new DeleteStreetcodeHandler(_repository.Object, _mockLogger.Object, _mockLocalizerFailedToDelete.Object, _mockLocalizerCannotFind.Object);
            // act
            var result = await handler.Handle(new DeleteStreetcodeCommand(id), CancellationToken.None);
            // assert
            Assert.True(result.IsSuccess);
        }

        [Theory]
        [InlineData(1)]
        public async Task Handle_ReturnsNullError(int id)
        {   
            // arrange
            string expectedErrorMessage = $"Cannot find a streetcode with corresponding categoryId: {id}";
            _mockLocalizerCannotFind.Setup(x => x[It.IsAny<string>(), It.IsAny<object>()])
               .Returns((string key, object[] args) =>
               {
                   if (args != null && args.Length > 0 && args[0] is int id)
                   {
                       return new LocalizedString(key, $"Cannot find a streetcode with corresponding categoryId: {id}");
                   }

                   return new LocalizedString(key, "Cannot find any streetcode with unknown categoryId");
               });

            int testSaveChangesSuccess = 1;

            RepositorySetup(null, testSaveChangesSuccess);

            var handler = new DeleteStreetcodeHandler(_repository.Object, _mockLogger.Object, _mockLocalizerFailedToDelete.Object, _mockLocalizerCannotFind.Object);
            // act
            var result = await handler.Handle(new DeleteStreetcodeCommand(id), CancellationToken.None);
            // assert
            Assert.Equal(expectedErrorMessage, result.Errors.Single().Message);
        }

        [Theory]
        [InlineData(1)]
        public async Task Handle_ReturnsSaveAsyncError(int id)
        {   
            // arrange
            var testStreetcode = new StreetcodeContent();
            string expectedErrorMessage = "Failed to delete a streetcode";
            _mockLocalizerFailedToDelete.Setup(x => x["FailedToDeleteStreetcode"])
          .Returns(new LocalizedString("FailedToDeleteStreetcode", expectedErrorMessage));

            int testSaveChangesFailed = -1;

            RepositorySetup(testStreetcode, testSaveChangesFailed);

            var handler = new DeleteStreetcodeHandler(_repository.Object, _mockLogger.Object, _mockLocalizerFailedToDelete.Object, _mockLocalizerCannotFind.Object);
            // act
            var result = await handler.Handle(new DeleteStreetcodeCommand(id), CancellationToken.None);
            // assert
            Assert.Equal(expectedErrorMessage, result.Errors.Single().Message);
        }

        private void RepositorySetup(StreetcodeContent streetcodeContent, int saveChangesVariable)
        {
            _repository.Setup(x => x.StreetcodeRepository.Delete(streetcodeContent));
            _repository.Setup(x => x.SaveChangesAsync()).ReturnsAsync(saveChangesVariable);
            _repository.Setup(x => x.StreetcodeRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
                 It.IsAny<Func<IQueryable<StreetcodeContent>,
                    IIncludableQueryable<StreetcodeContent, object>>>()))
                .ReturnsAsync(streetcodeContent);

        }
    }
}
