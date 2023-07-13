﻿using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Localization;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent;
using Streetcode.BLL.DTO.Transactions;
using Streetcode.BLL.MediatR.AdditionalContent.Tag.GetAll;
using Streetcode.BLL.MediatR.Transactions.TransactionLink.GetAll;
using Streetcode.BLL.SharedResource;
using Streetcode.DAL.Entities.AdditionalContent;
using Streetcode.DAL.Entities.Transactions;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using Xunit;

namespace Streetcode.XUnitTest.MediatRTests.Transactions.TransactionsTests.TransactionLinkTests
{
    public class GetAllTransactLinksHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _mockRepo;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IStringLocalizer<CannotFindSharedResource>> _mockLocalizerCannotFind;

        public GetAllTransactLinksHandlerTests()
        {
            _mockMapper = new Mock<IMapper>();
            _mockRepo = new Mock<IRepositoryWrapper>();
            _mockLocalizerCannotFind = new Mock<IStringLocalizer<CannotFindSharedResource>>();
        }

        async Task SetupRepository(List<TransactionLink> returnList)
        {
            _mockRepo.Setup(repo => repo.TransactLinksRepository.GetAllAsync(
                It.IsAny<Expression<Func<TransactionLink, bool>>>(), It.IsAny<Func<IQueryable<TransactionLink>,
                IIncludableQueryable<TransactionLink, object>>>())).ReturnsAsync(returnList);
        }

        async Task SetupMapper(List<TransactLinkDTO> returnList)
        {
            _mockMapper.Setup(x => x.Map<IEnumerable<TransactLinkDTO>>(It.IsAny<IEnumerable<object>>())).Returns(returnList);
        }

        [Fact]
        public async Task NotEmpty_List()
        {
            //Arrange
            await SetupRepository(transactions);
            await SetupMapper(transactionsDTOs);

            var handler = new GetAllTransactLinksHandler(_mockRepo.Object, _mockMapper.Object, _mockLocalizerCannotFind.Object);

            //Act
            var result = await handler.Handle(new GetAllTransactLinksQuery(), CancellationToken.None);

            //Assert
            Assert.Multiple(
                () => Assert.IsType<List<TransactLinkDTO>>(result.Value),
                () => Assert.True(result.Value.Count() == transactions.Count));
        }

        [Fact]
        public async Task Empty_List()
        {
            //Arrange
            await SetupRepository(new List<TransactionLink>());
            await SetupMapper(new List<TransactLinkDTO>());

            var handler = new GetAllTransactLinksHandler(_mockRepo.Object, _mockMapper.Object, _mockLocalizerCannotFind.Object);

            //Act
            var result = await handler.Handle(new GetAllTransactLinksQuery(), CancellationToken.None);

            //Assert
            Assert.Multiple(
                () => Assert.IsType<List<TransactLinkDTO>>(result.Value),
                () => Assert.Empty(result.Value));
        }

        private readonly List<TransactionLink> transactions = new List<TransactionLink>()
        {
            new TransactionLink
            {
                Id = 1,
                Url = "URL",
                UrlTitle = "Title",
                StreetcodeId = 1
            },
            new TransactionLink
            {
                Id = 2,
                Url = "URL2",
                UrlTitle = "Title2",
                StreetcodeId = 2
            }
        };

        private readonly List<TransactLinkDTO> transactionsDTOs = new List<TransactLinkDTO>()
        {
            new TransactLinkDTO
            {
                Id = 1,
                Url = new UrlDTO{ Href = "URL", Title = "Title" },
                QrCodeUrl = new UrlDTO{ Href = "URL", Title = "Title" },
                StreetcodeId = 1
            },
            new TransactLinkDTO
            {
                Id = 2,
                Url = new UrlDTO{ Href = "URL2", Title = "Title2" },
                QrCodeUrl = new UrlDTO{ Href = "URL2", Title = "Title2" },
                StreetcodeId = 2
            }
        };
    }
}