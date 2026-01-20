using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkBasicsDemo
{
	class Program
	{
		static async Task Main()
		{
			Console.WriteLine("=== БАЗОВЫЕ КОНЦЕПЦИИ КОМПЬЮТЕРНЫХ СЕТЕЙ ===\n");

			// Часть 1: Сетевая среда как отдельная среда выполнения
			Console.WriteLine("1. СЕТЬ КАК СРЕДА ВЫПОЛНЕНИЯ:");
			DemonstrateNetworkEnvironment();

			// Часть 2: Ненадёжность сети и асинхронность
			Console.WriteLine("\n2. НЕНАДЁЖНОСТЬ СЕТИ И АСИНХРОННОСТЬ:");
			await DemonstrateNetworkUnreliabilityAsync();

			// Часть 3: Распределённость и взаимодействие компонентов
			Console.WriteLine("\n3. РАСПРЕДЕЛЁННОСТЬ И ВЗАИМОДЕЙСТВИЕ:");
			await DemonstrateDistributedComponentsAsync();

			// Часть 4: Границы ответственности в сетевых приложениях
			Console.WriteLine("\n4. ГРАНИЦЫ ОТВЕТСТВЕННОСТИ:");
			DemonstrateResponsibilityBoundaries();

			// Часть 5: Практические последствия для разработчика
			Console.WriteLine("\n5. ПРАКТИЧЕСКИЕ СЛЕДСТВИЯ ДЛЯ РАЗРАБОТЧИКА:");
			DemonstrateDeveloperImplications();
		}

		static void DemonstrateNetworkEnvironment()
		{
			Console.WriteLine($"  ПРОГРАММА КАК УЧАСТНИК СЕТИ:");

			// Получаем информацию о текущем компьютере в сети
			string hostName = Dns.GetHostName();
			Console.WriteLine($"\n  Имя текущего хоста: {hostName}");

			IPAddress[] localIPs = Dns.GetHostAddresses(hostName);
			Console.WriteLine($"  IP адреса текущего хоста:");
			foreach (var ip in localIPs)
			{
				if (ip.AddressFamily == AddressFamily.InterNetwork) // IPv4
				{
					Console.WriteLine($"    - {ip} (IPv4)");
				}
			}

			// Проверяем доступность сетевых интерфейсов
			Console.WriteLine($"\n  СЕТЕВЫЕ ИНТЕРФЕЙСЫ:");
			NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
			int activeInterfaces = 0;

			foreach (var ni in interfaces)
			{
				if (ni.OperationalStatus == OperationalStatus.Up)
				{
					activeInterfaces++;
					Console.WriteLine($"    {ni.Name}: {ni.Description}");

					// Статистика по интерфейсу
					IPv4InterfaceStatistics stats = ni.GetIPv4Statistics();
					Console.WriteLine($"      Получено: {stats.BytesReceived} байт, " +
									$"Отправлено: {stats.BytesSent} байт");
				}
			}

			Console.WriteLine($"\n  Активных интерфейсов: {activeInterfaces}/{interfaces.Length}");

			// Пинг локального хоста (симуляция сетевого взаимодействия)
			Console.WriteLine($"\n  ПРОВЕРКА ЛОКАЛЬНОЙ СВЯЗИ:");
			try
			{
				Ping ping = new Ping();
				PingReply reply = ping.Send("127.0.0.1", 1000); // localhost

				if (reply.Status == IPStatus.Success)
				{
					Console.WriteLine($"    Ping 127.0.0.1: {reply.RoundtripTime} мс");
					Console.WriteLine($"    Программа работает в сетевой среде");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"    Ошибка ping: {ex.Message}");
			}

			// Демонстрация DNS разрешения
			Console.WriteLine($"\n  DNS РАЗРЕШЕНИЕ (преобразование имён в IP):");
			try
			{
				IPHostEntry googleEntry = Dns.GetHostEntry("www.google.com");
				Console.WriteLine($"    www.google.com разрешается в:");
				foreach (var ip in googleEntry.AddressList)
				{
					Console.WriteLine($"      - {ip}");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"    Ошибка DNS: {ex.Message}");
			}
		}

		static async Task DemonstrateNetworkUnreliabilityAsync()
		{
			Console.WriteLine($"  СЕТЬ ПОТЕНЦИАЛЬНО НЕНАДЁЖНА:");

			// Создаём простой сервер для тестирования
			var server = new SimpleTestServer();
			server.Start();

			// Даём серверу время на запуск
			await Task.Delay(100);

			Console.WriteLine($"\n  ТЕСТИРОВАНИЕ НАДЁЖНОСТИ:");

			// Синхронный подход - что происходит при задержке
			Console.WriteLine($"\n  1. СИНХРОННЫЙ ПОДХОД (проблемы):");
			try
			{
				string syncResult = server.SimulateSyncRequest("Test1");
				Console.WriteLine($"    Запрос 1 (быстрый): {syncResult}");

				// Долгий запрос заблокирует интерфейс
				Console.Write($"    Запрос 2 (медленный): ");
				string slowResult = server.SimulateSlowSyncRequest("Test2");
				Console.WriteLine($"{slowResult}");

				// Запрос с ошибкой
				Console.Write($"    Запрос 3 (с ошибкой): ");
				string errorResult = server.SimulateErrorSyncRequest("Test3");
				Console.WriteLine($"{errorResult}");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"    Исключение: {ex.Message}");
			}

			// Асинхронный подход - правильная работа с сетью
			Console.WriteLine($"\n  2. АСИНХРОННЫЙ ПОДХОД (правильно):");

			try
			{
				var tasks = new List<Task<string>>
				{
					server.SimulateAsyncRequest("Async1"),
					server.SimulateSlowAsyncRequest("Async2"),
					server.SimulateErrorAsyncRequest("Async3"),
					server.SimulateAsyncRequest("Async4")
				};

				Console.WriteLine($"    Отправлено {tasks.Count} запросов параллельно");

				// Ожидаем завершения с таймаутом
				Task completedTask = await Task.WhenAny(
					Task.WhenAll(tasks),
					Task.Delay(3000));

				if (completedTask == tasks[0]) // Все задачи завершены
				{
					for (int i = 0; i < tasks.Count; i++)
					{
						try
						{
							string result = await tasks[i];
							Console.WriteLine($"    Запрос {i + 1}: {result}");
						}
						catch (Exception ex)
						{
							Console.WriteLine($"    Запрос {i + 1}: ОШИБКА - {ex.Message}");
						}
					}
				}
				else
				{
					Console.WriteLine($"    Превышено время ожидания (таймаут)");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"    Исключение: {ex.Message}");
			}

			// Демонстрация таймаутов
			Console.WriteLine($"\n  3. ТАЙМАУТЫ И ПОВТОРНЫЕ ПОПЫТКИ:");

			var retryService = new RetryService();
			string finalResult = await retryService.ExecuteWithRetriesAsync(
				async () => await server.SimulateUnreliableRequest("RetryTest"),
				maxRetries: 3,
				delayMs: 1000);

			Console.WriteLine($"    Результат после повторов: {finalResult}");

			server.Stop();
		}

		static async Task DemonstrateDistributedComponentsAsync()
		{
			Console.WriteLine($"  РАСПРЕДЕЛЁННЫЕ КОМПОНЕНТЫ:");

			// Создаём несколько независимых компонентов
			Console.WriteLine($"\n  СОЗДАНИЕ КОМПОНЕНТОВ:");

			var componentA = new NetworkComponent("Компонент A");
			var componentB = new NetworkComponent("Компонент B");
			var componentC = new NetworkComponent("Компонент C");

			// Настраиваем связи между компонентами
			componentA.ConnectTo(componentB);
			componentB.ConnectTo(componentC);

			Console.WriteLine($"    Создано 3 компонента со связями");
			Console.WriteLine($"    A → B → C");

			// Демонстрация обмена сообщениями
			Console.WriteLine($"\n  ОБМЕН СООБЩЕНИЯМИ:");

			var messages = new[]
			{
				"Привет от компонента A",
				"Данные для обработки",
				"Запрос статуса",
				"Важное уведомление"
			};

			foreach (string message in messages)
			{
				Console.WriteLine($"\n    Отправка: '{message}'");

				try
				{
					string result = await componentA.SendMessageAsync(message);
					Console.WriteLine($"    Результат: {result}");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"    Ошибка: {ex.GetType().Name}: {ex.Message}");
				}

				await Task.Delay(200);
			}

			// Демонстрация независимой работы компонентов
			Console.WriteLine($"\n  НЕЗАВИСИМАЯ РАБОТА:");

			var tasks = new List<Task>
			{
				componentA.PerformWorkAsync("Работа A"),
				componentB.PerformWorkAsync("Работа B"),
				componentC.PerformWorkAsync("Работа C")
			};

			Console.WriteLine($"    Все компоненты работают параллельно");

			await Task.WhenAll(tasks);
			Console.WriteLine($"    Все компоненты завершили работу");

			// Статистика
			Console.WriteLine($"\n  СТАТИСТИКА КОМПОНЕНТОВ:");
			Console.WriteLine($"    A: {componentA.GetStatistics()}");
			Console.WriteLine($"    B: {componentB.GetStatistics()}");
			Console.WriteLine($"    C: {componentC.GetStatistics()}");

			componentA.Dispose();
			componentB.Dispose();
			componentC.Dispose();
		}

		static void DemonstrateResponsibilityBoundaries()
		{
			Console.WriteLine($"  ГРАНИЦЫ ОТВЕТСТВЕННОСТИ:");

			// Сервис с чёткими границами ответственности
			var boundaryService = new BoundaryService();

			Console.WriteLine($"\n  1. ОБРАБОТКА ВНЕШНИХ ОШИБОК:");

			string[] testInputs =
			{
				"valid_data",
				"", // пустые данные
                "error_data",
				"timeout_data",
				"very_long_data_that_might_cause_problems"
			};

			foreach (string input in testInputs)
			{
				Console.Write($"\n    Ввод: '{input}' -> ");

				try
				{
					string result = boundaryService.ProcessWithBoundaries(input);
					Console.WriteLine($"Результат: {result}");
				}
				catch (ValidationException ex)
				{
					Console.WriteLine($"ВНУТРЕННЯЯ ошибка: {ex.Message}");
					// Эту ошибку мы обрабатываем сами
				}
				catch (ExternalServiceException ex)
				{
					Console.WriteLine($"ВНЕШНЯЯ ошибка: {ex.Message}");
					// Запрашиваем действие у пользователя
				}
				catch (TimeoutException)
				{
					Console.WriteLine($"ТАЙМАУТ: Сервис не отвечает");
					// Предлагаем повторить позже
				}
				catch (Exception ex)
				{
					Console.WriteLine($"НЕИЗВЕСТНАЯ ошибка: {ex.GetType().Name}");
					// Логируем и сообщаем о проблеме
				}
			}

			Console.WriteLine($"\n  2. КОНТРОЛЬ И КВОТЫ:");

			var quotaService = new QuotaService(maxRequestsPerMinute: 5);

			for (int i = 1; i <= 7; i++)
			{
				Console.Write($"    Запрос {i}: ");

				if (quotaService.CanMakeRequest())
				{
					Console.WriteLine($"Разрешён");
					quotaService.RecordRequest();
				}
				else
				{
					Console.WriteLine($"Отклонён (превышена квота)");
				}
			}

			Console.WriteLine($"\n  3. ЛОКАЛЬНОЕ КЭШИРОВАНИЕ:");

			var cacheService = new CacheService();

			string[] keys = { "user:1", "config:app", "data:report" };

			foreach (string key in keys)
			{
				string data = cacheService.GetOrFetch(key, () =>
				{
					Console.WriteLine($"    Загрузка из внешнего источника: {key}");
					return $"Данные для {key}";
				});

				Console.WriteLine($"    Получено: {data}");
			}

			// Второй проход - данные берутся из кэша
			Console.WriteLine($"\n    Повторный запрос (из кэша):");
			foreach (string key in keys)
			{
				string data = cacheService.GetOrFetch(key, () =>
				{
					Console.WriteLine($"    Этого не должно быть видно!");
					return "ERROR";
				});

				Console.WriteLine($"    {key}: {data}");
			}
		}

		static void DemonstrateDeveloperImplications()
		{
			Console.WriteLine($"  ПРАКТИЧЕСКИЕ СЛЕДСТВИЯ:");

			Console.WriteLine($"\n  1. ПРОИЗВОДИТЕЛЬНОСТЬ И ОТЗЫВЧИВОСТЬ:");

			var uiSimulator = new UISimulator();

			// Синхронная операция блокирует UI
			Console.WriteLine($"\n    Синхронная операция:");
			uiSimulator.SimulateUIOperation(() =>
			{
				Thread.Sleep(2000); // Сетевая задержка
				return "Результат";
			});

			// Асинхронная операция не блокирует UI
			Console.WriteLine($"\n    Асинхронная операция:");
			uiSimulator.SimulateAsyncUIOperation(async () =>
			{
				await Task.Delay(2000); // Сетевая задержка
				return "Результат";
			});

			Console.WriteLine($"\n  2. ОБРАБОТКА ОШИБОК В РАСПРЕДЕЛЁННОЙ СИСТЕМЕ:");

			var errorHandler = new DistributedErrorHandler();

			string[] operations =
			{
				"read_database",
				"call_external_api",
				"process_file",
				"send_notification"
			};

			foreach (string operation in operations)
			{
				Console.WriteLine($"\n    Операция: {operation}");
				errorHandler.ExecuteWithErrorHandling(operation);
			}

			Console.WriteLine($"\n  3. МОНИТОРИНГ И ЛОГИРОВАНИЕ:");

			var monitoredService = new MonitoredService();

			monitoredService.DoSomethingImportant("Важная операция 1");
			monitoredService.DoSomethingImportant("Важная операция 2");

			Console.WriteLine($"\n    Статистика сервиса:");
			Console.WriteLine($"    Успешных операций: {monitoredService.SuccessCount}");
			Console.WriteLine($"    Ошибок: {monitoredService.ErrorCount}");
			Console.WriteLine($"    Среднее время: {monitoredService.AverageTimeMs:F0} мс");

			Console.WriteLine($"\n  4. ТЕСТИРОВАНИЕ СЕТЕВЫХ СЦЕНАРИЕВ:");

			var testScenarios = new NetworkTestScenarios();
			testScenarios.RunAllTests();
		}
	}

	// Вспомогательные классы для демонстрации

	class SimpleTestServer
	{
		private Random _random = new Random();
		private bool _isRunning = false;

		public void Start() => _isRunning = true;
		public void Stop() => _isRunning = false;

		public string SimulateSyncRequest(string data)
		{
			if (!_isRunning) throw new InvalidOperationException("Сервер не запущен");
			return $"Обработано: {data}";
		}

		public string SimulateSlowSyncRequest(string data)
		{
			Thread.Sleep(3000); // Долгая обработка
			return $"Медленно обработано: {data}";
		}

		public string SimulateErrorSyncRequest(string data)
		{
			throw new InvalidOperationException($"Ошибка обработки: {data}");
		}

		public async Task<string> SimulateAsyncRequest(string data)
		{
			await Task.Delay(_random.Next(50, 200)); // Случайная задержка
			return $"Асинхронно: {data}";
		}

		public async Task<string> SimulateSlowAsyncRequest(string data)
		{
			await Task.Delay(2000); // Долгая задержка
			return $"Медленно асинхронно: {data}";
		}

		public async Task<string> SimulateErrorAsyncRequest(string data)
		{
			await Task.Delay(100);
			if (_random.Next(0, 2) == 0) // 50% вероятность ошибки
				throw new InvalidOperationException($"Случайная ошибка: {data}");
			return $"Успешно: {data}";
		}

		public async Task<string> SimulateUnreliableRequest(string data)
		{
			await Task.Delay(100);
			// 70% вероятность ошибки в первый раз
			if (_random.Next(0, 10) < 7)
				throw new IOException("Временная сетевая ошибка");
			return $"Стабильно: {data}";
		}
	}

	class RetryService
	{
		public async Task<string> ExecuteWithRetriesAsync(
			Func<Task<string>> operation,
			int maxRetries,
			int delayMs)
		{
			for (int attempt = 1; attempt <= maxRetries; attempt++)
			{
				try
				{
					return await operation();
				}
				catch (Exception ex) when (attempt < maxRetries)
				{
					Console.WriteLine($"      Попытка {attempt} неудачна: {ex.Message}");
					Console.WriteLine($"      Повтор через {delayMs} мс...");
					await Task.Delay(delayMs);
				}
			}

			throw new InvalidOperationException($"Все {maxRetries} попыток неудачны");
		}
	}

	class NetworkComponent : IDisposable
	{
		private string _name;
		private List<NetworkComponent> _connections = new List<NetworkComponent>();
		private int _messagesSent = 0;
		private int _messagesReceived = 0;
		private Random _random = new Random();

		public NetworkComponent(string name)
		{
			_name = name;
		}

		public void ConnectTo(NetworkComponent other)
		{
			_connections.Add(other);
		}

		public async Task<string> SendMessageAsync(string message)
		{
			_messagesSent++;

			// Случайная задержка сети
			await Task.Delay(_random.Next(50, 500));

			// Случайная ошибка сети
			if (_random.Next(0, 10) == 0)
				throw new SocketException((int)SocketError.ConnectionReset);

			if (_connections.Count > 0)
			{
				var target = _connections[_random.Next(_connections.Count)];
				_messagesReceived++;
				return $"[{_name} → {target._name}] {message}";
			}

			return $"[{_name}] {message} (нет получателей)";
		}

		public async Task PerformWorkAsync(string work)
		{
			Console.WriteLine($"      [{_name}] Начинает: {work}");

			// Симуляция независимой работы
			await Task.Delay(_random.Next(1000, 3000));

			// Случайный результат
			if (_random.Next(0, 10) > 1) // 80% успеха
				Console.WriteLine($"      [{_name}] Завершил: {work} ✓");
			else
				Console.WriteLine($"      [{_name}] Ошибка в: {work} ✗");
		}

		public string GetStatistics()
		{
			return $"Отправлено: {_messagesSent}, Получено: {_messagesReceived}";
		}

		public void Dispose()
		{
			_connections.Clear();
		}
	}

	class ValidationException : Exception
	{
		public ValidationException(string message) : base(message) { }
	}

	class ExternalServiceException : Exception
	{
		public ExternalServiceException(string message) : base(message) { }
	}

	class BoundaryService
	{
		public string ProcessWithBoundaries(string input)
		{
			// Внутренняя валидация
			if (string.IsNullOrEmpty(input))
				throw new ValidationException("Входные данные не могут быть пустыми");

			if (input == "error_data")
				throw new ExternalServiceException("Внешний сервис вернул ошибку");

			if (input == "timeout_data")
				throw new TimeoutException("Превышено время ожидания ответа");

			if (input.Length > 50)
				throw new ArgumentException("Слишком длинные данные");

			return $"Обработано: {input}";
		}
	}

	class QuotaService
	{
		private int _maxRequestsPerMinute;
		private Queue<DateTime> _requestTimes = new Queue<DateTime>();

		public QuotaService(int maxRequestsPerMinute)
		{
			_maxRequestsPerMinute = maxRequestsPerMinute;
		}

		public bool CanMakeRequest()
		{
			DateTime now = DateTime.Now;
			DateTime cutoff = now.AddMinutes(-1);

			// Удаляем старые записи
			while (_requestTimes.Count > 0 && _requestTimes.Peek() < cutoff)
				_requestTimes.Dequeue();

			return _requestTimes.Count < _maxRequestsPerMinute;
		}

		public void RecordRequest()
		{
			_requestTimes.Enqueue(DateTime.Now);
		}
	}

	class CacheService
	{
		private Dictionary<string, (string value, DateTime expiry)> _cache = new();

		public string GetOrFetch(string key, Func<string> fetchFunction)
		{
			if (_cache.TryGetValue(key, out var cached) && cached.expiry > DateTime.Now)
				return cached.value;

			string value = fetchFunction();
			_cache[key] = (value, DateTime.Now.AddMinutes(5));
			return value;
		}
	}

	class UISimulator
	{
		public void SimulateUIOperation(Func<string> operation)
		{
			Console.Write("      UI: Операция начата... ");
			Stopwatch sw = Stopwatch.StartNew();

			// Блокирующий вызов
			string result = operation();

			sw.Stop();
			Console.WriteLine($"Завершено за {sw.ElapsedMilliseconds} мс: {result}");
		}

		public async void SimulateAsyncUIOperation(Func<Task<string>> operation)
		{
			Console.Write("      UI: Асинхронная операция начата... ");
			Stopwatch sw = Stopwatch.StartNew();

			// Неблокирующий вызов
			var task = operation();

			// UI остаётся отзывчивым
			Console.Write("(UI остаётся отзывчивым) ");

			string result = await task;
			sw.Stop();

			Console.WriteLine($"\n      UI: Завершено за {sw.ElapsedMilliseconds} мс: {result}");
		}
	}

	class DistributedErrorHandler
	{
		public void ExecuteWithErrorHandling(string operation)
		{
			try
			{
				// Симуляция операции, которая может завершиться по-разному
				Random rnd = new Random();
				int outcome = rnd.Next(0, 4);

				switch (outcome)
				{
					case 0:
						Console.WriteLine($"      ✓ Успешно");
						break;
					case 1:
						throw new IOException("Ошибка ввода-вывода");
					case 2:
						throw new UnauthorizedAccessException("Нет доступа");
					case 3:
						throw new InvalidOperationException("Некорректная операция");
				}
			}
			catch (IOException ex)
			{
				Console.WriteLine($"      Сетевая ошибка: {ex.Message}");
				Console.WriteLine($"      Действие: Повторить позже");
			}
			catch (UnauthorizedAccessException ex)
			{
				Console.WriteLine($"      Ошибка доступа: {ex.Message}");
				Console.WriteLine($"      Действие: Запросить права");
			}
			catch (InvalidOperationException ex)
			{
				Console.WriteLine($"      Логическая ошибка: {ex.Message}");
				Console.WriteLine($"      Действие: Исправить код");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"      Неизвестная ошибка: {ex.GetType().Name}");
				Console.WriteLine($"      Действие: Записать в лог и уведомить");
			}
		}
	}

	class MonitoredService
	{
		private int _successCount = 0;
		private int _errorCount = 0;
		private long _totalTimeMs = 0;
		private int _operationCount = 0;

		public int SuccessCount => _successCount;
		public int ErrorCount => _errorCount;
		public double AverageTimeMs => _operationCount > 0 ? (double)_totalTimeMs / _operationCount : 0;

		public void DoSomethingImportant(string operation)
		{
			Stopwatch sw = Stopwatch.StartNew();

			try
			{
				// Симуляция работы
				Thread.Sleep(new Random().Next(100, 500));

				// Случайная ошибка
				if (new Random().Next(0, 10) == 0)
					throw new InvalidOperationException("Случайная ошибка");

				_successCount++;
				Console.WriteLine($"      {operation}: Успешно ✓");
			}
			catch
			{
				_errorCount++;
				Console.WriteLine($"      {operation}: Ошибка ✗");
			}
			finally
			{
				sw.Stop();
				_totalTimeMs += sw.ElapsedMilliseconds;
				_operationCount++;
			}
		}
	}

	class NetworkTestScenarios
	{
		public void RunAllTests()
		{
			Console.WriteLine($"\n    Тестовые сценарии:");

			TestScenario("Быстрый успех", () => Task.FromResult("OK"));
			TestScenario("Долгий ответ", async () =>
			{
				await Task.Delay(2000);
				return "Slow";
			});
			TestScenario("Сетевая ошибка", async () =>
			{
				await Task.Delay(100);
				throw new IOException("Network failure");
			});
			TestScenario("Таймаут", async () =>
			{
				await Task.Delay(5000);
				return "Too late";
			});
		}

		private async void TestScenario(string name, Func<Task<string>> test)
		{
			Console.Write($"\n      {name}: ");

			try
			{
				var timeoutTask = Task.Delay(3000);
				var testTask = test();

				var completedTask = await Task.WhenAny(testTask, timeoutTask);

				if (completedTask == timeoutTask)
					Console.WriteLine("ТАЙМАУТ");
				else
					Console.WriteLine($"УСПЕХ: {await testTask}");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"ОШИБКА: {ex.GetType().Name}");
			}
		}
	}
}