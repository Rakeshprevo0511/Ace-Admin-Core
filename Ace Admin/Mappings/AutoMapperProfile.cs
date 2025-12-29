using Ace_Admin.Dto;
using Ace_Admin.Models;
using AutoMapper;

namespace Ace_Admin.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Example mapping configuration
            CreateMap<Employee, EmpProfileDto>().ReverseMap();
            CreateMap<Employee, EmpProfileDto>()
          .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.EmpName))
          .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
          .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.PhoneNumber))
          .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.FilePathPic))
          .ForMember(dest => dest.Designation, opt => opt.MapFrom(src => src.Position))
          .ForMember(dest => dest.WalletAmount,
              opt => opt.MapFrom(src => src.Wallet != null ? (decimal)src.Wallet.Balance : 0))
          .ForMember(dest => dest.Courses,
              opt => opt.MapFrom(src => src.EmployeeCourses
                  .Where(ec => ec.Status == "Completed"))) // ✅ Only completed
          .ReverseMap();

            // EmployeeCourse → EmployeeCourseDto
            CreateMap<EmployeeCourse, EmployeeCourseDto>()
                .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Course.CourseName))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.CompletionDate, opt => opt.MapFrom(src => src.CompletionDate))
                .ForMember(dest => dest.StudyHours, opt => opt.MapFrom(src => src.StudyHours));
            CreateMap<UserMessage, UserMessage>();
        }
    }
}
