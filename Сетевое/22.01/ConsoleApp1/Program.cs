using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;

namespace SocketDotNetImplementation
{
	// Демонстрация базовых классов System.Net
	public class SystemNetDemonstration
	{
		public static void DemonstrateAddressing()
		{
			Console.WriteLine("=== SYSTEM.NET: АДРЕСАЦИЯ И КОНЕЧНЫЕ ТОЧКИ ===\n");

			// 1. Класс IPAddress - строго типизированный сетевой адрес
			Console.WriteLine("1. КЛАСС IPAddress:");

			// Различные способы создания IP-адресов
			IPAddress loopback = IPAddress.Loopback;           // 127.0.0.1
			IPAddress any = IPAddress.Any;                    // 0.0.0.0
			IPAddress broadcast = IPAddress.Broadcast;        // 255.255.255.255
			IPAddress ipv6Loopback = IPAddress.IPv6Loopback; // ::1

			Console.WriteLine($"   IPAddress.Loopback: {loopback}");
			Console.WriteLine($"   IPAddress.Any: {any}");
			Console.WriteLine($"   IPAddress.Broadcast: {broadcast}");
			Console.WriteLine($"   IPAddress.IPv6Loopback: {ipv6Loopback}");

			// Создание из строки с валидацией
			IPAddress parsed = IPAddress.Parse("192.168.1.1");
			Console.WriteLine($"   IPAddress.Parse('192.168.1.1'): {parsed}");

			// Проверка на корректность перед парсингом
			bool isValid = IPAddress.TryParse("256.300.1.1", out IPAddress invalid);
			Console.WriteLine($"   TryParse('256.300.1.1'): {isValid}, результат: {invalid}");

			// Свойства адреса
			Console.WriteLine($"\n   Свойства IPAddress {parsed}:");
			Console.WriteLine($"     AddressFamily: {parsed.AddressFamily}");
			Console.WriteLine($"     IsIPv4MappedToIPv6: {parsed.IsIPv4MappedToIPv6}");
			Console.WriteLine($"     IsIPv6LinkLocal: {parsed.IsIPv6LinkLocal}");
			Console.WriteLine($"     IsIPv6SiteLocal: {parsed.IsIPv6SiteLocal}");
			Console.WriteLine($"     IsIPv6Multicast: {parsed.IsIPv6Multicast}");

			// 2. Класс IPEndPoint - связка адреса и порта
			Console.WriteLine("\n2. КЛАСС IPEndPoint:");

			// Создание конечных точек
			var endpoint1 = new IPEndPoint(loopback, 8080);
			var endpoint2 = new IPEndPoint(IPAddress.Parse("10.0.0.1"), 80);
			var endpoint3 = new IPEndPoint(IPAddress.IPv6Any, 443);

			Console.WriteLine($"   IPEndPoint(127.0.0.1, 8080): {endpoint1}");
			Console.WriteLine($"   IPEndPoint(10.0.0.1, 80): {endpoint2}");
			Console.WriteLine($"   IPEndPoint(IPv6Any, 443): {endpoint3}");

			// Свойства конечной точки
			Console.WriteLine($"\n   Свойства {endpoint1}:");
			Console.WriteLine($"     Address: {endpoint1.Address}");
			Console.WriteLine($"     Port: {endpoint1.Port}");
			Console.WriteLine($"     AddressFamily: {endpoint1.AddressFamily}");

			// Сериализация/десериализация конечных точек
			Console.WriteLine($"\n   Сериализация конечных точек:");

			// Сериализация IPEndPoint в SocketAddress
			SocketAddress socketAddress = endpoint1.Serialize();
			Console.WriteLine($"     Serialize(): Семейство адресов: {socketAddress.Family}");
			Console.WriteLine($"     Размер буфера: {socketAddress.Size}");

			// Получение байтов из Memory<byte>
			var bufferSpan = socketAddress.Buffer.Span;
			Console.Write($"     Байты (первые {Math.Min(16, socketAddress.Size)}): ");
			for (int i = 0; i < Math.Min(16, socketAddress.Size); i++)
			{
				Console.Write($"{bufferSpan[i]:X2} ");
				if (i % 8 == 7) Console.Write(" ");
			}
			Console.WriteLine();

			// Альтернативный способ с ToArray()
			Console.Write($"     Байты (через ToArray()): ");
			byte[] bufferArray = socketAddress.Buffer.ToArray();
			Console.WriteLine(BitConverter.ToString(bufferArray, 0, Math.Min(16, bufferArray.Length)));

			// Восстановление IPEndPoint из SocketAddress
			var restoredEndpoint = (IPEndPoint)endpoint1.Create(socketAddress);
			Console.WriteLine($"     Восстановленный: {restoredEndpoint}");

			// Демонстрация ручного создания SocketAddress
			Console.WriteLine($"\n   Ручное создание SocketAddress:");
			var manualSocketAddress = new SocketAddress(AddressFamily.InterNetwork, 16);
			Console.WriteLine($"     Размер: {manualSocketAddress.Size}");
			Console.WriteLine($"     Семейство: {manualSocketAddress.Family}");

			// Получение отдельных байтов из SocketAddress
			Console.WriteLine($"\n   Анализ SocketAddress:");
			Console.WriteLine($"     Байт 0 (семейство): {socketAddress[0]}");
			Console.WriteLine($"     Байт 1 (порт MSB): {socketAddress[1]}");
			Console.WriteLine($"     Байт 2 (порт LSB): {socketAddress[2]}");

			// IPv6 пример
			Console.WriteLine($"\n   SocketAddress для IPv6:");
			var ipv6Endpoint = new IPEndPoint(IPAddress.IPv6Loopback, 8080);
			SocketAddress ipv6SocketAddress = ipv6Endpoint.Serialize();
			Console.WriteLine($"     Размер IPv6 SocketAddress: {ipv6SocketAddress.Size}");
			Console.WriteLine($"     Семейство: {ipv6SocketAddress.Family}");

			// Сравнение SocketAddress
			Console.WriteLine($"\n   Сравнение SocketAddress:");
			var socketAddress1 = endpoint1.Serialize();
			var socketAddress2 = endpoint1.Serialize();
			Console.WriteLine($"     Equals: {socketAddress1.Equals(socketAddress2)}");
			Console.WriteLine($"     GetHashCode одинаковые: {socketAddress1.GetHashCode() == socketAddress2.GetHashCode()}");

			// 3. DNS-разрешение - часть System.Net
			Console.WriteLine("\n3. КЛАСС Dns:");

			try
			{
				var hostEntry = Dns.GetHostEntry("localhost");
				Console.WriteLine($"   Dns.GetHostEntry('localhost'):");
				Console.WriteLine($"     HostName: {hostEntry.HostName}");

				Console.WriteLine($"     Addresses:");
				foreach (var addr in hostEntry.AddressList)
				{
					Console.WriteLine($"       - {addr} ({addr.AddressFamily})");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"   Ошибка DNS: {ex.Message}");
			}
		}
	}

