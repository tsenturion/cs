using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace UdpMultipleClients
{
	// Демонстрация работы UDP с несколькими клиентами
	public class UdpMultiClientServer : IDisposable
	{
		private UdpClient _server;
		private Thread _receiverThread;
		private bool _isRunning;
		private int _port;

		// Учет клиентов - полностью на уровне приложения
		private readonly ConcurrentDictionary<string, ClientInfo> _clients = new();
		private readonly object _cleanupLock = new object();
		private Timer _cleanupTimer;

		// Жизненный цикл клиента
		private class ClientInfo
		{
			public IPEndPoint Endpoint { get; }
			public DateTime FirstSeen { get; }
			public DateTime LastSeen { get; set; }
			public int PacketCount { get; set; }
			public string ClientId { get; set; }
			public Dictionary<string, object> SessionData { get; } = new();

			public ClientInfo(IPEndPoint endpoint)
			{
				Endpoint = endpoint;
				FirstSeen = DateTime.UtcNow;
				LastSeen = FirstSeen;
				PacketCount = 0;
				ClientId = $"{endpoint.Address}:{endpoint.Port}";
			}

			public bool IsActive(TimeSpan timeout)
			{
				return DateTime.UtcNow - LastSeen < timeout;
			}

			public void UpdateActivity()
			{
				LastSeen = DateTime.UtcNow;
				PacketCount++;
			}
		}

		public int ActiveClients => _clients.Count;

		public UdpMultiClientServer(int port = 11060)
		{
			_port = port;
		}

		public void Start()
		{
			Console.WriteLine("=== UDP СЕРВЕР С РАБОТОЙ С НЕСКОЛЬКИМИ КЛИЕНТАМИ ===\n");

			try
			{
				// Один сокет для всех клиентов
				_server = new UdpClient(_port);
				_server.Client.ReceiveBufferSize = 1024 * 1024;

				Console.WriteLine($"Сервер запущен на порту {_port}");
				Console.WriteLine($"Один сокет обслуживает всех клиентов");
				Console.WriteLine($"ReceiveBufferSize: {_server.Client.ReceiveBufferSize / 1024} KB");

				_isRunning = true;
				_receiverThread = new Thread(ReceiveLoop);
				_receiverThread.IsBackground = true;
				_receiverThread.Start();

				// Таймер для очистки неактивных клиентов (полностью прикладная логика)
				_cleanupTimer = new Timer(CleanupInactiveClients, null, 5000, 5000);

				Console.WriteLine("\n[Сервер готов] Ожидает пакеты от любых отправителей...");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Ошибка запуска сервера: {ex.Message}");
			}
		}

		private void ReceiveLoop()
		{
			Console.WriteLine($"[Поток приёма] Запущен, PID: {Process.GetCurrentProcess().Id}");

			while (_isRunning)
			{
				try
				{
					// Блокирующий вызов - ОС ждёт любой пакет на этот порт
					IPEndPoint remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);
					byte[] data = _server.Receive(ref remoteEndpoint);

					// Обработка полученного пакета
					ProcessIncomingPacket(data, remoteEndpoint);
				}
				catch (SocketException ex) when (ex.SocketErrorCode == SocketError.Interrupted)
				{
					break;
				}
				catch (Exception ex)
				{
					Console.WriteLine($"[Ошибка приёма] {ex.Message}");
					Thread.Sleep(100);
				}
			}

			Console.WriteLine("[Поток приёма] Завершён");
		}

		private void ProcessIncomingPacket(byte[] data, IPEndPoint remoteEndpoint)
		{
			// Каждый пакет приходит с конкретным endpoint
			// Один IP + разные порты = разные клиенты
			string clientKey = $"{remoteEndpoint.Address}:{remoteEndpoint.Port}";

			// Обновляем или создаём информацию о клиенте
			var clientInfo = _clients.GetOrAdd(clientKey,
				key => new ClientInfo(remoteEndpoint));

			clientInfo.UpdateActivity();

			string message = Encoding.UTF8.GetString(data);

			Console.WriteLine($"\n[Пакет от {clientKey}]");
			Console.WriteLine($"  Размер: {data.Length} байт");
			Console.WriteLine($"  Сообщение: {message}");
			Console.WriteLine($"  Всего пакетов от этого клиента: {clientInfo.PacketCount}");
			Console.WriteLine($"  Активных клиентов всего: {_clients.Count}");

			// Демонстрация разных сценариев обработки
			ProcessMessageBasedOnContent(message, clientInfo, data);
		}

		private void ProcessMessageBasedOnContent(string message, ClientInfo clientInfo, byte[] originalData)
		{
			// Разная логика обработки в зависимости от содержимого
			byte[] response;

			if (message.StartsWith("ECHO:"))
			{
				// Эхо-ответ
				response = Encoding.UTF8.GetBytes($"ECHO_RESPONSE: {message.Substring(5)}");
			}
			else if (message.StartsWith("TIME"))
			{
				// Запрос времени
				response = Encoding.UTF8.GetBytes($"TIME: {DateTime.UtcNow:HH:mm:ss.fff}");
			}
			else if (message.StartsWith("STATS"))
			{
				// Статистика клиента
				var stats = $"STATS: Packets={clientInfo.PacketCount}, " +
						   $"Active={(DateTime.UtcNow - clientInfo.FirstSeen).TotalSeconds:F1}s";
				response = Encoding.UTF8.GetBytes(stats);
			}
			else if (message.StartsWith("BROADCAST_REQUEST"))
			{
				// Клиент запрашивает широковещательный ответ
				Console.WriteLine($"  [Широковещательный запрос от {clientInfo.ClientId}]");
				SendBroadcastResponse();
				return; // Не отправляем обычный ответ
			}
			else
			{
				response = Encoding.UTF8.GetBytes($"DEFAULT: {message}");
			}

			// Отправка ответа конкретному клиенту
			try
			{
				_server.Send(response, response.Length, clientInfo.Endpoint);
				Console.WriteLine($"  Ответ отправлен на {clientInfo.Endpoint}");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"  Ошибка отправки ответа: {ex.Message}");
			}
		}

		private void CleanupInactiveClients(object state)
		{
			lock (_cleanupLock)
			{
				var timeout = TimeSpan.FromSeconds(30); // Клиент считается неактивным через 30 секунд
				var keysToRemove = new List<string>();

				foreach (var kvp in _clients)
				{
					if (!kvp.Value.IsActive(timeout))
					{
						keysToRemove.Add(kvp.Key);
					}
				}

				if (keysToRemove.Count > 0)
				{
					foreach (var key in keysToRemove)
					{
						if (_clients.TryRemove(key, out var removedClient))
						{
							Console.WriteLine($"[Очистка] Удалён неактивный клиент: {removedClient.ClientId}");
							Console.WriteLine($"         Неактивен: {(DateTime.UtcNow - removedClient.LastSeen).TotalSeconds:F1} секунд");
						}
					}
				}
			}
		}

		public void SendToAllClients(string message)
		{
			Console.WriteLine($"\n[Рассылка всем клиентам] '{message}'");
			Console.WriteLine($"  Активных клиентов: {_clients.Count}");

			byte[] data = Encoding.UTF8.GetBytes($"BROADCAST_SERVER: {message}");

			int sentCount = 0;
			var failedClients = new List<string>();

			// Отправка каждому клиенту отдельно
			foreach (var kvp in _clients)
			{
				try
				{
					_server.Send(data, data.Length, kvp.Value.Endpoint);
					sentCount++;
				}
				catch (Exception ex)
				{
					failedClients.Add($"{kvp.Key} ({ex.Message})");
				}
			}

			Console.WriteLine($"  Отправлено успешно: {sentCount}/{_clients.Count}");
			if (failedClients.Count > 0)
			{
				Console.WriteLine($"  Не удалось отправить:");
				foreach (var failed in failedClients)
				{
					Console.WriteLine($"    - {failed}");
				}
			}
		}

		public void PrintClientStatistics()
		{
			Console.WriteLine($"\n[СТАТИСТИКА КЛИЕНТОВ]");
			Console.WriteLine($"  Всего уникальных endpoint-ов: {_clients.Count}");
			Console.WriteLine($"  Активных (последние 30 сек): {CountActiveClients(TimeSpan.FromSeconds(30))}");

			if (_clients.Count > 0)
			{
				Console.WriteLine($"\n  Детали по клиентам:");
				foreach (var kvp in _clients)
				{
					var client = kvp.Value;
					var inactiveTime = DateTime.UtcNow - client.LastSeen;

					Console.WriteLine($"    - {client.ClientId}:");
					Console.WriteLine($"        Пакетов: {client.PacketCount}");
					Console.WriteLine($"        Первый пакет: {client.FirstSeen:HH:mm:ss}");
					Console.WriteLine($"        Последний пакет: {client.LastSeen:HH:mm:ss}");
					Console.WriteLine($"        Неактивен: {inactiveTime.TotalSeconds:F1} секунд");
				}
			}
		}

		private int CountActiveClients(TimeSpan timeout)
		{
			int count = 0;
			foreach (var client in _clients.Values)
			{
				if (client.IsActive(timeout))
				{
					count++;
				}
			}
			return count;
		}

		private void SendBroadcastResponse()
		{
			try
			{
				Console.WriteLine($"\n[Отправка широковещательного ответа]");

				// Используем временный сокет для широковещания
				using (var broadcastClient = new UdpClient())
				{
					broadcastClient.EnableBroadcast = true;

					byte[] broadcastData = Encoding.UTF8.GetBytes("SERVER_DISCOVERY_RESPONSE");
					var broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, _port + 1);

					broadcastClient.Send(broadcastData, broadcastData.Length, broadcastEndpoint);
					Console.WriteLine($"  Широковещательный ответ отправлен на порт {_port + 1}");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"  Ошибка широковещательной отправки: {ex.Message}");
			}
		}

		public void Dispose()
		{
			_isRunning = false;

			_cleanupTimer?.Dispose();
			_server?.Close();

			_receiverThread?.Join(1000);

			Console.WriteLine($"\n[Сервер остановлен] Удалено клиентов: {_clients.Count}");
			_clients.Clear();
		}
	}

	// Демонстрация UDP клиента
	public class UdpMultiClientDemo
	{
		public static void RunClientSimulation(int serverPort)
		{
			Console.WriteLine("\n\n=== ДЕМОНСТРАЦИЯ КЛИЕНТОВ ===");

			// Симуляция нескольких клиентов
			var clients = new List<UdpClient>();
			var clientEndpoints = new List<IPEndPoint>();

			try
			{
				// Создаём нескольких клиентов
				Console.WriteLine($"\n1. Создание 3 клиентов:");
				for (int i = 0; i < 3; i++)
				{
					// Каждый клиент использует случайный порт (ОС назначает автоматически)
					var client = new UdpClient(0); // Порт 0 = ОС выберет свободный
					client.Connect("localhost", serverPort);

					clients.Add(client);
					clientEndpoints.Add((IPEndPoint)client.Client.LocalEndPoint);

					Console.WriteLine($"   Клиент {i + 1}: Локальный порт {clientEndpoints[i].Port}");
				}

				// Отправка сообщений от разных клиентов
				Console.WriteLine($"\n2. Отправка сообщений:");

				for (int i = 0; i < clients.Count; i++)
				{
					string message = $"Сообщение от клиента {i + 1} (порт {clientEndpoints[i].Port})";
					byte[] data = Encoding.UTF8.GetBytes(message);

					clients[i].Send(data, data.Length);
					Console.WriteLine($"   Клиент {i + 1} отправил: '{message}'");

					Thread.Sleep(500); // Пауза между отправками
				}

				// Широковещательный запрос
				Console.WriteLine($"\n3. Широковещательный запрос:");

				using (var broadcastClient = new UdpClient())
				{
					broadcastClient.EnableBroadcast = true;

					byte[] broadcastData = Encoding.UTF8.GetBytes("BROADCAST_REQUEST");
					var broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, serverPort);

					broadcastClient.Send(broadcastData, broadcastData.Length, broadcastEndpoint);
					Console.WriteLine($"   Широковещательный запрос отправлен");
				}

				// Получение ответов
				Console.WriteLine($"\n4. Получение ответов:");

				for (int i = 0; i < clients.Count; i++)
				{
					if (clients[i].Available > 0)
					{
						try
						{
							// Асинхронное получение с таймаутом
							var receiveTask = clients[i].ReceiveAsync();
							if (receiveTask.Wait(1000))
							{
								var result = receiveTask.Result;
								string response = Encoding.UTF8.GetString(result.Buffer);
								Console.WriteLine($"   Клиент {i + 1} получил ответ: '{response}'");
							}
						}
						catch { }
					}
				}

				// Смена порта клиента (симуляция)
				Console.WriteLine($"\n5. Смена порта клиента (симуляция NAT):");

				var originalClient = clients[0];
				var originalEndpoint = clientEndpoints[0];

				// "Забываем" старый сокет и создаём новый с другим портом
				originalClient.Close();

				var newClient = new UdpClient(0);
				newClient.Connect("localhost", serverPort);

				string newMessage = "Новое сообщение после смены порта";
				byte[] newData = Encoding.UTF8.GetBytes(newMessage);
				newClient.Send(newData, newData.Length);

				Console.WriteLine($"   Клиент сменил порт {originalEndpoint.Port} -> {((IPEndPoint)newClient.Client.LocalEndPoint).Port}");
				Console.WriteLine($"   Для сервера это выглядит как новый клиент");

				// Очистка
				Thread.Sleep(1000);
			}
			finally
			{
				foreach (var client in clients)
				{
					client.Close();
				}

				Console.WriteLine($"\n[Клиенты завершены]");
			}
		}

		public static void DemonstrateBroadcastDiscovery()
		{
			Console.WriteLine("\n\n=== ДЕМОНСТРАЦИЯ ШИРОКОВЕЩАТЕЛЬНОГО ОБНАРУЖЕНИЯ ===");

			// Клиент ищет серверы в локальной сети
			Console.WriteLine("\n1. Клиент отправляет широковещательный запрос:");

			using (var discoveryClient = new UdpClient(12000))
			{
				discoveryClient.EnableBroadcast = true;

				// Широковещательный запрос
				byte[] discoveryData = Encoding.UTF8.GetBytes("SERVICE_DISCOVERY");
				var broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, 12001); // Порт для ответов

				discoveryClient.Send(discoveryData, discoveryData.Length, broadcastEndpoint);
				Console.WriteLine($"   Запрос отправлен на {broadcastEndpoint}");

				// Ожидание ответов
				Console.WriteLine("\n2. Ожидание ответов от серверов:");

				discoveryClient.Client.ReceiveTimeout = 3000; // Таймаут 3 секунды

				var servers = new List<IPEndPoint>();
				var stopwatch = Stopwatch.StartNew();

				while (stopwatch.ElapsedMilliseconds < 3000)
				{
					try
					{
						IPEndPoint serverEndpoint = new IPEndPoint(IPAddress.Any, 0);
						byte[] response = discoveryClient.Receive(ref serverEndpoint);

						string responseMessage = Encoding.UTF8.GetString(response);
						Console.WriteLine($"   Найден сервер: {serverEndpoint}");
						Console.WriteLine($"     Ответ: {responseMessage}");

						servers.Add(serverEndpoint);
					}
					catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
					{
						break;
					}
					catch { }
				}

				Console.WriteLine($"\n   Всего найдено серверов: {servers.Count}");
			}
		}
	}

	// Главная программа
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("UDP: РАБОТА С НЕСКОЛЬКИМИ КЛИЕНТАМИ И ШИРОКОВЕЩАТЕЛЬНЫЕ СЦЕНАРИИ");
			Console.WriteLine("==============================================================\n");

			int serverPort = 11060;

			// Запуск сервера
			using (var server = new UdpMultiClientServer(serverPort))
			{
				server.Start();

				Thread.Sleep(1000); // Даём время серверу запуститься

				// Демонстрация работы клиентов
				UdpMultiClientDemo.RunClientSimulation(serverPort);

				// Рассылка всем клиентам
				Thread.Sleep(1000);
				server.SendToAllClients("Тестовая рассылка");

				// Статистика
				Thread.Sleep(1000);
				server.PrintClientStatistics();

				// Демонстрация широковещательного обнаружения
				Thread.Sleep(1000);
				UdpMultiClientDemo.DemonstrateBroadcastDiscovery();

				// Даём время завершиться
				Thread.Sleep(2000);
			}
		}
	}
}