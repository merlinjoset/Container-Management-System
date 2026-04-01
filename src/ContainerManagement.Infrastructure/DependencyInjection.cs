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
            services.AddScoped<IVesselsRepository, VesselsRepository>();
            services.AddScoped<IVendorsRepository, VendorsRepository>();
            services.AddScoped<IOperatorsRepository, OperatorsRepository>();
            services.AddScoped<IServicesRepository, ServicesRepository>();
            services.AddScoped<IRoutesRepository, RoutesRepository>();
            services.AddScoped<IDistancesRepository, DistancesRepository>();
            services.AddScoped<ISlotsRepository, SlotsRepository>();
            services.AddScoped<IVoyagesRepository, VoyagesRepository>();
            services.AddScoped<IVoyagePortsRepository, VoyagePortsRepository>();
            services.AddScoped<IVoyagePortArrivalsRepository, VoyagePortArrivalsRepository>();
            services.AddScoped<IVoyagePortDeparturesRepository, VoyagePortDeparturesRepository>();
            services.AddScoped<IJobsRepository, JobsRepository>();
            services.AddScoped<IJobAttachmentsRepository, JobAttachmentsRepository>();
            services.AddScoped<ITugUsageRepository, TugUsageRepository>();
            services.AddScoped<IBunkerOnArrivalRepository, BunkerOnArrivalRepository>();
            services.AddScoped<IBunkerOnDepartureRepository, BunkerOnDepartureRepository>();
            services.AddScoped<IBunkerSupplyRepository, BunkerSupplyRepository>();

            return services;
        }
    }
}
