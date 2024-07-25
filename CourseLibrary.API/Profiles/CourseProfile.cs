using AutoMapper;

namespace CourseLibrary.API.Profiles;
public class CoursesProfile : Profile
{
    public CoursesProfile()
    {
        CreateMap<Entities.Course, Models.CourseDto>();
        CreateMap<Models.CourseCreationDto, Entities.Course>();
        CreateMap<Models.CourseUpdateDto, Entities.Course>().ReverseMap();
    }
}