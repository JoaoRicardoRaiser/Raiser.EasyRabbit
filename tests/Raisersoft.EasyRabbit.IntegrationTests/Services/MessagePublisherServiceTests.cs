using System.Text;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Raisersoft.EasyRabbit.IntegrationTests.Fixtures;
using Raisersoft.EasyRabbit.Interfaces;

namespace Raisersoft.EasyRabbit.IntegrationTests.Services;

[Collection(nameof(RaiserEasyRabbitMqCollectionFixture))]
public class MessagePublisherServiceTests(ApplicationFixture appFixture, RabbitMqFixture rabbitMqFixture)
{

    [Fact]
    public async Task ShouldPublishMessageOnExchange()
    {
        // Arrange
        var channel = await rabbitMqFixture.RabbitMqService.CreateChannelAsync();

        var messagePublisher = appFixture.ServiceProvider.GetRequiredService<IMessagePublisherService<TestClass>>();

        var queue = await channel.QueueDeclareAsync($"queue_test_{Guid.NewGuid()}", true, true, false);
        await channel.QueueBindAsync(queue.QueueName, "test_exchange", "*");

        var messageToPublish = new TestClass { Name = "Test Name"};

        // Act
        await messagePublisher.SendMessageAsync(messageToPublish);

        // Assert
        var messageOnQueue = await channel.BasicGetAsync(queue.QueueName, true);

        var messageString = Encoding.UTF8.GetString(messageOnQueue!.Body.ToArray());
        var messageOnQueueObject = JsonConvert.DeserializeObject<TestClass>(messageString);

        messageOnQueueObject.Should().BeEquivalentTo(messageToPublish);
    }
}
