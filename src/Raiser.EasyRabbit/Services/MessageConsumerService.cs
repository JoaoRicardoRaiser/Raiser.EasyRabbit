using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Raiser.EasyRabbit.Interfaces;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Raiser.EasyRabbit.Services;

public class MessageConsumerService<T>(IServiceScopeFactory serviceScopeFactory, string consumerConfigKey) : IMessageConsumerService<T>
{
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    public async Task ConsumeAsync()
    {
        var scope = _serviceScopeFactory.CreateScope();

        var rabbitMqService = scope.ServiceProvider.GetRequiredService<IRabbitMqService>();

        await rabbitMqService.ConfigureConsumerAsync(consumerConfigKey);

        var messageHandler = scope.ServiceProvider.GetRequiredService<IMessageHandler<T>>();
        
        var channel = await rabbitMqService.CreateChannelAsync();
        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);

                var dto = JsonSerializer.Deserialize<T>(messageJson);

                await messageHandler.HandleAsync(dto!);

                await channel.BasicAckAsync(ea.DeliveryTag, false);

            }
            catch
            {
                await channel.BasicRejectAsync(ea.DeliveryTag, false);
            }
        };

        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var queue = configuration[$"RabbitMq:Consumers:{consumerConfigKey}:Queue"];

        await channel.BasicConsumeAsync(queue: queue, autoAck: false, consumer: consumer);
    }
}