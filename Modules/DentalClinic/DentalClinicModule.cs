using clinical.APIs.Modules.DentalClinic.Services;

namespace clinical.APIs.Modules.DentalClinic
{
    public static class DentalClinicModule
    {

        public static IServiceCollection AddDentalClinicModule(this IServiceCollection services)
        { 

            // Mappings
            services.AddScoped<IEHRChangeLogService, EHRChangeLogService>();
            services.AddScoped<IEHRMappingService, EHRMappingService>();
            services.AddScoped<IAppointmentMappingService, AppointmentMappingService>();
            services.AddScoped<IPatientMappingService, PatientMappingService>();
            services.AddScoped<INurseMappingService,NurseMappingService>();
            services.AddScoped<IDoctorMappingService, DoctorMappingService>();
            services.AddScoped<IStockTransactionMappingService, StockTransactionMappingService>();

            // AI
            services.AddHttpClient<ILlamaService, LlamaService>();
            services.AddScoped<ILlamaService, LlamaService>();
            services.AddSingleton<OllamaManager>();



            return services;
        }







    }
}
