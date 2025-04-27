using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using Raiser.EasyRabbit.Extensions;
using Raiser.EasyRabbit.Interfaces;
using Testcontainers.RabbitMq;

namespace Raiser.EasyRabbit.IntegrationTests.Services;

public class MessagePublisherServiceTests : IAsyncLifetime
{
    private IHost _host;
    private IConnection _connection;

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
                
                services.AddScoped<IMessageHandler<TestClass>, TestClassMessageHandler>();
            })
            .Build();

        await _host.StartAsync();

        _connection = _host.Services.GetRequiredService<IConnection>();
    }

    [Fact]
    public async Task PublishAndConsumeRealMessage()
    {
        // Arrange
        var channel = await _connection.CreateChannelAsync();
        
        var queue = await channel.QueueDeclareAsync($"queue_test_{Guid.NewGuid()}", true, true, false);
        await channel.QueueBindAsync(queue.QueueName, "test_exchange", "*");

        var message = new TestClass { Name = "Test Name"};
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        // Act
        await channel.BasicPublishAsync(exchange: "test_exchange", routingKey: "test_routing_key", body: body);

        // Assert
        var messageOnQueue = await channel.BasicGetAsync(queue.QueueName, true);

        messageOnQueue!.Body.ToString().Should().BeEquivalentTo(body.ToString());
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
