using Microsoft.Extensions.Hosting;
using Raiser.EasyRabbit.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Raiser.EasyRabbit.Workers;

public class MessageConsumerWorker<T>(IMessageConsumerService<T> messageConsumerService) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        => await messageConsumerService.ConsumeAsync();
}