﻿using RabbitMQ.Client;
using System.Threading.Tasks;

namespace Raisersoft.EasyRabbit.Interfaces;

public interface IRabbitMqService
{
    Task<IChannel> CreateChannelAsync();
    Task ConfigurePublisherAsync(string publisherConfigKey);
    Task ConfigureConsumerAsync(string consumerConfigKey);
}
