using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using Raisersoft.EasyRabbit.Interfaces;
using Raisersoft.EasyRabbit.Services;
using Raisersoft.EasyRabbit.Workers;
using System;

namespace Raisersoft.EasyRabbit.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddEasyRabbitMq(this IServiceCollection services)
    {
        services.AddSingleton(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();

            var factory = new ConnectionFactory
            {
                HostName = configuration[$"RabbitMq:Infrastructure:HostName"],
                UserName = configuration[$"RabbitMq:Infrastructure:UserName"],
                Password = configuration[$"RabbitMq:Infrastructure:Password"],
                Port = int.Parse(configuration[$"RabbitMq:Infrastructure:Port"])
            };

            var policy = Policy
                .Handle<BrokerUnreachableException>()
                .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(5));

            IConnection connection = default!;

            policy.Execute(async () =>
            {
                connection = await factory.CreateConnectionAsync();

            }).Wait();

            return connection;
        });

        services.AddSingleton<IRabbitMqService, RabbitMqService>();

        return services;
    }

    public static IServiceCollection AddConsumer<T>(this IServiceCollection services, string consumerConfigKey)
    {
        var serviceProvider = services.BuildServiceProvider();

        services.AddSingleton<IMessageConsumerService<T>>(new MessageConsumerService<T>(
            serviceProvider.GetRequiredService<IConfiguration>(),
            serviceProvider.GetRequiredService<IServiceScopeFactory>(), 
            serviceProvider.GetRequiredService<IRabbitMqService>(), 
            consumerConfigKey));

        services.AddHostedService<MessageConsumerWorker<T>>();

        return services;
    }

    public static IServiceCollection AddPublisher<T>(this IServiceCollection services, string publisherConfigKey)
    {
        services.AddSingleton<IMessagePublisherService<T>>(new MessagePublisherService<T>(services, publisherConfigKey));
        return services;
    }
}
