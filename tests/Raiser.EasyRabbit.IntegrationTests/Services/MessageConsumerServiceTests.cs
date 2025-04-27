using Raiser.EasyRabbit.IntegrationTests.Fixtures;

namespace Raiser.EasyRabbit.IntegrationTests.Services;

[Collection(nameof(RaiserEasyRabbitMqCollectionFixture))]
public class MessageConsumerServiceTests(ApplicationFixture appFixture, RabbitMqFixture rabbitMqFixture)
{

}
