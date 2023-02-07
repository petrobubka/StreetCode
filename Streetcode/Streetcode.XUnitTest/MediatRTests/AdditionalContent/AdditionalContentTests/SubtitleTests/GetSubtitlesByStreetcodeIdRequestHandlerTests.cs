﻿using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.AdditionalContent.Subtitles;
using Streetcode.BLL.MediatR.AdditionalContent.Subtitle.GetByStreetcodeId;
using Streetcode.DAL.Entities.AdditionalContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;

namespace Streetcode.XUnitTest.MediatRTests.AdditionalContent.SubtitleTests
{
    public class GetSubtitlesByStreetcodeIdRequestHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _mockRepo;
        private readonly Mock<IMapper> _mockMapper;

        public GetSubtitlesByStreetcodeIdRequestHandlerTests()
        {
            _mockRepo = new Mock<IRepositoryWrapper>();
            _mockMapper = new Mock<IMapper>();
        }

        private const int _streetcode_id = 1;

        private readonly List<Subtitle> subtitles = new List<Subtitle>
        {
            new Subtitle
            {
                Id = 1,
                StreetcodeId = 1
            },
            new Subtitle
            {
                Id = 2,
                StreetcodeId = 1
            }
        };
        private readonly List<SubtitleDTO> subtitlesDTO = new List<SubtitleDTO>
        {
            new SubtitleDTO
            {
                Id = 1,
                StreetcodeId = 1
            },
            new SubtitleDTO
            {
                Id = 2,
                StreetcodeId = 1
            }
        };

        [Fact]
        public async Task GetSubtitlesByStreetcodeId_ReturnsList()
        {
            //Arrange
            _mockRepo.Setup(repo => repo.SubtitleRepository.GetAllAsync(
                It.IsAny<Expression<Func<Subtitle, bool>>>(),
                It.IsAny<Func<IQueryable<Subtitle>,
                IIncludableQueryable<Subtitle, object>>>()))
                .ReturnsAsync(subtitles);

            _mockMapper.Setup(x => x.Map<IEnumerable<SubtitleDTO>>(It.IsAny<IEnumerable<object>>()))
                .Returns(subtitlesDTO);
            
            var handler = new GetSubtitlesByStreetcodeIdQueryHandler(_mockRepo.Object, _mockMapper.Object);

            //Act
            var result = await handler.Handle(new GetSubtitlesByStreetcodeIdQuery(_streetcode_id), CancellationToken.None);
            
            //Assert
            Assert.NotNull(result.Value);

            Assert.IsType<List<SubtitleDTO>>(result.Value);

            Assert.True(result.Value.All(x => x.StreetcodeId == _streetcode_id));
        }

        [Fact]
        public async Task GetSubtitlesByStreetcodeId_ReturnsNoElements()
        {
            _mockRepo.Setup(repo => repo.SubtitleRepository.GetAllAsync(
                It.IsAny<Expression<Func<Subtitle, bool>>>(),
                It.IsAny<Func<IQueryable<Subtitle>,
                IIncludableQueryable<Subtitle, object>>>()))
                .ReturnsAsync(new List<Subtitle>()); //default value

            _mockMapper.Setup(x => x.Map<IEnumerable<SubtitleDTO>>(It.IsAny<IEnumerable<object>>()))
                .Returns(new List<SubtitleDTO>());

            var handler = new GetSubtitlesByStreetcodeIdQueryHandler(_mockRepo.Object, _mockMapper.Object);

            //Act
            var result = await handler.Handle(new GetSubtitlesByStreetcodeIdQuery(_streetcode_id), CancellationToken.None);

            //Assert
            Assert.Empty(result.Value);
        }
    }
}
