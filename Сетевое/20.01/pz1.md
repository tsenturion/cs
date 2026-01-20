# Практическое задание: Моделирование распределённой системы управления задачами

## **Цель задания**

Разработать систему распределённой обработки задач, демонстрирующую понимание:

- Ролей узлов в сети
- Уровневой организации взаимодействия
- Автономности и независимости компонентов системы

## **Задание**

### **Часть 1: Создание базовых компонентов**

**Шаг 1.1: Определите архитектурные роли**
Создайте перечисление `NodeRole` со следующими ролями:

- `TaskDispatcher` - распределяет задачи между исполнителями
- `TaskExecutor` - выполняет задачи
- `ResultCollector` - собирает результаты
- `Monitor` - отслеживает состояние системы
- `LoadBalancer` - балансирует нагрузку

**Шаг 1.2: Реализуйте базовый класс `DistributedNode`**

```csharp
public abstract class DistributedNode
{
    public string NodeId { get; }
    public string NodeName { get; }
    public NodeRole Role { get; protected set; }
    public bool IsActive { get; protected set; }

    // Здесь должна быть коллекция соединений с другими узлами

    public abstract Task InitializeAsync();
    public abstract Task<string> ProcessRequestAsync(string request);
    protected abstract Task<bool> ValidateConnection(DistributedNode otherNode);
}
```

**Шаг 1.3: Создайте класс `TaskItem` для представления задачи**

```csharp
public class TaskItem
{
    public Guid TaskId { get; }
    public string Description { get; }
    public TaskPriority Priority { get; }
    public DateTime CreatedAt { get; }
    public TaskStatus Status { get; set; }
    public string AssignedTo { get; set; }
    public string Result { get; set; }
}
```

### **Часть 2: Реализация конкретных узлов**

**Шаг 2.1: Создайте `DispatcherNode`**

- Должен хранить очередь задач
- Распределять задачи по `TaskExecutor` узлам
- Отслеживать время выполнения задач
- Иметь метод `SubmitTask()` для добавления новых задач

**Шаг 2.2: Создайте `ExecutorNode`**

- Должен обрабатывать задачи разной сложности
- Иметь ограничение по количеству параллельных задач
- Возвращать результаты в `ResultCollector`
- Отслеживать свою нагрузку

**Шаг 2.3: Создайте `CollectorNode`**

- Собирает результаты от всех исполнителей
- Группирует результаты по типу задачи
- Формирует отчёты о выполнении
- Имеет кэш результатов

### **Часть 3: Организация взаимодействия**

**Шаг 3.1: Реализуйте систему соединений между узлами**

```csharp
public class NodeConnection
{
    public DistributedNode Source { get; }
    public DistributedNode Target { get; }
    public int LatencyMs { get; set; }
    public bool IsReliable { get; set; }
    public DateTime LastUsed { get; private set; }

    public async Task<string> SendAsync(string message)
    {
        // Реализуйте отправку с учётом задержки
        // И возможность сбоя соединения
    }
}
```

**Шаг 3.2: Создайте менеджер соединений**

```csharp
public class ConnectionManager
{
    private Dictionary<string, List<NodeConnection>> _connections;

    public void EstablishConnection(DistributedNode node1, DistributedNode node2);
    public NodeConnection FindOptimalPath(DistributedNode source, DistributedNode target);
    public List<NodeConnection> GetAlternativePaths(DistributedNode source, DistributedNode target);
}
```

**Шаг 3.3: Реализуйте протокол обмена сообщениями**

```csharp
public class NetworkMessage
{
    public Guid MessageId { get; }
    public string MessageType { get; } // "TASK", "RESULT", "HEARTBEAT"
    public string SenderId { get; }
    public string RecipientId { get; }
    public string Payload { get; }
    public DateTime Timestamp { get; }

    public string Serialize();
    public static NetworkMessage Deserialize(string data);
}
```

### **Часть 4: Симуляция работы системы**

**Шаг 4.1: Создайте симулятор нагрузки**

```csharp
public class LoadSimulator
{
    public async Task SimulateWorkloadAsync(
        DispatcherNode dispatcher, 
        List<ExecutorNode> executors,
        int numberOfTasks)
    {
        // Генерируйте задачи разной сложности
        // Отправляйте их через dispatcher
        // Отслеживайте прогресс выполнения
    }
}
```

**Шаг 4.2: Реализуйте мониторинг системы**

