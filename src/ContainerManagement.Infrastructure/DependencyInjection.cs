using Microsoft.Extensions.DependencyInjection;
using ContainerManagement.Application.Abstractions;
using ContainerManagement.Infrastructure.Persistence.Repositories;

namespace ContainerManagement.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<IPortsRepository, PortsRepository>();
            services.AddScoped<IRegionsRepository, RegionsRepository>();
            services.AddScoped<ICountriesRepository, CountriesRepository>();
            services.AddScoped<ITerminalsRepository, TerminalsRepository>();

            return services;
        }
    }
}