	// Демонстрация классов System.Net.Sockets
	public class SystemNetSocketsDemonstration : IDisposable
	{
		private Socket _serverSocket;
		private Socket _clientSocket;
		private Thread _serverThread;
		private bool _isRunning;
		private readonly int _port;

		public SystemNetSocketsDemonstration(int port = 11010)
		{
			_port = port;
		}

		public void DemonstrateSocketClass()
		{
			Console.WriteLine("\n=== SYSTEM.NET.SOCKETS: КЛАСС Socket ===\n");

			// 1. Создание сокета - инициализация системного ресурса
			Console.WriteLine("1. СОЗДАНИЕ СОКЕТА:");

			// Параметры конструктора определяют тип сокета
			// AddressFamily.InterNetwork - IPv4
			// SocketType.Stream - TCP сокет
			// ProtocolType.Tcp - протокол TCP
			var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			Console.WriteLine($"   Socket(InterNetwork, Stream, Tcp) создан:");
			Console.WriteLine($"     Дескриптор: {socket.Handle}");
			Console.WriteLine($"     AddressFamily: {socket.AddressFamily}");
			Console.WriteLine($"     SocketType: {socket.SocketType}");
			Console.WriteLine($"     ProtocolType: {socket.ProtocolType}");
			Console.WriteLine($"     Blocking: {socket.Blocking}");
			Console.WriteLine($"     Connected: {socket.Connected}");

			// 2. Конфигурация сокета - передача параметров ОС
			Console.WriteLine("\n2. КОНФИГУРАЦИЯ СОКЕТА:");

			// Настройка параметров через свойства
			socket.LingerState = new LingerOption(true, 3); // Задержка при закрытии
			socket.NoDelay = true;                          // Отключение алгоритма Нагля
			socket.ReceiveBufferSize = 8192;                // Размер буфера приёма
			socket.SendBufferSize = 8192;                   // Размер буфера отправки
			socket.ReceiveTimeout = 5000;                   // Таймаут приёма (мс)
			socket.SendTimeout = 5000;                      // Таймаут отправки (мс)
			socket.Ttl = 64;                                // Time To Live для пакетов

			Console.WriteLine($"   Параметры установлены:");
			Console.WriteLine($"     LingerState: Enabled={socket.LingerState.Enabled}, LingerTime={socket.LingerState.LingerTime}s");
			Console.WriteLine($"     NoDelay: {socket.NoDelay}");
			Console.WriteLine($"     ReceiveBufferSize: {socket.ReceiveBufferSize}");
			Console.WriteLine($"     SendBufferSize: {socket.SendBufferSize}");
			Console.WriteLine($"     ReceiveTimeout: {socket.ReceiveTimeout}");
			Console.WriteLine($"     SendTimeout: {socket.SendTimeout}");
			Console.WriteLine($"     Ttl: {socket.Ttl}");

			// 3. Вспомогательные типы и перечисления
			Console.WriteLine("\n3. ВСПОМОГАТЕЛЬНЫЕ ТИПЫ И ПЕРЕЧИСЛЕНИЯ:");

			Console.WriteLine("   AddressFamily:");
			Console.WriteLine($"     InterNetwork: {AddressFamily.InterNetwork} - IPv4");
			Console.WriteLine($"     InterNetworkV6: {AddressFamily.InterNetworkV6} - IPv6");
			Console.WriteLine($"     Unix: {AddressFamily.Unix} - UNIX domain sockets");

			Console.WriteLine("\n   SocketType:");
			Console.WriteLine($"     Stream: {SocketType.Stream} - TCP");
			Console.WriteLine($"     Dgram: {SocketType.Dgram} - UDP");
			Console.WriteLine($"     Raw: {SocketType.Raw} - RAW сокеты");

			Console.WriteLine("\n   ProtocolType:");
			Console.WriteLine($"     Tcp: {ProtocolType.Tcp}");
			Console.WriteLine($"     Udp: {ProtocolType.Udp}");
			Console.WriteLine($"     Raw: {ProtocolType.Raw}");

			// 4. Состояния сокета
			Console.WriteLine("\n4. СОСТОЯНИЯ СОКЕТА:");

			Console.WriteLine($"   До Bind: IsBound={socket.IsBound}, Connected={socket.Connected}");

			// Привязка к локальному адресу
			socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
			Console.WriteLine($"   После Bind: IsBound={socket.IsBound}, Connected={socket.Connected}");

			Console.WriteLine($"   До Listen: IsBound={socket.IsBound}");
			socket.Listen(10);
			Console.WriteLine($"   После Listen: IsBound={socket.IsBound}");

			// Освобождение ресурса
			socket.Close();
			Console.WriteLine($"\n   Ресурс сокета освобождён");
		}

