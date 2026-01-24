using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;

namespace TCPConnectionArchitecture
{
	// Демонстрация архитектуры TCP-соединения
	public class TCPArchitectureDemo
	{
		public static async Task DemonstrateTCPArchitecture()
		{
			Console.WriteLine("=== АРХИТЕКТУРА TCP-СОЕДИНЕНИЯ ===\n");

			// 1. Установка соединения (трехстороннее рукопожатие)
			Console.WriteLine("1. УСТАНОВКА СОЕДИНЕНИЯ (трехстороннее рукопожатие):");
			await DemonstrateConnectionEstablishment();

			// 2. Поток байтов и буферизация
			Console.WriteLine("\n2. ПОТОК БАЙТОВ И БУФЕРИЗАЦИЯ:");
			await DemonstrateStreamBuffering();

			// 3. Управление потоком данных
			Console.WriteLine("\n3. УПРАВЛЕНИЕ ПОТОКОМ ДАННЫХ:");
			await DemonstrateFlowControl();

			// 4. Корректное закрытие соединения
			Console.WriteLine("\n4. ЗАКРЫТИЕ СОЕДИНЕНИЯ:");
			await DemonstrateConnectionClosure();

			// 5. Разрыв соединения и обработка ошибок
			Console.WriteLine("\n5. РАЗРЫВЫ СОЕДИНЕНИЯ:");
			await DemonstrateConnectionBreak();

			// 6. Тайм-ауты и устойчивость
			Console.WriteLine("\n6. ТАЙМ-АУТЫ И УСТОЙЧИВОСТЬ:");
			await DemonstrateTimeoutsAndResilience();

			// 7. Полная архитектура TCP в действии
			Console.WriteLine("\n7. ПОЛНАЯ АРХИТЕКТУРА В ДЕЙСТВИИ:");
			await DemonstrateFullArchitecture();
		}

		private static async Task DemonstrateConnectionEstablishment()
		{
			Console.WriteLine("   Демонстрация установки соединения:");

			const int port = 11030;

			// Сервер: подготовка к приёму соединений
			var serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			serverSocket.Bind(new IPEndPoint(IPAddress.Loopback, port));
			serverSocket.Listen(1);

			Console.WriteLine($"\n   Сервер готов слушать порт {port}");
			Console.WriteLine($"   Состояние сервера: IsBound={serverSocket.IsBound}, Connected={serverSocket.Connected}");

			// Асинхронное принятие соединения
			var acceptTask = Task.Run(() =>
			{
				Console.WriteLine($"   Сервер: вызываем Accept() - ожидание подключения...");
				return serverSocket.Accept();
			});

			// Даём время серверу начать ожидание
			await Task.Delay(500);

			// Клиент: установка соединения
			var clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			Console.WriteLine($"\n   Клиент: вызов Connect() - начало трехстороннего рукопожатия...");

			var stopwatch = Stopwatch.StartNew();
			clientSocket.Connect(new IPEndPoint(IPAddress.Loopback, port));
			stopwatch.Stop();

			Console.WriteLine($"   Клиент: соединение установлено за {stopwatch.ElapsedMilliseconds}мс");
			Console.WriteLine($"   Состояние клиента: Connected={clientSocket.Connected}");

			// Сервер получает клиентский сокет
			var acceptedSocket = await acceptTask;
			Console.WriteLine($"   Сервер: получил клиентское соединение");
			Console.WriteLine($"   Состояние серверного клиента: Connected={acceptedSocket.Connected}");

			// Проверка адресов
			Console.WriteLine($"\n   Адреса соединения:");
			Console.WriteLine($"     Клиент Local: {clientSocket.LocalEndPoint}");
			Console.WriteLine($"     Клиент Remote: {clientSocket.RemoteEndPoint}");
			Console.WriteLine($"     Серверный клиент Local: {acceptedSocket.LocalEndPoint}");
			Console.WriteLine($"     Серверный клиент Remote: {acceptedSocket.RemoteEndPoint}");

			// Освобождение ресурсов
			clientSocket.Close();
			acceptedSocket.Close();
			serverSocket.Close();

			Console.WriteLine($"\n   Вывод: Соединение установлено через трехстороннее рукопожатие");
			Console.WriteLine($"   Клиент отправляет SYN → Сервер отвечает SYN-ACK → Клиент отправляет ACK");
		}

