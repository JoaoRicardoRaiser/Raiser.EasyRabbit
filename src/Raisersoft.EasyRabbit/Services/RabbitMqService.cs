using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using Raisersoft.EasyRabbit.Exceptions;
using Raisersoft.EasyRabbit.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Raisersoft.EasyRabbit.Services;

public class RabbitMqService(IConfiguration configuration, IConnection connection) : IRabbitMqService
{
    private readonly IConfiguration _configuration = configuration;
    public readonly IConnection _connection = connection;

    public async Task<IChannel> CreateChannelAsync()
        => await _connection.CreateChannelAsync();

    public async Task ConfigureConsumerAsync(string consumerConfigKey)
    {
        ValidateConsumerKey(consumerConfigKey);

        var exchange = _configuration[$"RabbitMq:Consumers:{consumerConfigKey}:Exchange"];
        var queue = _configuration[$"RabbitMq:Consumers:{consumerConfigKey}:Queue"];

        await DeclareExchangeAsync(exchange);
        await DeclareQueueAsync(queue);
        await BindQueueAsync(
            _configuration[$"RabbitMq:Consumers:{consumerConfigKey}:Exchange"],
            _configuration[$"RabbitMq:Consumers:{consumerConfigKey}:Queue"],
            _configuration[$"RabbitMq:Consumers:{consumerConfigKey}:RoutingKey"]);
    }

    public async Task ConfigurePublisherAsync(string publisherConfigKey)
    {
        ValidatePublisherKey(publisherConfigKey);

        var exchange = _configuration[$"RabbitMq:Publishers:{publisherConfigKey}:Exchange"];

        await DeclareExchangeAsync(exchange);
    }

    private void ValidateConsumerKey(string consumerConfigKey)
    {
        IEnumerable<string> requiredPaths = [
            $"RabbitMq:Consumers:{consumerConfigKey}",
        ];

        IEnumerable<string> optionalValues = [
            "RoutingKey"
        ];

        foreach (var requiredPath in requiredPaths)
            if (_configuration.GetSection(requiredPath) is null)
                throw new SectionNotFoundException(requiredPath);

        foreach (var optionalValue in optionalValues)
        {
            var completeSection = $"RabbitMq:Consumers:{consumerConfigKey}:{optionalValue}";
            var completeDefaultSection = $"RabbitMq:Consumers:Default:{optionalValue}";

            if (_configuration[completeSection] is null && _configuration[completeDefaultSection] is null)
                throw new SectionNotFoundException(completeSection);
        }
    }

    private void ValidatePublisherKey(string publisherConfigKey)
    {
        IEnumerable<string> requiredPathValues = [
            $"RabbitMq:Publishers:{publisherConfigKey}:Exchange"
        ];

        IEnumerable<string> optionalValues = [
            "RoutingKey"
        ];

        foreach (var requiredSection in requiredPathValues)
            if (_configuration[requiredSection] is null)
                throw new SectionNotFoundException(requiredSection);

        foreach (var optionalValue in optionalValues)
        {
            var completeValuePath = $"RabbitMq:Publishers:{publisherConfigKey}:{optionalValue}";
            var completeDefaultValuePath = $"RabbitMq:Publishers:Default:{optionalValue}";

            if (_configuration[completeValuePath] is null && _configuration[completeDefaultValuePath] is null)
                throw new SectionNotFoundException(completeValuePath);
        }
    }

    private async Task DeclareExchangeAsync(string exchangeConfigKey)
    {
        ValidateExchangeKey(exchangeConfigKey);

        var exchange = exchangeConfigKey;

        var type = _configuration[$"RabbitMq:Exchanges:{exchangeConfigKey}:Type"]
            ?? _configuration[$"RabbitMq:Exchanges:Default:Type"];

        var durable = bool.Parse(_configuration[$"RabbitMq:Exchanges:{exchangeConfigKey}:Durable"]
            ?? _configuration[$"RabbitMq:Exchanges:Default:Durable"]);

        var autoDelete = bool.Parse(_configuration[$"RabbitMq:Exchanges:{exchangeConfigKey}:AutoDelete"]
            ?? _configuration[$"RabbitMq:Exchanges:Default:AutoDelete"]);

        var channel = await CreateChannelAsync();
        await channel.ExchangeDeclareAsync(exchange, type, durable, autoDelete);
    }

    private async Task DeclareQueueAsync(string queueConfigKey)
    {
        ValidateQueueKey(queueConfigKey);

        var queue = queueConfigKey;
        var durable = bool.Parse(_configuration[$"RabbitMq:Queues:{queueConfigKey}:Durable"]
            ?? _configuration[$"RabbitMq:Queues:Default:Durable"]);
        var exclusive = bool.Parse(_configuration[$"RabbitMq:Queues:{queueConfigKey}:Exclusive"]
            ?? _configuration[$"RabbitMq:Queues:Default:Exclusive"]);
        var autoDelete = bool.Parse(_configuration[$"RabbitMq:Queues:{queueConfigKey}:AutoDelete"]
            ?? _configuration[$"RabbitMq:Queues:Default:AutoDelete"]);

        var channel = await CreateChannelAsync();
        await channel.QueueDeclareAsync(queue, durable, exclusive, autoDelete);
    }

    private async Task BindQueueAsync(string exchange, string queue, string routingKey)
    {
        var exchangeSectionPath = $"RabbitMq:Exchanges:{exchange}";

        _ = _configuration.GetSection(exchangeSectionPath)
            ?? throw new SectionNotFoundException(exchangeSectionPath);

        var queueSectionPath = $"RabbitMq:Queues:{queue}";
        _ = _configuration.GetSection(queueSectionPath) ??
            throw new SectionNotFoundException(queueSectionPath);

        var channel = await CreateChannelAsync();
        await channel.QueueBindAsync(queue, exchange, routingKey);
    }

    private void ValidateExchangeKey(string exchangeConfigKey)
    {
        IEnumerable<string> requiredSections = [
            $"RabbitMq:Exchanges:{exchangeConfigKey}"
        ];

        IEnumerable<string> optionalValues = [
            "Type",
            "Durable",
            "AutoDelete"
        ];

        foreach (var requiredSection in requiredSections)
            if (_configuration.GetSection(requiredSection) is null)
                throw new SectionNotFoundException(requiredSection);

        foreach (var optionalValue in optionalValues)
        {
            var completeSection = $"RabbitMq:Exchanges:{exchangeConfigKey}:{optionalValue}";
            var completeDefaultSection = $"RabbitMq:Exchanges:Default:{optionalValue}";

            if (_configuration[completeSection] is null && _configuration[completeDefaultSection] is null)
                throw new SectionNotFoundException(completeSection);
        }
    }

    private void ValidateQueueKey(string queueConfigKey)
    {
        IEnumerable<string> requiredSections = [
            $"RabbitMq:Queues:{queueConfigKey}"
        ];

        IEnumerable<string> optionalValues = [
            "Durable",
            "AutoDelete",
            "Exclusive"
        ];

        foreach (var requiredSection in requiredSections)
            if (_configuration.GetSection(requiredSection) is null)
                throw new SectionNotFoundException(requiredSection);

        foreach (var optionalValue in optionalValues)
        {
            var completeSection = $"RabbitMq:Queues:{queueConfigKey}:{optionalValue}";
            var completeDefaultSection = $"RabbitMq:Queues:Default:{optionalValue}";

            if (_configuration[completeSection] is null && _configuration[completeDefaultSection] is null)
                throw new SectionNotFoundException(completeSection);
        }
    }
}