		public void DemonstrateThreadSafety()
		{
			Console.WriteLine("\n5. ПУТЕБОЗОПАСНОСТЬ:");

			var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			// Демонстрация работы из нескольких потоков
			Console.WriteLine("   Тест работы из нескольких потоков:");

			var tasks = new List<Task>();
			var exceptions = new List<Exception>();
			var counter = 0;

			for (int i = 0; i < 3; i++)
			{
				tasks.Add(Task.Run(() =>
				{
					try
					{
						// Каждый поток пытается настроить сокет
						// Это вызовет исключение, т.к. сокет не потокобезопасен
						socket.ReceiveTimeout = 1000 * (Thread.CurrentThread.ManagedThreadId % 5);
						Interlocked.Increment(ref counter);
					}
					catch (Exception ex)
					{
						lock (exceptions)
							exceptions.Add(ex);
					}
				}));
			}

			Task.WaitAll(tasks.ToArray());

			Console.WriteLine($"   Успешных операций: {counter}/3");
			Console.WriteLine($"   Исключений: {exceptions.Count}");
			if (exceptions.Count > 0)
			{
				Console.WriteLine($"   Пример исключения: {exceptions[0].GetType().Name}");
			}

			socket.Dispose();
		}

		public void DemonstrateClientServerInteraction()
		{
			Console.WriteLine("\n6. КЛИЕНТ-СЕРВЕРНОЕ ВЗАИМОДЕЙСТВИЕ:");

			_isRunning = true;

			// Запуск сервера
			_serverThread = new Thread(RunServer);
			_serverThread.IsBackground = true;
			_serverThread.Start();

			Thread.Sleep(1000); // Даём время серверу запуститься

			// Клиентское подключение
			try
			{
				_clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

				Console.WriteLine("   Клиент подключается...");
				_clientSocket.Connect(new IPEndPoint(IPAddress.Loopback, _port));
				Console.WriteLine($"   Подключение установлено: {_clientSocket.Connected}");
				Console.WriteLine($"   Локальная точка: {_clientSocket.LocalEndPoint}");
				Console.WriteLine($"   Удалённая точка: {_clientSocket.RemoteEndPoint}");

				// Отправка данных
				string message = "Hello from client!";
				byte[] sendBuffer = Encoding.UTF8.GetBytes(message);

				Console.WriteLine($"\n   Отправка сообщения: '{message}'");
				int bytesSent = _clientSocket.Send(sendBuffer);
				Console.WriteLine($"   Отправлено байт: {bytesSent}");

				// Получение ответа
				byte[] receiveBuffer = new byte[1024];
				int bytesReceived = _clientSocket.Receive(receiveBuffer);

				string response = Encoding.UTF8.GetString(receiveBuffer, 0, bytesReceived);
				Console.WriteLine($"   Получен ответ: '{response}'");
				Console.WriteLine($"   Получено байт: {bytesReceived}");

				// Проверка состояния
				Console.WriteLine($"\n   Состояние после обмена:");
				Console.WriteLine($"     Connected: {_clientSocket.Connected}");
				Console.WriteLine($"     Available: {_clientSocket.Available}");

				// Корректное закрытие
				Console.WriteLine("\n   Корректное закрытие соединения...");
				_clientSocket.Shutdown(SocketShutdown.Both);
				_clientSocket.Close();
				Console.WriteLine($"   Соединение закрыто: {!_clientSocket.Connected}");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"   Ошибка: {ex.Message}");
			}

