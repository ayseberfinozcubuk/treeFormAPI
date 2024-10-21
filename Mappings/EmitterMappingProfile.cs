using AutoMapper;
using tree_form_API.Dtos;
using tree_form_API.Models;

public class EmitterMappingProfile : Profile
{
    public EmitterMappingProfile()
    {
        // Map DTO to Domain (ignore Id fields)
        CreateMap<EmitterDto, Emitter>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()); // Ignore Id

        CreateMap<EmitterModeDto, EmitterMode>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.EmitterId, opt => opt.Ignore()); // Ignore Ids and relations

        CreateMap<EmitterModeBeamDto, EmitterModeBeam>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.EmitterModeId, opt => opt.Ignore());

        CreateMap<EmitterModeBeamPositionDwellDurationValueDto, EmitterModeBeamPositionDwellDurationValue>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.EmitterModeBeamId, opt => opt.Ignore());

        CreateMap<EmitterModeBeamPositionSequenceDto, EmitterModeBeamPositionSequence>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.EmitterModeBeamId, opt => opt.Ignore());

        CreateMap<EmitterModeBeamPositionFiringOrderDto, EmitterModeBeamPositionFiringOrder>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.EmitterModeBeamPositionSequenceId, opt => opt.Ignore());

        // Domain to DTO (for returning data)
        CreateMap<Emitter, EmitterDto>();
        CreateMap<EmitterMode, EmitterModeDto>();
        CreateMap<EmitterModeBeam, EmitterModeBeamDto>();
        CreateMap<EmitterModeBeamPositionDwellDurationValue, EmitterModeBeamPositionDwellDurationValueDto>();
        CreateMap<EmitterModeBeamPositionSequence, EmitterModeBeamPositionSequenceDto>();
        CreateMap<EmitterModeBeamPositionFiringOrder, EmitterModeBeamPositionFiringOrderDto>();
    }
}
