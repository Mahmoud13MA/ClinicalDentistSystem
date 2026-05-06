using AutoMapper;
using clinical.APIs.Modules.DentalClinic.Models;
using clinical.APIs.Modules.ProsthodonticLab.DTOs;

namespace clinical.APIs.Modules.ProsthodonticLab.MappingProfiles
{
    public class ProsthodonticLabMappingProfile : Profile
    {
        public ProsthodonticLabMappingProfile()
        {
            // Lab Technician Auth mappings
            CreateMap<LabTechnicianRegisterRequest, LabTechnician>();
            CreateMap<LabTechnician, LabTechnicianLoginResponse>();

            // Lab Technician mappings
            CreateMap<LabTechnicianCreateRequest, LabTechnician>();
            CreateMap<LabTechnicianUpdateRequest, LabTechnician>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<LabTechnician, LabTechnicianResponse>();

            // Order mappings
            CreateMap<OrderCreateRequest, Order>();
            CreateMap<OrderUpdateRequest, Order>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Order, OrderResponse>();

            // Prescription mappings
            CreateMap<PrescriptionCreateRequest, Prescription>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Pending"));
            CreateMap<PrescriptionUpdateRequest, Prescription>()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Prescription, PrescriptionResponse>();
        }
    }
}
