using System;
using System.Linq;
using AutoMapper;
using FountainFlow.Api.DTOs;
using FountainFlow.Api.Entities;

namespace FountainFlow.Api.RequestHelpers;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Archetype, ArchetypeReadDto>()
            .ForMember(dest => dest.ArchetypeBeatIds,
                opt => opt.MapFrom(src => src.ArchetypeBeats.Select(ab => ab.Id)))
            .ForMember(dest => dest.ArchetypeGenreIds,
                opt => opt.MapFrom(src => src.ArchetypeGenres.Select(ag => ag.Id)));

        CreateMap<ArchetypeDto, Archetype>()
            .ForMember(dest => dest.ArchetypeBeats, opt => opt.Ignore())
            .ForMember(dest => dest.ArchetypeGenres, opt => opt.Ignore());

        CreateMap<ArchetypeBeat, ArchetypeBeatReadDto>()
            .ForMember(dest => dest.ParentSequence, opt => opt.MapFrom(src => src.ParentSequence))
            .ForMember(dest => dest.ChildSequence, opt => opt.MapFrom(src => src.ChildSequence))
            .ForMember(dest => dest.GrandchildSequence, opt => opt.MapFrom(src => src.GrandchildSequence))
            .ForMember(dest => dest.Prompt, opt => opt.MapFrom(src => src.Prompt));

        CreateMap<ArchetypeBeatDto, ArchetypeBeat>()
            .ForMember(dest => dest.ParentSequence, opt => opt.MapFrom(src => src.ParentSequence))
            .ForMember(dest => dest.ChildSequence, opt => opt.MapFrom(src => src.ChildSequence))
            .ForMember(dest => dest.GrandchildSequence, opt => opt.MapFrom(src => src.GrandchildSequence))
            .ForMember(dest => dest.Prompt, opt => opt.MapFrom(src => src.Prompt))
            .ForMember(dest => dest.CreatedUTC, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedUTC, opt => opt.Ignore());

        CreateMap<ArchetypeGenre, ArchetypeGenreReadDto>();
        CreateMap<ArchetypeGenreDto, ArchetypeGenre>()
            .ForMember(dest => dest.CreatedUTC, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedUTC, opt => opt.Ignore());
    }
}
