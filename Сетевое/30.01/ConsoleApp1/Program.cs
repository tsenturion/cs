using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;

namespace UnicastDemo
{
	// Демонстрация unicast через TCP
	public class TcpUnicastDemonstration : IDisposable
	{
		private TcpListener _listener;
		private List<TcpClient> _connectedClients = new List<TcpClient>();
		private readonly object _clientsLock = new object();
		private bool _isRunning;
		private int _port;

		public TcpUnicastDemonstration(int port = 11060)
		{
			_port = port;
		}

		public void DemonstrateTcpUnicast()
		{
			Console.WriteLine("=== UNICAST ЧЕРЕЗ TCP ===");
			Console.WriteLine("Один отправитель → один получатель с установлением соединения\n");

			// Запуск TCP-сервера (получатель)
			StartTcpServer();

			// Даём время серверу запуститься
			Thread.Sleep(1000);

			// Подключение нескольких TCP-клиентов (отправители)
			Console.WriteLine("\n1. УСТАНОВЛЕНИЕ СОЕДИНЕНИЙ:");
			var clients = new List<TcpClient>();

			for (int i = 1; i <= 3; i++)
			{
				try
				{
					var client = new TcpClient();
					client.Connect(IPAddress.Loopback, _port);
					clients.Add(client);

					Console.WriteLine($"   Клиент #{i} подключился к серверу");
					Console.WriteLine($"     Локальная конечная точка: {client.Client.LocalEndPoint}");
					Console.WriteLine($"     Удалённая конечная точка: {client.Client.RemoteEndPoint}");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"   Ошибка подключения клиента #{i}: {ex.Message}");
				}
			}

			// Обмен данными в unicast-режиме
			Console.WriteLine("\n2. UNICAST ОБМЕН ДАННЫМИ:");

			// Клиент 1 отправляет сообщение серверу
			if (clients.Count > 0)
			{
				var client1 = clients[0];
				string message1 = "Привет от клиента #1";
				byte[] data1 = Encoding.UTF8.GetBytes(message1);

				client1.GetStream().Write(data1, 0, data1.Length);
				Console.WriteLine($"   Клиент #1 → Сервер: '{message1}'");

				// Сервер получает сообщение от конкретного клиента
				Thread.Sleep(100);
			}

			// Клиент 2 отправляет сообщение
			if (clients.Count > 1)
			{
				var client2 = clients[1];
				string message2 = "Сообщение от клиента #2";
				byte[] data2 = Encoding.UTF8.GetBytes(message2);

				client2.GetStream().Write(data2, 0, data2.Length);
				Console.WriteLine($"   Клиент #2 → Сервер: '{message2}'");
			}

			// Сервер отправляет ответы конкретным клиентам
			Console.WriteLine("\n3. СЕРВЕР ОТВЕЧАЕТ КОНКРЕТНЫМ КЛИЕНТАМ:");

			List<TcpClient> connectedSnapshot;
			lock (_clientsLock)
			{
				connectedSnapshot = new List<TcpClient>(_connectedClients);
			}

			foreach (var client in connectedSnapshot)
			{
				try
				{
					string response = $"Ответ для {client.Client.RemoteEndPoint}";
					byte[] responseData = Encoding.UTF8.GetBytes(response);
					client.GetStream().Write(responseData, 0, responseData.Length);

					Console.WriteLine($"   Сервер → {client.Client.RemoteEndPoint}: '{response}'");
				}
				catch { }
			}

			// Демонстрация изоляции соединений
			Console.WriteLine("\n4. ИЗОЛЯЦИЯ UNICAST-СОЕДИНЕНИЙ:");

			int connectedCount;
			lock (_clientsLock)
			{
				connectedCount = _connectedClients.Count;
			}

			if (clients.Count > 2 && connectedCount > 2)
			{
				// Закрываем соединение только с третьим клиентом
				clients[2].Close();
				Console.WriteLine($"   Соединение с клиентом #3 закрыто");

				// Пытаемся отправить данные через закрытое соединение
				try
				{
					byte[] testData = Encoding.UTF8.GetBytes("Тест");
					clients[2].GetStream().Write(testData, 0, testData.Length);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"   Ошибка отправки через закрытое соединение: {ex.Message}");
					Console.WriteLine($"   Другие соединения продолжают работать");
				}
			}

