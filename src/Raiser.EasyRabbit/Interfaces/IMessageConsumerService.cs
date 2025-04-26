using System.Threading.Tasks;

namespace Raiser.EasyRabbit.Interfaces;

public interface IMessageConsumerService<T>
{
    public Task ConsumeAsync();
}
