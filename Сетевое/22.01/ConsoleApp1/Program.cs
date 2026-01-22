using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace LowLevelSocketDemo
{
	// Простой TCP сокет-сервер с минимальной абстракцией
	public class RawSocketServer : IDisposable
	{
		private Socket _listener;
		private Thread _acceptThread;
		private bool _isRunning;
		private readonly ConcurrentBag<Socket> _activeConnections = new ConcurrentBag<Socket>();
		private readonly object _lock = new object();

		// Дескриптор сокета - низкоуровневый идентификатор
		public IntPtr Handle => _listener?.Handle ?? IntPtr.Zero;

		public RawSocketServer(IPAddress ipAddress, int port)
		{
			// Создание сокета - запрос ресурса у ОС
			_listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			// Настройка сокета - передача параметров ОС
			_listener.LingerState = new LingerOption(true, 0);
			_listener.NoDelay = true;

			// Привязка к конечной точке - регистрация в ОС
			var endpoint = new IPEndPoint(ipAddress, port);
			_listener.Bind(endpoint);

			// Установка в режим прослушивания - изменение состояния сокета в ОС
			_listener.Listen(10);

			Console.WriteLine($"[Server] Сокет создан. Дескриптор: {Handle.ToInt32()}");
			Console.WriteLine($"[Server] Привязан к {endpoint}");
			Console.WriteLine($"[Server] Состояние: {_listener.IsBound}");
		}

		public void Start()
		{
			if (_isRunning) return;

			_isRunning = true;
			_acceptThread = new Thread(AcceptConnections);
			_acceptThread.IsBackground = true;
			_acceptThread.Start();

			Console.WriteLine($"[Server] Сервер запущен. Поток приёма: {_acceptThread.ManagedThreadId}");
		}

		private void AcceptConnections()
		{
			Console.WriteLine($"[Server] Начало приёма подключений...");

			while (_isRunning)
			{
				try
				{
					// БЛОКИРУЮЩИЙ ВЫЗОВ: ОС приостанавливает поток до подключения
					Socket clientSocket = _listener.Accept();

					Console.WriteLine($"[Server] Принято подключение. Дескриптор клиента: {clientSocket.Handle.ToInt32()}");
					Console.WriteLine($"[Server] Клиент: {clientSocket.RemoteEndPoint}");
					Console.WriteLine($"[Server] Состояние клиентского сокета: Connected={clientSocket.Connected}, Blocking={clientSocket.Blocking}");

					_activeConnections.Add(clientSocket);

					// Запускаем обработку в отдельном потоке
					ThreadPool.QueueUserWorkItem(HandleClient, clientSocket);
				}
				catch (SocketException ex) when (ex.SocketErrorCode == SocketError.Interrupted)
				{
					Console.WriteLine($"[Server] Операция прервана");
					break;
				}
				catch (Exception ex)
				{
					Console.WriteLine($"[Server] Ошибка при принятии подключения: {ex.Message}");
					Thread.Sleep(1000);
				}
			}

			Console.WriteLine($"[Server] Поток приёма подключений завершён");
		}

		private void HandleClient(object state)
		{
			Socket clientSocket = (Socket)state;
			byte[] buffer = new byte[1024]; // Буфер для байтов

			Console.WriteLine($"[Client {clientSocket.Handle.ToInt32()}] Начало обработки");

			try
			{
				while (clientSocket.Connected && _isRunning)
				{
					// БЛОКИРУЮЩИЙ ВЫЗОВ: ОС ждёт данные от клиента
					// Сокет не знает о "сообщениях", только о потоке байтов
					int bytesRead = clientSocket.Receive(buffer, 0, buffer.Length, SocketFlags.None);

					if (bytesRead == 0)
					{
						Console.WriteLine($"[Client {clientSocket.Handle.ToInt32()}] Клиент отключился");
						break;
					}

					// НИЗКОУРОВНЕВЫЙ ДОСТУП: работа с сырыми байтами
					Console.WriteLine($"[Client {clientSocket.Handle.ToInt32()}] Получено байт: {bytesRead}");

					// Сокет доставил байты, но не знает об их содержимом
					// Декодирование и логика - ответственность приложения
					string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
					Console.WriteLine($"[Client {clientSocket.Handle.ToInt32()}] Данные (как текст): {receivedData}");

					// Эхо-ответ - просто отправляем байты обратно
					byte[] response = Encoding.UTF8.GetBytes($"Эхо: {receivedData}");
					int bytesSent = clientSocket.Send(response);

					Console.WriteLine($"[Client {clientSocket.Handle.ToInt32()}] Отправлено байт: {bytesSent}");
					Console.WriteLine($"[Client {clientSocket.Handle.ToInt32()}] Доступно байт в буфере: {clientSocket.Available}");
				}
			}
			catch (SocketException ex)
			{
				Console.WriteLine($"[Client {clientSocket.Handle.ToInt32()}] Ошибка сокета: {ex.SocketErrorCode} - {ex.Message}");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[Client {clientSocket.Handle.ToInt32()}] Ошибка: {ex.Message}");
			}
			finally
			{
				// ОБЯЗАТЕЛЬНОЕ освобождение ресурса ОС
				SafeCloseSocket(clientSocket);
				_activeConnections.TryTake(out _);

				Console.WriteLine($"[Client {clientSocket.Handle.ToInt32()}] Обработка завершена");
			}
		}

		private void SafeCloseSocket(Socket socket)
		{
			if (socket == null) return;

			try
			{
				// Изменение состояния сокета перед закрытием
				socket.Shutdown(SocketShutdown.Both);

				// Закрытие сокета - освобождение ресурса в ОС
				socket.Close();

				// Dispose для освобождения управляемых ресурсов .NET
				socket.Dispose();

				Console.WriteLine($"[Socket {socket.Handle.ToInt32()}] Ресурс освобождён");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[Socket] Ошибка при закрытии сокета: {ex.Message}");
			}
		}

		public void Stop()
		{
			Console.WriteLine($"[Server] Остановка сервера...");

			_isRunning = false;

			// Прерывание блокирующего вызова Accept()
			if (_listener != null)
			{
				_listener.Close();
			}

			// Закрытие всех активных подключений
			Console.WriteLine($"[Server] Закрытие {_activeConnections.Count} активных подключений");
			foreach (var socket in _activeConnections)
			{
				SafeCloseSocket(socket);
			}

			_acceptThread?.Join(5000);

			Console.WriteLine($"[Server] Сервер остановлен");
		}

		public void Dispose()
		{
			Stop();
			_listener?.Dispose();
		}
	}

	// Низкоуровневый TCP клиент
	public class RawSocketClient : IDisposable
	{
		private Socket _socket;
		private readonly IPEndPoint _endpoint;

		public bool IsConnected => _socket?.Connected == true;
		public IntPtr Handle => _socket?.Handle ?? IntPtr.Zero;

		public RawSocketClient(string host, int port)
		{
			// DNS-разрешение - не часть сокетов, но необходим для установки соединения
			IPAddress[] addresses = Dns.GetHostAddresses(host);
			_endpoint = new IPEndPoint(addresses[0], port);

			Console.WriteLine($"[Client] Конечная точка: {_endpoint}");
		}

		public void Connect()
		{
			if (_socket != null && _socket.Connected)
			{
				Console.WriteLine($"[Client] Уже подключен");
				return;
			}

			// Создание нового сокета для каждого подключения
			_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			// Настройка параметров ОС через сокет
			_socket.ReceiveTimeout = 5000; // Таймаут в миллисекундах
			_socket.SendTimeout = 5000;

			Console.WriteLine($"[Client] Создан сокет. Дескриптор: {Handle.ToInt32()}");
			Console.WriteLine($"[Client] Параметры: ReceiveTimeout={_socket.ReceiveTimeout}, SendTimeout={_socket.SendTimeout}");

			try
			{
				// БЛОКИРУЮЩИЙ ВЫЗОВ: ОС устанавливает TCP-соединение
				Stopwatch sw = Stopwatch.StartNew();
				_socket.Connect(_endpoint);
				sw.Stop();

				Console.WriteLine($"[Client] Подключение установлено за {sw.ElapsedMilliseconds}мс");
				Console.WriteLine($"[Client] Локальная конечная точка: {_socket.LocalEndPoint}");
				Console.WriteLine($"[Client] Удалённая конечная точка: {_socket.RemoteEndPoint}");
				Console.WriteLine($"[Client] Состояние: Connected={_socket.Connected}, Available={_socket.Available}");
			}
			catch (SocketException ex)
			{
				Console.WriteLine($"[Client] Ошибка подключения: {ex.SocketErrorCode} - {ex.Message}");
				throw;
			}
		}

		public void SendData(string data)
		{
			if (!IsConnected)
			{
				Console.WriteLine($"[Client] Не подключен");
				return;
			}

			// Преобразование логических данных в байты - НЕ часть работы сокета
			byte[] buffer = Encoding.UTF8.GetBytes(data);

			Console.WriteLine($"[Client] Отправка {buffer.Length} байт");
			Console.WriteLine($"[Client] Данные как текст: {data}");

			try
			{
				// Отправка байтов через сокет - вызов ОС
				// Сокет не гарантирует, что все байты будут отправлены одной операцией
				int totalSent = 0;
				while (totalSent < buffer.Length)
				{
					int sent = _socket.Send(buffer, totalSent, buffer.Length - totalSent, SocketFlags.None);

					if (sent == 0)
					{
						Console.WriteLine($"[Client] Сокет закрыт при отправке");
						break;
					}

					totalSent += sent;
					Console.WriteLine($"[Client] Отправлено байт за операцию: {sent}, всего: {totalSent}/{buffer.Length}");
				}

				Console.WriteLine($"[Client] Всего отправлено байт: {totalSent}");
			}
			catch (SocketException ex)
			{
				Console.WriteLine($"[Client] Ошибка отправки: {ex.SocketErrorCode} - {ex.Message}");
				throw;
			}
		}

		public string ReceiveData(int bufferSize = 1024)
		{
			if (!IsConnected)
			{
				Console.WriteLine($"[Client] Не подключен");
				return string.Empty;
			}

			byte[] buffer = new byte[bufferSize];

			try
			{
				// БЛОКИРУЮЩИЙ ВЫЗОВ: ОС ждёт данные
				// Может получить меньше, чем запрошено, или байты могут прийти несколькими операциями
				int bytesRead = _socket.Receive(buffer, 0, buffer.Length, SocketFlags.None);

				Console.WriteLine($"[Client] Получено байт за операцию: {bytesRead}");
				Console.WriteLine($"[Client] Всего доступно байт в буфере сокета: {_socket.Available}");

				if (bytesRead == 0)
				{
					Console.WriteLine($"[Client] Сокет закрыт сервером");
					return string.Empty;
				}

				// Преобразование байтов в строку - ответственность приложения
				string received = Encoding.UTF8.GetString(buffer, 0, bytesRead);
				Console.WriteLine($"[Client] Полученные данные как текст: {received}");

				return received;
			}
			catch (SocketException ex)
			{
				Console.WriteLine($"[Client] Ошибка приёма: {ex.SocketErrorCode} - {ex.Message}");
				throw;
			}
		}

		public void Disconnect()
		{
			if (_socket == null) return;

			try
			{
				Console.WriteLine($"[Client] Отключение...");

				// Корректное закрытие TCP-соединения
				_socket.Shutdown(SocketShutdown.Both);
				_socket.Close();

				Console.WriteLine($"[Client] Отключён");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[Client] Ошибка при отключении: {ex.Message}");
			}
			finally
			{
				_socket.Dispose();
				_socket = null;
			}
		}

		public void Dispose()
		{
			Disconnect();
		}
	}

	// Демонстрация работы с сырыми сокетами
	public static class SocketDemonstration
	{
		public static void DemonstrateRawSocketOperations()
		{
			Console.WriteLine("=== ДЕМОНСТРАЦИЯ НИЗКОУРОВНЕВЫХ СОКЕТОВ ===\n");

			// Часть 1: Создание и настройка сокета
			Console.WriteLine("1. СОЗДАНИЕ И НАСТРОЙКА СОКЕТА:");
			DemonstrateSocketCreation();

			// Часть 2: Блокирующие операции
			Console.WriteLine("\n2. БЛОКИРУЮЩИЕ ОПЕРАЦИИ:");
			DemonstrateBlockingOperations();

			// Часть 3: Работа с байтами
			Console.WriteLine("\n3. РАБОТА С БАЙТАМИ:");
			DemonstrateByteOperations();

			// Часть 4: Состояния сокета
			Console.WriteLine("\n4. СОСТОЯНИЯ СОКЕТА:");
			DemonstrateSocketStates();

			// Часть 5: Полный пример клиент-сервер
			Console.WriteLine("\n5. ПОЛНЫЙ ПРИМЕР КЛИЕНТ-СЕРВЕР:");
			DemonstrateClientServer();
		}

		private static void DemonstrateSocketCreation()
		{
			Console.WriteLine("   Создание TCP сокета...");

			Socket socket = null;
			try
			{
				// Запрос ресурса у ОС
				socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

				Console.WriteLine($"   Сокет создан:");
				Console.WriteLine($"     Дескриптор: {socket.Handle.ToInt32()}");
				Console.WriteLine($"     AddressFamily: {socket.AddressFamily}");
				Console.WriteLine($"     SocketType: {socket.SocketType}");
				Console.WriteLine($"     ProtocolType: {socket.ProtocolType}");
				Console.WriteLine($"     Blocking: {socket.Blocking}");
				Console.WriteLine($"     Connected: {socket.Connected}");
				Console.WriteLine($"     Available: {socket.Available}");

				// Настройка параметров ОС
				socket.ReceiveBufferSize = 8192;
				socket.SendBufferSize = 8192;
				socket.NoDelay = true; // Отключение алгоритма Нагля

				Console.WriteLine($"   Параметры установлены:");
				Console.WriteLine($"     ReceiveBufferSize: {socket.ReceiveBufferSize}");
				Console.WriteLine($"     SendBufferSize: {socket.SendBufferSize}");
				Console.WriteLine($"     NoDelay: {socket.NoDelay}");
			}
			finally
			{
				// ОБЯЗАТЕЛЬНОЕ освобождение ресурса
				socket?.Dispose();
				Console.WriteLine($"   Ресурс сокета освобождён");
			}
		}

		private static void DemonstrateBlockingOperations()
		{
			Console.WriteLine("   Демонстрация блокирующих вызовов...");

			// Создаём временный сокет для демонстрации
			using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
			{
				Console.WriteLine($"   Блокирующий режим по умолчанию: {socket.Blocking}");

				// Установка таймаута
				socket.ReceiveTimeout = 1000;
				socket.SendTimeout = 1000;

				Console.WriteLine($"   Таймауты установлены:");
				Console.WriteLine($"     ReceiveTimeout: {socket.ReceiveTimeout}мс");
				Console.WriteLine($"     SendTimeout: {socket.SendTimeout}мс");

				try
				{
					// Попытка чтения из неподключенного сокета вызовет блокировку
					Console.WriteLine($"   Попытка чтения (блокирующий вызов)...");
					byte[] buffer = new byte[1024];
					int bytesRead = socket.Receive(buffer);

					Console.WriteLine($"   Прочитано байт: {bytesRead}");
				}
				catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
				{
					Console.WriteLine($"   Операция прервана по таймауту (ожидаемо)");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"   Ошибка: {ex.Message}");
				}
			}
		}

		private static void DemonstrateByteOperations()
		{
			Console.WriteLine("   Демонстрация работы с байтами...");

			// Симуляция отправки и приёма данных
			string testData = "Hello, Raw Sockets!";
			byte[] originalBytes = Encoding.UTF8.GetBytes(testData);

			Console.WriteLine($"   Исходные данные: '{testData}'");
			Console.WriteLine($"   Байтовое представление: {BitConverter.ToString(originalBytes)}");
			Console.WriteLine($"   Длина в байтах: {originalBytes.Length}");

			// Демонстрация фрагментации данных (имитация сетевой передачи)
			Console.WriteLine($"\n   Имитация фрагментации данных:");

			int chunkSize = 5; // Маленькие "пакеты"
			for (int i = 0; i < originalBytes.Length; i += chunkSize)
			{
				int remaining = originalBytes.Length - i;
				int currentChunkSize = Math.Min(chunkSize, remaining);

				byte[] chunk = new byte[currentChunkSize];
				Array.Copy(originalBytes, i, chunk, 0, currentChunkSize);

				Console.WriteLine($"     Чанк {i / chunkSize + 1}: {BitConverter.ToString(chunk)}");

				// В реальном сокете каждый чанк мог бы быть отдельной операцией Send/Receive
			}

			// Сборка обратно
			Console.WriteLine($"\n   Сборка данных из чанков:");
			string reconstructed = Encoding.UTF8.GetString(originalBytes);
			Console.WriteLine($"   Восстановленные данные: '{reconstructed}'");
			Console.WriteLine($"   Совпадение с исходными: {testData == reconstructed}");
		}

		private static void DemonstrateSocketStates()
		{
			Console.WriteLine("   Демонстрация состояний сокета...");

			using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
			{
				Console.WriteLine($"   Исходное состояние:");
				PrintSocketState(socket);

				// Привязка к локальному адресу
				socket.Bind(new IPEndPoint(IPAddress.Loopback, 0));
				Console.WriteLine($"\n   После Bind:");
				PrintSocketState(socket);

				// Установка в режим прослушивания
				socket.Listen(1);
				Console.WriteLine($"\n   После Listen:");
				PrintSocketState(socket);
			}
		}

		private static void PrintSocketState(Socket socket)
		{
			Console.WriteLine($"     IsBound: {socket.IsBound}");
			Console.WriteLine($"     Connected: {socket.Connected}");
			Console.WriteLine($"     Available: {socket.Available}");
			Console.WriteLine($"     Blocking: {socket.Blocking}");

			try
			{
				Console.WriteLine($"     LocalEndPoint: {socket.LocalEndPoint}");
				Console.WriteLine($"     RemoteEndPoint: {socket.RemoteEndPoint}");
			}
			catch
			{
				Console.WriteLine($"     EndPoints: N/A");
			}
		}

		private static void DemonstrateClientServer()
		{
			Console.WriteLine("   Запуск сервера на localhost:9999...");

			// Запуск сервера в отдельном потоке
			var server = new RawSocketServer(IPAddress.Loopback, 9999);
			server.Start();

			Thread.Sleep(1000); // Даём время серверу запуститься

			// Создание клиента
			using (var client = new RawSocketClient("localhost", 9999))
			{
				try
				{
					Console.WriteLine("\n   Клиент подключается...");
					client.Connect();

					// Обмен данными
					Console.WriteLine("\n   Отправка данных от клиента:");
					client.SendData("Привет от клиента!");

					Console.WriteLine("\n   Получение ответа от сервера:");
					string response = client.ReceiveData();
					Console.WriteLine($"   Ответ: {response}");

					// Ещё одна отправка для демонстрации
					Console.WriteLine("\n   Вторая отправка:");
					client.SendData("Второе сообщение");
					response = client.ReceiveData();
					Console.WriteLine($"   Ответ: {response}");

					Console.WriteLine("\n   Корректное отключение клиента:");
					client.Disconnect();
				}
				catch (Exception ex)
				{
					Console.WriteLine($"   Ошибка: {ex.Message}");
				}
			}

			// Остановка сервера
			Thread.Sleep(1000);
			server.Stop();
			server.Dispose();

			Console.WriteLine("   Демонстрация завершена");
		}
	}

	// Программа для запуска демонстрации
	public class Program
	{
		public static void Main()
		{
			Console.WriteLine("НИЗКОУРОВНЕВАЯ РАБОТА С СОКЕТАМИ В C#");
			Console.WriteLine("====================================\n");

			SocketDemonstration.DemonstrateRawSocketOperations();
		}
	}
}