using Newtonsoft.Json;
using RabbitMQ.Client;
using Raiser.EasyRabbit.IntegrationTests.Helpers;
using Raiser.EasyRabbit.Interfaces;
using Raiser.EasyRabbit.Services;
using Testcontainers.RabbitMq;

namespace Raiser.EasyRabbit.IntegrationTests.Fixtures
{
    public class RabbitMqFixture
    {
        private readonly RabbitMqContainer _dbContainer = new RabbitMqBuilder()
                .WithImage("rabbitmq:3-management")
                .WithUsername("guest")
                .WithPassword("guest")
                .WithPortBinding(15672, 15672)
                .WithPortBinding(5672, 5672)
                .Build();

        public readonly IRabbitMqService RabbitMqService;

        public RabbitMqFixture()
        {
            _dbContainer.StartAsync().Wait();

            ConnectionFactory factory = new()
            {
                HostName = "localhost",
                Port = 5672,
                UserName = "guest",
                Password = "guest"
            };

            IConnection connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();

            RabbitMqService = new RabbitMqService(ConfigurationHelper.GetConfiguration(), connection);
        }

        public async Task<T?> GetMessageFromQueueAsync<T>(string queueName)
        {
            var channel = await RabbitMqService.CreateChannelAsync();

            var result = await channel.BasicGetAsync(queueName, false);

            if (result == null)
                return default;

            var body = (result!.Body.ToArray());
            var text = System.Text.Encoding.UTF8.GetString(body);

            return JsonConvert.DeserializeObject<T>(text);
        }

        public async Task<uint?> GetCountFromQueueAsync(string queueName)
        {
            var channel = await RabbitMqService.CreateChannelAsync();
            return await channel.MessageCountAsync(queueName);

        }
    }
}