		private static async Task DemonstrateStreamBuffering()
		{
			Console.WriteLine("   Демонстрация потока байтов и буферизации:");

			const int port = 11031;

			// Запускаем тестовый сервер
			var server = new TCPServer(port);
			var serverTask = server.StartAsync();

			await Task.Delay(500);

			using (var client = new TCPClient(port))
			{
				await client.ConnectAsync();

				// Тест 1: Множественные отправки - одно чтение
				Console.WriteLine($"\n   Тест 1: Разные отправки - одно чтение");

				Console.WriteLine($"   Клиент отправляет: 'Hello', ' ', 'World', '!'");
				await client.SendAsync("Hello");
				await Task.Delay(100);
				await client.SendAsync(" ");
				await Task.Delay(100);
				await client.SendAsync("World");
				await Task.Delay(100);
				await client.SendAsync("!");

				// Сервер читает всё одним вызовом
				string received1 = await server.GetLastReceivedAsync(1000);
				Console.WriteLine($"   Сервер получил одним чтением: '{received1}'");
				Console.WriteLine($"   Вывод: TCP объединил отправки в один поток байтов");

				// Тест 2: Одна отправка - множество чтений
				Console.WriteLine($"\n   Тест 2: Одна отправка - множество чтений");

				string longMessage = new string('X', 1000); // 1000 символов
				Console.WriteLine($"   Клиент отправляет 1000 символов одной операцией");
				await client.SendAsync(longMessage);

				// Сервер читает по частям
				var chunks = await server.ReceiveInChunksAsync(100, 2000);
				Console.WriteLine($"   Сервер получил {chunks.Count} чанками:");
				Console.WriteLine($"     Первый чанк: {chunks.FirstOrDefault()?.Length ?? 0} байт");
				Console.WriteLine($"     Последний чанк: {chunks.LastOrDefault()?.Length ?? 0} байт");
				Console.WriteLine($"   Вывод: TCP может разбить данные на фрагменты");

				// Тест 3: Размеры буферов
				Console.WriteLine($"\n   Тест 3: Размеры буферов TCP");

				Console.WriteLine($"   Клиент SendBufferSize: {client.GetSendBufferSize()}");
				Console.WriteLine($"   Клиент ReceiveBufferSize: {client.GetReceiveBufferSize()}");
				Console.WriteLine($"   Сервер SendBufferSize: {server.GetSendBufferSize()}");
				Console.WriteLine($"   Сервер ReceiveBufferSize: {server.GetReceiveBufferSize()}");

				Console.WriteLine($"   Вывод: У каждого сокета есть свои буферы отправки и приёма");
			}

			server.Stop();
			await serverTask;
		}

		private static async Task DemonstrateFlowControl()
		{
			Console.WriteLine("   Демонстрация управления потоком данных:");

			const int port = 11032;

			// Создаём сервер с маленьким буфером
			var server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			server.ReceiveBufferSize = 256; // Маленький буфер специально
			server.Bind(new IPEndPoint(IPAddress.Loopback, port));
			server.Listen(1);

			var acceptTask = Task.Run(() => server.Accept());

			// Клиент с большим сообщением
			var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			client.SendBufferSize = 1024;
			client.Connect(new IPEndPoint(IPAddress.Loopback, port));

			var acceptedSocket = await acceptTask;

			// Тест отправки большего объёма данных
			Console.WriteLine($"\n   Тест управления потоком:");
			Console.WriteLine($"   Клиент буфер отправки: {client.SendBufferSize} байт");
			Console.WriteLine($"   Сервер буфер приёма: {acceptedSocket.ReceiveBufferSize} байт");

			// Отправляем данные быстрее, чем сервер их обрабатывает
			Console.WriteLine($"\n   Клиент отправляет 10 сообщений по 100 байт...");

			var sendTasks = new Task[10];
			for (int i = 0; i < 10; i++)
			{
				string message = $"Сообщение {i}: {new string('A', 90)}";
				byte[] data = Encoding.UTF8.GetBytes(message);

				sendTasks[i] = Task.Run(() =>
				{
					try
					{
						client.Send(data);
					}
					catch (SocketException ex)
					{
						Console.WriteLine($"   Ошибка отправки: {ex.SocketErrorCode}");
					}
				});
			}

			// Сервер медленно читает
			Console.WriteLine($"   Сервер начинает медленное чтение...");

			int totalReceived = 0;
			var buffer = new byte[128]; // Читаем маленькими порциями

			Task readingTask = Task.Run(() =>
			{
				for (int i = 0; i < 20 && totalReceived < 1000; i++)
				{
					try
					{
						if (acceptedSocket.Available > 0)
						{
							int bytesRead = acceptedSocket.Receive(buffer);
							totalReceived += bytesRead;
							Console.WriteLine($"   Сервер прочитал {bytesRead} байт, всего {totalReceived}");

							// Имитация медленной обработки
							Thread.Sleep(200);
						}
					}
					catch { break; }
				}
			});

			// Ждём завершения
			Task.WaitAll(sendTasks);
			await readingTask;

			Console.WriteLine($"\n   Итоги:");
			Console.WriteLine($"   Отправлено: ~1000 байт");
			Console.WriteLine($"   Получено: {totalReceived} байт");
			Console.WriteLine($"   Вывод: TCP замедлил отправку из-за заполнения буфера");

			client.Close();
			acceptedSocket.Close();
			server.Close();
		}

