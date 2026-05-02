using AutoMapper;
using clinical.APIs.Modules.Radiology.DTOs;
using Radiology.Models;

namespace clinical.APIs.Modules.Radiology.MappingProfiles
{
    public class RadiologyMappingProfile : Profile
    {

        public RadiologyMappingProfile() {
            // Equipment
            CreateMap<Equipment, EquipmentResponse>();
            CreateMap<EquipmentCreateRequest, Equipment>();
            CreateMap<EquipmentUpdateRequest, Equipment>();
            CreateMap<Equipment, EquipmentBasicInfo>();

            //ImagingAppointment
            CreateMap<ImagingAppointment, ImagingAppointmentResponse>();
            CreateMap<ImagingAppointmentCreateRequest, ImagingAppointment>();
            CreateMap<ImagingAppointmentUpdateRequest, ImagingAppointment>();
            CreateMap<ImagingAppointment, ImagingAppointmentBasicInfo>();



            //Radiologist
            CreateMap<Radiologist, RadiologistResponse>();
            CreateMap<RadiologistCreateRequest, Radiologist>();
            CreateMap<RadiologistUpdateRequest, Radiologist>();
            CreateMap<Radiologist, RadiologistLoginResponse>();
            CreateMap<Radiologist, RadiologistBasicInfo>();


            // Report
            CreateMap<Report, ReportResponse>();
            CreateMap<ReportCreateRequest, Report>();
            CreateMap<ReportUpdateRequest, Report>();
            CreateMap<Report, ReportBasicInfo>();
        }

    }
}
