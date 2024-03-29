using AutoMapper;
using EQ_Shared;

namespace EQ_Server
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<EQ_Server.Models.Queue, Number>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Number));

        }
    }
}