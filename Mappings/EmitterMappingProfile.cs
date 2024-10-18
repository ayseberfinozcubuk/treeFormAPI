using AutoMapper;
using tree_form_API.Models;
using tree_form_API.Dtos;

namespace tree_form_API.Mappings
{
    public class EmitterMappingProfile : Profile
    {
        public EmitterMappingProfile()
        {
            // Map DTO to Domain
            CreateMap<EmitterDto, Emitter>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()); // Ignore Id

            CreateMap<EmitterModeDto, EmitterMode>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.EmitterId, opt => opt.Ignore()); // Ignore Ids and relations

            CreateMap<EmitterModeBeamDto, EmitterModeBeam>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.EmitterId, opt => opt.Ignore());

            CreateMap<EmitterModeBeamPositionDwellDurationValueDto, EmitterModeBeamPositionDwellDurationValue>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.EmitterModeBeamId, opt => opt.Ignore());

            CreateMap<EmitterModeBeamPositionSequenceDto, EmitterModeBeamPositionSequence>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.EmitterModeBeamId, opt => opt.Ignore());

            CreateMap<EmitterModeBeamPositionFiringOrderDto, EmitterModeBeamPositionFiringOrder>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.EmitterModeBeamPositionSequenceId, opt => opt.Ignore());

            // Map Domain to DTO
            CreateMap<Emitter, EmitterDto>();
            CreateMap<EmitterMode, EmitterModeDto>();
            CreateMap<EmitterModeBeam, EmitterModeBeamDto>();
            CreateMap<EmitterModeBeamPositionDwellDurationValue, EmitterModeBeamPositionDwellDurationValueDto>();
            CreateMap<EmitterModeBeamPositionSequence, EmitterModeBeamPositionSequenceDto>();
            CreateMap<EmitterModeBeamPositionFiringOrder, EmitterModeBeamPositionFiringOrderDto>();
        }
    }
}
