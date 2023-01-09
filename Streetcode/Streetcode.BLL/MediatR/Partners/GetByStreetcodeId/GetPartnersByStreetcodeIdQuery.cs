﻿using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Partners;

namespace Streetcode.BLL.MediatR.Partners.GetByStreetcodeId;

public record GetPartnersByStreetcodeIdQuery(int streetcodeId) : IRequest<Result<IEnumerable<PartnerDTO>>>;
