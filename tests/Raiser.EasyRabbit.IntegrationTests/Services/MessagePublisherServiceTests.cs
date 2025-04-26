using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Raiser.EasyRabbit.Interfaces;
using Raiser.EasyRabbit.Extensions;
using System.Text;
using RabbitMQ.Client;
using System.Text.Json;
using Raiser.EasyRabbit.Workers;
using Testcontainers.RabbitMq;

namespace Raiser.EasyRabbit.IntegrationTests.Services;

public class MessagePublisherServiceTests : IAsyncLifetime
{
    private IHost _host;
    private IRabbitMqService _rabbitMqService;
    private IConnection _connection;
    private MessageConsumerWorker<TestClass> _messageConsumerWorker;

    private readonly RabbitMqContainer _dbContainer = new RabbitMqBuilder()
            .WithImage("rabbitmq:3-management")
            .WithUsername("guest")
            .WithPassword("guest")
            .WithPortBinding(15672, 15672)
            .WithPortBinding(5672, 5672)
            .Build();

    public async Task InitializeAsync()
    {
        _dbContainer.StartAsync().Wait();

        _host = Host.CreateDefaultBuilder()
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

        await _host.StartAsync();

        _rabbitMqService = _host.Services.GetRequiredService<IRabbitMqService>();
    }

    [Fact]
    public async Task PublishAndConsumeRealMessage()
    {
        // Arrange
        await _rabbitMqService.ConfigureConsumerAsync("TestConsumer");

        var channel = await _connection.CreateChannelAsync();
        var message = new TestClass { Name = "Test Name"};
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        // Act
        await channel.BasicPublishAsync(exchange: "test_exchange", routingKey: "test_routing_key", body: body);

        await _messageConsumerWorker.StartAsync(CancellationToken.None);
        await Task.Delay(TimeSpan.FromSeconds(5));
        await _messageConsumerWorker.StopAsync(CancellationToken.None);

        var messageOnQueue = await channel.BasicGetAsync("test_queue", true);

        // Assert
        Assert.Null(messageOnQueue);

    }

    public async Task DisposeAsync()
    {
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
        await _dbContainer.StopAsync();
    }
}

public class TestClass
{
    public string Name { get; set; } = default!;
}

public class TestClassMessageHandler : IMessageHandler<TestClass>
{
    public Task HandleAsync(TestClass message)
    {
        return Task.CompletedTask;
    }
}
