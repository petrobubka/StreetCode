using Streetcode.BLL.DTO.AdditionalContent;
using Streetcode.BLL.DTO.Streetcode;

namespace Streetcode.BLL.DTO.Partners;

public class PartnerDTO
{
    public int Id { get; set; }
    public bool IsKeyPartner { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public int LogoId { get; set; }
    public string TargetUrl { get; set; }
    public List<PartnerSourceLinkDTO>? PartnerSourceLinks { get; set; }
}