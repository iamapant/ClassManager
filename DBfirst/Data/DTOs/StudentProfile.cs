using AutoMapper;
using DBfirst.Models;

namespace DBfirst.Data.DTOs
{
    public class StudentProfile : Profile
    {
        public StudentProfile()
        {
            CreateMap<CreateStudentDto, Student>()
               .ForMember(dest => dest.StudentDetails, opt => opt.MapFrom(src => src.StudentDetails));

            CreateMap<StudentDetailDto, StudentDetail>();
        }
    }
}