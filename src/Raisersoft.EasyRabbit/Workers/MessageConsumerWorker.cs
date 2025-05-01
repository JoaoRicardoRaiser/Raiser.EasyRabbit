using Microsoft.Extensions.Hosting;
using Raisersoft.EasyRabbit.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Raisersoft.EasyRabbit.Workers;

public class MessageConsumerWorker<T>(IMessageConsumerService<T> messageConsumerService) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        => await messageConsumerService.ConsumeAsync();
}