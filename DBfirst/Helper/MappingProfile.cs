using AutoMapper;
using DBfirst.Data.DTOs;
using DBfirst.Models;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Ánh xạ từ StudentDTO đến Student
        CreateMap<StudentDTO, Student>()
            .ForMember(dest => dest.StudentDetails, opt => opt.MapFrom(src => src.StudentDetails))
            .ForMember(dest => dest.Evaluations, opt => opt.Ignore()); // Nếu bạn không muốn cập nhật Evaluations

        // Ánh xạ từ Student đến StudentDTO
        CreateMap<Student, StudentDTO>();

        // Ánh xạ từ StudentDetailDTO đến StudentDetail
        CreateMap<StudentDetailDTO, StudentDetail>()
            .ForMember(dest => dest.StudentDetailsId, opt => opt.Ignore()) // Nếu không cần ánh xạ ID
            .ForMember(dest => dest.StudentId, opt => opt.Ignore()); // Nếu không cần ánh xạ StudentId

        // Ánh xạ từ StudentDetail đến StudentDetailDTO
        CreateMap<StudentDetail, StudentDetailDTO>();

        // Ánh xạ từ EvaluationDTO đến Evaluation
        CreateMap<EvaluationDTO, Evaluation>();

        // Ánh xạ từ Evaluation đến EvaluationDTO
        CreateMap<Evaluation, EvaluationDTO>();
    }
}
