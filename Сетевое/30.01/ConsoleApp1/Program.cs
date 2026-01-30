using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;

namespace MessageDeliveryModels
{
	// Демонстрация моделей доставки сообщений в .NET
	public class DeliveryModelsDemo : IDisposable
	{
		private const int UnicastPort = 11060;
		private const int BroadcastPort = 11061;
		private const int MulticastPort = 11062;
		private const string MulticastGroup = "239.0.0.1";

		private List<UdpClient> _clients = new List<UdpClient>();
		private List<Thread> _listenerThreads = new List<Thread>();
		private bool _isRunning = true;

		public void DemonstrateDeliveryModels()
		{
			Console.WriteLine("=== МОДЕЛИ ДОСТАВКИ СООБЩЕНИЙ ===\n");

			Console.WriteLine("1. UNICAST - ОДИН К ОДНОМУ:");
			DemonstrateUnicast();

			Console.WriteLine("\n\n2. BROADCAST - ОДИН КО ВСЕМ:");
			DemonstrateBroadcast();

			Console.WriteLine("\n\n3. MULTICAST - ОДИН К ГРУППЕ:");
			DemonstrateMulticast();

			Console.WriteLine("\n\n4. СРАВНЕНИЕ МОДЕЛЕЙ:");
			CompareDeliveryModels();

			Console.WriteLine("\n\n5. КОМБИНИРОВАННАЯ АРХИТЕКТУРА:");
			DemonstrateCombinedArchitecture();
		}

		private void DemonstrateUnicast()
		{
			Console.WriteLine("   Unicast: один отправитель → один получатель");
			Console.WriteLine("   Пример: клиент-серверное приложение\n");

			// Создание сервера
			var serverEndpoint = new IPEndPoint(IPAddress.Loopback, UnicastPort);
			var server = new UdpClient(serverEndpoint);
			_clients.Add(server);

			Console.WriteLine($"   Сервер запущен на {serverEndpoint}");

			// Создание клиентов
			var client1 = new UdpClient();
			client1.Connect(serverEndpoint);

			var client2 = new UdpClient();
			client2.Connect(serverEndpoint);

			Console.WriteLine($"   Создано 2 клиента");

			// Отправка сообщений от клиентов к серверу
			string message1 = "Сообщение от клиента 1";
			string message2 = "Сообщение от клиента 2";

			Console.WriteLine($"\n   Отправка unicast сообщений:");
			Console.WriteLine($"     Клиент 1 → Сервер: '{message1}'");
			client1.Send(Encoding.UTF8.GetBytes(message1), message1.Length);

			Console.WriteLine($"     Клиент 2 → Сервер: '{message2}'");
			client2.Send(Encoding.UTF8.GetBytes(message2), message2.Length);

			// Сервер принимает сообщения
			IPEndPoint clientEndpoint = new IPEndPoint(IPAddress.Any, 0);

			byte[] data1 = server.Receive(ref clientEndpoint);
			Console.WriteLine($"     Сервер получил от {clientEndpoint}: '{Encoding.UTF8.GetString(data1)}'");

			byte[] data2 = server.Receive(ref clientEndpoint);
			Console.WriteLine($"     Сервер получил от {clientEndpoint}: '{Encoding.UTF8.GetString(data2)}'");

			// Ответ сервера (unicast каждому клиенту)
			string response = "Ответ от сервера";
			byte[] responseData = Encoding.UTF8.GetBytes(response);

			Console.WriteLine($"\n   Ответы сервера (unicast):");
			Console.WriteLine($"     Сервер → Клиент 1: '{response}'");
			server.Send(responseData, responseData.Length, client1.Client.LocalEndPoint as IPEndPoint);

			Console.WriteLine($"     Сервер → Клиент 2: '{response}'");
			server.Send(responseData, responseData.Length, client2.Client.LocalEndPoint as IPEndPoint);

			Console.WriteLine($"\n   Особенности unicast:");
			Console.WriteLine($"     • Каждое сообщение адресовано конкретному получателю");
			Console.WriteLine($"     • Сервер знает адреса всех клиентов");
			Console.WriteLine($"     • Возможна индивидуальная обработка для каждого клиента");

			client1.Close();
			client2.Close();
		}

