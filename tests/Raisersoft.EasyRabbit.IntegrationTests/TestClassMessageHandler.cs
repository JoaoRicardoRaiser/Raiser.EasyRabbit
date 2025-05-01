using Raisersoft.EasyRabbit.Interfaces;

namespace Raisersoft.EasyRabbit.IntegrationTests;

public class TestClassMessageHandler : IMessageHandler<TestClass>
{
    public Task HandleAsync(TestClass message)
    {
        return Task.CompletedTask;
    }
}
