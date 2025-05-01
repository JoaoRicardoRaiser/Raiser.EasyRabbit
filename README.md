# Raiser.EasyRabbit 🐇

**Raiser.EasyRabbit** is a .NET library that simplifies integration with RabbitMQ by providing a clean and scalable abstraction for asynchronous message publishing and consumption.

> Ideal for applications that need decoupled messaging, but without directly dealing with the details of RabbitMQ configuration or repetitive boilerplate.

---

## ✨ Funcionalidades

- 📤 Publishing messages with automatic serialization.
- ⚙️ Support for simple settings via `appsettings.json`.
- 📦 Easy integration with `WorkerService`, `ASP.NET Core` or `Console` projects.
---

## 📦 How to install

```bash
dotnet add package Raiser.EasyRabbit
```

---

## 🛠️ Settings

### 1. Add configurations on `appsettings.json`:

```json
"RabbitMq": {
    "Infrastructure": {
      "HostName": "",
      "UserName": "",
      "Password": "",
      "Port": null
    },
    "Publishers": {
      "Default": {
        "RoutingKey": ""
      },
      "Test": {
        "Exchange": "",
        "RoutingKey": ""
      }
    },
    "Consumers": {
      "Default": {
        "RoutingKey": ""
      },
      "Test": {
        "Exchange": "",
        "Queue": "",
        "RoutingKey": ""
      }
    },
    "Exchanges": {
      "Default": {
        "Type": "",
        "Durable": null,
        "AutoDelete": null
      },
      "test_exchange": {
        "Type": ""
      }
    },
    "Queues": {
      "Default": {
        "Durable": null,
        "Exclusive": null,
        "AutoDelete": null
      },
      "test_queue": {}
    }
  }
```

---

### 2. Register services on `Program.cs` (example with ASP.NET Core):

```csharp

services.AddEasyRabbitMq();

```

---

### 3. Publishing messages:

Create a model class:

```csharp

public class TestClass
{
    public string Name { get; set; } = default!;
}

```

Register on `Program.cs`:

```csharp

services.AddPublisher<TestClass>("Test");

```

// Use the IMessagePublisher<> Interface to get publisher service, and use SendMessageAsync() to publish a message:

```csharp
public class MyService
{
    
    private readonly IMessagePublisherService<TestClass> _publisher

    public MyService(IMessagePublisherService<TestClass> publisher)
    {
        _publisher = publisher;
    }

    public async Task MyMethod()
    {
        await publisher.SendMessageAsync(new TestClass { Name = "TestName" });
    }
}

```

---

### 4. Consumindo mensagens:

Implement a IMessageHandler<>:

```csharp
public class TestClassMessageHandler : IMessageHandler<TestClass>
{
    public Task HandleAsync(TestClass message)
    {
        Console.WriteLine(message.Name)
        return Task.CompletedTask;
    }
}
```

And register on `Program.cs`:

```csharp
services.AddScoped<IMessageHandler<TestClass>, TestClassMessageHandler>();

services.AddConsumer<TestClass>("Test");
```

---

## 🤝 Contribuindo

Contributions are welcome! Feel free to open Issues and Pull Requests.


## 📬 Contato

- GitHub: [https://github.com/JoaoRicardoRaiser/Raisersoft.EasyRabbit]
- Email: joaorraiser@gmail.com
