using System.Threading.Tasks;

namespace Raisersoft.EasyRabbit.Interfaces;

public interface IMessageConsumerService<T>
{
    public Task ConsumeAsync();
}