		private static async Task DemonstrateConnectionClosure()
		{
			Console.WriteLine("   Демонстрация корректного закрытия соединения:");

			const int port = 11033;

			var server = new TCPServer(port);
			var serverTask = server.StartAsync();

			await Task.Delay(500);

			using (var client = new TCPClient(port))
			{
				await client.ConnectAsync();

				Console.WriteLine($"\n   Этапы корректного закрытия:");
				Console.WriteLine($"   1. Клиент хочет закрыть соединение");

				// Отправляем последние данные
				await client.SendAsync("Последнее сообщение");

				Console.WriteLine($"   2. Клиент вызывает Shutdown(Send) - больше не будет отправлять");
				client.ShutdownSend();

				// Даём время серверу получить все данные
				await Task.Delay(500);

				Console.WriteLine($"   3. Сервер получает признак конца потока");
				string lastMessage = await server.GetLastReceivedAsync(1000);
				Console.WriteLine($"      Сервер получил: '{lastMessage}'");

				Console.WriteLine($"   4. Сервер вызывает Shutdown(Send) в ответ");
				server.ShutdownSend();

				Console.WriteLine($"   5. Клиент получает признак конца потока");
				bool clientEOF = await client.CheckForEOFAsync();
				Console.WriteLine($"      Клиент получил EOF: {clientEOF}");

				Console.WriteLine($"   6. Обе стороны вызывают Close()");

				// Освобождение
				server.Stop();
				await serverTask;
			}

			Console.WriteLine($"\n   Вывод: Корректное закрытие - это процесс, а не одно действие");
		}

		private static async Task DemonstrateConnectionBreak()
		{
			Console.WriteLine("   Демонстрация разрывов соединения:");

			const int port = 11034;

			// Тест 1: Резкое закрытие клиента
			Console.WriteLine($"\n   Тест 1: Клиент неожиданно закрывается");

			var server = new TCPServer(port);
			var serverTask = server.StartAsync();

			await Task.Delay(500);

			var client = new TCPClient(port);
			await client.ConnectAsync();

			// Клиент резко закрывается (без Shutdown)
			Console.WriteLine($"   Клиент резко закрывает соединение...");
			client.Dispose();

			// Проверяем, как сервер узнаёт о разрыве
			try
			{
				await server.WaitForConnectionBreakAsync(2000);
				Console.WriteLine($"   Сервер обнаружил разрыв соединения");
			}
			catch (TimeoutException)
			{
				Console.WriteLine($"   Сервер не обнаружил разрыв вовремя");
			}

			server.Stop();
			await serverTask;

			// Тест 2: Сетевая ошибка при отправке
			Console.WriteLine($"\n   Тест 2: Ошибка сети при отправке");

			server = new TCPServer(port);
			serverTask = server.StartAsync();

			await Task.Delay(500);

			using (var client2 = new TCPClient(port))
			{
				await client2.ConnectAsync();

				// Закрываем серверную сторону, чтобы симулировать сбой
				server.StopImmediately();

				Console.WriteLine($"   Серверная сторона закрыта, клиент пытается отправить...");

				try
				{
					await client2.SendAsync("Тестовое сообщение");
					Console.WriteLine($"   ОШИБКА: Отправка должна была завершиться с ошибкой");
				}
				catch (SocketException ex)
				{
					Console.WriteLine($"   Ожидаемая ошибка: {ex.SocketErrorCode}");
				}
			}

			await serverTask;

			Console.WriteLine($"\n   Вывод: Разрыв соединения - нормальная ситуация в сетевом коде");
		}

