using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;

namespace UdpImplementationDemo
{
	// Демонстрация UDP сокетов через низкоуровневый API
	public class UdpRawSocketDemo : IDisposable
	{
		private Socket _udpSocket;
		private Thread _receiveThread;
		private bool _isRunning;
		private readonly int _port;

		public UdpRawSocketDemo(int port = 9000)
		{
			_port = port;
		}

		public void DemonstrateUdpSocket()
		{
			Console.WriteLine("=== UDP RAW SOCKET ДЕМОНСТРАЦИЯ ===\n");

			// 1. Создание UDP сокета
			Console.WriteLine("1. СОЗДАНИЕ UDP СОКЕТА:");

			// SocketType.Dgram для UDP, ProtocolType.Udp
			_udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

			Console.WriteLine($"   Создан UDP сокет:");
			Console.WriteLine($"     SocketType: {_udpSocket.SocketType}");
			Console.WriteLine($"     ProtocolType: {_udpSocket.ProtocolType}");
			Console.WriteLine($"     AddressFamily: {_udpSocket.AddressFamily}");
			Console.WriteLine($"     Handle: {_udpSocket.Handle}");

			// 2. Привязка сокета к порту (для получения данных)
			Console.WriteLine("\n2. ПРИВЯЗКА К ПОРТУ:");

			_udpSocket.Bind(new IPEndPoint(IPAddress.Any, _port));
			Console.WriteLine($"   Сокет привязан к порту {_port}");
			Console.WriteLine($"     IsBound: {_udpSocket.IsBound}");
			Console.WriteLine($"     LocalEndPoint: {_udpSocket.LocalEndPoint}");

			// 3. Демонстрация отправки датаграмм
			Console.WriteLine("\n3. ОТПРАВКА ДАТАГРАММ:");

			// Создаем отдельный сокет для отправки
			using (var senderSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
			{
				var targetEndpoint = new IPEndPoint(IPAddress.Loopback, _port);
				string[] messages =
				{
					"Первая датаграмма",
					"Вторая датаграмма",
					"Третья датаграмма"
				};

				foreach (var message in messages)
				{
					byte[] data = Encoding.UTF8.GetBytes(message);

					// Отправка без установки соединения
					int bytesSent = senderSocket.SendTo(data, targetEndpoint);

					Console.WriteLine($"   Отправлено: '{message}'");
					Console.WriteLine($"     Байт: {bytesSent}");
					Console.WriteLine($"     Получатель: {targetEndpoint}");

					Thread.Sleep(100); // Небольшая задержка для демонстрации
				}
			}

			// 4. Демонстрация приема датаграмм
			Console.WriteLine("\n4. ПРИЁМ ДАТАГРАММ:");

			Console.WriteLine("   Ожидание входящих датаграмм...");

			// Принимаем несколько датаграмм
			for (int i = 0; i < 3; i++)
			{
				var buffer = new byte[1024];
				var senderEndpoint = new IPEndPoint(IPAddress.Any, 0);
				EndPoint remoteEP = senderEndpoint;

				// ReceiveFrom - блокирующий вызов
				int bytesReceived = _udpSocket.ReceiveFrom(buffer, ref remoteEP);

				string message = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
				var sender = (IPEndPoint)remoteEP;

				Console.WriteLine($"   Принято #{i + 1}:");
				Console.WriteLine($"     Сообщение: '{message}'");
				Console.WriteLine($"     Байт: {bytesReceived}");
				Console.WriteLine($"     Отправитель: {sender.Address}:{sender.Port}");
				Console.WriteLine($"     Время получения: {DateTime.Now:HH:mm:ss.fff}");
			}

			// 5. Демонстрация многопоточного приема
			Console.WriteLine("\n5. МНОГОПОТОЧНЫЙ ПРИЁМ:");
			DemonstrateMultithreadedReceive();

			// 6. Демонстрация без гарантий UDP
			Console.WriteLine("\n6. ДЕМОНСТРАЦИЯ БЕЗ ГАРАНТИЙ UDP:");
			DemonstrateUdpLimitations();
		}

		private void DemonstrateMultithreadedReceive()
		{
			_isRunning = true;

			// Запускаем поток для приема в фоне
			_receiveThread = new Thread(() =>
			{
				Console.WriteLine($"   Поток приема запущен (ID: {Thread.CurrentThread.ManagedThreadId})");

				while (_isRunning)
				{
					try
					{
						var buffer = new byte[1024];
						var senderEndpoint = new IPEndPoint(IPAddress.Any, 0);
						EndPoint remoteEP = senderEndpoint;

						// Неблокирующий прием
						if (_udpSocket.Poll(1000, SelectMode.SelectRead))
						{
							int bytesReceived = _udpSocket.ReceiveFrom(buffer, ref remoteEP);

							if (bytesReceived > 0)
							{
								string message = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
								var sender = (IPEndPoint)remoteEP;

								Console.WriteLine($"     [Поток] Принято: '{message}' от {sender.Address}:{sender.Port}");
							}
						}
					}
					catch (SocketException ex) when (ex.SocketErrorCode == SocketError.Interrupted)
					{
						break;
					}
					catch (Exception ex)
					{
						Console.WriteLine($"     [Поток] Ошибка: {ex.Message}");
					}
				}

				Console.WriteLine($"   Поток приема завершен");
			});

			_receiveThread.IsBackground = true;
			_receiveThread.Start();

			// Отправляем тестовые данные
			Thread.Sleep(500);

			using (var sender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
			{
				var target = new IPEndPoint(IPAddress.Loopback, _port);

				for (int i = 1; i <= 3; i++)
				{
					string message = $"Асинхронное сообщение #{i}";
					byte[] data = Encoding.UTF8.GetBytes(message);
					sender.SendTo(data, target);

					Console.WriteLine($"   Отправлено в фон: '{message}'");
					Thread.Sleep(200);
				}
			}

			Thread.Sleep(1000);
			_isRunning = false;
			_receiveThread.Join(1000);
		}

		private void DemonstrateUdpLimitations()
		{
			Console.WriteLine("   Демонстрация ограничений UDP:");

			// Создаем два сокета для демонстрации
			var socket1 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			var socket2 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

			socket1.Bind(new IPEndPoint(IPAddress.Loopback, 9001));
			socket2.Bind(new IPEndPoint(IPAddress.Loopback, 9002));

			// Демонстрация 1: Потеря пакетов (симуляция)
			Console.WriteLine($"\n   a) Потеря пакетов (симуляция):");

			var messages = new[] { "Пакет 1", "Пакет 2", "Пакет 3" };
			int lostPacketIndex = 1; // Симулируем потерю второго пакета

			for (int i = 0; i < messages.Length; i++)
			{
				if (i == lostPacketIndex)
				{
					Console.WriteLine($"     [СИМУЛЯЦИЯ] Пакет #{i + 1} потерян в сети");
					continue;
				}

				byte[] data = Encoding.UTF8.GetBytes(messages[i]);
				var target = new IPEndPoint(IPAddress.Loopback, 9001);
				socket2.SendTo(data, target);
				Console.WriteLine($"     Отправлен пакет #{i + 1}: '{messages[i]}'");
			}

			// Демонстрация 2: Изменение порядка
			Console.WriteLine($"\n   b) Изменение порядка доставки:");

			var outOfOrderMessages = new[]
			{
				"Первое (отправлено первым)",
				"Второе (отправлено вторым)",
				"Третье (отправлено третьим)"
			};

			// Отправляем в обратном порядке для демонстрации
			for (int i = outOfOrderMessages.Length - 1; i >= 0; i--)
			{
				byte[] data = Encoding.UTF8.GetBytes(outOfOrderMessages[i]);
				var target = new IPEndPoint(IPAddress.Loopback, 9002);
				socket1.SendTo(data, target);
				Console.WriteLine($"     Отправлен: '{outOfOrderMessages[i]}'");
			}

			// Демонстрация 3: Дублирование пакетов
			Console.WriteLine($"\n   c) Дублирование пакетов:");

			string duplicateMessage = "Оригинальное сообщение";
			byte[] duplicateData = Encoding.UTF8.GetBytes(duplicateMessage);
			var duplicateTarget = new IPEndPoint(IPAddress.Loopback, 9001);

			// Отправляем один пакет несколько раз (симуляция дублирования)
			for (int i = 0; i < 3; i++)
			{
				socket2.SendTo(duplicateData, duplicateTarget);
				Console.WriteLine($"     Отправлена копия #{i + 1}: '{duplicateMessage}'");
				Thread.Sleep(50);
			}

			socket1.Dispose();
			socket2.Dispose();
		}

		public void Dispose()
		{
			_isRunning = false;
			_udpSocket?.Dispose();
			_receiveThread?.Join(1000);
		}
	}

	// Демонстрация через UdpClient (высокоуровневый API)
	public class UdpClientDemo
	{
		public static void DemonstrateUdpClient()
		{
			Console.WriteLine("\n\n=== UDPCLIENT ДЕМОНСТРАЦИЯ ===\n");

			// 1. Базовое использование
			Console.WriteLine("1. БАЗОВОЕ ИСПОЛЬЗОВАНИЕ:");

			var serverPort = 9010;
			var clientPort = 9011;

			// Сервер (получатель)
			using (var server = new UdpClient(serverPort))
			{
				// Клиент (отправитель)
				using (var client = new UdpClient())
				{
					client.Client.Bind(new IPEndPoint(IPAddress.Loopback, clientPort));

					string message = "Привет через UdpClient!";
					byte[] data = Encoding.UTF8.GetBytes(message);

					// Отправка датаграммы
					Console.WriteLine($"   Отправка: '{message}'");
					int bytesSent = client.Send(data, data.Length, "127.0.0.1", serverPort);
					Console.WriteLine($"     Отправлено байт: {bytesSent}");

					// Прием датаграммы
					Console.WriteLine($"\n   Ожидание получения...");
					IPEndPoint remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);
					byte[] receivedData = server.Receive(ref remoteEndpoint);

					string receivedMessage = Encoding.UTF8.GetString(receivedData);
					Console.WriteLine($"   Получено: '{receivedMessage}'");
					Console.WriteLine($"     Отправитель: {remoteEndpoint.Address}:{remoteEndpoint.Port}");
				}
			}

			// 2. Асинхронные операции
			Console.WriteLine("\n2. АСИНХРОННЫЕ ОПЕРАЦИИ:");
			DemonstrateAsyncOperations().Wait();

			// 3. Множественные отправители
			Console.WriteLine("\n3. МНОЖЕСТВЕННЫЕ ОТПРАВИТЕЛИ:");
			DemonstrateMultipleSenders();

			// 4. Широковещательная рассылка
			Console.WriteLine("\n4. ШИРОКОВЕЩАТЕЛЬНАЯ РАССЫЛКА:");
			DemonstrateBroadcast();
		}

		private static async Task DemonstrateAsyncOperations()
		{
			var port = 9020;

			using (var server = new UdpClient(port))
			using (var client = new UdpClient())
			{
				// Асинхронная отправка
				string message = "Асинхронное сообщение";
				byte[] data = Encoding.UTF8.GetBytes(message);

				Console.WriteLine($"   Начало асинхронной отправки...");
				var sendTask = client.SendAsync(data, data.Length, "127.0.0.1", port);

				// Асинхронный прием
				Console.WriteLine($"   Ожидание асинхронного приема...");
				var receiveTask = server.ReceiveAsync();

				// Ожидаем обе операции
				await Task.WhenAll(sendTask, receiveTask);

				UdpReceiveResult result = receiveTask.Result;
				string receivedMessage = Encoding.UTF8.GetString(result.Buffer);

				Console.WriteLine($"   Получено асинхронно: '{receivedMessage}'");
				Console.WriteLine($"     Отправитель: {result.RemoteEndPoint.Address}:{result.RemoteEndPoint.Port}");
			}
		}

		private static void DemonstrateMultipleSenders()
		{
			var port = 9030;

			using (var server = new UdpClient(port))
			{
				Console.WriteLine($"   Сервер слушает порт {port}");
				Console.WriteLine($"   Ожидание сообщений от разных отправителей...");

				// Запускаем несколько отправителей
				var senders = new List<Task>();
				var senderCount = 3;

				for (int i = 0; i < senderCount; i++)
				{
					int senderId = i + 1;
					senders.Add(Task.Run(() =>
					{
						using (var client = new UdpClient())
						{
							string message = $"Сообщение от отправителя #{senderId}";
							byte[] data = Encoding.UTF8.GetBytes(message);

							client.Send(data, data.Length, "127.0.0.1", port);
							Console.WriteLine($"     Отправитель #{senderId} отправил: '{message}'");
						}
					}));
				}

				// Принимаем сообщения
				Task.Run(async () =>
				{
					for (int i = 0; i < senderCount; i++)
					{
						var result = await server.ReceiveAsync();
						string message = Encoding.UTF8.GetString(result.Buffer);

						Console.WriteLine($"   Сервер получил: '{message}'");
						Console.WriteLine($"     Отправитель: {result.RemoteEndPoint.Address}:{result.RemoteEndPoint.Port}");
					}
				}).Wait();

				Task.WaitAll(senders.ToArray());
			}
		}

		private static void DemonstrateBroadcast()
		{
			var broadcastPort = 9040;
			var broadcastAddress = IPAddress.Broadcast; // 255.255.255.255

			// Сервер для приема широковещательных сообщений
			using (var server = new UdpClient(broadcastPort))
			{
				server.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

				Console.WriteLine($"   Сервер слушает широковещательные сообщения на порту {broadcastPort}");

				// Клиент для отправки широковещательных сообщений
				using (var client = new UdpClient())
				{
					client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);

					string message = "Широковещательное сообщение!";
					byte[] data = Encoding.UTF8.GetBytes(message);

					Console.WriteLine($"   Отправка широковещательного сообщения...");
					client.Send(data, data.Length, broadcastAddress.ToString(), broadcastPort);

					// Прием на сервере
					IPEndPoint remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);
					byte[] receivedData = server.Receive(ref remoteEndpoint);

					string receivedMessage = Encoding.UTF8.GetString(receivedData);
					Console.WriteLine($"   Получено широковещательное: '{receivedMessage}'");
					Console.WriteLine($"     Отправитель: {remoteEndpoint.Address}:{remoteEndpoint.Port}");
				}
			}
		}
	}

	// Демонстрация практических сценариев
	public class PracticalUdpScenarios
	{
		public static void DemonstrateScenarios()
		{
			Console.WriteLine("\n\n=== ПРАКТИЧЕСКИЕ СЦЕНАРИИ UDP ===\n");

			// 1. Видеостриминг (симуляция)
			Console.WriteLine("1. ВИДЕОСТРИМИНГ (СИМУЛЯЦИЯ):");
			DemonstrateVideoStreaming();

			// 2. Игровая телеметрия
			Console.WriteLine("\n2. ИГРОВАЯ ТЕЛЕМЕТРИЯ:");
			DemonstrateGameTelemetry();

			// 3. Сервис обнаружения
			Console.WriteLine("\n3. СЕРВИС ОБНАРУЖЕНИЯ:");
			DemonstrateDiscoveryService();

			// 4. Надежный UDP поверх ненадежного
			Console.WriteLine("\n4. НАДЕЖНЫЙ UDP ПОВЕРХ UDP:");
			DemonstrateReliableUdp();
		}

		private static void DemonstrateVideoStreaming()
		{
			// Симуляция видеопотока
			var frameCount = 10;
			var frameLossProbability = 0.2; // 20% вероятность потери кадра

			Console.WriteLine($"   Симуляция видеопотока ({frameCount} кадров):");
			Console.WriteLine($"   Вероятность потери кадра: {frameLossProbability:P0}");

			int framesSent = 0;
			int framesLost = 0;
			int framesReceived = 0;

			var random = new Random();

			for (int frame = 1; frame <= frameCount; frame++)
			{
				framesSent++;

				// Симуляция потери кадра
				if (random.NextDouble() < frameLossProbability)
				{
					Console.WriteLine($"     Кадр #{frame}: ПОТЕРЯН");
					framesLost++;
					continue;
				}

				framesReceived++;

				// Симуляция задержки
				int delay = random.Next(10, 50);
				Console.WriteLine($"     Кадр #{frame}: доставлен (задержка: {delay}мс)");
				Thread.Sleep(delay);
			}

			Console.WriteLine($"\n   Итоги:");
			Console.WriteLine($"     Отправлено кадров: {framesSent}");
			Console.WriteLine($"     Получено кадров: {framesReceived}");
			Console.WriteLine($"     Потеряно кадров: {framesLost}");
			Console.WriteLine($"     Потери: {(double)framesLost / framesSent:P0}");

			Console.WriteLine($"   Вывод: Для видео потеря части кадров приемлема, важна низкая задержка");
		}

		private static void DemonstrateGameTelemetry()
		{
			// Симуляция игровой телеметрии
			Console.WriteLine($"   Симуляция игровой телеметрии:");

			var playerStates = new[]
			{
				"Игрок1: X=100, Y=200, HP=100",
				"Игрок1: X=102, Y=203, HP=99",
				"Игрок1: X=105, Y=207, HP=95",
				"Игрок1: X=110, Y=212, HP=90",
				"Игрок1: X=115, Y=218, HP=85"
			};

			Console.WriteLine($"   Отправка состояний игрока:");

			var random = new Random();
			int outOfOrder = 0;
			int duplicates = 0;

			for (int i = 0; i < playerStates.Length; i++)
			{
				// Симуляция изменения порядка
				if (i == 2 && random.NextDouble() < 0.3)
				{
					Console.WriteLine($"     Состояние #{i + 1} пришло ПОСЛЕ #{i + 2}");
					outOfOrder++;
				}

				// Симуляция дублирования
				if (i == 3 && random.NextDouble() < 0.2)
				{
					Console.WriteLine($"     Состояние #{i + 1} ДУБЛИРОВАНО");
					duplicates++;
				}

				Console.WriteLine($"     Отправлено состояние #{i + 1}: {playerStates[i]}");
				Thread.Sleep(30);
			}

			Console.WriteLine($"\n   Вывод: Для игр важно последнее состояние, порядок можно корректировать");
		}

		private static void DemonstrateDiscoveryService()
		{
			// Симуляция сервиса обнаружения в локальной сети
			Console.WriteLine($"   Сервис обнаружения устройств:");

			var discoveryPort = 9050;
			var devices = new[]
			{
				("Принтер", "192.168.1.100"),
				("Медиасервер", "192.168.1.101"),
				("Умная колонка", "192.168.1.102"),
				("Камера", "192.168.1.103")
			};

			// Симуляция широковещательного запроса
			Console.WriteLine($"   Отправка широковещательного запроса обнаружения...");

			// Симуляция ответов устройств
			Console.WriteLine($"   Ответы устройств:");

			var random = new Random();
			int responsesReceived = 0;

			foreach (var (deviceName, ipAddress) in devices)
			{
				// Не все устройства могут ответить
				if (random.NextDouble() < 0.8) // 80% вероятность ответа
				{
					int delay = random.Next(50, 300);
					Console.WriteLine($"     {deviceName} ({ipAddress}) ответил через {delay}мс");
					responsesReceived++;
					Thread.Sleep(delay);
				}
				else
				{
					Console.WriteLine($"     {deviceName} ({ipAddress}) не ответил");
				}
			}

			Console.WriteLine($"\n   Обнаружено устройств: {responsesReceived}/{devices.Length}");
			Console.WriteLine($"   Вывод: UDP идеален для обнаружения - не нужны постоянные соединения");
		}

		private static void DemonstrateReliableUdp()
		{
			// Демонстрация реализации надежности поверх UDP
			Console.WriteLine($"   Реализация надежного UDP:");

			Console.WriteLine($"\n   Компоненты надежного UDP:");
			Console.WriteLine($"     1. Нумерация пакетов");
			Console.WriteLine($"     2. Подтверждения доставки (ACK)");
			Console.WriteLine($"     3. Повторные отправки (Retransmission)");
			Console.WriteLine($"     4. Контроль потока");

			Console.WriteLine($"\n   Пример структуры пакета:");
			Console.WriteLine($"     +----------------------+");
			Console.WriteLine($"     | Sequence Number (4б) |");
			Console.WriteLine($"     +----------------------+");
			Console.WriteLine($"     | ACK Number (4б)      |");
			Console.WriteLine($"     +----------------------+");
			Console.WriteLine($"     | Flags (1б)           |");
			Console.WriteLine($"     +----------------------+");
			Console.WriteLine($"     | Данные (до 1400б)    |");
			Console.WriteLine($"     +----------------------+");

			Console.WriteLine($"\n   Процесс обмена:");
			Console.WriteLine($"     Клиент -> Сервер: Пакет #1");
			Console.WriteLine($"     Сервер -> Клиент: ACK #1");
			Console.WriteLine($"     [Таймаут 1с, нет ACK]");
			Console.WriteLine($"     Клиент -> Сервер: Пакет #1 (повторно)");
			Console.WriteLine($"     Сервер -> Клиент: ACK #1");
			Console.WriteLine($"     Клиент -> Сервер: Пакет #2");

			Console.WriteLine($"\n   Вывод: Можно реализовать надежность поверх UDP при необходимости");
		}
	}

	// Главная программа
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("РАБОТА С UDP В C#");
			Console.WriteLine("=================\n");

			// Демонстрация низкоуровневых UDP сокетов
			using (var udpDemo = new UdpRawSocketDemo())
			{
				udpDemo.DemonstrateUdpSocket();
			}

			// Демонстрация UdpClient
			UdpClientDemo.DemonstrateUdpClient();

			// Практические сценарии
			PracticalUdpScenarios.DemonstrateScenarios();

			Console.WriteLine("\n\n=== ИТОГИ UDP ===");
			Console.WriteLine("✓ Не требует установки соединения");
			Console.WriteLine("✓ Минимальные накладные расходы");
			Console.WriteLine("✓ Низкая задержка");
			Console.WriteLine("✗ Нет гарантий доставки");
			Console.WriteLine("✗ Нет гарантий порядка");
			Console.WriteLine("✗ Нет контроля перегрузки");
			Console.WriteLine("\nИдеален для: видео, голоса, игр, телеметрии, обнаружения");
		}
	}
}