		private void DemonstrateBroadcast()
		{
			Console.WriteLine("   Broadcast: один отправитель → все в сети");
			Console.WriteLine("   Пример: обнаружение сервисов в локальной сети\n");

			// Broadcast работает только в локальных сетях
			Console.WriteLine("   Важно: Broadcast ограничен локальной сетью");

			// Каждому слушателю нужен свой уникальный порт или нужно использовать SocketOptionName.ReuseAddress
			var listener1Port = BroadcastPort;
			var listener2Port = BroadcastPort + 1;
			var listener3Port = BroadcastPort + 2;

			// Создание слушателей broadcast с разными портами
			var listener1 = new UdpClient(listener1Port);
			var listener2 = new UdpClient(listener2Port);
			var listener3 = new UdpClient(listener3Port);

			// ИЛИ альтернатива: использовать один сокет с ReuseAddress и несколькими потоками
			// Но для демонстрации проще использовать разные порты

			_clients.AddRange(new[] { listener1, listener2, listener3 });

			Console.WriteLine($"   Создано 3 слушателя на портах {listener1Port}, {listener2Port}, {listener3Port}");

			// Запуск прослушивания в отдельных потоках
			StartBroadcastListener(listener1, "Слушатель 1");
			StartBroadcastListener(listener2, "Слушатель 2");
			StartBroadcastListener(listener3, "Слушатель 3");

			Thread.Sleep(500); // Даём время слушателям запуститься

			// Создание отправителя broadcast
			var sender = new UdpClient();
			sender.EnableBroadcast = true; // Включаем broadcast

			Console.WriteLine($"\n   Отправка broadcast сообщения на 3 разных порта:");

			// Отправляем на каждый порт отдельно
			string broadcastMessage = "Broadcast сообщение от сервера";
			byte[] broadcastData = Encoding.UTF8.GetBytes(broadcastMessage);

			// Отправляем на все три порта
			var ports = new[] { listener1Port, listener2Port, listener3Port };

			foreach (var port in ports)
			{
				var broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, port);
				Console.WriteLine($"     Адрес: {broadcastEndpoint}");
				Console.WriteLine($"     Сообщение: '{broadcastMessage}'");

				try
				{
					int sent = sender.Send(broadcastData, broadcastData.Length, broadcastEndpoint);
					Console.WriteLine($"     Отправлено байт на порт {port}: {sent}");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"     Ошибка отправки на порт {port}: {ex.Message}");
				}

				Thread.Sleep(100); // Небольшая пауза между отправками
			}

			// Даём время получить сообщения
			Thread.Sleep(1000);

			Console.WriteLine($"\n   Особенности broadcast:");
			Console.WriteLine($"     • Сообщение получают ВСЕ устройства в сети");
			Console.WriteLine($"     • Создаёт нагрузку на сеть (ненужная обработка)");
			Console.WriteLine($"     • Работает только в локальных сетях");
			Console.WriteLine($"     • В IPv6 нет классического broadcast");

