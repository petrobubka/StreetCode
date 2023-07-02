﻿using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.AdditionalContent.Coordinates.Types;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.AdditionalContent.Coordinate.GetByStreetcodeId;

public class GetCoordinatesByStreetcodeIdHandler : IRequestHandler<GetCoordinatesByStreetcodeIdQuery, Result<IEnumerable<StreetcodeCoordinateDTO>>>
{
    private readonly IMapper _mapper;
    private readonly IRepositoryWrapper _repositoryWrapper;
    private readonly ILoggerService _logger;

    public GetCoordinatesByStreetcodeIdHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
    {
        _repositoryWrapper = repositoryWrapper;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<StreetcodeCoordinateDTO>>> Handle(GetCoordinatesByStreetcodeIdQuery request, CancellationToken cancellationToken)
    {
        var coordinates = await _repositoryWrapper.StreetcodeCoordinateRepository
            .GetAllAsync(c => c.StreetcodeId == request.StreetcodeId);

        if (coordinates is null)
        {
            string errorMsg = $"Cannot find a coordinates by a streetcode id: {request.StreetcodeId}";
            _logger?.LogError("GetCoordinatesByStreetcodeIdQuery handled with an error");
            _logger?.LogError(errorMsg);
            return Result.Fail(new Error(errorMsg));
        }

        var coordinatesDto = _mapper.Map<IEnumerable<StreetcodeCoordinateDTO>>(coordinates);
        _logger?.LogInformation($"GetCoordinatesByStreetcodeIdQuery handled successfully");
        return Result.Ok(coordinatesDto);
    }
}
