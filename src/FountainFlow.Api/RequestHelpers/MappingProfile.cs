using System;
using AutoMapper;
using FountainFlow.Api.DTOs;
using FountainFlow.Api.Entities;

namespace FountainFlow.Api.RequestHelpers;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Map Archetype to ArchetypeReadDto and vice versa
        CreateMap<Archetype, ArchetypeReadDto>()
            .ForMember(dest => dest.ArchetypeBeatIds, opt => opt.MapFrom(src => src.ArchetypeBeats))
            .ForMember(dest => dest.ArchetypeGenreIds, opt => opt.MapFrom(src => src.ArchetypeGenres));

        CreateMap<ArchetypeDto, Archetype>()
            .ForMember(dest => dest.ArchetypeBeats, opt => opt.Ignore())
            .ForMember(dest => dest.ArchetypeGenres, opt => opt.Ignore());
    }
}