			_isRunning = false;
			Thread.Sleep(1000);
		}

		private void RunServer()
		{
			try
			{
				_serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				_serverSocket.Bind(new IPEndPoint(IPAddress.Loopback, _port));
				_serverSocket.Listen(1);

				Console.WriteLine($"   Сервер запущен на порту {_port}");

				while (_isRunning)
				{
					Socket client = _serverSocket.Accept();
					Console.WriteLine($"   Сервер принял подключение от {client.RemoteEndPoint}");

					// Обработка клиента
					byte[] buffer = new byte[1024];
					int bytesRead = client.Receive(buffer);

					string received = Encoding.UTF8.GetString(buffer, 0, bytesRead);
					Console.WriteLine($"   Сервер получил: '{received}'");

					// Эхо-ответ
					string response = $"Echo: {received}";
					byte[] responseBytes = Encoding.UTF8.GetBytes(response);
					client.Send(responseBytes);

					client.Shutdown(SocketShutdown.Both);
					client.Close();

					break; // Для демонстрации обрабатываем только одно подключение
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"   Ошибка сервера: {ex.Message}");
			}
		}

		public void DemonstrateWrapperClasses()
		{
			Console.WriteLine("\n7. КЛАССЫ-ОБЁРТКИ:");

			// 1. TcpClient/TcpListener - высокоуровневые обёртки
			Console.WriteLine("\n   TcpClient/TcpListener:");

			var listener = new TcpListener(IPAddress.Loopback, _port + 1);
			listener.Start();

			Console.WriteLine($"   TcpListener запущен на порту {_port + 1}");

			// Асинхронное принятие подключения
			var acceptTask = listener.AcceptTcpClientAsync();

			// Клиентское подключение
			var client = new TcpClient();
			client.Connect(IPAddress.Loopback, _port + 1);
			Console.WriteLine($"   TcpClient подключён");

			var serverClient = acceptTask.Result;
			Console.WriteLine($"   Сервер принял TcpClient");

			// Обмен данными через NetworkStream
			var message = "Test через TcpClient";
			var data = Encoding.UTF8.GetBytes(message);

			client.GetStream().Write(data, 0, data.Length);
			Console.WriteLine($"   Клиент отправил: '{message}'");

			byte[] buffer = new byte[1024];
			int bytesRead = serverClient.GetStream().Read(buffer, 0, buffer.Length);
			string received = Encoding.UTF8.GetString(buffer, 0, bytesRead);
			Console.WriteLine($"   Сервер получил: '{received}'");

			// Освобождение ресурсов
			client.Close();
			serverClient.Close();
			listener.Stop();

			// 2. UdpClient - обёртка для UDP сокетов
			Console.WriteLine("\n   UdpClient:");

			var udpServer = new UdpClient(_port + 2);
			var udpClient = new UdpClient();

			udpClient.Connect(IPAddress.Loopback, _port + 2);

			var udpMessage = "UDP сообщение";
			var udpData = Encoding.UTF8.GetBytes(udpMessage);

			udpClient.Send(udpData, udpData.Length);
			Console.WriteLine($"   UDP клиент отправил: '{udpMessage}'");

			IPEndPoint remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);
			byte[] udpReceived = udpServer.Receive(ref remoteEndpoint);
			string udpReceivedMessage = Encoding.UTF8.GetString(udpReceived);
			Console.WriteLine($"   UDP сервер получил: '{udpReceivedMessage}' от {remoteEndpoint}");