			// Очистка
			foreach (var client in clients)
			{
				client.Close();
			}

			StopTcpServer();

			Console.WriteLine("\n   TCP unicast демонстрация завершена");
		}

		private void StartTcpServer()
		{
			_listener = new TcpListener(IPAddress.Loopback, _port);
			_listener.Start();

			Console.WriteLine($"   TCP сервер запущен на {_listener.LocalEndpoint}");
			Console.WriteLine($"   Ожидает unicast-подключений");

			_isRunning = true;

			// Асинхронный приём подключений
			Task.Run(async () =>
			{
				while (_isRunning)
				{
					try
					{
						var client = await _listener.AcceptTcpClientAsync();
						lock (_clientsLock)
						{
							_connectedClients.Add(client);
						}

						Console.WriteLine($"   Сервер принял unicast-соединение от {client.Client.RemoteEndPoint}");

						// Асинхронная обработка данных от клиента
						_ = Task.Run(async () =>
						{
							try
							{
								var stream = client.GetStream();
								var buffer = new byte[1024];

								while (client.Connected)
								{
									int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
									if (bytesRead == 0) break;

									string received = Encoding.UTF8.GetString(buffer, 0, bytesRead);
									Console.WriteLine($"   Сервер получил от {client.Client.RemoteEndPoint}: '{received}'");
								}
							}
							catch { }
							finally
							{
								lock (_clientsLock)
								{
									_connectedClients.Remove(client);
								}
								client.Close();
							}
						});
					}
					catch { }
				}
			});
		}

		private void StopTcpServer()
		{
			_isRunning = false;
			_listener?.Stop();

			List<TcpClient> clientsToClose;
			lock (_clientsLock)
			{
				clientsToClose = new List<TcpClient>(_connectedClients);
				_connectedClients.Clear();
			}

			foreach (var client in clientsToClose)
			{
				client.Close();
			}
		}

		public void Dispose()
		{
			StopTcpServer();
		}
	}

	// Демонстрация unicast через UDP
	public class UdpUnicastDemonstration : IDisposable
	{
		private UdpClient _server;
		private UdpClient _client1;
		private UdpClient _client2;
		private bool _isRunning;
		private int _serverPort;
		private int _client1Port;
		private int _client2Port;

		public UdpUnicastDemonstration(int basePort = 11070)
		{
			_serverPort = basePort;
			_client1Port = basePort + 1;
			_client2Port = basePort + 2;
		}

		public void DemonstrateUdpUnicast()
		{
			Console.WriteLine("\n\n=== UNICAST ЧЕРЕЗ UDP ===");
			Console.WriteLine("Один отправитель → один получатель без установления соединения\n");

			try
			{
				// Создание UDP-сервера
				_server = new UdpClient(_serverPort);
				Console.WriteLine($"   UDP сервер запущен на порту {_serverPort}");
				Console.WriteLine($"   Локальная конечная точка: {_server.Client.LocalEndPoint}");

				// Создание UDP-клиентов с фиксированными портами
				_client1 = new UdpClient(_client1Port);
				_client2 = new UdpClient(_client2Port);

				Console.WriteLine($"\n   UDP клиенты созданы:");
				Console.WriteLine($"     Клиент #1 на порту {_client1Port}");
				Console.WriteLine($"     Клиент #2 на порту {_client2Port}");

				// Явное указание удалённых адресов для unicast
				var serverEndpoint = new IPEndPoint(IPAddress.Loopback, _serverPort);
				var client1Endpoint = new IPEndPoint(IPAddress.Loopback, _client1Port);
				var client2Endpoint = new IPEndPoint(IPAddress.Loopback, _client2Port);

				Console.WriteLine($"\n1. ЯВНОЕ УКАЗАНИЕ UNICAST-АДРЕСОВ:");
				Console.WriteLine($"   Сервер слушает: {serverEndpoint}");
				Console.WriteLine($"   Клиент #1 адрес: {client1Endpoint}");
				Console.WriteLine($"   Клиент #2 адрес: {client2Endpoint}");

				// Запуск приёма на сервере
				_isRunning = true;
				Task.Run(ServerReceiveLoop);

				// Даём время серверу начать приём
				Thread.Sleep(500);

				Console.WriteLine("\n2. UNICAST ОТПРАВКА ОТ КОНКРЕТНЫХ КЛИЕНТОВ:");

				// Клиент 1 отправляет сообщение конкретному серверу
				string message1 = "Сообщение от клиента #1 серверу";
				byte[] data1 = Encoding.UTF8.GetBytes(message1);
				_client1.Send(data1, data1.Length, serverEndpoint);
				Console.WriteLine($"   Клиент #1 → Сервер: '{message1}'");

				Thread.Sleep(200);

				// Клиент 2 отправляет сообщение тому же серверу
				string message2 = "Данные от клиента #2";
				byte[] data2 = Encoding.UTF8.GetBytes(message2);
				_client2.Send(data2, data2.Length, serverEndpoint);
				Console.WriteLine($"   Клиент #2 → Сервер: '{message2}'");

				Thread.Sleep(200);

				Console.WriteLine("\n3. СЕРВЕР ОТВЕЧАЕТ КОНКРЕТНЫМ КЛИЕНТАМ:");

				// Сервер отвечает клиенту 1
				string response1 = "Ответ клиенту #1";
				byte[] responseData1 = Encoding.UTF8.GetBytes(response1);
				_server.Send(responseData1, responseData1.Length, client1Endpoint);
				Console.WriteLine($"   Сервер → Клиент #1: '{response1}'");

				// Сервер отвечает клиенту 2
				string response2 = "Ответ для клиента #2";
				byte[] responseData2 = Encoding.UTF8.GetBytes(response2);
				_server.Send(responseData2, responseData2.Length, client2Endpoint);
				Console.WriteLine($"   Сервер → Клиент #2: '{response2}'");

				Thread.Sleep(200);

				Console.WriteLine("\n4. ПРЯМОЕ UNICAST ВЗАИМОДЕЙСТВИЕ КЛИЕНТОВ:");

				// Клиент 1 отправляет сообщение напрямую клиенту 2
				string directMessage = "Привет от клиента #1 клиенту #2";
				byte[] directData = Encoding.UTF8.GetBytes(directMessage);
				_client1.Send(directData, directData.Length, client2Endpoint);
				Console.WriteLine($"   Клиент #1 → Клиент #2: '{directMessage}'");

				// Клиент 2 принимает сообщение
				Task.Run(async () =>
				{
					try
					{
						var result = await _client2.ReceiveAsync();
						string received = Encoding.UTF8.GetString(result.Buffer);
						Console.WriteLine($"   Клиент #2 получил: '{received}' от {result.RemoteEndPoint}");
					}
					catch { }
				});

				Thread.Sleep(500);

				Console.WriteLine("\n5. ДЕМОНСТРАЦИЯ ОТСУТСТВИЯ ГАРАНТИЙ В UDP UNICAST:");

				// Отправка нескольких сообщений для демонстрации возможных потерь
				Console.WriteLine($"   Отправка 10 быстрых сообщений...");
				for (int i = 1; i <= 10; i++)
				{
					string fastMessage = $"Быстрое сообщение #{i}";
					byte[] fastData = Encoding.UTF8.GetBytes(fastMessage);
					_client1.Send(fastData, fastData.Length, serverEndpoint);

					// Минимальная задержка для имитации быстрой отправки
					Thread.Sleep(5);
				}

				Thread.Sleep(300);

				Console.WriteLine("\n   UDP unicast демонстрация завершена");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"   Ошибка: {ex.Message}");
			}
		}

		private async void ServerReceiveLoop()
		{
			Console.WriteLine($"   [Сервер] Начало приёма unicast-пакетов...");

			while (_isRunning && _server != null)
			{
				try
				{
					var result = await _server.ReceiveAsync();
					string message = Encoding.UTF8.GetString(result.Buffer);

					Console.WriteLine($"   [Сервер] Получен unicast-пакет от {result.RemoteEndPoint}: '{message}'");

					// Сохраняем информацию об отправителе для возможного ответа
					// В реальном приложении здесь могла бы быть логика маршрутизации
				}
				catch (ObjectDisposedException)
				{
					break;
				}
				catch (Exception ex)
				{
					Console.WriteLine($"   [Сервер] Ошибка приёма: {ex.Message}");
					Thread.Sleep(100);
				}
			}

			Console.WriteLine($"   [Сервер] Приём unicast-пакетов завершён");
		}

		public void Dispose()
		{
			_isRunning = false;

			_server?.Close();
			_client1?.Close();
			_client2?.Close();

			Console.WriteLine($"\n   Ресурсы UDP unicast освобождены");
		}
	}

	// Демонстрация безопасности в unicast
	public class SecureUnicastDemonstration
	{
		public void DemonstrateSecurityAspects()
		{
			Console.WriteLine("\n\n=== БЕЗОПАСНОСТЬ В UNICAST ===");
			Console.WriteLine("Контроль отправителя и получателя в одноадресной передаче\n");

			Console.WriteLine("1. ИДЕНТИФИКАЦИЯ УЧАСТНИКОВ:");
			Console.WriteLine($"   • Отправитель всегда знает точный адрес получателя");
			Console.WriteLine($"   • Получатель всегда знает точный адрес отправителя");
			Console.WriteLine($"   • Нет неопределённости в маршрутизации");

			Console.WriteLine("\n2. ВОЗМОЖНОСТИ АУТЕНТИФИКАЦИИ:");

			// Пример простой аутентификации в unicast
			Console.WriteLine($"   Пример кода аутентификации:");
			Console.WriteLine($"   ```csharp");
			Console.WriteLine($"   // Сервер проверяет клиента");
			Console.WriteLine($"   bool AuthenticateClient(IPEndPoint clientEndpoint)");
			Console.WriteLine($"   {{");
			Console.WriteLine($"       // Проверка по белому списку");
			Console.WriteLine($"       var allowedClients = new[] {{ \"192.168.1.100\", \"10.0.0.5\" }};");
			Console.WriteLine($"       return allowedClients.Contains(clientEndpoint.Address.ToString());");
			Console.WriteLine($"   }}");
			Console.WriteLine($"   ```");

			Console.WriteLine("\n3. ШИФРОВАНИЕ ДАННЫХ:");
			Console.WriteLine($"   • TLS/SSL поверх TCP для защищённого канала");
			Console.WriteLine($"   • Диффи-Хеллман для обмена ключами");
			Console.WriteLine($"   • Индивидуальные ключи для каждой пары участников");

			Console.WriteLine("\n4. АУДИТ И ЛОГИРОВАНИЕ:");
			Console.WriteLine($"   • Каждое взаимодействие можно залогировать");
			Console.WriteLine($"   • Легко отслеживать кто, кому и что отправил");
			Console.WriteLine($"   • Простая трассировка цепочки событий");

			// Демонстрация валидации отправителя
			Console.WriteLine("\n5. ПРАКТИЧЕСКАЯ ДЕМОНСТРАЦИЯ ВАЛИДАЦИИ:");

			var trustedClients = new List<IPAddress>
			{
				IPAddress.Parse("127.0.0.1"),
				IPAddress.Parse("192.168.1.100")
			};

			// Тестовые конечные точки
			var trustedEndpoint = new IPEndPoint(IPAddress.Loopback, 12345);
			var untrustedEndpoint = new IPEndPoint(IPAddress.Parse("192.168.1.200"), 12345);

			bool isTrusted1 = trustedClients.Contains(trustedEndpoint.Address);
			bool isTrusted2 = trustedClients.Contains(untrustedEndpoint.Address);

			Console.WriteLine($"   Проверка {trustedEndpoint.Address}: {(isTrusted1 ? "ДОВЕРЕННЫЙ" : "НЕДОВЕРЕННЫЙ")}");
			Console.WriteLine($"   Проверка {untrustedEndpoint.Address}: {(isTrusted2 ? "ДОВЕРЕННЫЙ" : "НЕДОВЕРЕННЫЙ")}");

			Console.WriteLine("\n6. ОГРАНИЧЕНИЯ ДОСТУПА:");
			Console.WriteLine($"   • Ролевая модель доступа (RBAC)");
			Console.WriteLine($"   • Квоты на количество соединений");
			Console.WriteLine($"   • Лимиты скорости передачи (rate limiting)");

			Console.WriteLine("\n   Unicast обеспечивает основу для построения безопасных систем");
		}
	}

	// Демонстрация массовой рассылки через unicast (неоптимальный способ)
	public class UnicastMassDistribution
	{
		public void DemonstrateMassUnicast()
		{
			Console.WriteLine("\n\n=== МАССОВАЯ РАССЫЛКА ЧЕРЕЗ UNICAST ===");
			Console.WriteLine("Почему unicast неэффективен для широковещательных сценариев\n");

			// Симуляция списка клиентов
			var clientEndpoints = new List<IPEndPoint>
			{
				new IPEndPoint(IPAddress.Parse("192.168.1.10"), 10001),
				new IPEndPoint(IPAddress.Parse("192.168.1.11"), 10002),
				new IPEndPoint(IPAddress.Parse("192.168.1.12"), 10003),
				new IPEndPoint(IPAddress.Parse("192.168.1.13"), 10004),
				new IPEndPoint(IPAddress.Parse("192.168.1.14"), 10005)
			};

			string broadcastMessage = "Важное сообщение для всех клиентов";
			byte[] messageData = Encoding.UTF8.GetBytes(broadcastMessage);

			Console.WriteLine("1. РАССЫЛКА ОДИНАКОВОГО СООБЩЕНИЯ:");
			Console.WriteLine($"   Сообщение: '{broadcastMessage}'");
			Console.WriteLine($"   Получателей: {clientEndpoints.Count}");
			Console.WriteLine($"   Размер сообщения: {messageData.Length} байт");

			Console.WriteLine("\n2. UNICAST ПОДХОД (НЕЭФФЕКТИВНЫЙ):");

			using (var udpClient = new UdpClient())
			{
				int totalBytesSent = 0;
				Stopwatch sw = Stopwatch.StartNew();

				foreach (var endpoint in clientEndpoints)
				{
					// Каждому клиенту - отдельная отправка
					int bytesSent = udpClient.Send(messageData, messageData.Length, endpoint);
					totalBytesSent += bytesSent;

					Console.WriteLine($"   Отправка {endpoint}: {bytesSent} байт");

					// Имитация сетевой задержки
					Thread.Sleep(10);
				}

				sw.Stop();

				Console.WriteLine($"\n   ИТОГИ UNICAST РАССЫЛКИ:");
				Console.WriteLine($"     Всего отправок: {clientEndpoints.Count}");
				Console.WriteLine($"     Всего байт отправлено: {totalBytesSent}");
				Console.WriteLine($"     Избыточных данных: {totalBytesSent - messageData.Length} байт");
				Console.WriteLine($"     Время выполнения: {sw.ElapsedMilliseconds} мс");
				Console.WriteLine($"     Среднее время на клиента: {sw.ElapsedMilliseconds / (double)clientEndpoints.Count:F1} мс");
			}

			Console.WriteLine("\n3. ПРОБЛЕМЫ UNICAST ДЛЯ РАССЫЛКИ:");
			Console.WriteLine($"   • Множественные копии одинаковых данных");
			Console.WriteLine($"   • Высокая нагрузка на сервер");
			Console.WriteLine($"   • Повторная передача по одному маршруту");
			Console.WriteLine($"   • Сложность синхронизации доставки");

			Console.WriteLine("\n4. КОГДА UNICAST ПОДХОДИТ ДЛЯ РАССЫЛКИ:");
			Console.WriteLine($"   • Малое количество получателей (< 10)");
			Console.WriteLine($"   • Индивидуализированные сообщения");
			Console.WriteLine($"   • Требуется подтверждение доставки");
			Console.WriteLine($"   • Важна безопасность каждого канала");

			Console.WriteLine("\n   Вывод: unicast оптимален для точечного взаимодействия,");
			Console.WriteLine("          но неэффективен для массовых одинаковых рассылок");
		}
	}

	// Главная программа
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("UNICAST ВЗАИМОДЕЙСТВИЕ В C#");
			Console.WriteLine("===========================\n");

			// Демонстрация unicast через TCP
			using (var tcpDemo = new TcpUnicastDemonstration())
			{
				tcpDemo.DemonstrateTcpUnicast();
			}

			// Демонстрация unicast через UDP
			using (var udpDemo = new UdpUnicastDemonstration())
			{
				udpDemo.DemonstrateUdpUnicast();
			}

			// Демонстрация аспектов безопасности
			var secureDemo = new SecureUnicastDemonstration();
			secureDemo.DemonstrateSecurityAspects();

			// Демонстрация проблем массовой рассылки через unicast
			var massDemo = new UnicastMassDistribution();
			massDemo.DemonstrateMassUnicast();
		}
	}
}