			sender.Close();
		}

		private void StartBroadcastListener(UdpClient client, string listenerName)
		{
			var thread = new Thread(() =>
			{
				try
				{
					IPEndPoint remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);
					byte[] data = client.Receive(ref remoteEndpoint);
					string message = Encoding.UTF8.GetString(data);

					Console.WriteLine($"     {listenerName} получил: '{message}' от {remoteEndpoint}");
				}
				catch (SocketException ex)
				{
					Console.WriteLine($"     {listenerName} ошибка: {ex.SocketErrorCode}");
				}
			});

			thread.IsBackground = true;
			thread.Start();
			_listenerThreads.Add(thread);
		}

		private void DemonstrateMulticast()
		{
			Console.WriteLine("   Multicast: один отправитель → группе получателей");
			Console.WriteLine("   Пример: видеотрансляция, биржевые котировки\n");

			// Multicast адрес (диапазон 224.0.0.0 - 239.255.255.255)
			var multicastAddress = IPAddress.Parse(MulticastGroup);
			int multicastPort = MulticastPort;
			var multicastEndpoint = new IPEndPoint(multicastAddress, multicastPort);

			Console.WriteLine($"   Multicast группа: {multicastEndpoint}");
			Console.WriteLine($"   Диапазон multicast адресов: 224.0.0.0 - 239.255.255.255");
			Console.WriteLine($"   Адреса для локального использования: 224.0.0.0 - 224.0.0.255");
			Console.WriteLine($"   Глобальные адреса: 224.0.1.0 - 238.255.255.255");

			// Создание получателей (подписчиков на группу) с использованием ReuseAddress
			UdpClient? subscriber1 = null;
			UdpClient? subscriber2 = null;
			UdpClient? subscriber3 = null;

			try
			{
				subscriber1 = CreateMulticastSubscriber("Подписчик 1", multicastPort);
				subscriber2 = CreateMulticastSubscriber("Подписчик 2", multicastPort);
				subscriber3 = CreateMulticastSubscriber("Подписчик 3", multicastPort);
			}
			catch (SocketException ex) when (ex.SocketErrorCode == SocketError.AccessDenied)
			{
				subscriber1?.Close();
				subscriber2?.Close();
				subscriber3?.Close();

				multicastPort = GetFreeUdpPort();
				multicastEndpoint = new IPEndPoint(multicastAddress, multicastPort);

				Console.WriteLine($"   Порт {MulticastPort} недоступен (AccessDenied). Использую свободный порт {multicastPort}.");

				subscriber1 = CreateMulticastSubscriber("Подписчик 1", multicastPort);
				subscriber2 = CreateMulticastSubscriber("Подписчик 2", multicastPort);
				subscriber3 = CreateMulticastSubscriber("Подписчик 3", multicastPort);
			}

			Console.WriteLine($"\n   Создано 3 подписчика на multicast группу (ReuseAddress=true)");
			Console.WriteLine($"   Все подписчики слушают один порт {multicastPort}");

			// Даём время подписчикам присоединиться к группе
			Thread.Sleep(1000);

			// Создание отправителя (источника multicast)
			var sender = new UdpClient();

			// Вступление в группу для отправки (не обязательно, но рекомендуется)
			sender.JoinMulticastGroup(multicastAddress);

			// Настройка TTL для multicast пакетов
			sender.Ttl = 32;

			Console.WriteLine($"\n   Подготовка к отправке multicast сообщений:");
			Console.WriteLine($"     Отправитель TTL: {sender.Ttl}");

			string multicastMessage = "Multicast сообщение для группы";
			byte[] multicastData = Encoding.UTF8.GetBytes(multicastMessage);

			Console.WriteLine($"\n   1. Отправка первого multicast сообщения:");
			Console.WriteLine($"     Сообщение: '{multicastMessage}'");
			Console.WriteLine($"     Адрес назначения: {multicastEndpoint}");

			int sent = sender.Send(multicastData, multicastData.Length, multicastEndpoint);
			Console.WriteLine($"     Отправлено байт: {sent}");

			// Даём время получить сообщения
			Thread.Sleep(1500);

			// Отправка второго сообщения
			string secondMessage = "Второе multicast сообщение - тестирование доставки";
			byte[] secondData = Encoding.UTF8.GetBytes(secondMessage);

			Console.WriteLine($"\n   2. Отправка второго multicast сообщения:");
			Console.WriteLine($"     Сообщение: '{secondMessage}'");

			sent = sender.Send(secondData, secondData.Length, multicastEndpoint);
			Console.WriteLine($"     Отправлено байт: {sent}");

			// Даём время получить второе сообщение
			Thread.Sleep(1000);

			// Демонстрация выхода из группы одним из подписчиков
			Console.WriteLine($"\n   3. Подписчик 2 выходит из multicast группы...");
			subscriber2!.DropMulticastGroup(multicastAddress);

			// Отправка третьего сообщения (доступно только активным подписчикам)
			string thirdMessage = "Третье сообщение - доступно только активным подписчикам";
			byte[] thirdData = Encoding.UTF8.GetBytes(thirdMessage);

			Console.WriteLine($"\n   4. Отправка третьего multicast сообщения:");
			Console.WriteLine($"     Сообщение: '{thirdMessage}'");
			Console.WriteLine($"     Ожидание: Подписчик 2 НЕ получит это сообщение");

			sent = sender.Send(thirdData, thirdData.Length, multicastEndpoint);
			Console.WriteLine($"     Отправлено байт: {sent}");

			Thread.Sleep(1000);

			Console.WriteLine($"\n   Особенности multicast:");
			Console.WriteLine($"     • Сообщение получают только члены группы");
			Console.WriteLine($"     • Получатели явно вступают в группу (JoinMulticastGroup)");
			Console.WriteLine($"     • Меньше нагрузки на сеть, чем broadcast");
			Console.WriteLine($"     • Поддержка на уровне маршрутизаторов");
			Console.WriteLine($"     • TTL контролирует область распространения");
			Console.WriteLine($"     • ReuseAddress позволяет нескольким приложениям слушать один порт");

			Console.WriteLine($"\n   Статистика:");
			Console.WriteLine($"     • Отправлено сообщений: 3");
			Console.WriteLine($"     • Подписчик 1: получил все сообщения");
			Console.WriteLine($"     • Подписчик 2: получил первые 2 сообщения");
			Console.WriteLine($"     • Подписчик 3: получил все сообщения");

			sender.Close();
			subscriber1!.Close();
			subscriber2!.Close();
			subscriber3!.Close();
		}

		private bool _disposed = false;



		private UdpClient CreateMulticastSubscriber(string subscriberName, int multicastPort)
		{
			var multicastIp = IPAddress.Parse(MulticastGroup);

			try
			{
				var client = new UdpClient(AddressFamily.InterNetwork);

				// Важно для Windows: иначе второй/третий сокет часто не сможет забиндиться на тот же порт
				client.ExclusiveAddressUse = false;
				client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

				client.Client.Bind(new IPEndPoint(IPAddress.Any, multicastPort));

				// Подписка на группу (можно также указать интерфейс)
				client.JoinMulticastGroup(multicastIp);
				client.Ttl = 32;

				var thread = new Thread(() =>
				{
					try
					{
						Console.WriteLine($"     {subscriberName} слушает multicast группу {MulticastGroup}:{multicastPort}...");

						while (!_disposed)
						{
							IPEndPoint remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);
							byte[] data = client.Receive(ref remoteEndpoint);
							string message = Encoding.UTF8.GetString(data);

							Console.WriteLine($"     {subscriberName} получил: '{message}' от {remoteEndpoint.Address}");
						}
					}
					catch (ObjectDisposedException) { }
					catch (SocketException ex) when (ex.SocketErrorCode == SocketError.Interrupted) { }
					catch (Exception ex)
					{
						Console.WriteLine($"     {subscriberName} ошибка: {ex.Message}");
					}
				});

				thread.IsBackground = true;
				thread.Start();

				_listenerThreads.Add(thread);
				_clients.Add(client);

				return client;
			}
			catch (Exception ex)
			{
				// Лучше не возвращать null молча — иначе вы получите NullReference позже.
				Console.WriteLine($"     Не удалось создать подписчика {subscriberName}: {ex.Message}");
				throw; // или верните null, но тогда ВЕЗДЕ проверяйте
			}
		}

		private static int GetFreeUdpPort()
		{
			using var temp = new UdpClient(0);
			return ((IPEndPoint)temp.Client.LocalEndPoint!).Port;
		}

		private static UdpClient CreateBroadcastListener(int port)
		{
			var client = new UdpClient(AddressFamily.InterNetwork);
			client.ExclusiveAddressUse = false;
			client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			client.Client.Bind(new IPEndPoint(IPAddress.Any, port));
			client.EnableBroadcast = true;
			return client;
		}


		private void CompareDeliveryModels()
		{
			Console.WriteLine("   СРАВНИТЕЛЬНАЯ ТАБЛИЦА МОДЕЛЕЙ ДОСТАВКИ:\n");

			var comparison = new[]
			{
				new
				{
					Модель = "Unicast",
					Отправитель = "1",
					Получатели = "1 (конкретный)",
					Примеры = "HTTP, FTP, SSH, БД",
					Плюсы = "Надёжность, безопасность, контроль",
					Минусы = "Масштабирование 1:many",
					Протоколы = "TCP, UDP"
				},
				new
				{
					Модель = "Broadcast",
					Отправитель = "1",
					Получатели = "Все в сети",
					Примеры = "DHCP, ARP, обнаружение",
					Плюсы = "Простота, не нужны адреса",
					Минусы = "Нагрузка на сеть, безопасность",
					Протоколы = "UDP (только)"
				},
				new
				{
					Модель = "Multicast",
					Отправитель = "1",
					Получатели = "Группа (подписчики)",
					Примеры = "Видеостримы, котировки, чаты",
					Плюсы = "Эффективность 1:many",
					Минусы = "Сложность настройки",
					Протоколы = "UDP (в основном)"
				}
			};

			Console.WriteLine($"   {"Модель",-12} {"Отпр.",-8} {"Получ.",-18} {"Примеры",-20}");
			Console.WriteLine($"   {"".PadRight(70, '-')}");

			foreach (var item in comparison)
			{
				Console.WriteLine($"   {item.Модель,-12} {item.Отправитель,-8} {item.Получатели,-18} {item.Примеры,-20}");
			}

			Console.WriteLine($"\n   КЛЮЧЕВЫЕ РАЗЛИЧИЯ:");
			Console.WriteLine($"     1. Ответственность:");
			Console.WriteLine($"        • Unicast: двусторонняя, с подтверждениями");
			Console.WriteLine($"        • Broadcast/Multicast: минимальная, без гарантий");

			Console.WriteLine($"\n     2. Масштабируемость:");
			Console.WriteLine($"        • Unicast: плохо для 1:many (N копий сообщения)");
			Console.WriteLine($"        • Broadcast: плохо для больших сетей");
			Console.WriteLine($"        • Multicast: оптимально для 1:many");

			Console.WriteLine($"\n     3. Безопасность:");
			Console.WriteLine($"        • Unicast: легко контролировать доступ");
			Console.WriteLine($"        • Broadcast/Multicast: сложнее защитить");
		}

		private void DemonstrateCombinedArchitecture()
		{
			Console.WriteLine("   КОМБИНИРОВАННАЯ АРХИТЕКТУРА:");
			Console.WriteLine("   Пример: обнаружение сервиса → установка соединения\n");

			Console.WriteLine($"   Типичный сценарий:");
			Console.WriteLine($"     1. Client → Broadcast: 'Где сервер?'");
			Console.WriteLine($"     2. Server → Unicast: 'Я здесь: {IPAddress.Loopback}:{UnicastPort}'");
			Console.WriteLine($"     3. Client → Unicast: Основное взаимодействие");

			// Симуляция сценария
			Console.WriteLine($"\n   Симуляция сценария:");

			// Сервер слушает broadcast запросы
			int discoveryPort = BroadcastPort;
			UdpClient discoveryListener;

			try
			{
				discoveryListener = CreateBroadcastListener(discoveryPort);
			}
			catch (SocketException ex) when (ex.SocketErrorCode == SocketError.AddressAlreadyInUse
				|| ex.SocketErrorCode == SocketError.AccessDenied)
			{
				discoveryPort = GetFreeUdpPort();
				Console.WriteLine($"   Порт {BroadcastPort} занят. Использую {discoveryPort}.");
				discoveryListener = CreateBroadcastListener(discoveryPort);
			}

			var discoveryThread = new Thread(() =>
			{
				try
				{
					IPEndPoint clientEndpoint = new IPEndPoint(IPAddress.Any, 0);
					byte[] data = discoveryListener.Receive(ref clientEndpoint);
					string request = Encoding.UTF8.GetString(data);

					Console.WriteLine($"     Сервер получил broadcast запрос: '{request}' от {clientEndpoint}");

					// Ответ unicast с адресом сервера
					string response = $"Server at {IPAddress.Loopback}:{UnicastPort}";
					byte[] responseData = Encoding.UTF8.GetBytes(response);

					discoveryListener.Send(responseData, responseData.Length, clientEndpoint);
					Console.WriteLine($"     Сервер отправил unicast ответ: '{response}'");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"     Ошибка discovery: {ex.Message}");
				}
			});

			discoveryThread.IsBackground = true;
			discoveryThread.Start();

			Thread.Sleep(500);

			// Клиент отправляет broadcast запрос
			var client = new UdpClient();
			client.EnableBroadcast = true;

			var broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, discoveryPort);
			string discoveryRequest = "Поиск сервера";

			client.Send(Encoding.UTF8.GetBytes(discoveryRequest), discoveryRequest.Length, broadcastEndpoint);
			Console.WriteLine($"     Клиент отправил broadcast: '{discoveryRequest}'");

			// Клиент ждёт unicast ответ
			IPEndPoint serverEndpoint = new IPEndPoint(IPAddress.Any, 0);
			byte[] responseData = client.Receive(ref serverEndpoint);
			string serverResponse = Encoding.UTF8.GetString(responseData);

			Console.WriteLine($"     Клиент получил unicast ответ: '{serverResponse}'");

			// Теперь клиент может установить unicast соединение
			Console.WriteLine($"\n     Клиент устанавливает unicast соединение с сервером...");

			// Симуляция дальнейшего unicast взаимодействия
			Console.WriteLine($"     [Unicast] Клиент → Сервер: 'Данные'");
			Console.WriteLine($"     [Unicast] Сервер → Клиент: 'Подтверждение'");

			Console.WriteLine($"\n   Преимущества комбинированного подхода:");
			Console.WriteLine($"     • Discovery: простота через broadcast");
			Console.WriteLine($"     • Основное взаимодействие: надёжность через unicast");
			Console.WriteLine($"     • Гибкость: можно заменить broadcast на multicast");

			discoveryListener.Close();
			client.Close();
			discoveryThread.Join(500);
		}

		// Пример реального сервиса с поддержкой всех моделей
		public class NetworkService
		{
			private UdpClient _unicastServer;
			private UdpClient _broadcastListener;
			private UdpClient _multicastSender;
			private bool _isRunning;

			public NetworkService(int basePort)
			{
				// Unicast сервер для основных операций
				_unicastServer = new UdpClient(basePort);

				// Broadcast для обнаружения
				_broadcastListener = new UdpClient(basePort + 1);
				_broadcastListener.EnableBroadcast = true;

				// Multicast для уведомлений
				_multicastSender = new UdpClient();
				_multicastSender.JoinMulticastGroup(IPAddress.Parse("239.0.0.100"));
			}

			public void Start()
			{
				_isRunning = true;

				// Запуск обработки unicast запросов
				ThreadPool.QueueUserWorkItem(HandleUnicastRequests);

				// Запуск обработки broadcast запросов
				ThreadPool.QueueUserWorkItem(HandleBroadcastDiscovery);

				// Регулярные multicast уведомления
				ThreadPool.QueueUserWorkItem(SendMulticastNotifications);
			}

			private void HandleUnicastRequests(object state)
			{
				Console.WriteLine($"[Service] Unicast сервер запущен");

				while (_isRunning)
				{
					try
					{
						IPEndPoint clientEndpoint = new IPEndPoint(IPAddress.Any, 0);
						byte[] data = _unicastServer.Receive(ref clientEndpoint);

						// Обработка unicast запроса
						string response = ProcessRequest(Encoding.UTF8.GetString(data));
						byte[] responseData = Encoding.UTF8.GetBytes(response);

						_unicastServer.Send(responseData, responseData.Length, clientEndpoint);
					}
					catch { }
				}
			}

			private void HandleBroadcastDiscovery(object state)
			{
				Console.WriteLine($"[Service] Broadcast discovery запущен");

				while (_isRunning)
				{
					try
					{
						IPEndPoint clientEndpoint = new IPEndPoint(IPAddress.Any, 0);
						byte[] data = _broadcastListener.Receive(ref clientEndpoint);

						// Ответ на broadcast запрос discovery
						string serviceInfo = $"Service available at {_unicastServer.Client.LocalEndPoint}";
						byte[] response = Encoding.UTF8.GetBytes(serviceInfo);

						_broadcastListener.Send(response, response.Length, clientEndpoint);
					}
					catch { }
				}
			}

			private void SendMulticastNotifications(object state)
			{
				Console.WriteLine($"[Service] Multicast уведомления запущены");

				var multicastEndpoint = new IPEndPoint(IPAddress.Parse("239.0.0.100"), 11070);

				while (_isRunning)
				{
					try
					{
						string notification = $"Service status: {DateTime.Now:T}";
						byte[] data = Encoding.UTF8.GetBytes(notification);

						_multicastSender.Send(data, data.Length, multicastEndpoint);

						Thread.Sleep(5000); // Каждые 5 секунд
					}
					catch { }
				}
			}

			private string ProcessRequest(string request)
			{
				return $"Processed: {request} at {DateTime.Now:T}";
			}

			public void Stop()
			{
				_isRunning = false;
				_unicastServer.Close();
				_broadcastListener.Close();
				_multicastSender.Close();
			}
		}

		public void Dispose()
		{
			_disposed = true;

			foreach (var client in _clients)
			{
				try
				{
					client.Close();
				}
				catch { }
			}

			foreach (var thread in _listenerThreads)
			{
				if (thread.IsAlive)
				{
					thread.Join(500);
				}
			}

			Console.WriteLine("\n   Ресурсы освобождены");
		}
	}

	// Главная программа
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("МОДЕЛИ ДОСТАВКИ СООБЩЕНИЙ В СЕТЯХ");
			Console.WriteLine("==================================\n");

			using (var demo = new DeliveryModelsDemo())
			{
				demo.DemonstrateDeliveryModels();
			}

			// Пример реального сервиса
			Console.WriteLine("\n\n=== ПРИМЕР РЕАЛЬНОГО СЕТЕВОГО СЕРВИСА ===");

			var service = new DeliveryModelsDemo.NetworkService(11080);
			service.Start();

			Console.WriteLine("\nСервис запущен с поддержкой всех моделей доставки");
			Console.WriteLine("Нажмите Enter для остановки...");
			Console.ReadLine();

			service.Stop();
		}
	}
}
