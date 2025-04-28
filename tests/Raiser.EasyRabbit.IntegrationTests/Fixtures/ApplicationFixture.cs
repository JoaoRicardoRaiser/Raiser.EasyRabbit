using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raiser.EasyRabbit.Extensions;
using Raiser.EasyRabbit.Interfaces;
using Raiser.EasyRabbit.Workers;

namespace Raiser.EasyRabbit.IntegrationTests.Fixtures;

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
