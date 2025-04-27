using Raiser.EasyRabbit.Interfaces;

namespace Raiser.EasyRabbit.IntegrationTests;

public class TestClassMessageHandler : IMessageHandler<TestClass>
{
    public Task HandleAsync(TestClass message)
    {
        return Task.CompletedTask;
    }
}
