using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raisersoft.EasyRabbit.Extensions;
using Raisersoft.EasyRabbit.Interfaces;
using Raisersoft.EasyRabbit.Workers;

namespace Raisersoft.EasyRabbit.IntegrationTests.Fixtures;

public class ApplicationFixture
{
    public IServiceProvider ServiceProvider { get; set; }

    public ApplicationFixture()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(configBuilder =>
            {
                configBuilder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appSettings.IntegrationTests.json");
            })
            .ConfigureServices((context, services) =>
            {
                services.AddEasyRabbitMq();
                
                services.AddPublisher<TestClass>("Test");

                services.AddScoped<IMessageHandler<TestClass>, TestClassMessageHandler>();
                services.AddConsumer<TestClass>("Test");

                RemoveIHostServices(services);
            })
            .Build();

        ServiceProvider = host.Services;
        
        host.StartAsync().Wait();
    }

    private static void RemoveIHostServices(IServiceCollection services)
    {
        var descriptorsToRemove = services
            .Where(d => typeof(IHostedService).IsAssignableFrom(d.ServiceType))
            .ToList();

        foreach (var descriptor in descriptorsToRemove)
            services.Remove(descriptor);
    }
}
