using ContainerManagement.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ContainerManagement.Application
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<UserAdminService>();
            services.AddScoped<PortService>();
            services.AddScoped<RegionService>();
            services.AddScoped<CountryService>();
            services.AddScoped<TerminalService>();
            services.AddScoped<VesselService>();

            return services;
        }
    }
}