		private static async Task DemonstrateTimeoutsAndResilience()
		{
			Console.WriteLine("   Демонстрация тайм-аутов и устойчивости:");

			// Тест 1: Тайм-аут подключения
			Console.WriteLine($"\n   Тест 1: Тайм-аут при подключении");

			var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			try
			{
				// Пытаемся подключиться к несуществующему порту
				client.Connect(new IPEndPoint(IPAddress.Loopback, 99999));
				Console.WriteLine($"   ОШИБКА: Подключение не должно было состояться");
			}
			catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionRefused)
			{
				Console.WriteLine($"   Ожидаемая ошибка: ConnectionRefused");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"   Ошибка: {ex.Message}");
			}

			client.Close();

			// Тест 2: Настройка тайм-аутов
			Console.WriteLine($"\n   Тест 2: Настройка тайм-аутов сокета");

			var socketWithTimeout = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			// Устанавливаем короткие тайм-ауты
			socketWithTimeout.ReceiveTimeout = 1000; // 1 секунда
			socketWithTimeout.SendTimeout = 1000;

			Console.WriteLine($"   ReceiveTimeout: {socketWithTimeout.ReceiveTimeout}мс");
			Console.WriteLine($"   SendTimeout: {socketWithTimeout.SendTimeout}мс");

			// Тест чтения с тайм-аутом
			try
			{
				socketWithTimeout.Bind(new IPEndPoint(IPAddress.Loopback, 0));
				socketWithTimeout.Listen(1);

				Console.WriteLine($"\n   Тест операции с тайм-аутом:");
				Console.WriteLine($"   Вызов Receive() на слушающем сокете...");

				// Этот вызов завершится по тайм-ауту
				socketWithTimeout.Receive(new byte[1]);

				Console.WriteLine($"   ОШИБКА: Receive должен был завершиться по тайм-ауту");
			}
			catch (SocketException ex)
			{
				Console.WriteLine($"   Ожидаемый тайм-аут: {ex.SocketErrorCode}");
			}

			socketWithTimeout.Close();

			// Тест 3: Устойчивость к временным сбоям
			Console.WriteLine($"\n   Тест 3: Устойчивое приложение");

			const int port = 11035;
			var resilientServer = new ResilientTCPServer(port);
			var serverTask = resilientServer.StartAsync();

			await Task.Delay(500);

			// Имитация различных проблем
			Console.WriteLine($"   Тест устойчивого клиента:");

			using (var resilientClient = new ResilientTCPClient(port))
			{
				int attempts = 0;
				bool connected = false;

				while (attempts < 3 && !connected)
				{
					attempts++;
					Console.WriteLine($"   Попытка подключения #{attempts}...");

					try
					{
						await resilientClient.ConnectWithRetryAsync(3, 1000);
						connected = true;
						Console.WriteLine($"   Подключено с попытки #{attempts}");
					}
					catch (Exception ex)
					{
						Console.WriteLine($"   Попытка {attempts} не удалась: {ex.Message}");
						await Task.Delay(500);
					}
				}

				if (connected)
				{
					Console.WriteLine($"   Отправка тестового сообщения...");
					await resilientClient.SendWithRetryAsync("Тестовое сообщение", 3);
					Console.WriteLine($"   Сообщение отправлено");
				}
			}

			resilientServer.Stop();
			await serverTask;