```csharp
public class SystemMonitor
{
    public void TrackNodePerformance(DistributedNode node);
    public void TrackConnectionReliability(NodeConnection connection);
    public void GeneratePerformanceReport();
    public void DetectBottlenecks();
}
```

**Шаг 4.3: Создайте тестовый сценарий**

```csharp
public class TestScenario
{
    public async Task RunScenarioAsync()
    {
        // 1. Создайте 1 Dispatcher, 3 Executor, 1 Collector
        // 2. Установите соединения между ними
        // 3. Запустите 20 задач через симулятор
        // 4. Соберите статистику выполнения
        // 5. Выведите отчёт
    }
}
```

## **Подсказки по ключевым частям**

### **Для Части 1:**

- Используйте `Guid.NewGuid()` для уникальных идентификаторов
- Реализуйте `IEquatable<T>` для классов узлов
- Добавьте свойство `List<DistributedNode> ConnectedNodes` в базовый класс

### **Для Части 2:**

- `DispatcherNode` должен использовать алгоритм round-robin или на основе нагрузки
- `ExecutorNode` должен симулировать обработку с помощью `Task.Delay()`
- `CollectorNode` может использовать `ConcurrentDictionary` для потокобезопасности

### **Для Части 3:**

- Реализуйте таймауты для отправки сообщений
- Добавьте возможность повторной отправки при сбое
- Используйте `Random` для симуляции ненадёжности сети

### **Для Части 4:**

- Создайте разные типы задач (быстрые, медленные, с ошибками)
- Имплементируйте логирование всех операций
- Добавьте визуализацию состояния системы в консоли

## **Что проверить перед отправкой (чек-лист)**

### **Обязательные требования:**

- [ ] Система успешно создаёт и настраивает все типы узлов
- [ ] Узлы могут устанавливать соединения друг с другом
- [ ] Dispatcher корректно распределяет задачи между исполнителями
- [ ] Executor обрабатывает задачи и возвращает результаты
- [ ] Collector собирает и группирует результаты
- [ ] Система обрабатывает хотя бы 20 задач за тестовый прогон
- [ ] Реализована обработка ошибок при сбоях соединений
- [ ] Все публичные методы имеют XML-комментарии
- [ ] Код проходит компиляцию без ошибок и предупреждений

### **Дополнительные проверки:**

- [ ] Реализован балансировщик нагрузки между исполнителями
- [ ] Есть мониторинг состояния узлов в реальном времени
- [ ] Система восстанавливается после сбоя узла
- [ ] Реализовано кэширование часто запрашиваемых результатов
- [ ] Есть возможность динамически добавлять/удалять узлы
- [ ] Реализованы юнит-тесты для ключевых компонентов

## **Советы по улучшению работы**

### **Архитектурные улучшения:**

1. **Разделение ответственности:** Каждый класс должен делать одну вещь и делать её хорошо
2. **Интерфейсы вместо конкретных классов:** Определите интерфейсы для ключевых компонентов
3. **Внедрение зависимостей:** Используйте DI для тестируемости
4. **Асинхронность:** Все сетевые операции должны быть асинхронными
5. **Отказоустойчивость:** Система должна продолжать работать при сбое отдельных узлов

### **Кодовые улучшения:**

1. **Именование:** Используйте понятные имена переменных и методов
2. **DRY (Don't Repeat Yourself):** Выносите повторяющийся код в отдельные методы
3. **SOLID принципы:** Особенно Single Responsibility и Open/Closed
4. **Обработка исключений:** Логируйте исключения, но не скрывайте их
5. **Производительность:** Используйте `StringBuilder` для конкатенации строк, `ConcurrentCollection` для многопоточности

### **Тестовые улучшения:**

1. **Модульное тестирование:** Напишите тесты для каждого компонента
2. **Интеграционные тесты:** Протестируйте взаимодействие компонентов
3. **Нагрузочное тестирование:** Проверьте систему под нагрузкой
4. **Тестирование граничных случаев:** Что происходит при отказе сети? При перегрузке узла?

### **Документация:**

1. **README файл:** Опишите архитектуру системы
2. **Диаграммы последовательности:** Покажите типичные сценарии работы
3. **Комментарии в коде:** Особенно для сложных алгоритмов
4. **Примеры использования:** Покажите, как работать с системой

### **Бонусные задачи:**

1. Реализуйте механизм транзакций для задач
2. Добавьте шифрование сообщений между узлами
3. Создайте веб-интерфейс для мониторинга системы
4. Реализуйте автоматическое масштабирование (автоскейлинг)
5. Добавьте поддержку различных стратегий балансировки нагрузки
