﻿using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.News;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.DAL.Entities.News;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Streetcode.BLL.SharedResource;

namespace Streetcode.BLL.MediatR.Newss.GetByUrl
{
    public class GetNewsByUrlHandler : IRequestHandler<GetNewsByUrlQuery, Result<NewsDTO>>
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IBlobService _blobService;
        private readonly IStringLocalizer<NoSharedResource> _stringLocalizerNo;
        public GetNewsByUrlHandler(IMapper mapper, IRepositoryWrapper repositoryWrapper, IBlobService blobService, IStringLocalizer<NoSharedResource> stringLocalizerNo)
        {
            _mapper = mapper;
            _repositoryWrapper = repositoryWrapper;
            _blobService = blobService;
            _stringLocalizerNo = stringLocalizerNo;
        }

        public async Task<Result<NewsDTO>> Handle(GetNewsByUrlQuery request, CancellationToken cancellationToken)
        {
            string url = request.url;
            var newsDTO = _mapper.Map<NewsDTO>(await _repositoryWrapper.NewsRepository.GetFirstOrDefaultAsync(
                predicate: sc => sc.URL == url,
                include: scl => scl
                    .Include(sc => sc.Image)));
            if(newsDTO is null)
            {
                return Result.Fail(_stringLocalizerNo["NoNewsByEnteredUrl", url].Value);
            }

            if (newsDTO.Image is not null)
            {
                newsDTO.Image.Base64 = _blobService.FindFileInStorageAsBase64(newsDTO.Image.BlobName);
            }

            return Result.Ok(newsDTO);
        }
    }
}