			Console.WriteLine($"\n   Вывод: Тайм-ауты и повторные попытки - основа устойчивых сетевых приложений");
		}

		private static async Task DemonstrateFullArchitecture()
		{
			Console.WriteLine("   Полная архитектура TCP в действии:");

			const int port = 11036;

			// Создаем эхо-сервер, который демонстрирует все аспекты TCP
			var echoServer = new EchoServer(port);
			var serverTask = echoServer.StartAsync();

			await Task.Delay(1000);

			Console.WriteLine($"\n   Тестирование полного цикла TCP:");

			// Тест 1: Нормальная работа
			Console.WriteLine($"\n   1. Нормальное взаимодействие:");

			using (var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
			{
				Console.WriteLine($"   Этап 1: Установка соединения");
				client.Connect(new IPEndPoint(IPAddress.Loopback, port));
				Console.WriteLine($"     Соединение установлено, состояние: Connected={client.Connected}");

				Console.WriteLine($"\n   Этап 2: Передача данных");
				string message = "Hello, TCP Architecture!";
				byte[] sendData = Encoding.UTF8.GetBytes(message);

				Console.WriteLine($"     Отправка: '{message}' ({sendData.Length} байт)");
				int sent = client.Send(sendData);
				Console.WriteLine($"     Отправлено байт: {sent}");

				Console.WriteLine($"\n   Этап 3: Приём ответа");
				byte[] receiveBuffer = new byte[1024];
				int received = client.Receive(receiveBuffer);
				string response = Encoding.UTF8.GetString(receiveBuffer, 0, received);
				Console.WriteLine($"     Получен ответ: '{response}' ({received} байт)");

				Console.WriteLine($"\n   Этап 4: Корректное закрытие");
				client.Shutdown(SocketShutdown.Both);
				Console.WriteLine($"     Shutdown выполнен");
				client.Close();
				Console.WriteLine($"     Сокет закрыт, состояние: Connected={client.Connected}");
			}

			// Тест 2: Проверка буферизации
			Console.WriteLine($"\n   2. Тест буферизации:");

			using (var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
			{
				client.Connect(new IPEndPoint(IPAddress.Loopback, port));

				// Отправляем данные частями
				string[] parts = { "Часть1", "Часть2", "Часть3", "Часть4" };

				foreach (var part in parts)
				{
					byte[] data = Encoding.UTF8.GetBytes(part);
					client.Send(data);
					Console.WriteLine($"     Отправлена часть: '{part}'");
					await Task.Delay(50);
				}

				// Читаем ответ (скорее всего, одним куском)
				byte[] buffer = new byte[1024];
				int totalReceived = client.Receive(buffer);
				string fullResponse = Encoding.UTF8.GetString(buffer, 0, totalReceived);
				Console.WriteLine($"     Получен ответ: '{fullResponse}' ({totalReceived} байт)");

				client.Close();
			}

			// Тест 3: Обработка разрыва
			Console.WriteLine($"\n   3. Тест обработки разрыва:");

			var client3 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			client3.Connect(new IPEndPoint(IPAddress.Loopback, port));

			Console.WriteLine($"   Соединение установлено");

			// Резко останавливаем сервер
			echoServer.Stop();
			await serverTask;

			Console.WriteLine($"   Сервер остановлен, проверяем состояние клиента...");

			try
			{
				// Попытка отправить данные на закрытый сервер
				client3.Send(Encoding.UTF8.GetBytes("Test"));
				Console.WriteLine($"   ОШИБКА: Отправка должна была завершиться с ошибкой");
			}
			catch (SocketException ex)
			{
				Console.WriteLine($"   Ожидаемая ошибка: {ex.SocketErrorCode}");
				Console.WriteLine($"   TCP обнаружил разрыв соединения");
			}

			client3.Close();

			Console.WriteLine($"\n   ИТОГИ ДЕМОНСТРАЦИИ:");
			Console.WriteLine($"   1. TCP-соединение - это состояние, а не физический канал");
			Console.WriteLine($"   2. Установка требует трехстороннего рукопожатия");
			Console.WriteLine($"   3. Данные передаются как поток байтов через буферы");
			Console.WriteLine($"   4. TCP управляет порядком и надёжностью данных");
			Console.WriteLine($"   5. Закрытие - это процесс с обоюдным подтверждением");
			Console.WriteLine($"   6. Разрывы - нормальная часть сетевой работы");
			Console.WriteLine($"   7. Приложение отвечает за семантику данных поверх TCP");
		}
	}

	// Вспомогательные классы для демонстрации
	public class TCPServer : IDisposable
	{
		private Socket _serverSocket;
		private Socket _clientSocket;
		private Thread _serverThread;
		private bool _isRunning;
		private readonly int _port;
		private readonly ConcurrentQueue<string> _receivedMessages = new();

		public TCPServer(int port)
		{
			_port = port;
		}

		public async Task StartAsync()
		{
			_serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			_serverSocket.Bind(new IPEndPoint(IPAddress.Loopback, _port));
			_serverSocket.Listen(1);

			_isRunning = true;
			_serverThread = new Thread(RunServer);
			_serverThread.Start();

			await Task.Delay(100);
		}

		private void RunServer()
		{
			try
			{
				_clientSocket = _serverSocket.Accept();

				var buffer = new byte[1024];
				while (_isRunning && _clientSocket.Connected)
				{
					if (_clientSocket.Available > 0)
					{
						int bytesRead = _clientSocket.Receive(buffer);
						if (bytesRead > 0)
						{
							string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
							_receivedMessages.Enqueue(message);

							// Эхо-ответ
							_clientSocket.Send(Encoding.UTF8.GetBytes($"Echo: {message}"));
						}
					}
					Thread.Sleep(10);
				}
			}
			catch { }
		}

		public async Task<string> GetLastReceivedAsync(int timeoutMs)
		{
			var startTime = DateTime.Now;

			while ((DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
			{
				if (_receivedMessages.TryDequeue(out string message))
					return message;

				await Task.Delay(10);
			}

			return string.Empty;
		}

		public async Task<List<string>> ReceiveInChunksAsync(int chunkSize, int timeoutMs)
		{
			var chunks = new List<string>();
			var buffer = new byte[chunkSize];
			var startTime = DateTime.Now;

			while ((DateTime.Now - startTime).TotalMilliseconds < timeoutMs && _clientSocket != null && _clientSocket.Connected)
			{
				if (_clientSocket.Available > 0)
				{
					try
					{
						int bytesRead = _clientSocket.Receive(buffer, Math.Min(chunkSize, _clientSocket.Available), SocketFlags.None);
						if (bytesRead > 0)
						{
							chunks.Add(Encoding.UTF8.GetString(buffer, 0, bytesRead));
						}
					}
					catch { break; }
				}
				await Task.Delay(10);
			}

			return chunks;
		}

		public void ShutdownSend()
		{
			_clientSocket?.Shutdown(SocketShutdown.Send);
		}

		public async Task WaitForConnectionBreakAsync(int timeoutMs)
		{
			var startTime = DateTime.Now;

			while ((DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
			{
				if (_clientSocket == null || !_clientSocket.Connected || _clientSocket.Available < 0)
					return;

				await Task.Delay(10);
			}

			throw new TimeoutException("Разрыв соединения не обнаружен");
		}

		public void Stop()
		{
			_isRunning = false;
			_clientSocket?.Close();
			_serverSocket?.Close();
			_serverThread?.Join(1000);
		}

		public void StopImmediately()
		{
			_clientSocket?.Close();
			_serverSocket?.Close();
		}

		public int GetSendBufferSize() => _serverSocket?.SendBufferSize ?? 0;
		public int GetReceiveBufferSize() => _serverSocket?.ReceiveBufferSize ?? 0;

		public void Dispose() => Stop();
	}

	public class TCPClient : IDisposable
	{
		private Socket _socket;
		private readonly int _port;

		public TCPClient(int port)
		{
			_port = port;
		}

		public async Task ConnectAsync()
		{
			_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			await Task.Run(() => _socket.Connect(new IPEndPoint(IPAddress.Loopback, _port)));
		}

		public async Task SendAsync(string message)
		{
			byte[] data = Encoding.UTF8.GetBytes(message);
			await Task.Run(() => _socket.Send(data));
		}

		public void ShutdownSend()
		{
			_socket.Shutdown(SocketShutdown.Send);
		}

		public async Task<bool> CheckForEOFAsync()
		{
			var buffer = new byte[1];
			try
			{
				int received = await Task.Run(() => _socket.Receive(buffer, SocketFlags.Peek));
				return received == 0;
			}
			catch
			{
				return true;
			}
		}

		public int GetSendBufferSize() => _socket?.SendBufferSize ?? 0;
		public int GetReceiveBufferSize() => _socket?.ReceiveBufferSize ?? 0;

		public void Dispose()
		{
			_socket?.Close();
			_socket?.Dispose();
		}
	}

	public class ResilientTCPServer : IDisposable
	{
		private Socket _serverSocket;
		private bool _isRunning;
		private readonly int _port;

		public ResilientTCPServer(int port)
		{
			_port = port;
		}

		public async Task StartAsync()
		{
			_serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			_serverSocket.Bind(new IPEndPoint(IPAddress.Loopback, _port));
			_serverSocket.Listen(1);

			_isRunning = true;

			_ = Task.Run(async () =>
			{
				while (_isRunning)
				{
					try
					{
						var client = await Task.Factory.FromAsync(
							_serverSocket.BeginAccept(null, null),
							_serverSocket.EndAccept);

						// Обработка клиента
						_ = Task.Run(() => HandleClient(client));
					}
					catch { }
				}
			});

			await Task.Delay(100);
		}

		private void HandleClient(Socket client)
		{
			try
			{
				var buffer = new byte[1024];
				while (client.Connected)
				{
					int bytesRead = client.Receive(buffer);
					if (bytesRead == 0) break;

					// Простая эхо-логика
					client.Send(buffer, 0, bytesRead, SocketFlags.None);
				}
			}
			catch { }
			finally
			{
				client.Close();
			}
		}

		public void Stop()
		{
			_isRunning = false;
			_serverSocket?.Close();
		}

		public void Dispose() => Stop();
	}

	public class ResilientTCPClient : IDisposable
	{
		private Socket _socket;
		private readonly int _port;

		public ResilientTCPClient(int port)
		{
			_port = port;
		}

		public async Task ConnectWithRetryAsync(int maxAttempts, int delayMs)
		{
			for (int attempt = 1; attempt <= maxAttempts; attempt++)
			{
				try
				{
					_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
					await Task.Run(() => _socket.Connect(new IPEndPoint(IPAddress.Loopback, _port)));
					return;
				}
				catch (SocketException)
				{
					if (attempt == maxAttempts) throw;
					await Task.Delay(delayMs);
				}
			}
		}

		public async Task SendWithRetryAsync(string message, int maxAttempts)
		{
			byte[] data = Encoding.UTF8.GetBytes(message);

			for (int attempt = 1; attempt <= maxAttempts; attempt++)
			{
				try
				{
					await Task.Run(() => _socket.Send(data));
					return;
				}
				catch (SocketException)
				{
					if (attempt == maxAttempts) throw;
					await Task.Delay(100);
				}
			}
		}

		public void Dispose()
		{
			_socket?.Close();
			_socket?.Dispose();
		}
	}

	public class EchoServer : IDisposable
	{
		private Socket _serverSocket;
		private bool _isRunning;
		private readonly int _port;
		private Thread _serverThread;

		public EchoServer(int port)
		{
			_port = port;
		}

		public async Task StartAsync()
		{
			_serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			_serverSocket.Bind(new IPEndPoint(IPAddress.Loopback, _port));
			_serverSocket.Listen(10);

			_isRunning = true;
			_serverThread = new Thread(RunServer);
			_serverThread.Start();

			await Task.Delay(100);
		}

		private void RunServer()
		{
			try
			{
				while (_isRunning)
				{
					var client = _serverSocket.Accept();
					_ = Task.Run(() => HandleEchoClient(client));
				}
			}
			catch { }
		}

		private void HandleEchoClient(Socket client)
		{
			try
			{
				var buffer = new byte[1024];
				while (client.Connected)
				{
					int bytesRead = client.Receive(buffer);
					if (bytesRead == 0) break;

					// Эхо-ответ
					client.Send(buffer, 0, bytesRead, SocketFlags.None);
				}
			}
			catch { }
			finally
			{
				client.Close();
			}
		}

		public void Stop()
		{
			_isRunning = false;
			_serverSocket?.Close();
			_serverThread?.Join(1000);
		}

		public void Dispose() => Stop();
	}

	// Главная программа
	class Program
	{
		static async Task Main(string[] args)
		{
			Console.WriteLine("АРХИТЕКТУРА TCP-СОЕДИНЕНИЯ В C#");
			Console.WriteLine("===============================\n");

			await TCPArchitectureDemo.DemonstrateTCPArchitecture();
		}
	}
}