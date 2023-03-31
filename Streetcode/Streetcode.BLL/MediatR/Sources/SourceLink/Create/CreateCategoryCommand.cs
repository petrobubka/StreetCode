﻿using FluentResults;
using MediatR;
using Streetcode.BLL.DTO.Sources;
using Streetcode.BLL.DTO.Streetcode.TextContent;

namespace Streetcode.BLL.MediatR.Sources.SourceLink.Create
{
    public record CreateCategoryCommand(SourceLinkDTO Category) : IRequest<Result<Unit>>;
}
