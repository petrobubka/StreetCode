﻿using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Microsoft.EntityFrameworkCore;

namespace Streetcode.BLL.MediatR.Media.Art.GetByStreetcodeId
{
    public class GetArtsByStreetcodeIdHandler : IRequestHandler<GetArtsByStreetcodeIdQuery, Result<IEnumerable<ArtDTO>>>
    {
        private readonly IBlobService _blobService;
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repositoryWrapper;

        public GetArtsByStreetcodeIdHandler(
            IRepositoryWrapper repositoryWrapper,
            IMapper mapper,
            IBlobService blobService)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _blobService = blobService;
        }

        public async Task<Result<IEnumerable<ArtDTO>>> Handle(GetArtsByStreetcodeIdQuery request, CancellationToken cancellationToken)
        {
            var arts = await _repositoryWrapper.ArtRepository
                .GetAllAsync(
                predicate: sc => sc.StreetcodeArts.Any(s => s.StreetcodeId == request.StreetcodeId),
                include: scl => scl
                    .Include(sc => sc.Image) !);

            if (arts is null)
            {
                return Result.Fail(new Error($"Cannot find any art with corresponding streetcode id: {request.StreetcodeId}"));
            }

            var imageDetailsIds = arts.Where(a => a.Image != null).Select(a => a.Image!.ImageDetailsId);

            var imageDetailsTask = _repositoryWrapper.ImageDetailsRepository.GetAllAsync(i => imageDetailsIds.Contains(i.Id));

            var artsDto = _mapper.Map<IEnumerable<ArtDTO>>(arts);
            var imageDetails = await imageDetailsTask;
            foreach (var artDto in artsDto)
            {
                if (artDto.Image != null && artDto.Image.BlobName != null)
                {
                    artDto.Image.Base64 = _blobService.FindFileInStorageAsBase64(artDto.Image.BlobName);
                    if(artDto.Image.ImageDetailsId != 0)
                    {
                        artDto.Image.ImageDetails = _mapper.Map<ImageDetailsDto?>(imageDetails.FirstOrDefault(d => d.Id == artDto.Image.ImageDetailsId));
                    }
                }
            }

            return Result.Ok(artsDto);
        }
    }
}
