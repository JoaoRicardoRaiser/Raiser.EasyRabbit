using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Raiser.EasyRabbit.Interfaces;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Raiser.EasyRabbit.Services;

public class MessageConsumerService<T> : IMessageConsumerService<T>
{
    private readonly string _queue;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IRabbitMqService _rabbitMqService;

    public MessageConsumerService(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory, IRabbitMqService rabbitMqService, string consumerConfigKey)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _rabbitMqService = rabbitMqService;

        _rabbitMqService.ConfigureConsumerAsync(consumerConfigKey);
        
        _queue = configuration[$"RabbitMq:Consumers:{consumerConfigKey}:Queue"];
    }

    public async Task ConsumeAsync()
    {
        var scope = _serviceScopeFactory.CreateScope();

        var messageHandler = scope.ServiceProvider.GetRequiredService<IMessageHandler<T>>();
        
        var channel = await _rabbitMqService.CreateChannelAsync();
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

        await channel.BasicConsumeAsync(queue: _queue, autoAck: false, consumer: consumer);
    }
}