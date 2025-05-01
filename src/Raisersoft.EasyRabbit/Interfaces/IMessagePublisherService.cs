using System.Threading.Tasks;

namespace Raisersoft.EasyRabbit.Interfaces;

public interface IMessagePublisherService<T>
{
    Task SendMessageAsync(T message);
}
