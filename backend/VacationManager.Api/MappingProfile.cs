using AutoMapper;
using VacationManager.Core.DTOs;
using VacationManager.Core.Entities;

namespace VacationManager.Api;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Vacation mappings with custom UserName mapping
        CreateMap<Vacation, VacationDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.DisplayName : null));
        CreateMap<CreateVacationDto, Vacation>();
        CreateMap<UpdateVacationDto, Vacation>();

        // User mappings
        CreateMap<User, UserDto>();
        CreateMap<CreateUserDto, User>();
        CreateMap<UpdateUserDto, User>();

        // Team mappings
        CreateMap<Team, TeamDto>();
        CreateMap<CreateTeamDto, Team>();
        CreateMap<UpdateTeamDto, Team>();
    }
}
