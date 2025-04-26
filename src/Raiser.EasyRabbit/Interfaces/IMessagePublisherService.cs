using System.Threading.Tasks;

namespace Raiser.EasyRabbit.Interfaces;

public interface IMessagePublisherService<T>
{
    Task SendMessageAsync(T message);
}
