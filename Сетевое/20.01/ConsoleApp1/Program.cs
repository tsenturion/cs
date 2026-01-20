using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkNodesAndRolesDemo
{
    class Program
    {
        static async Task Main()
        {
            Console.WriteLine("=== УЗЛЫ СЕТИ И ИХ РОЛИ ===\n");
            
            // Часть 1: Базовые роли узлов - инициатор и получатель
            Console.WriteLine("1. БАЗОВЫЕ РОЛИ УЗЛОВ:");
            await DemonstrateBasicNodeRolesAsync();
            
            // Часть 2: Динамические роли узлов
            Console.WriteLine("\n2. ДИНАМИЧЕСКИЕ РОЛИ УЗЛОВ:");
            await DemonstrateDynamicNodeRolesAsync();
            
            // Часть 3: Промежуточные узлы и маршрутизация
            Console.WriteLine("\n3. ПРОМЕЖУТОЧНЫЕ УЗЛЫ И МАРШРУТИЗАЦИЯ:");
            await DemonstrateIntermediateNodesAsync();
            
            // Часть 4: Архитектурные роли в распределённой системе
            Console.WriteLine("\n4. АРХИТЕКТУРНЫЕ РОЛИ:");
            DemonstrateArchitecturalRoles();
            
            // Часть 5: Автономность и независимость узлов
            Console.WriteLine("\n5. АВТОНОМНОСТЬ И НЕЗАВИСИМОСТЬ УЗЛОВ:");
            await DemonstrateNodeAutonomyAsync();
        }
        
        static async Task DemonstrateBasicNodeRolesAsync()
        {
            Console.WriteLine($"  ИНИЦИАТОР И ПОЛУЧАТЕЛЬ:");
            
            // Создаём узлы с базовыми ролями
            var initiator = new NetworkNode("Инициатор-1", NodeRole.Initiator);
            var receiver = new NetworkNode("Получатель-1", NodeRole.Receiver);
            
            Console.WriteLine($"\n  Созданы узлы:");
            Console.WriteLine($"    {initiator.Name} ({initiator.Role})");
            Console.WriteLine($"    {receiver.Name} ({receiver.Role})");
            
            // Демонстрация взаимодействия
            Console.WriteLine($"\n  1. СЦЕНАРИЙ: Инициатор отправляет запрос получателю");
            
            initiator.ConnectTo(receiver);
            
            for (int i = 1; i <= 3; i++)
            {
                string request = $"Запрос #{i}";
                Console.Write($"\n    {initiator.Name}: Отправляет '{request}'... ");
                
                try
                {
                    string response = await initiator.SendToAsync(receiver, request);
                    Console.WriteLine($"Получен ответ: {response}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ОШИБКА: {ex.Message}");
                }
            }
            
            // Обратная связь
            Console.WriteLine($"\n  2. СЦЕНАРИЙ: Роли меняются местами");
            
            string reverseRequest = "Запрос от бывшего получателя";
            Console.Write($"    {receiver.Name}: Теперь инициирует запрос... ");
            
            try
            {
                string reverseResponse = await receiver.SendToAsync(initiator, reverseRequest);
                Console.WriteLine($"Ответ: {reverseResponse}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ОШИБКА: {ex.Message}");
            }
            
            // Статистика
            Console.WriteLine($"\n  СТАТИСТИКА ВЗАИМОДЕЙСТВИЯ:");
            Console.WriteLine($"    {initiator.Name}: {initiator.GetStatistics()}");
            Console.WriteLine($"    {receiver.Name}: {receiver.GetStatistics()}");
        }
        
        static async Task DemonstrateDynamicNodeRolesAsync()
        {
            Console.WriteLine($"  ДИНАМИЧЕСКИЕ РОЛИ УЗЛОВ:");
            
            // Создаём узлы с возможностью смены ролей
            var nodeA = new DynamicNode("Узел-A");
            var nodeB = new DynamicNode("Узел-B");
            var nodeC = new DynamicNode("Узел-C");
            
            // Устанавливаем связи
            nodeA.ConnectTo(nodeB);
            nodeB.ConnectTo(nodeC);
            
            Console.WriteLine($"\n  СЕТЬ ИЗ 3 УЗЛОВ:");
            Console.WriteLine($"    A ↔ B ↔ C");
            
            // Демонстрация цепочки взаимодействий
            Console.WriteLine($"\n  1. ЦЕПОЧКА ВЗАИМОДЕЙСТВИЙ:");
            
            string message = "Важное сообщение";
            Console.WriteLine($"    {nodeA.Name}: Начинает передачу '{message}'");
            
            try
            {
                // A → B → C
                string result = await nodeA.SendThroughChainAsync(message, new[] { nodeB, nodeC });
                Console.WriteLine($"    Конечный результат: {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    Ошибка в цепочке: {ex.Message}");
            }
            
            // Демонстрация смены ролей
            Console.WriteLine($"\n  2. СМЕНА РОЛЕЙ В РЕАЛЬНОМ ВРЕМЕНИ:");
            
            Console.WriteLine($"    Текущие роли:");
            Console.WriteLine($"      {nodeA.Name}: {nodeA.CurrentRole}");
            Console.WriteLine($"      {nodeB.Name}: {nodeB.CurrentRole}");
            Console.WriteLine($"      {nodeC.Name}: {nodeC.CurrentRole}");
            
            // Симуляция изменения нагрузки
            Console.WriteLine($"\n    Симуляция изменения нагрузки:");
            Console.WriteLine($"      Узел B становится перегруженным...");
            
            nodeB.SetLoadLevel(LoadLevel.High);
            nodeB.ChangeRole(NodeRole.Processor);
            
            // Пытаемся отправить данные через перегруженный узел
            Console.Write($"      Отправка через перегруженный узел... ");
            
            try
            {
                string result2 = await nodeA.SendToAsync(nodeC, "Срочное сообщение");
                Console.WriteLine($"Успешно: {result2}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ОШИБКА: {ex.Message}");
                Console.WriteLine($"      Обход перегруженного узла...");
                
                // Прямое соединение A → C
                nodeA.ConnectTo(nodeC);
                string alternativeResult = await nodeA.SendToAsync(nodeC, "Альтернативный путь");
                Console.WriteLine($"      Результат: {alternativeResult}");
            }
            
            // Восстановление нормальной работы
            nodeB.SetLoadLevel(LoadLevel.Normal);
            
            Console.WriteLine($"\n    Итоговые роли:");
            Console.WriteLine($"      {nodeA.Name}: {nodeA.CurrentRole} (также может быть {NodeRole.Receiver})");
            Console.WriteLine($"      {nodeB.Name}: {nodeB.CurrentRole} (был перегружен)");
            Console.WriteLine($"      {nodeC.Name}: {nodeC.CurrentRole} (конечный получатель)");
        }
        
        static async Task DemonstrateIntermediateNodesAsync()
        {
            Console.WriteLine($"  ПРОМЕЖУТОЧНЫЕ УЗЛЫ:");
            
            // Создаём сложную сеть с промежуточными узлами
            Console.WriteLine($"\n  СОЗДАНИЕ СЛОЖНОЙ СЕТИ:");
            
            var source = new NetworkNode("Источник", NodeRole.Initiator);
            var destination = new NetworkNode("Назначение", NodeRole.Receiver);
            
            // Промежуточные узлы
            var router1 = new IntermediateNode("Маршрутизатор-1", IntermediateRole.Router);
            var router2 = new IntermediateNode("Маршрутизатор-2", IntermediateRole.Router);
            var processor = new IntermediateNode("Процессор-данных", IntermediateRole.Processor);
            var cache = new IntermediateNode("Кэш-сервер", IntermediateRole.Cache);
            
            // Строим сеть
            source.ConnectTo(router1);
            router1.ConnectTo(router2);
            router1.ConnectTo(processor);
            router2.ConnectTo(cache);
            processor.ConnectTo(destination);
            cache.ConnectTo(destination);
            
            Console.WriteLine($"    Сеть построена с 4 промежуточными узлами");


			// Демонстрация маршрутизации
            Console.WriteLine($"\n  1. МАРШРУТИЗАЦИЯ ЧЕРЕЗ ПРОМЕЖУТОЧНЫЕ УЗЛЫ:");

			            var messages = new[]
			            {
	            "Важные данные",
	            "Запрос на обработку",
	            "Кэшируемые данные",
	            "Срочное сообщение"
            };

			foreach (var message in messages)
			{
				Console.Write($"\n    Отправка: '{message}'... ");

				try
				{
					// Указываем путь через промежуточные узлы
					string path = await SendThroughPathAsync(source, destination, message,
						new[] { router1, router2, cache });

					Console.WriteLine($"Доставлено через: {path}");
					Console.WriteLine($"      Задержки: {GetPathDelays(new[] { router1, router2, cache })}");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"ОШИБКА: {ex.Message}");

					// Попробовать альтернативный путь
					Console.Write($"      Пробуем альтернативный путь... ");

					try
					{
						string altPath = await SendThroughPathAsync(source, destination, message,
							new[] { router1, processor });
						Console.WriteLine($"Доставлено через: {altPath}");
					}
					catch (Exception ex2)
					{
						Console.WriteLine($"И альтернативный путь не сработал: {ex2.Message}");
					}
				}
			}

			// Демонстрация обработки данных промежуточными узлами
			Console.WriteLine($"\n  2. ОБРАБОТКА ДАННЫХ ПРОМЕЖУТОЧНЫМИ УЗЛАМИ:");
            
            string rawData = "сырые,данные,для,обработки";
            Console.WriteLine($"    Исходные данные: {rawData}");
            
            // Отправляем через процессор
            Console.Write($"    Отправка через процессор... ");
            string processed = await processor.ProcessDataAsync(rawData);
            Console.WriteLine($"Результат: {processed}");
            
            // Отправляем через кэш
            Console.Write($"    Отправка через кэш... ");
            string cached = await cache.ProcessDataAsync(rawData);
            Console.WriteLine($"Результат: {cached}");
            
            // Вторая попытка через кэш (должно быть быстрее)
            Console.Write($"    Повторный запрос к кэшу... ");
            string cachedAgain = await cache.ProcessDataAsync(rawData);
            Console.WriteLine($"Результат: {cachedAgain} (из кэша)");
        }
        
        static void DemonstrateArchitecturalRoles()
        {
            Console.WriteLine($"  АРХИТЕКТУРНЫЕ РОЛИ В РАСПРЕДЕЛЁННОЙ СИСТЕМЕ:");
            
            // Создаём распределённую систему с разными ролями
            var distributedSystem = new DistributedSystem();
            
            // Добавляем узлы с архитектурными ролями
            distributedSystem.AddNode(new SystemNode("API-Шлюз", SystemRole.ApiGateway));
            distributedSystem.AddNode(new SystemNode("База-Данных", SystemRole.Database));
            distributedSystem.AddNode(new SystemNode("Сервер-Приложений", SystemRole.ApplicationServer));
            distributedSystem.AddNode(new SystemNode("Кэш-Кластер", SystemRole.Cache));
            distributedSystem.AddNode(new SystemNode("Балансировщик", SystemRole.LoadBalancer));
            distributedSystem.AddNode(new SystemNode("Сервис-Аутентификации", SystemRole.AuthService));
            
            Console.WriteLine($"\n  РАСПРЕДЕЛЁННАЯ СИСТЕМА:");
            distributedSystem.PrintSystemOverview();
            
            // Демонстрация запроса через систему
            Console.WriteLine($"\n  1. ОБРАБОТКА ЗАПРОСА ПОЛЬЗОВАТЕЛЯ:");
            
            var userRequest = new UserRequest
            {
                Id = "REQ-001",
                UserId = "user123",
                Action = "getData",
                Parameters = new Dictionary<string, string>
                {
                    ["id"] = "item456",
                    ["format"] = "json"
                }
            };
            
            Console.WriteLine($"    Запрос: {userRequest.Id} от {userRequest.UserId}");
            Console.WriteLine($"    Действие: {userRequest.Action}");
            
            try
            {
                var result = distributedSystem.ProcessRequest(userRequest);
                Console.WriteLine($"\n    РЕЗУЛЬТАТ ОБРАБОТКИ:");
                Console.WriteLine($"      Маршрут запроса: {result.Path}");
                Console.WriteLine($"      Узлы обработки: {string.Join(" → ", result.ProcessedBy)}");
                Console.WriteLine($"      Время обработки: {result.TotalTimeMs} мс");
                Console.WriteLine($"      Данные: {result.Data}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    ОШИБКА ОБРАБОТКИ: {ex.Message}");
                Console.WriteLine($"    Анализ проблемы: {distributedSystem.AnalyzeFailure(userRequest)}");
            }
            
            // Анализ критичности узлов
            Console.WriteLine($"\n  2. АНАЛИЗ КРИТИЧНОСТИ УЗЛОВ:");
            
            var criticalityReport = distributedSystem.AnalyzeCriticality();
            Console.WriteLine($"    Наиболее критичные узлы:");
            
            foreach (var item in criticalityReport.OrderByDescending(x => x.CriticalityScore))
            {
                string indicator = item.CriticalityScore >= 80 ? "⚠️ " : "";
                Console.WriteLine($"      {indicator}{item.NodeName}: {item.Role} - {item.CriticalityScore}/100");
            }
            
            // Демонстрация масштабирования
            Console.WriteLine($"\n  3. МАСШТАБИРОВАНИЕ СИСТЕМЫ:");
            
            Console.WriteLine($"    Добавляем копию сервера приложений...");
            distributedSystem.AddNode(new SystemNode("Сервер-Приложений-2", SystemRole.ApplicationServer));
            
            Console.WriteLine($"    Добавляем реплику базы данных...");
            distributedSystem.AddNode(new SystemNode("База-Данных-Реplica", SystemRole.Database));
            
            Console.WriteLine($"\n    Обновлённая система:");
            distributedSystem.PrintSystemOverview();
        }
        
        static async Task DemonstrateNodeAutonomyAsync()
        {
            Console.WriteLine($"  АВТОНОМНОСТЬ И НЕЗАВИСИМОСТЬ УЗЛОВ:");
            
            // Создаём автономные узлы
            Console.WriteLine($"\n  АВТОНОМНЫЕ УЗЛЫ:");
            
            var autonomousNodes = new List<AutonomousNode>
            {
                new AutonomousNode("Узел-Альфа", NodeBehavior.Consistent),
                new AutonomousNode("Узел-Бета", NodeBehavior.Unpredictable),
                new AutonomousNode("Узел-Гамма", NodeBehavior.Reliable),
                new AutonomousNode("Узел-Дельта", NodeBehavior.Flaky)
            };
            
            foreach (var node in autonomousNodes)
            {
                Console.WriteLine($"    {node.Name}: {node.Behavior} (автономность: {node.AutonomyLevel})");
            }
            
            // Демонстрация независимости узлов
            Console.WriteLine($"\n  1. НЕЗАВИСИМОЕ ПОВЕДЕНИЕ УЗЛОВ:");
            
            var tasks = new List<Task<NodeStatus>>();
            
            foreach (var node in autonomousNodes)
            {
                tasks.Add(Task.Run(async () =>
                {
                    Console.Write($"      {node.Name}: Запуск работы... ");
                    return await node.PerformAutonomousWorkAsync();
                }));
            }
            
            Console.WriteLine($"\n    Все узлы работают независимо...");
            
            var results = await Task.WhenAll(tasks);
            
            Console.WriteLine($"\n    РЕЗУЛЬТАТЫ НЕЗАВИСИМОЙ РАБОТЫ:");
            foreach (var result in results)
            {
                Console.WriteLine($"      {result.NodeName}: {result.Status} (время: {result.DurationMs} мс)");
            }
            
            // Демонстрация непредсказуемого поведения
            Console.WriteLine($"\n  2. НЕПРЕДСКАЗУЕМОЕ ПОВЕДЕНИЕ В СЕТИ:");
            
            var sender = autonomousNodes[0]; // Альфа - стабильный
            var receiver = autonomousNodes[1]; // Бета - непредсказуемый
            
            Console.WriteLine($"    Отправка от {sender.Name} ({sender.Behavior}) к {receiver.Name} ({receiver.Behavior})");
            
            for (int i = 1; i <= 5; i++)
            {
                Console.Write($"\n    Попытка #{i}: ");
                
                try
                {
                    string response = await sender.SendToAutonomousNodeAsync(receiver, $"Сообщение {i}");
                    Console.WriteLine($"Успех: {response}");
                }
                catch (TimeoutException)
                {
                    Console.WriteLine($"ТАЙМАУТ: Узел не ответил вовремя");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ОШИБКА: {ex.GetType().Name}: {ex.Message}");
                }
                
                // Небольшая пауза между попытками
                await Task.Delay(100);
            }
            
            // Демонстрация самостоятельного восстановления
            Console.WriteLine($"\n  3. САМОСТОЯТЕЛЬНОЕ ВОССТАНОВЛЕНИЕ УЗЛОВ:");
            
            var flakyNode = autonomousNodes[3]; // Дельта - нестабильный
            Console.WriteLine($"    Мониторинг узла {flakyNode.Name}...");
            
            for (int i = 0; i < 10; i++)
            {
                var status = await flakyNode.GetCurrentStatusAsync();
                Console.WriteLine($"      Цикл {i + 1}: {status.State} (здоровье: {status.HealthScore}/100)");
                
                if (status.State == NodeState.Failed)
                {
                    Console.WriteLine($"      ⚠️  Узел вышел из строя. Восстановление...");
                    await flakyNode.AttemptRecoveryAsync();
                }
                
                await Task.Delay(200);
            }
            
            // Итоговая статистика
            Console.WriteLine($"\n  4. ИТОГОВАЯ СТАТИСТИКА АВТОНОМНОСТИ:");
            foreach (var node in autonomousNodes)
            {
                var stats = node.GetAutonomyStatistics();
                Console.WriteLine($"    {node.Name}:");
                Console.WriteLine($"      Успешных операций: {stats.SuccessfulOperations}");
                Console.WriteLine($"      Ошибок: {stats.FailedOperations}");
                Console.WriteLine($"      Время простоя: {stats.DowntimePercent:F1}%");
                Console.WriteLine($"      Самостоятельных решений: {stats.AutonomousDecisions}");
            }
        }
        
        static string GetPathDelays(IEnumerable<IntermediateNode> nodes)
        {
            var delays = nodes.Select(n => $"{n.Name}:{n.ProcessingDelayMs}мс");
            return string.Join(" + ", delays);
        }

		private static async Task<string> SendThroughPathAsync(NetworkNode source, NetworkNode destination,
	string message, IEnumerable<NetworkNode> path)
		{
			var nodesList = path.ToList();
			NetworkNode currentNode = source;
			string currentMessage = message;
			var pathDescription = new List<string> { source.Name };

			foreach (var nextNode in nodesList)
			{
				currentMessage = await currentNode.SendToAsync(nextNode, currentMessage);
				currentNode = nextNode;
				pathDescription.Add(currentNode.Name);
			}

			// Отправка конечному получателю
			if (currentNode != destination)
			{
				currentMessage = await currentNode.SendToAsync(destination, currentMessage);
				pathDescription.Add(destination.Name);
			}

			return string.Join(" → ", pathDescription) + $": {currentMessage}";
		}
	}
    
    // Вспомогательные классы для демонстрации
    
    enum NodeRole { Initiator, Receiver, Processor, Router, Cache, Gateway }
    enum IntermediateRole { Router, Processor, Cache, LoadBalancer, Firewall }
    enum SystemRole { ApiGateway, Database, ApplicationServer, Cache, LoadBalancer, AuthService, MessageQueue }
    enum NodeBehavior { Consistent, Unpredictable, Reliable, Flaky, Aggressive }
    enum NodeState { Ready, Busy, Slow, Failed, Recovering }
    enum LoadLevel { Low, Normal, High, Critical }
    
    class NetworkNode
    {
        public string Name { get; }
        public NodeRole Role { get; protected set; }
        public List<NetworkNode> Connections { get; } = new();
        protected int MessagesSent { get; set; }
        protected int MessagesReceived { get; set; }
        protected Random Random { get; } = new();

		public NetworkNode(string name, NodeRole role) 
		{
			Name = name;
			Role = role;
		}

		public virtual void ConnectTo(NetworkNode other)
        {
            if (!Connections.Contains(other))
            {
                Connections.Add(other);
                other.Connections.Add(this);
                Console.WriteLine($"      {Name} ↔ {other.Name} соединение установлено");
            }
        }

		public virtual async Task<string> SendToAsync(NetworkNode receiver, string message)
        {
            if (!Connections.Contains(receiver))
                throw new InvalidOperationException($"Нет соединения с {receiver.Name}");
            
            MessagesSent++;
            Console.Write($"→ ");
            
            // Симуляция сетевой задержки
            int delay = Random.Next(50, 200);
            await Task.Delay(delay);
            
            // Получатель обрабатывает сообщение
            string response = await receiver.ReceiveFromAsync(this, message);
            
            MessagesReceived++;
            Console.Write($"← ");
            
            return response;
        }
        
        public virtual async Task<string> ReceiveFromAsync(NetworkNode sender, string message)
        {
            MessagesReceived++;
            
            // Симуляция обработки
            await Task.Delay(Random.Next(10, 50));
            
            return $"{Name} получил '{message}' от {sender.Name}";
        }
        
        public virtual string GetStatistics()
        {
            return $"Отправлено: {MessagesSent}, Получено: {MessagesReceived}";
        }
    }
    
    class DynamicNode : NetworkNode
    {
        public NodeBehavior Behavior { get; private set; }
        public LoadLevel CurrentLoad { get; private set; } = LoadLevel.Normal;
        
        public DynamicNode(string name) : base(name, NodeRole.Initiator)
        {
            Behavior = NodeBehavior.Consistent;
        }
        
        public string CurrentRole => Role.ToString();
        
        public void ChangeRole(NodeRole newRole)
        {
            Console.WriteLine($"      {Name}: Роль изменена {Role} → {newRole}");
            Role = newRole;
        }
        
        public void SetLoadLevel(LoadLevel level)
        {
            CurrentLoad = level;
            Console.WriteLine($"      {Name}: Нагрузка установлена на {level}");
        }
        
        public async Task<string> SendThroughChainAsync(string message, IEnumerable<DynamicNode> chain)
        {
            Console.WriteLine($"      Начинается передача по цепочке...");
            
            string currentMessage = message;
            DynamicNode currentNode = this;
            
            foreach (var nextNode in chain)
            {
                Console.Write($"      {currentNode.Name} → {nextNode.Name}: ");
                
                try
                {
                    currentMessage = await currentNode.SendToAsync(nextNode, currentMessage);
                    currentNode = nextNode;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Сбой в цепочке на {currentNode.Name} → {nextNode.Name}: {ex.Message}");
                }
            }
            
            return currentMessage;
        }
        
        public override async Task<string> SendToAsync(NetworkNode receiver, string message)
        {
            // Учитываем нагрузку при отправке
            if (CurrentLoad >= LoadLevel.High)
            {
                int chance = Random.Next(100);
                if (chance < 30) // 30% вероятность отказа при высокой нагрузке
                {
                    await Task.Delay(Random.Next(100, 500));
                    throw new TimeoutException($"Узел {Name} перегружен");
                }
            }
            
            return await base.SendToAsync(receiver, message);
        }
    }
    
    class IntermediateNode : NetworkNode
    {
        public IntermediateRole IntermediateRole { get; }
        public int ProcessingDelayMs { get; private set; }
        
        public IntermediateNode(string name, IntermediateRole role) 
            : base(name, NodeRole.Processor)
        {
            IntermediateRole = role;
            ProcessingDelayMs = role switch
            {
                IntermediateRole.Router => 5,
                IntermediateRole.Processor => 100,
                IntermediateRole.Cache => 10,
                _ => 50
            };
        }
        
        public async Task<string> ProcessDataAsync(string data)
        {
            Console.Write($"Обработка ({IntermediateRole})... ");
            
            await Task.Delay(ProcessingDelayMs);
            
            string result = IntermediateRole switch
            {
                IntermediateRole.Processor => $"Обработано: {data.ToUpper()}",
                IntermediateRole.Cache => $"Кэшировано: {data}",
                IntermediateRole.Router => $"Маршрутизировано: {data}",
                _ => $"Пропущено: {data}"
            };
            
            return result;
        }
        
        public override async Task<string> ReceiveFromAsync(NetworkNode sender, string message)
        {
            // Промежуточные узлы могут изменять сообщения
            await Task.Delay(ProcessingDelayMs);
            
            string processedMessage = IntermediateRole switch
            {
                IntermediateRole.Processor => $"[ОБРАБОТАНО] {message}",
                IntermediateRole.Cache => $"[КЭШ] {message}",
                IntermediateRole.Router => $"[МАРШРУТ] {message}",
                _ => message
            };
            
            return await base.ReceiveFromAsync(sender, processedMessage);
        }
    }
    
    class UserRequest
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Action { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
        
        public override string ToString() => $"{Id} ({Action})";
    }
    
    class RequestResult
    {
        public bool Success { get; set; }
        public string Data { get; set; }
        public List<string> ProcessedBy { get; set; } = new();
        public string Path { get; set; }
        public int TotalTimeMs { get; set; }
    }
    
    class SystemNode : NetworkNode
    {
        public SystemRole SystemRole { get; }
        public int Capacity { get; private set; }
        public int CurrentLoad { get; private set; }
        
        public SystemNode(string name, SystemRole role) : base(name, NodeRole.Processor)
        {
            SystemRole = role;
            Capacity = role switch
            {
                SystemRole.ApiGateway => 1000,
                SystemRole.Database => 500,
                SystemRole.ApplicationServer => 200,
                SystemRole.Cache => 10000,
                SystemRole.LoadBalancer => 5000,
                SystemRole.AuthService => 300,
                _ => 100
            };
        }
        
        public bool CanHandleRequest()
        {
            return CurrentLoad < Capacity * 0.8; // 80% загрузки - предельное значение
        }
        
        public void IncreaseLoad(int amount = 1)
        {
            CurrentLoad += amount;
        }
        
        public void DecreaseLoad(int amount = 1)
        {
            CurrentLoad = Math.Max(0, CurrentLoad - amount);
        }
    }
    
    class DistributedSystem
    {
        private List<SystemNode> _nodes = new();
        private Dictionary<SystemRole, List<SystemNode>> _nodesByRole = new();
        private Random _random = new();
        
        public void AddNode(SystemNode node)
        {
            _nodes.Add(node);
            
            if (!_nodesByRole.ContainsKey(node.SystemRole))
                _nodesByRole[node.SystemRole] = new List<SystemNode>();
            
            _nodesByRole[node.SystemRole].Add(node);
            
            // Устанавливаем связи на основе ролей
            EstablishConnections(node);
        }
        
        private void EstablishConnections(SystemNode newNode)
        {
            switch (newNode.SystemRole)
            {
                case SystemRole.ApiGateway:
                    // API Gateway подключается ко всем серверам приложений
                    foreach (var appServer in GetNodesByRole(SystemRole.ApplicationServer))
                        newNode.ConnectTo(appServer);
                    break;
                    
                case SystemRole.ApplicationServer:
                    // Сервер приложений подключается к БД и кэшу
                    foreach (var db in GetNodesByRole(SystemRole.Database))
                        newNode.ConnectTo(db);
                    foreach (var cache in GetNodesByRole(SystemRole.Cache))
                        newNode.ConnectTo(cache);
                    break;
            }
        }
        
        public IEnumerable<SystemNode> GetNodesByRole(SystemRole role)
        {
            return _nodesByRole.ContainsKey(role) ? _nodesByRole[role] : Enumerable.Empty<SystemNode>();
        }
        
        public RequestResult ProcessRequest(UserRequest request)
        {
            var result = new RequestResult();
            var stopwatch = Stopwatch.StartNew();
            var path = new List<string>();
            
            try
            {
                // API Gateway получает запрос
                var apiGateway = GetNodesByRole(SystemRole.ApiGateway).FirstOrDefault();
                if (apiGateway == null)
                    throw new InvalidOperationException("API Gateway недоступен");
                
                path.Add(apiGateway.Name);
                apiGateway.IncreaseLoad();
                
                // Аутентификация
                var authService = GetNodesByRole(SystemRole.AuthService).FirstOrDefault();
                if (authService != null && authService.CanHandleRequest())
                {
                    path.Add(authService.Name);
                    authService.IncreaseLoad();
                }
                
                // Выбор сервера приложений (простая балансировка)
                var appServers = GetNodesByRole(SystemRole.ApplicationServer)
                    .Where(s => s.CanHandleRequest())
                    .ToList();
                
                if (appServers.Count == 0)
                    throw new InvalidOperationException("Нет доступных серверов приложений");
                
                var selectedAppServer = appServers[_random.Next(appServers.Count)];
                path.Add(selectedAppServer.Name);
                selectedAppServer.IncreaseLoad();
                
                // Работа с данными (БД или кэш)
                SystemNode dataSource = null;
                if (_random.Next(100) < 70) // 70% вероятность использования кэша
                {
                    var cacheNodes = GetNodesByRole(SystemRole.Cache)
                        .Where(c => c.CanHandleRequest())
                        .ToList();
                    
                    if (cacheNodes.Count > 0)
                    {
                        dataSource = cacheNodes[_random.Next(cacheNodes.Count)];
                        path.Add(dataSource.Name + "(кэш)");
                    }
                }
                
                if (dataSource == null)
                {
                    var dbNodes = GetNodesByRole(SystemRole.Database)
                        .Where(d => d.CanHandleRequest())
                        .ToList();
                    
                    if (dbNodes.Count == 0)
                        throw new InvalidOperationException("Нет доступных баз данных");
                    
                    dataSource = dbNodes[_random.Next(dbNodes.Count)];
                    path.Add(dataSource.Name + "(БД)");
                }
                
                dataSource.IncreaseLoad();
                
                stopwatch.Stop();
                
                result.Success = true;
                result.Data = $"Данные для {request.UserId} ({request.Action})";
                result.ProcessedBy = path;
                result.Path = string.Join(" → ", path);
                result.TotalTimeMs = (int)stopwatch.ElapsedMilliseconds;
            }
            finally
            {
                // Уменьшаем нагрузку на узлы
                foreach (var node in _nodes)
                    node.DecreaseLoad();
            }
            
            return result;
        }
        
        public void PrintSystemOverview()
        {
            Console.WriteLine($"    Узлов в системе: {_nodes.Count}");
            Console.WriteLine($"    Распределение по ролям:");
            
            foreach (var role in Enum.GetValues<SystemRole>())
            {
                var nodes = GetNodesByRole(role);
                if (nodes.Any())
                {
                    Console.WriteLine($"      {role}: {nodes.Count()} узлов");
                }
            }
        }
        
        public List<(string NodeName, SystemRole Role, int CriticalityScore)> AnalyzeCriticality()
        {
            var report = new List<(string, SystemRole, int)>();
            
            foreach (var node in _nodes)
            {
                int score = CalculateCriticalityScore(node);
                report.Add((node.Name, node.SystemRole, score));
            }
            
            return report;
        }
        
        private int CalculateCriticalityScore(SystemNode node)
        {
            int score = 0;

			// Зависимость других узлов
			int dependentNodes = _nodes.Count(n => n.Connections.Contains(node));
			score += Math.Min(dependentNodes * 10, 40);

			// Уникальность роли
			int sameRoleCount = GetNodesByRole(node.SystemRole).Count();
            score += sameRoleCount == 1 ? 30 : 50 / sameRoleCount;
            
            // Нагрузка
            score += (int)((node.CurrentLoad / (double)node.Capacity) * 20);
            
            return Math.Min(score, 100);
        }
        
        public string AnalyzeFailure(UserRequest failedRequest)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Анализ возможных причин:");
            
            if (GetNodesByRole(SystemRole.ApiGateway).All(n => !n.CanHandleRequest()))
                sb.AppendLine("- Все API Gateways перегружены");
            
            if (GetNodesByRole(SystemRole.ApplicationServer).All(n => !n.CanHandleRequest()))
                sb.AppendLine("- Все серверы приложений перегружены");
            
            if (GetNodesByRole(SystemRole.Database).All(n => !n.CanHandleRequest()))
                sb.AppendLine("- Все базы данных недоступны");
            
            return sb.ToString();
        }
    }
    
    class AutonomousNode
    {
        public string Name { get; }
        public NodeBehavior Behavior { get; }
        public NodeState State { get; private set; } = NodeState.Ready;
        private Random _random = new();
        private int _successCount = 0;
        private int _failureCount = 0;
        private int _autonomousDecisions = 0;
        private DateTime? _lastFailure = null;
        
        public AutonomousNode(string name, NodeBehavior behavior)
        {
            Name = name;
            Behavior = behavior;
        }
        
        public string AutonomyLevel => Behavior switch
        {
            NodeBehavior.Consistent => "Высокая",
            NodeBehavior.Reliable => "Очень высокая",
            NodeBehavior.Unpredictable => "Средняя",
            NodeBehavior.Flaky => "Низкая",
            _ => "Неизвестно"
        };
        
        public async Task<NodeStatus> PerformAutonomousWorkAsync()
        {
            State = NodeState.Busy;
            var stopwatch = Stopwatch.StartNew();
            
            // Имитация работы с учётом поведения
            await Task.Delay(_random.Next(100, 500));
            
            bool success = DetermineSuccess();
            stopwatch.Stop();
            
            if (success)
            {
                _successCount++;
                State = NodeState.Ready;
                return new NodeStatus(Name, "Завершено успешно", stopwatch.ElapsedMilliseconds);
            }
            else
            {
                _failureCount++;
                State = NodeState.Failed;
                _lastFailure = DateTime.Now;
                return new NodeStatus(Name, "Завершено с ошибкой", stopwatch.ElapsedMilliseconds);
            }
        }
        
        public async Task<string> SendToAutonomousNodeAsync(AutonomousNode receiver, string message)
        {
            _autonomousDecisions++;
            
            // Учитываем поведение при отправке
            if (ShouldFailBasedOnBehavior())
            {
                await Task.Delay(_random.Next(200, 1000));
                throw new InvalidOperationException($"Узел {Name} отказал при отправке");
            }
            
            // Имитация задержки
            int delay = Behavior switch
            {
                NodeBehavior.Consistent => _random.Next(50, 150),
                NodeBehavior.Reliable => _random.Next(20, 100),
                NodeBehavior.Unpredictable => _random.Next(10, 500),
                NodeBehavior.Flaky => _random.Next(100, 1000),
                _ => 200
            };
            
            await Task.Delay(delay);
            
            // Реакция получателя
            if (receiver.ShouldFailBasedOnBehavior())
            {
                throw new TimeoutException($"Узел {receiver.Name} не ответил");
            }
            
            return $"{receiver.Name} получил '{message}' от {Name}";
        }
        
        public async Task<(NodeState State, int HealthScore)> GetCurrentStatusAsync()
        {
            await Task.Delay(10);
            
            int healthScore = 100;
            
            // Влияние поведения на здоровье
            healthScore -= Behavior switch
            {
                NodeBehavior.Flaky => 40,
                NodeBehavior.Unpredictable => 20,
                _ => 0
            };
            
            // Влияние последней ошибки
            if (_lastFailure.HasValue && (DateTime.Now - _lastFailure.Value).TotalSeconds < 10)
                healthScore -= 30;
            
            // Случайные флуктуации
            healthScore -= _random.Next(0, 10);
            
            healthScore = Math.Max(0, Math.Min(100, healthScore));
            
            if (healthScore < 30)
                State = NodeState.Failed;
            else if (healthScore < 60)
                State = NodeState.Slow;
            else
                State = NodeState.Ready;
            
            return (State, healthScore);
        }
        
        public async Task AttemptRecoveryAsync()
        {
            Console.Write($"        {Name}: Попытка восстановления... ");
            
            State = NodeState.Recovering;
            await Task.Delay(_random.Next(500, 2000));
            
            bool recoverySuccess = _random.Next(100) > 20; // 80% успеха восстановления
            
            if (recoverySuccess)
            {
                State = NodeState.Ready;
                Console.WriteLine("Успешно ✓");
            }
            else
            {
                State = NodeState.Failed;
                Console.WriteLine("Не удалось ✗");
            }
        }
        
        public (int SuccessfulOperations, int FailedOperations, double DowntimePercent, int AutonomousDecisions) 
            GetAutonomyStatistics()
        {
            double totalOps = _successCount + _failureCount;
            double downtimePercent = totalOps > 0 ? (_failureCount / totalOps) * 100 : 0;
            
            return (_successCount, _failureCount, downtimePercent, _autonomousDecisions);
        }
        
        private bool DetermineSuccess()
        {
            return Behavior switch
            {
                NodeBehavior.Consistent => _random.Next(100) > 5,   // 95% успеха
                NodeBehavior.Reliable => _random.Next(100) > 2,     // 98% успеха
                NodeBehavior.Unpredictable => _random.Next(100) > 40, // 60% успеха
                NodeBehavior.Flaky => _random.Next(100) > 60,       // 40% успеха
                _ => _random.Next(100) > 50
            };
        }
        
        private bool ShouldFailBasedOnBehavior()
        {
            return Behavior switch
            {
                NodeBehavior.Consistent => _random.Next(100) < 5,   // 5% вероятность отказа
                NodeBehavior.Reliable => _random.Next(100) < 2,     // 2% вероятность отказа
                NodeBehavior.Unpredictable => _random.Next(100) < 40, // 40% вероятность отказа
                NodeBehavior.Flaky => _random.Next(100) < 60,       // 60% вероятность отказа
                _ => _random.Next(100) < 30
            };
        }
    }
    
    record NodeStatus(string NodeName, string Status, long DurationMs);
}