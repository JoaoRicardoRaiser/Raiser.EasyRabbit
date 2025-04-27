namespace Raiser.EasyRabbit.IntegrationTests.Fixtures;

[CollectionDefinition(nameof(RaiserEasyRabbitMqCollectionFixture))]
public class RaiserEasyRabbitMqCollectionFixture :
    ICollectionFixture<RabbitMqFixture>,
    ICollectionFixture<ApplicationFixture>
{
}
