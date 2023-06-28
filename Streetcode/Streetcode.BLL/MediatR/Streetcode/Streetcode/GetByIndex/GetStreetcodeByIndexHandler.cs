﻿using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.GetByIndex;

public class GetStreetcodeByIndexHandler : IRequestHandler<GetStreetcodeByIndexQuery, Result<StreetcodeDTO>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;

    public GetStreetcodeByIndexHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService? logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<StreetcodeDTO>> Handle(GetStreetcodeByIndexQuery request, CancellationToken cancellationToken)
    {
        var streetcode = await _repositoryWrapper.StreetcodeRepository.GetFirstOrDefaultAsync(
            predicate: st => st.Index == request.Index,
            include: source => source.Include(l => l.Tags));

        if (streetcode is null)
        {
            string errorMsg = $"Cannot find any streetcode with corresponding index: {request.Index}";
            _logger?.LogError("GetStreetcodeByIndexQuery handled with an error");
            _logger?.LogError(errorMsg);
            return Result.Fail(new Error(errorMsg));
        }

        var streetcodeDto = _mapper.Map<StreetcodeDTO>(streetcode);
        _logger?.LogInformation($"GetStreetcodeByIndexQuery handled successfully");
        return Result.Ok(streetcodeDto);
    }
}