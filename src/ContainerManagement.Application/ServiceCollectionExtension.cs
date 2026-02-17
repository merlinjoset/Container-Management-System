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

            return services;
        }
    }
}
