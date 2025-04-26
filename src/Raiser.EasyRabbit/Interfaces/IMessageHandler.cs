using System.Threading.Tasks;

namespace Raiser.EasyRabbit.Interfaces;

public interface IMessageHandler<T>
{
    Task HandleAsync(T message);
}
