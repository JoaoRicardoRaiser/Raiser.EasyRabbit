using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raiser.EasyRabbit.Extensions;
using Raiser.EasyRabbit.Interfaces;

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
                services.AddConsumer<TestClass>("Test");

                services.AddScoped<IMessageHandler<TestClass>, TestClassMessageHandler>();
            })
            .Build();

        ServiceProvider = host.Services;

        host.StartAsync().Wait();
    }
}
