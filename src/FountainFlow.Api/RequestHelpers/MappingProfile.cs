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

        CreateMap<ArchetypeBeat, ArchetypeBeatReadDto>();
        CreateMap<ArchetypeBeatDto, ArchetypeBeat>()
            .ForMember(dest => dest.CreatedUTC, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedUTC, opt => opt.Ignore());

        CreateMap<ArchetypeGenre, ArchetypeGenreReadDto>();
        CreateMap<ArchetypeGenreDto, ArchetypeGenre>()
            .ForMember(dest => dest.CreatedUTC, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedUTC, opt => opt.Ignore());
    }
}
