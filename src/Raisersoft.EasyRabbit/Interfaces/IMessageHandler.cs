using System.Threading.Tasks;

namespace Raisersoft.EasyRabbit.Interfaces;

public interface IMessageHandler<T>
{
    Task HandleAsync(T message);
}
