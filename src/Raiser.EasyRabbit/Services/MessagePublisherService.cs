using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Raiser.EasyRabbit.Interfaces;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Raiser.EasyRabbit.Services;

public class MessagePublisherService<T> : IMessagePublisherService<T>
{
    private readonly IRabbitMqService _rabbitMqService;
    private readonly string _exchange;
    private readonly string _routingKey;

    public MessagePublisherService(IServiceCollection services, string publisherConfigKey)
    {
        var serviceProvider = services.BuildServiceProvider();

        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        _exchange = configuration[$"RabbitMq:Publishers:{publisherConfigKey}:Exchange"];
        _routingKey = configuration[$"RabbitMq:Publishers:{publisherConfigKey}:RoutingKey"];

        _rabbitMqService = serviceProvider.GetRequiredService<IRabbitMqService>();

        _rabbitMqService.ConfigurePublisherAsync(publisherConfigKey).Wait();
    }

    public async Task SendMessageAsync(T message)
    {
        var channel = await _rabbitMqService.CreateChannelAsync();
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        await channel.BasicPublishAsync(_exchange, _routingKey, body);
    }
}
