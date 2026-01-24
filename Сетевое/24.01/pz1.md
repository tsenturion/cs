# **ПРАКТИЧЕСКОЕ ЗАДАНИЕ: TCP ЭХО-СЕРВЕР С РАСШИРЕННЫМИ ВОЗМОЖНОСТЯМИ**

## **ЦЕЛЬ ЗАДАНИЯ**

Разработать полноценный TCP эхо-сервер и клиент на C#, который демонстрирует понимание архитектуры TCP-соединений, управление состоянием, обработку ошибок и устойчивость к сетевым проблемам.

## **ЗАДАНИЕ (Шаги)**

### **Часть 1: Базовый эхо-сервер**

1. Создайте класс `TcpEchoServer`:
   - Принимает порт в конструкторе
   - Запускается методом `StartAsync()`
   - Поддерживает множественные одновременные подключения
   - Каждому клиенту отправляет обратно полученные данные (эхо)

2. Реализуйте обработку клиентов:
   - Каждое подключение обрабатывается в отдельном потоке/задаче
   - Чтение данных в цикле до закрытия соединения
   - Корректное закрытие ресурсов

3. Добавьте логирование:
   - Время подключения/отключения клиентов
   - Количество активных соединений
   - Объём переданных данных

### **Часть 2: Улучшенный TCP клиент**

1. Создайте класс `TcpEchoClient`:
   - Подключается к указанному хосту и порту
   - Отправляет пользовательские сообщения
   - Отображает ответы сервера
   - Поддерживает режим интерактивного ввода

2. Реализуйте функции:
   - Автоматическое тестирование (отправка набора тестовых сообщений)
   - Измерение времени отклика
   - Статистика передачи данных

### **Часть 3: Расширенные возможности**

1. Добавьте поддержку команд:
   - `!stats` - показать статистику сервера
   - `!clients` - список активных клиентов
   - `!time` - текущее время сервера
   - `!exit` - корректное закрытие соединения

2. Реализуйте устойчивость:
   - Автоподключение при разрыве
   - Обработку тайм-аутов
   - Логирование ошибок

### **Часть 4: Тестирование**

1. Напишите юнит-тесты для:
   - Корректного подключения/отключения
   - Обработки больших объёмов данных
   - Одновременной работы нескольких клиентов
   - Восстановления после ошибок

## **ПОДСКАЗКИ ПО КЛЮЧЕВЫМ ЧАСТЯМ**

### **Для сервера:**

```csharp
// Структура сервера
public class TcpEchoServer : IDisposable
{
    private TcpListener _listener;
    private ConcurrentDictionary<Guid, ClientInfo> _clients;
    private CancellationTokenSource _cancellationTokenSource;

    public async Task StartAsync(int port)
    {
        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start();

        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            var client = await _listener.AcceptTcpClientAsync();
            _ = Task.Run(() => HandleClientAsync(client));
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        // Реализация обработки клиента
    }
}
```

### **Для клиента:**

```csharp
// Структура клиента
public class TcpEchoClient : IDisposable
{
    private TcpClient _client;
    private NetworkStream _stream;

    public async Task ConnectAsync(string host, int port, int timeout = 5000)
    {
        _client = new TcpClient();
        _client.ReceiveTimeout = timeout;
        _client.SendTimeout = timeout;
        await _client.ConnectAsync(host, port);
        _stream = _client.GetStream();
    }

    public async Task<string> SendMessageAsync(string message)
    {
        // Отправка и получение ответа
    }
}
```

## **ЧТО ПРОВЕРИТЬ ПЕРЕД ОТПРАВКОЙ (чек-лист)**

### **Обязательные проверки:**

- [ ] Сервер запускается и слушает указанный порт
- [ ] Клиент успешно подключается к серверу
- [ ] Эхо-ответ работает корректно для разных сообщений
- [ ] Поддерживается множественные одновременные подключения
- [ ] Корректно закрываются соединения (без утечек ресурсов)
- [ ] Обрабатываются разрывы соединения
- [ ] Работает в локальной сети (localhost)

### **Расширенные проверки:**

- [ ] Обработка больших сообщений (>1MB)
- [ ] Устойчивость к быстрым подключениям/отключениям
- [ ] Статистика собирается и отображается корректно
- [ ] Команды сервера (!stats, !time и т.д.) работают
- [ ] Тайм-ауты настроены и работают
- [ ] Есть базовые юнит-тесты

### **Производительность и надежность:**

- [ ] Нет утечек памяти (проверить через монитор ресурсов)
- [ ] Максимальное количество соединений ограничено
- [ ] Есть обработка исключений для всех сетевых операций
- [ ] Логирование ошибок работает
- [ ] Код читаем и хорошо структурирован

## **СОВЕТЫ ПО УЛУЧШЕНИЮ РАБОТЫ**

### **Архитектурные улучшения:**

1. **Используйте пул потоков** для обработки клиентов вместо создания нового потока на каждого клиента
2. **Реализуйте асинхронные операции** везде, где это возможно
3. **Добавьте конфигурацию** через appsettings.json или аргументы командной строки
4. **Создайте интерфейсы** для легкого тестирования (ITcpServer, ITcpClient)

### **Безопасность:**

```csharp
// Ограничение размера сообщений
private const int MaxMessageSize = 10 * 1024 * 1024; // 10MB

// Проверка перед обработкой
if (data.Length > MaxMessageSize)
{
    await SendErrorAsync("Message too large");
    return;
}
```

### **Производительность:**

```csharp
// Используйте буферизацию
private readonly byte[] _buffer = new byte[8192];

// Используйте ValueTask для уменьшения аллокаций
public async ValueTask<string> ReceiveMessageAsync()
```

### **Улучшенное логирование:**

```csharp
// Структурированное логирование
public class ClientLogger
{
    public void LogConnection(Guid clientId, IPEndPoint endpoint)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [CONNECT] {clientId} from {endpoint}");
    }

    public void LogDisconnection(Guid clientId, long bytesSent, long bytesReceived)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [DISCONNECT] {clientId} " +
                         $"sent: {bytesSent}, received: {bytesReceived}");
    }
}
```

### **Мониторинг:**

```csharp
// Сбор метрик
public class ServerMetrics
{
    public int ActiveConnections { get; private set; }
    public long TotalBytesReceived { get; private set; }
    public long TotalBytesSent { get; private set; }
    public DateTime StartTime { get; } = DateTime.UtcNow;

    // Методы для обновления метрик
}
```

### **Тестирование:**

```csharp
// Пример теста
[Test]
public async Task Server_Should_Echo_Multiple_Clients_Simultaneously()
{
    // Arrange
    var server = new TcpEchoServer(11000);
    await server.StartAsync();

    // Act
    var clientTasks = Enumerable.Range(0, 10)
        .Select(i => SendTestMessageAsync("localhost", 11000, $"Message{i}"))
        .ToList();

    var results = await Task.WhenAll(clientTasks);

    // Assert
    Assert.That(results, Has.All.EqualTo("Echo: MessageX")); // Подставьте правильный формат
}
```

### **Дополнительные фичи для продвинутой реализации:**

1. **Поддержка SSL/TLS** для безопасного соединения
2. **Аутентификация клиентов** (по токену или логину/паролю)
3. **Компрессия данных** для больших сообщений
4. **Поддержка разных кодировок** (UTF-8, UTF-16, ASCII)
5. **Консоль администратора** для управления сервером во время работы
6. **Экспорт статистики** в формате JSON или Prometheus
7. **Графический интерфейс** для мониторинга
8. **Поддержка Docker** для легкого развертывания
