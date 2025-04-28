using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Raiser.EasyRabbit.IntegrationTests.Fixtures;
using Raiser.EasyRabbit.Interfaces;

namespace Raiser.EasyRabbit.IntegrationTests.Services;

[Collection(nameof(RaiserEasyRabbitMqCollectionFixture))]
public class MessageConsumerServiceTests(ApplicationFixture appFixture, RabbitMqFixture rabbitMqFixture)
{

    [Fact]
    public async Task Should_Consume_Message_Successfully()
    {
        // Arrange
        var messagePublisher = appFixture.ServiceProvider.GetRequiredService<IMessagePublisherService<TestClass>>();

        var message = new TestClass { Name = "Test Name" };

        // Act 
        await messagePublisher.SendMessageAsync(message);
        var countMessagesOnQueue = await rabbitMqFixture.GetCountFromQueueAsync("test_queue");

        countMessagesOnQueue.Should().Be(1);

        var messageConsumer = appFixture.ServiceProvider.GetRequiredService<IMessageConsumerService<TestClass>>();
        await messageConsumer.ConsumeAsync();
        
        await Task.Delay(TimeSpan.FromSeconds(3));

        // Assert

        countMessagesOnQueue = await rabbitMqFixture.GetCountFromQueueAsync("test_queue");
        countMessagesOnQueue.Should().Be(0);
    }
}
