using Microsoft.Extensions.Configuration;

namespace Raiser.EasyRabbit.IntegrationTests.Helpers;

public static class ConfigurationHelper
{
    public static IConfigurationRoot GetConfiguration()
        => new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appSettings.IntegrationTests.json")
        .Build();
}