			udpClient.Close();
			udpServer.Close();
		}

		public void DemonstrateResourceManagement()
		{
			Console.WriteLine("\n8. УПРАВЛЕНИЕ РЕСУРСАМИ:");

			Console.WriteLine("   Тест утечки ресурсов:");

			// Создаём сокет без освобождения
			var socket1 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			Console.WriteLine($"   Создан сокет #{socket1.Handle}");

			// Используем using для автоматического освобождения
			using (var socket2 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
			{
				Console.WriteLine($"   Создан сокет #{socket2.Handle} в using");
			}

			// Явный вызов Dispose
			var socket3 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			Console.WriteLine($"   Создан сокет #{socket3.Handle}");
			socket3.Dispose();

			Console.WriteLine($"   Сокет #{socket3.Handle} освобождён через Dispose");

			// Проверка состояния после Dispose
			try
			{
				socket3.Bind(new IPEndPoint(IPAddress.Loopback, 0));
				Console.WriteLine($"   ОШИБКА: Операция после Dispose не вызвала исключение");
			}
			catch (ObjectDisposedException)
			{
				Console.WriteLine($"   Корректно: ObjectDisposedException после Dispose");
			}

			// Забытый сокет
			Console.WriteLine($"\n   ВНИМАНИЕ: Сокет #{socket1.Handle} не освобождён!");
			// socket1 должен быть освобождён в Dispose класса

			// Демонстрация финализатора
			var weakRef = new WeakReference<Socket>(socket1);
			socket1 = null; // Убираем сильную ссылку

			GC.Collect();
			GC.WaitForPendingFinalizers();

			if (weakRef.TryGetTarget(out Socket releasedSocket))
			{
				Console.WriteLine($"   Сокет всё ещё в памяти");
			}
			else
			{
				Console.WriteLine($"   Сокет был собран сборщиком мусора");
			}
		}

		public void Dispose()
		{
			_isRunning = false;

			_clientSocket?.Dispose();
			_serverSocket?.Dispose();

			_serverThread?.Join(1000);

			GC.SuppressFinalize(this);
		}
	}

	// Архитектурное разделение слоёв
	public class ArchitectureLayers
	{
		// Слой System.Net - описание и идентификация
		public class NetworkAddressingLayer
		{
			public IPEndPoint ParseEndpoint(string address, int port)
			{
				// Валидация и создание конечной точки
				if (!IPAddress.TryParse(address, out IPAddress ipAddress))
					throw new ArgumentException($"Некорректный IP-адрес: {address}");

				return new IPEndPoint(ipAddress, port);
			}

			public IPAddress[] ResolveHost(string hostname)
			{
				// DNS-разрешение
				return Dns.GetHostAddresses(hostname);
			}
		}

		// Слой System.Net.Sockets - инфраструктура
		public class SocketInfrastructureLayer : IDisposable
		{
			private Socket _socket;
			private readonly IPEndPoint _endpoint;

			public SocketInfrastructureLayer(IPEndPoint endpoint)
			{
				_endpoint = endpoint;
				_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			}

			public void Configure(SocketConfiguration config)
			{
				// Конфигурация системного ресурса
				_socket.ReceiveTimeout = config.ReceiveTimeout;
				_socket.SendTimeout = config.SendTimeout;
				_socket.ReceiveBufferSize = config.BufferSize;
				_socket.SendBufferSize = config.BufferSize;
				_socket.NoDelay = config.NoDelay;
			}

			public void Bind()
			{
				_socket.Bind(_endpoint);
			}

			public void Listen(int backlog)
			{
				_socket.Listen(backlog);
			}

			public Socket Accept()
			{
				return _socket.Accept();
			}

			public void Connect()
			{
				_socket.Connect(_endpoint);
			}

			public void Send(byte[] data)
			{
				_socket.Send(data);
			}

			public byte[] Receive(int bufferSize)
			{
				var buffer = new byte[bufferSize];
				int received = _socket.Receive(buffer);

				var result = new byte[received];
				Array.Copy(buffer, result, received);
				return result;
			}

			public void Dispose()
			{
				_socket?.Dispose();
			}
		}

		// Слой бизнес-логики - использует инфраструктуру
		public class BusinessLogicLayer
		{
			private readonly SocketInfrastructureLayer _infrastructure;

			public BusinessLogicLayer(SocketInfrastructureLayer infrastructure)
			{
				_infrastructure = infrastructure;
			}

			public string ProcessRequest(string request)
			{
				// Бизнес-логика не зависит от деталей реализации сокетов
				return $"Обработано: {request}";
			}
		}

		public record SocketConfiguration
		{
			public int ReceiveTimeout { get; init; } = 30000;
			public int SendTimeout { get; init; } = 30000;
			public int BufferSize { get; init; } = 8192;
			public bool NoDelay { get; init; } = true;
		}

		public static void DemonstrateLayerSeparation()
		{
			Console.WriteLine("\n=== АРХИТЕКТУРНОЕ РАЗДЕЛЕНИЕ СЛОЁВ ===\n");

			// 1. Слой описания (System.Net)
			var addressing = new NetworkAddressingLayer();
			var endpoint = addressing.ParseEndpoint("127.0.0.1", 11011);
			Console.WriteLine($"1. Слой описания создал конечную точку: {endpoint}");

			// 2. Слой инфраструктуры (System.Net.Sockets)
			var infrastructure = new SocketInfrastructureLayer(endpoint);
			var config = new SocketConfiguration
			{
				ReceiveTimeout = 5000,
				SendTimeout = 5000,
				BufferSize = 4096
			};

			infrastructure.Configure(config);
			Console.WriteLine($"2. Слой инфраструктуры настроил сокет");

			// 3. Слой бизнес-логики
			var businessLogic = new BusinessLogicLayer(infrastructure);
			var result = businessLogic.ProcessRequest("тестовый запрос");
			Console.WriteLine($"3. Слой бизнес-логики обработал запрос: {result}");

			// Освобождение ресурсов
			infrastructure.Dispose();
			Console.WriteLine($"4. Ресурсы корректно освобождены");
		}
	}

	// Главная программа
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("РЕАЛИЗАЦИЯ СОКЕТОВ В .NET");
			Console.WriteLine("========================\n");

			// Демонстрация классов System.Net
			SystemNetDemonstration.DemonstrateAddressing();

			// Демонстрация классов System.Net.Sockets
			using (var socketsDemo = new SystemNetSocketsDemonstration())
			{
				socketsDemo.DemonstrateSocketClass();
				socketsDemo.DemonstrateThreadSafety();
				socketsDemo.DemonstrateClientServerInteraction();
				socketsDemo.DemonstrateWrapperClasses();
				socketsDemo.DemonstrateResourceManagement();
			}

			// Демонстрация архитектурного разделения
			ArchitectureLayers.DemonstrateLayerSeparation();
		}
	}
}