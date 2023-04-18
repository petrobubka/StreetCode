using AutoMapper;
using Streetcode.BLL.DTO.Streetcode;
using Streetcode.BLL.DTO.Streetcode.Create;
using Streetcode.DAL.Entities.Streetcode;

namespace Streetcode.BLL.Mapping.Streetcode;

public class StreetcodeProfile : Profile
{
    public StreetcodeProfile()
    {
        CreateMap<StreetcodeContent, StreetcodeDTO>().ReverseMap();
        CreateMap<StreetcodeContent, StreetcodeShortDTO>().ReverseMap();

        CreateMap<StreetcodeCreateDTO, StreetcodeContent>()
          .ForMember(x => x.Tags, conf => conf.Ignore())
          .ForMember(x => x.Partners, conf => conf.Ignore())
          /*.ForMember(x => x.Text, conf => conf.Ignore())*/
          .ForMember(x => x.TimelineItems, conf => conf.Ignore()).ReverseMap();
    }
}
