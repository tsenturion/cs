using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace NetworkingFundamentals
{
	// Демонстрация основ компьютерных сетей
	public class NetworkBasicsDemo
	{
		public static void DemonstrateNetworkConcepts()
		{
			Console.WriteLine("=== ОСНОВЫ КОМПЬЮТЕРНЫХ СЕТЕЙ ===\n");

			// 1. Клиент и сервер как роли программы
			Console.WriteLine("1. КЛИЕНТ И СЕРВЕР - РОЛИ ПРОГРАММ:");
			DemonstrateClientServerRoles();

			// 2. IP-адреса и порты
			Console.WriteLine("\n2. IP-АДРЕСА И ПОРТЫ:");
			DemonstrateAddressing();

			// 3. Локальные и удалённые соединения
			Console.WriteLine("\n3. ЛОКАЛЬНЫЕ И УДАЛЁННЫЕ СОЕДИНЕНИЯ:");
			DemonstrateConnectionTypes();

			// 4. TCP как надёжный протокол
			Console.WriteLine("\n4. TCP - НАДЁЖНЫЙ ПРОТОКОЛ:");
			DemonstrateTCPFeatures();

			// 5. Поток байтов в TCP
			Console.WriteLine("\n5. TCP КАК ПОТОК БАЙТОВ:");
			DemonstrateTCPStream();

			// 6. Управление состояниями соединения
			Console.WriteLine("\n6. СОСТОЯНИЯ TCP-СОЕДИНЕНИЯ:");
			DemonstrateConnectionStates();

			// 7. Практическая демонстрация
			Console.WriteLine("\n7. ПРАКТИЧЕСКАЯ ДЕМОНСТРАЦИЯ:");
			DemonstratePracticalExample();
		}

		private static void DemonstrateClientServerRoles()
		{
			Console.WriteLine("   Демонстрация ролей клиента и сервера:");

			// Симуляция: один компьютер запускает обе роли
			Console.WriteLine($"\n   Ситуация 1: Отдельные программы");
			Console.WriteLine($"     Компьютер 1: Запускает СерверПрограмма.exe");
			Console.WriteLine($"     Компьютер 2: Запускает КлиентПрограмма.exe");
			Console.WriteLine($"     Результат: Клиент подключается к Серверу");

			Console.WriteLine($"\n   Ситуация 2: Одна программа, две роли");
			Console.WriteLine($"     Компьютер: Запускает Программа.exe");
			Console.WriteLine($"     Программа создаёт:");
			Console.WriteLine($"       - Серверную часть (слушает порт 8080)");
			Console.WriteLine($"       - Клиентскую часть (подключается к localhost:8080)");
			Console.WriteLine($"     Результат: Внутреннее взаимодействие");

			Console.WriteLine($"\n   Ситуация 3: Многосервисная архитектура");
			Console.WriteLine($"     Сервис A: Сервер для клиентов, Клиент для Сервиса B");
			Console.WriteLine($"     Сервис B: Сервер для Сервиса A");
			Console.WriteLine($"     Результат: Сложное взаимодействие");

			// Практический пример в коде
			Console.WriteLine($"\n   Кодовая демонстрация:");

			var server = new TcpListener(IPAddress.Loopback, 11020);
			server.Start();
			Console.WriteLine($"     Сервер запущен и слушает порт 11020");

			var client = new TcpClient();
			client.Connect(IPAddress.Loopback, 11020);
			Console.WriteLine($"     Клиент подключился к серверу");

			server.Stop();
			client.Close();
			Console.WriteLine($"     Демонстрация завершена");
		}

		private static void DemonstrateAddressing()
		{
			Console.WriteLine("   Адресация в сети:");

			// IP-адреса как адреса домов
			Console.WriteLine($"\n   IP-адрес - 'адрес дома':");

			IPAddress localhost = IPAddress.Loopback;           // 127.0.0.1
			IPAddress localNetwork = IPAddress.Parse("192.168.1.100");
			IPAddress external = IPAddress.Parse("8.8.8.8");    // Google DNS

			Console.WriteLine($"     localhost (этот компьютер): {localhost}");
			Console.WriteLine($"     Локальная сеть: {localNetwork}");
			Console.WriteLine($"     Внешний сервер: {external}");

			// Порт как номер квартиры
			Console.WriteLine($"\n   Порт - 'номер квартиры':");

			var wellKnownPorts = new Dictionary<int, string>
			{
				[80] = "HTTP (веб-серверы)",
				[443] = "HTTPS (безопасный веб)",
				[25] = "SMTP (почта)",
				[53] = "DNS",
				[22] = "SSH (удалённое управление)"
			};

			Console.WriteLine($"     Зарезервированные порты:");
			foreach (var port in wellKnownPorts)
			{
				Console.WriteLine($"       {port.Key} - {port.Value}");
			}

			Console.WriteLine($"\n   Случайные порты (для приложений):");
			Random rand = new Random();
			for (int i = 0; i < 3; i++)
			{
				int port = rand.Next(1024, 65535);
				Console.WriteLine($"       Пример: {port}");
			}

			// Полная адресация: IP + порт
			Console.WriteLine($"\n   Конечная точка (IP + порт):");

			var endpoint1 = new IPEndPoint(localhost, 8080);
			var endpoint2 = new IPEndPoint(external, 443);

			Console.WriteLine($"     Веб-сервер на локальной машине: {endpoint1}");
			Console.WriteLine($"     Google HTTPS сервер: {endpoint2}");

			// Создание сокета с конкретным портом
			Console.WriteLine($"\n   Практика: привязка сокета к порту:");
			try
			{
				var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				socket.Bind(new IPEndPoint(IPAddress.Loopback, 11021));
				Console.WriteLine($"     Успешно привязан к порту 11021");
				socket.Close();
			}
			catch (SocketException ex)
			{
				Console.WriteLine($"     Ошибка: {ex.Message}");
				Console.WriteLine($"     Возможно, порт уже используется");
			}
		}

		private static void DemonstrateConnectionTypes()
		{
			Console.WriteLine("   Типы соединений:");

			// Локальные соединения
			Console.WriteLine($"\n   ЛОКАЛЬНЫЕ СОЕДИНЕНИЯ:");
			Console.WriteLine($"     Клиент и сервер на одном компьютере");
			Console.WriteLine($"     Используется IP-адрес: {IPAddress.Loopback}");
			Console.WriteLine($"     Преимущества:");
			Console.WriteLine($"       - Минимальные задержки");
			Console.WriteLine($"       - Нет потерь данных");
			Console.WriteLine($"       - Простая отладка");
			Console.WriteLine($"     Использование: тестирование, IPC");

			// Удалённые соединения
			Console.WriteLine($"\n   УДАЛЁННЫЕ СОЕДИНЕНИЯ:");
			Console.WriteLine($"     Клиент и сервер на разных компьютерах");
			Console.WriteLine($"     Используются реальные IP-адреса");
			Console.WriteLine($"     Особенности:");
			Console.WriteLine($"       - Задержки (латентность)");
			Console.WriteLine($"       - Возможны потери пакетов");
			Console.WriteLine($"       - Может быть фильтрация трафика");
			Console.WriteLine($"       - Разная пропускная способность");
			Console.WriteLine($"     Использование: веб, облачные сервисы, распределённые системы");

			// Практическая демонстрация
			Console.WriteLine($"\n   ДЕМОНСТРАЦИЯ В КОДЕ:");

			// Локальное соединение
			Console.WriteLine($"\n   Локальное соединение:");
			var localServer = new TcpListener(IPAddress.Loopback, 11022);
			localServer.Start();

			var localClient = new TcpClient();
			localClient.Connect(IPAddress.Loopback, 11022);

			Console.WriteLine($"     Сервер запущен на: {localServer.LocalEndpoint}");
			Console.WriteLine($"     Клиент подключён к: {localClient.Client.LocalEndPoint} -> {localClient.Client.RemoteEndPoint}");
			Console.WriteLine($"     Это одно и то же устройство: {((IPEndPoint)localServer.LocalEndpoint).Address.Equals(IPAddress.Loopback)}");

			localClient.Close();
			localServer.Stop();

			// Симуляция проблем удалённого соединения
			Console.WriteLine($"\n   Симуляция проблем удалённого соединения:");
			Console.WriteLine($"     В реальном коде всегда нужно учитывать:");
			Console.WriteLine($"       - Таймауты операций");
			Console.WriteLine($"       - Повторные попытки подключения");
			Console.WriteLine($"       - Обработку разрывов соединения");
			Console.WriteLine($"       - Размер передаваемых данных");

			// Пример с таймаутами
			var remoteClient = new TcpClient();
			remoteClient.ReceiveTimeout = 5000; // 5 секунд
			remoteClient.SendTimeout = 5000;
			Console.WriteLine($"     Установлены таймауты: Receive={remoteClient.ReceiveTimeout}ms, Send={remoteClient.SendTimeout}ms");

			remoteClient.Close();
		}

		private static void DemonstrateTCPFeatures()
		{
			Console.WriteLine("   Особенности TCP протокола:");

			// Надёжность TCP
			Console.WriteLine($"\n   НАДЁЖНОСТЬ TCP:");
			Console.WriteLine($"     Что гарантирует TCP:");
			Console.WriteLine($"       1. Доставка данных (или уведомление об ошибке)");
			Console.WriteLine($"       2. Сохранение порядка данных");
			Console.WriteLine($"       3. Отсутствие дубликатов");
			Console.WriteLine($"       4. Контроль перегрузки сети");
			Console.WriteLine($"       5. Управление потоком данных");

			// Установка соединения (Three-way handshake)
			Console.WriteLine($"\n   УСТАНОВКА СОЕДИНЕНИЯ:");
			Console.WriteLine($"     1. Клиент → Сервер: SYN (запрос на соединение)");
			Console.WriteLine($"     2. Сервер → Клиент: SYN-ACK (подтверждение)");
			Console.WriteLine($"     3. Клиент → Сервер: ACK (окончательное подтверждение)");
			Console.WriteLine($"     Только после этого начинается передача данных");

			// Процесс передачи данных
			Console.WriteLine($"\n   ПЕРЕДАЧА ДАННЫХ:");
			Console.WriteLine($"     - Данные разбиваются на сегменты");
			Console.WriteLine($"     - Каждый сегмент нумеруется");
			Console.WriteLine($"     - Получатель подтверждает получение (ACK)");
			Console.WriteLine($"     - Если подтверждения нет, отправитель повторяет отправку");

			// Закрытие соединения
			Console.WriteLine($"\n   ЗАКРЫТИЕ СОЕДИНЕНИЯ:");
			Console.WriteLine($"     1. Сторона A → Сторона B: FIN (завершение)");
			Console.WriteLine($"     2. Сторона B → Сторона A: ACK (подтверждение)");
			Console.WriteLine($"     3. Сторона B → Сторона A: FIN (со своей стороны)");
			Console.WriteLine($"     4. Сторона A → Сторона B: ACK (окончательное)");
			Console.WriteLine($"     Соединение полностью закрыто");

			// Практическая демонстрация
			Console.WriteLine($"\n   ДЕМОНСТРАЦИЯ В КОДЕ:");

			// Создание TCP сокета (автоматически использует TCP протокол)
			var tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			Console.WriteLine($"     Создан TCP сокет:");
			Console.WriteLine($"       SocketType.Stream = {SocketType.Stream}");
			Console.WriteLine($"       ProtocolType.Tcp = {ProtocolType.Tcp}");

			// Настройка параметров TCP через сокет
			tcpSocket.NoDelay = false; // Включение алгоритма Нагля (оптимизация для небольших пакетов)
			tcpSocket.LingerState = new LingerOption(true, 5); // Задержка при закрытии

			Console.WriteLine($"     Параметры TCP:");
			Console.WriteLine($"       NoDelay: {tcpSocket.NoDelay} (false = оптимизация включена)");
			Console.WriteLine($"       Linger: Enabled={tcpSocket.LingerState.Enabled}, Time={tcpSocket.LingerState.LingerTime}s");

			tcpSocket.Close();
		}

		private static void DemonstrateTCPStream()
		{
			Console.WriteLine("   TCP как поток байтов:");

			// Ключевая концепция
			Console.WriteLine($"\n   КЛЮЧЕВАЯ КОНЦЕПЦИЯ:");
			Console.WriteLine($"     TCP не знает о 'сообщениях', он знает только о байтах");
			Console.WriteLine($"     Пример:");
			Console.WriteLine($"       Клиент отправляет: 'Hello' 'World'");
			Console.WriteLine($"       Сервер может получить: 'HelloWorld' (всё вместе)");
			Console.WriteLine($"       Или: 'He' 'llo' 'Wor' 'ld' (по частям)");
			Console.WriteLine($"       Или: 'HelloW' 'orld' (любая комбинация)");
			Console.WriteLine($"     Это нормально и ожидаемо!");

			// Проблема фрагментации
			Console.WriteLine($"\n   ПРОБЛЕМА ФРАГМЕНТАЦИИ:");
			Console.WriteLine($"     Данные могут быть разбиты по пути:");
			Console.WriteLine($"       - Размером буфера операционной системы");
			Console.WriteLine($"       - Сетевыми устройствами (маршрутизаторами)");
			Console.WriteLine($"       - Настройками протокола");
			Console.WriteLine($"     Решение: приложение само должно определять границы сообщений");

			// Методы определения границ
			Console.WriteLine($"\n   МЕТОДЫ ОПРЕДЕЛЕНИЯ ГРАНИЦ:");
			Console.WriteLine($"     1. Фиксированная длина: все сообщения одинакового размера");
			Console.WriteLine($"     2. Разделители: специальные символы между сообщениями");
			Console.WriteLine($"     3. Заголовок с длиной: сначала отправляется размер данных");
			Console.WriteLine($"     4. Самозавершающиеся форматы (JSON, XML)");

			// Практическая демонстрация
			Console.WriteLine($"\n   ДЕМОНСТРАЦИЯ В КОДЕ:");

			// Тестовые данные
			string[] messages = { "Hello", "World", "TCP", "Stream" };

			Console.WriteLine($"\n   Отправка данных:");
			foreach (var message in messages)
			{
				Console.WriteLine($"     Отправлено: '{message}' ({message.Length} байт)");
			}

			Console.WriteLine($"\n   Что может получить сервер:");
			Console.WriteLine($"     Вариант 1: 'HelloWorldTCPStream' (всё вместе)");
			Console.WriteLine($"     Вариант 2: 'He' 'lloWor' 'ldTCP' 'Stream' (случайные фрагменты)");
			Console.WriteLine($"     Вариант 3: 'H' 'e' 'l' 'l' 'o' 'W' 'o' 'r' 'l' 'd'... (по одному байту)");

			// Демонстрация с разделителями
			Console.WriteLine($"\n   РЕШЕНИЕ: Использование разделителей");
			var delimiter = Encoding.UTF8.GetBytes("\n"); // Перевод строки как разделитель

			foreach (var message in messages)
			{
				byte[] data = Encoding.UTF8.GetBytes(message + "\n");
				Console.WriteLine($"     Формирование: '{message}\\n' = {data.Length} байт");
			}

			// Демонстрация с заголовком длины
			Console.WriteLine($"\n   РЕШЕНИЕ: Заголовок с длиной");
			foreach (var message in messages)
			{
				byte[] messageBytes = Encoding.UTF8.GetBytes(message);
				byte[] lengthBytes = BitConverter.GetBytes(messageBytes.Length);
				Console.WriteLine($"     Формирование: [{lengthBytes.Length} байт длины] + [{messageBytes.Length} байт данных]");
			}

			Console.WriteLine($"\n   ВЫВОД: TCP доставляет байты, границы определяет приложение");
		}

		private static void DemonstrateConnectionStates()
		{
			Console.WriteLine("   Состояния TCP-соединения:");

			// Диаграмма состояний
			Console.WriteLine($"\n   ОСНОВНЫЕ СОСТОЯНИЯ:");
			Console.WriteLine($"     1. LISTEN: Сервер слушает порт");
			Console.WriteLine($"     2. SYN_SENT: Клиент отправил запрос на подключение");
			Console.WriteLine($"     3. SYN_RECEIVED: Сервер получил запрос");
			Console.WriteLine($"     4. ESTABLISHED: Соединение установлено, можно передавать данные");
			Console.WriteLine($"     5. FIN_WAIT: Начало закрытия соединения");
			Console.WriteLine($"     6. CLOSE_WAIT: Ожидание закрытия");
			Console.WriteLine($"     7. CLOSED: Соединение закрыто");

			// Жизненный цикл соединения
			Console.WriteLine($"\n   ЖИЗНЕННЫЙ ЦИКЛ СОЕДИНЕНИЯ:");
			Console.WriteLine($"     Сервер: Создать сокет → Bind → Listen → Accept");
			Console.WriteLine($"     Клиент: Создать сокет → Connect");
			Console.WriteLine($"     Оба: Send/Receive → Shutdown → Close");

			// Разрывы соединения
			Console.WriteLine($"\n   РАЗРЫВЫ СОЕДИНЕНИЯ:");
			Console.WriteLine($"     Причины разрыва:");
			Console.WriteLine($"       - Явное закрытие (Close)");
			Console.WriteLine($"       - Таймаут неактивности");
			Console.WriteLine($"       - Сетевая ошибка");
			Console.WriteLine($"       - Перезагрузка устройства");
			Console.WriteLine($"       - Программа завершила работу");

			Console.WriteLine($"\n     Важно: программа должна быть готова к разрыву в любой момент");

			// Практическая демонстрация
			Console.WriteLine($"\n   ДЕМОНСТРАЦИЯ В КОДЕ:");

			try
			{
				// Серверная часть
				var server = new TcpListener(IPAddress.Loopback, 11023);
				server.Start();
				Console.WriteLine($"     Сервер: состояние LISTEN на порту 11023");

				// Клиентская часть
				var client = new TcpClient();
				Console.WriteLine($"     Клиент: создан сокет");

				client.Connect(IPAddress.Loopback, 11023);
				Console.WriteLine($"     Клиент: состояние ESTABLISHED");

				// Получение сервером подключения
				var serverClient = server.AcceptTcpClient();
				Console.WriteLine($"     Сервер: принял подключение, состояние ESTABLISHED");

				// Передача данных
				string message = "Test";
				byte[] data = Encoding.UTF8.GetBytes(message);
				client.GetStream().Write(data, 0, data.Length);
				Console.WriteLine($"     Клиент: отправил данные");

				// Получение данных
				byte[] buffer = new byte[1024];
				int bytesRead = serverClient.GetStream().Read(buffer, 0, buffer.Length);
				Console.WriteLine($"     Сервер: получил {bytesRead} байт");

				// Закрытие соединения
				Console.WriteLine($"\n     Закрытие соединения:");
				client.Close();
				Console.WriteLine($"     Клиент: инициировал закрытие");

				serverClient.Close();
				Console.WriteLine($"     Сервер: закрыл клиентское соединение");

				server.Stop();
				Console.WriteLine($"     Сервер: остановлен");

				Console.WriteLine($"     Состояние: CLOSED");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"     Ошибка: {ex.Message}");
			}
		}

		private static void DemonstratePracticalExample()
		{
			Console.WriteLine("   Практический пример: простой эхо-сервер");

			const int port = 11024;
			var cts = new CancellationTokenSource();

			// Запуск сервера в фоновом потоке
			var serverTask = Task.Run(() => RunEchoServer(port, cts.Token));

			// Даём время серверу запуститься
			Thread.Sleep(1000);

			// Запуск нескольких клиентов
			var clientTasks = new List<Task>();

			for (int i = 1; i <= 3; i++)
			{
				int clientId = i;
				clientTasks.Add(Task.Run(() => RunEchoClient(clientId, port)));
			}

			// Ожидание завершения клиентов
			Task.WaitAll(clientTasks.ToArray(), 5000);

			// Остановка сервера
			cts.Cancel();
			serverTask.Wait(1000);

			Console.WriteLine($"\n   Демонстрация завершена");
		}

		private static async Task RunEchoServer(int port, CancellationToken cancellationToken)
		{
			var server = new TcpListener(IPAddress.Loopback, port);
			server.Start();

			Console.WriteLine($"\n   [Сервер] Запущен на порту {port}");
			Console.WriteLine($"   [Сервер] Ожидает подключений...");

			try
			{
				while (!cancellationToken.IsCancellationRequested)
				{
					var client = await server.AcceptTcpClientAsync();
					_ = Task.Run(() => HandleClient(client));
				}
			}
			catch (Exception) when (cancellationToken.IsCancellationRequested)
			{
				// Ожидаемое прерывание
			}
			finally
			{
				server.Stop();
				Console.WriteLine($"   [Сервер] Остановлен");
			}
		}

		private static async Task HandleClient(TcpClient client)
		{
			var endpoint = client.Client.RemoteEndPoint;
			Console.WriteLine($"   [Сервер] Принял подключение от {endpoint}");

			try
			{
				var stream = client.GetStream();
				var buffer = new byte[1024];

				while (client.Connected)
				{
					int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
					if (bytesRead == 0)
						break;

					string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
					Console.WriteLine($"   [Сервер] Получено от {endpoint}: '{message}' ({bytesRead} байт)");

					// Эхо-ответ
					await stream.WriteAsync(buffer, 0, bytesRead);
					Console.WriteLine($"   [Сервер] Отправлен эхо-ответ {endpoint}");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"   [Сервер] Ошибка с клиентом {endpoint}: {ex.Message}");
			}
			finally
			{
				client.Close();
				Console.WriteLine($"   [Сервер] Закрыл соединение с {endpoint}");
			}
		}

		private static async Task RunEchoClient(int clientId, int port)
		{
			try
			{
				using (var client = new TcpClient())
				{
					Console.WriteLine($"   [Клиент {clientId}] Подключение к localhost:{port}");
					await client.ConnectAsync(IPAddress.Loopback, port);

					var stream = client.GetStream();
					string message = $"Привет от клиента {clientId}";
					byte[] data = Encoding.UTF8.GetBytes(message);

					Console.WriteLine($"   [Клиент {clientId}] Отправка: '{message}'");
					await stream.WriteAsync(data, 0, data.Length);

					var buffer = new byte[1024];
					int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
					string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

					Console.WriteLine($"   [Клиент {clientId}] Получен ответ: '{response}'");

					client.Close();
					Console.WriteLine($"   [Клиент {clientId}] Отключён");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"   [Клиент {clientId}] Ошибка: {ex.Message}");
			}
		}
	}

	// Главная программа
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("ОСНОВЫ КОМПЬЮТЕРНЫХ СЕТЕЙ И TCP В C#");
			Console.WriteLine("=====================================\n");

			NetworkBasicsDemo.DemonstrateNetworkConcepts();

			Console.WriteLine("\n\nВыводы из модуля:");
			Console.WriteLine("1. Сеть связывает устройства через IP-адреса и порты");
			Console.WriteLine("2. Клиент и сервер - это роли программ, а не компьютеров");
			Console.WriteLine("3. TCP обеспечивает надёжное соединение с гарантией доставки");
			Console.WriteLine("4. TCP работает с потоком байтов, не с готовыми сообщениями");
			Console.WriteLine("5. Соединение - это состояние, которое может оборваться в любой момент");
			Console.WriteLine("6. Сетевой код должен быть готов к ошибкам и нестабильности");
		}
	}
}