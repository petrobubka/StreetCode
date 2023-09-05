using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.DTO.Media.Images;
using Streetcode.BLL.MediatR.Media.Image.GetAll;
using Streetcode.BLL.MediatR.Media.Image.GetBaseImage;
using Streetcode.BLL.MediatR.Media.Image.GetById;
using Streetcode.BLL.MediatR.Media.Image.GetByStreetcodeId;
using Streetcode.BLL.MediatR.Media.Image.Create;
using Streetcode.BLL.MediatR.Media.Image.Delete;
using Streetcode.BLL.MediatR.Media.Image.Update;
using Streetcode.WebApi.Attributes;
using Streetcode.DAL.Enums;
using Microsoft.Net.Http.Headers;

namespace Streetcode.WebApi.Controllers.Media.Images;

public class ImageController : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return HandleResult(await Mediator.Send(new GetAllImagesQuery()));
    }

    [HttpGet("{streetcodeId:int}")]
    [ResponseCache(Duration = 600, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetByStreetcodeId([FromRoute] int streetcodeId)
    {
        var isAdmin = HttpContext.User.IsInRole("MainAdministrator");
        if (!isAdmin)
        {
            return HandleResult(await Mediator.Send(new GetImageByStreetcodeIdQuery(streetcodeId)));
        }
        else
        {
            Response.Headers[HeaderNames.CacheControl] = "no-store, no-cache";
            return HandleResult(await Mediator.Send(new GetImageByStreetcodeIdQuery(streetcodeId)));
        }
    }

    [HttpGet("{id:int}")]
    [ResponseCache(Duration = 600, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var isAdmin = HttpContext.User.IsInRole("MainAdministrator");
        if (!isAdmin)
        {
            return HandleResult(await Mediator.Send(new GetImageByIdQuery(id)));
        }
        else
        {
            Response.Headers[HeaderNames.CacheControl] = "no-store, no-cache";
            return HandleResult(await Mediator.Send(new GetImageByIdQuery(id)));
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ImageFileBaseCreateDTO image)
    {
        return HandleResult(await Mediator.Send(new CreateImageCommand(image)));
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] ImageFileBaseUpdateDTO image)
    {
        return HandleResult(await Mediator.Send(new UpdateImageCommand(image)));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        return HandleResult(await Mediator.Send(new DeleteImageCommand(id)));
    }

    [HttpGet("{id:int}")]
    [ResponseCache(Duration = 600, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetBaseImage([FromRoute] int id)
    {
        return HandleResult(await Mediator.Send(new GetBaseImageQuery(id)));
    }
}
