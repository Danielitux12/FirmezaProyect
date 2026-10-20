using Firmeza.Application.Services.Clients;
using Firmeza.Application.Services.Products;
using Firmeza.Application.Services.SaleDetails;
using Firmeza.Application.Services.Sales;
using Microsoft.Extensions.DependencyInjection;

namespace Firmeza.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<ISaleService, SaleService>();
        services.AddScoped<ISaleDetailService, SaleDetailService>();

        return services;
    }
}
