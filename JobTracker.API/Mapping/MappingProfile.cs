using System;
using AutoMapper;
using JobTracker.API.Dtos;
using JobTracker.API.Models;

namespace JobTracker.API.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ---------- Company ----------
            CreateMap<Company, CompanyDto>();
            CreateMap<CompanyDto, Company>();

            // ---------- Job ----------
            CreateMap<Job, JobReadDto>()
                .ForMember(dest => dest.CompanyName,
                    opt => opt.MapFrom(src =>
                        src.Company != null ? src.Company.Name : null));

            CreateMap<JobCreateDto, Job>();

            // ---------- JobApplication ----------
            CreateMap<JobApplication, JobApplicationReadDto>()
                .ForMember(dest => dest.CompanyName,
                    opt => opt.MapFrom(src =>
                        src.Company != null ? src.Company.Name : null));

            CreateMap<JobApplicationCreateDto, JobApplication>()
                .ForMember(dest => dest.AppliedDate,
                    opt => opt.MapFrom(src =>
                        src.AppliedDate ?? DateTime.UtcNow));

            // ---------- Interview ----------
            CreateMap<Interview, InterviewReadDto>();
            CreateMap<InterviewCreateDto, Interview>();
        }
    }
}
