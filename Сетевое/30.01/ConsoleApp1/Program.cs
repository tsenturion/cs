using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;

namespace UdpBroadcastDemo
{
	// Демонстрация широковещательной передачи данных
	public class UdpBroadcastDemonstration : IDisposable
	{
		private UdpClient _broadcastSender;
		private UdpClient _broadcastReceiver;
		private Thread _receiverThread;
		private bool _isRunning;
		private int _port;
		private int _receivedMessages = 0;

		public UdpBroadcastDemonstration(int port = 11060)
		{
			_port = port;
		}

		public async Task DemonstrateBroadcast()
		{
			Console.WriteLine("=== UDP BROADCAST: ШИРОКОВЕЩАТЕЛЬНАЯ ПЕРЕДАЧА ===\n");

			// Часть 1: Основные концепции broadcast
			Console.WriteLine("1. ОСНОВНЫЕ КОНЦЕПЦИИ BROADCAST:");
			ExplainBroadcastConcepts();

			// Часть 2: Техническая реализация broadcast адресации
			Console.WriteLine("\n2. BROADCAST АДРЕСАЦИЯ:");
			DemonstrateBroadcastAddressing();

			// Часть 3: Service Discovery - основной сценарий использования
			Console.WriteLine("\n3. SERVICE DISCOVERY:");
			await DemonstrateServiceDiscovery();

			// Часть 4: Ограничения и проблемы broadcast
			Console.WriteLine("\n4. ОГРАНИЧЕНИЯ И ПРОБЛЕМЫ:");
			DemonstrateBroadcastLimitations();

			// Часть 5: Безопасность broadcast
			Console.WriteLine("\n5. БЕЗОПАСНОСТЬ:");
			DemonstrateBroadcastSecurity();

			Console.WriteLine("\nДемонстрация завершена");
		}

		private void ExplainBroadcastConcepts()
		{
			Console.WriteLine("   Broadcast - отправка сообщения ВСЕМ узлам в сегменте сети");
			Console.WriteLine("   Ключевые особенности:");
			Console.WriteLine("     • Ограничен локальной сетью (не проходит через маршрутизаторы)");
			Console.WriteLine("     • Работает только поверх UDP (TCP не поддерживает broadcast)");
			Console.WriteLine("     • Ненадёжная доставка (нет подтверждений)");
			Console.WriteLine("     • Пакет получают ВСЕ устройства в сети");
			Console.WriteLine("     • Отправитель не знает конкретных получателей");

			Console.WriteLine("\n   Сравнение моделей доставки:");
			Console.WriteLine("     Unicast:   один отправитель → один получатель");
			Console.WriteLine("     Broadcast: один отправитель → все в сети");
			Console.WriteLine("     Multicast: один отправитель → группа получателей");
		}

		private void DemonstrateBroadcastAddressing()
		{
			Console.WriteLine("   Специальные broadcast адреса IPv4:");

			// 1. Ограниченный broadcast (255.255.255.255)
			Console.WriteLine("\n   1. Ограниченный broadcast:");
			Console.WriteLine($"     Адрес: 255.255.255.255");
			Console.WriteLine($"     Достигает всех узлов в ЛОКАЛЬНОМ сегменте сети");
			Console.WriteLine($"     Не проходит через маршрутизаторы");

			// 2. Направленный broadcast (адрес сети + все биты хоста)
			Console.WriteLine("\n   2. Направленный broadcast:");

			try
			{
				// Получаем локальные IP адреса
				string hostName = Dns.GetHostName();
				IPAddress[] addresses = Dns.GetHostAddresses(hostName);

				foreach (var addr in addresses)
				{
					if (addr.AddressFamily == AddressFamily.InterNetwork) // IPv4
					{
						byte[] bytes = addr.GetAddressBytes();

						// Пример вычисления broadcast адреса для сети /24
						if (bytes.Length == 4)
						{
							bytes[3] = 255; // Последний октет = 255
							IPAddress broadcastAddr = new IPAddress(bytes);

							Console.WriteLine($"     Для сети {addr}/24: {broadcastAddr}");
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"     Ошибка получения адресов: {ex.Message}");
			}

			// 3. Пример настройки UdpClient для broadcast
			Console.WriteLine("\n   3. Настройка UdpClient для broadcast:");

			_broadcastSender = new UdpClient();
			_broadcastSender.EnableBroadcast = true; // Ключевая настройка!

			Console.WriteLine($"     EnableBroadcast = {_broadcastSender.EnableBroadcast}");
			Console.WriteLine($"     Важно: по умолчанию broadcast отключен");

			// Проверка текущей конфигурации
			Console.WriteLine($"\n   4. Проверка конфигурации сокета:");
			Socket socket = _broadcastSender.Client;
			try
			{
				// Проверка опции broadcast на уровне сокета
				object? optionValue = socket.GetSocketOption(
					SocketOptionLevel.Socket,
					SocketOptionName.Broadcast);
				bool socketLevelBroadcast = optionValue is int optionInt && optionInt != 0;
				Console.WriteLine($"     Уровень сокета: Broadcast = {socketLevelBroadcast}");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"     Ошибка проверки опций сокета: {ex.Message}");
			}
		}

		private async Task DemonstrateServiceDiscovery()
		{
			Console.WriteLine("   Service Discovery - основной сценарий использования broadcast");

			// Создаём и запускаем "сервис" в отдельном потоке
			var service = new NetworkService(_port);
			service.Start();

			// Даём время сервису запуститься
			await Task.Delay(1000);

			Console.WriteLine("\n   Шаг 1: Клиент отправляет broadcast-запрос");

			// Создаём клиента для поиска сервисов
			using (var discoveryClient = new UdpClient())
			{
				discoveryClient.EnableBroadcast = true;

				// Отправляем broadcast-запрос
				string discoveryMessage = "SERVICE_DISCOVERY:FILE_SERVER";
				byte[] requestData = Encoding.UTF8.GetBytes(discoveryMessage);

				Console.WriteLine($"     Отправка запроса: '{discoveryMessage}'");
				Console.WriteLine($"     Адрес: 255.255.255.255:{_port}");

				// Отправка на broadcast адрес
				int bytesSent = discoveryClient.Send(
					requestData,
					requestData.Length,
					new IPEndPoint(IPAddress.Broadcast, _port));

				Console.WriteLine($"     Отправлено байт: {bytesSent}");

				// Ожидаем ответ от сервиса
				Console.WriteLine("\n   Шаг 2: Ожидание ответа от сервиса");

				discoveryClient.Client.ReceiveTimeout = 3000; // Таймаут 3 секунды

				try
				{
					IPEndPoint serverEndpoint = new IPEndPoint(IPAddress.Any, 0);
					byte[] responseData = discoveryClient.Receive(ref serverEndpoint);

					string response = Encoding.UTF8.GetString(responseData);
					Console.WriteLine($"     Получен ответ: '{response}'");
					Console.WriteLine($"     От сервера: {serverEndpoint}");

					Console.WriteLine("\n   Шаг 3: Установка unicast соединения");
					Console.WriteLine($"     Теперь клиент знает адрес сервера: {serverEndpoint}");
					Console.WriteLine($"     Дальнейшее взаимодействие через unicast");
				}
				catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
				{
					Console.WriteLine($"     Сервис не найден (таймаут)");
				}
			}

			// Останавливаем сервис
			service.Stop();
		}

		private void DemonstrateBroadcastLimitations()
		{
			Console.WriteLine("   Ограничения broadcast передачи:");

			// Создаём несколько приёмников для демонстрации нагрузки
			var receivers = new List<UdpClient>();
			int receiverCount = 5;

			Console.WriteLine($"\n   1. Нагрузка на сеть и устройства:");
			Console.WriteLine($"     Создаем {receiverCount} приёмников...");

			for (int i = 0; i < receiverCount; i++)
			{
				try
				{
					var receiver = new UdpClient(_port + i);
					receivers.Add(receiver);

					// Запускаем асинхронный приём в каждом
					_ = Task.Run(() => SimulateReceiver(receiver, i));
				}
				catch { }
			}

			// Отправляем broadcast сообщение
			if (_broadcastSender != null)
			{
				string message = "BROADCAST_TEST_MESSAGE";
				byte[] data = Encoding.UTF8.GetBytes(message);

				Console.WriteLine($"\n   2. Отправка broadcast сообщения:");
				Console.WriteLine($"     Сообщение: '{message}'");
				Console.WriteLine($"     Все {receiverCount} приёмников получат это сообщение");

				int sent = _broadcastSender.Send(data, data.Length,
					new IPEndPoint(IPAddress.Broadcast, _port));

				Console.WriteLine($"     Отправлено байт: {sent}");

				// Даём время на обработку
				Thread.Sleep(2000);

				Console.WriteLine($"\n   3. Статистика:");
				Console.WriteLine($"     Сообщение получено {_receivedMessages} раз");
				Console.WriteLine($"     Каждый пакет обработан {receiverCount} раза");
				Console.WriteLine($"     В реальной сети - умножьте на количество устройств!");
			}

			// Очистка
			foreach (var receiver in receivers)
			{
				receiver.Close();
			}

			// Демонстрация невозможности использования в интернете
			Console.WriteLine($"\n   4. Ограничение маршрутизаторами:");
			Console.WriteLine($"     Broadcast НЕ проходит через маршрутизаторы");
			Console.WriteLine($"     Локальная сеть → ✓");
			Console.WriteLine($"     Интернет → ✗");
			Console.WriteLine($"     Разные сегменты сети → ✗");

			// Демонстрация отсутствия подтверждений
			Console.WriteLine($"\n   5. Отсутствие подтверждений:");
			Console.WriteLine($"     Отправитель не знает, кто получил сообщение");
			Console.WriteLine($"     Нет повторной отправки при потере");
			Console.WriteLine($"     Нет гарантии доставки");
		}

		private async Task SimulateReceiver(UdpClient receiver, int id)
		{
			try
			{
				IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, 0);
				byte[] data = receiver.Receive(ref endpoint);

				Interlocked.Increment(ref _receivedMessages);

				// Имитация обработки
				await Task.Delay(100);

				// В реальном приложении здесь была бы проверка типа сообщения
				// и отправка ответа если нужно
			}
			catch { }
		}

		private void DemonstrateBroadcastSecurity()
		{
			Console.WriteLine("   Проблемы безопасности broadcast:");

			Console.WriteLine($"\n   1. Прослушивание всеми:");
			Console.WriteLine($"     Любое устройство в сети может:");
			Console.WriteLine($"       • Прослушивать broadcast трафик");
			Console.WriteLine($"       • Анализировать содержимое сообщений");
			Console.WriteLine($"       • Подделывать broadcast сообщения");

			// Демонстрация "прослушки"
			Console.WriteLine($"\n   2. Демонстрация прослушивания:");

			using (var sniffer = new UdpClient(_port + 100))
			{
				sniffer.Client.SetSocketOption(
					SocketOptionLevel.Socket,
					SocketOptionName.ReuseAddress,
					true);

				// Отправляем тестовое сообщение
				if (_broadcastSender != null)
				{
					string secretMessage = "SECRET:BROADCAST_DATA";
					byte[] data = Encoding.UTF8.GetBytes(secretMessage);

					_broadcastSender.Send(data, data.Length,
						new IPEndPoint(IPAddress.Broadcast, _port + 100));

					Console.WriteLine($"     Отправлено: '{secretMessage}'");

					// Пытаемся "подслушать"
					sniffer.Client.ReceiveTimeout = 1000;

					try
					{
						IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, 0);
						byte[] received = sniffer.Receive(ref endpoint);
						string sniffed = Encoding.UTF8.GetString(received);

						Console.WriteLine($"     'Сниффер' получил: '{sniffed}'");
						Console.WriteLine($"     Доказано: данные не защищены!");
					}
					catch { }
				}
			}

			Console.WriteLine($"\n   3. Рекомендации по безопасности:");
			Console.WriteLine($"     • Не передавать конфиденциальные данные");
			Console.WriteLine($"     • Использовать шифрование при необходимости");
			Console.WriteLine($"     • Проверять подлинность отправителя");
			Console.WriteLine($"     • Ограничивать частоту broadcast сообщений");

			Console.WriteLine($"\n   4. Альтернативы для безопасного общения:");
			Console.WriteLine($"     • Multicast с аутентификацией");
			Console.WriteLine($"     • Сервер обнаружения с авторизацией");
			Console.WriteLine($"     • Предварительная настройка адресов");
		}

		public void Dispose()
		{
			_isRunning = false;

			_broadcastSender?.Close();
			_broadcastReceiver?.Close();

			_receiverThread?.Join(1000);
		}

		// Класс для имитации сетевого сервиса
		private class NetworkService : IDisposable
		{
			private UdpClient _serviceReceiver;
			private Thread _serviceThread;
			private bool _isRunning;
			private int _port;

			public NetworkService(int port)
			{
				_port = port;
			}

			public void Start()
			{
				_isRunning = true;
				_serviceReceiver = new UdpClient(_port);

				_serviceThread = new Thread(ServiceWorker);
				_serviceThread.IsBackground = true;
				_serviceThread.Start();

				Console.WriteLine($"     Сервис запущен на порту {_port}");
			}

			private void ServiceWorker()
			{
				Console.WriteLine($"     [Сервис] Ожидание broadcast запросов...");

				while (_isRunning)
				{
					try
					{
						IPEndPoint clientEndpoint = new IPEndPoint(IPAddress.Any, 0);
						byte[] requestData = _serviceReceiver.Receive(ref clientEndpoint);

						string request = Encoding.UTF8.GetString(requestData);

						if (request.StartsWith("SERVICE_DISCOVERY:"))
						{
							string serviceType = request.Substring("SERVICE_DISCOVERY:".Length);

							Console.WriteLine($"     [Сервис] Получен запрос на {serviceType} от {clientEndpoint}");

							// Отправляем ответ с информацией о сервисе
							string response = $"SERVICE_RESPONSE:FILE_SERVER:{Dns.GetHostName()}:{_port}";
							byte[] responseData = Encoding.UTF8.GetBytes(response);

							// Ответ отправляется unicast - напрямую клиенту
							_serviceReceiver.Send(responseData, responseData.Length, clientEndpoint);
							Console.WriteLine($"     [Сервис] Ответ отправлен клиенту");
						}
					}
					catch (SocketException ex) when (ex.SocketErrorCode == SocketError.Interrupted)
					{
						break;
					}
					catch (Exception ex)
					{
						Console.WriteLine($"     [Сервис] Ошибка: {ex.Message}");
						Thread.Sleep(1000);
					}
				}
			}

			public void Stop()
			{
				_isRunning = false;
				_serviceReceiver?.Close();
				_serviceThread?.Join(1000);

				Console.WriteLine($"     Сервис остановлен");
			}

			public void Dispose()
			{
				Stop();
			}
		}
	}

	// Практический пример: Discovery сервер для домашней сети
	public class HomeNetworkDiscovery
	{
		public class DeviceInfo
		{
			public string DeviceName { get; set; }
			public string DeviceType { get; set; }
			public IPAddress IPAddress { get; set; }
			public int Port { get; set; }
			public DateTime LastSeen { get; set; }

			public override string ToString()
			{
				return $"{DeviceName} ({DeviceType}) at {IPAddress}:{Port} - seen {LastSeen:HH:mm:ss}";
			}
		}

		private UdpClient _discoveryServer;
		private Dictionary<string, DeviceInfo> _discoveredDevices = new();
		private bool _isRunning;
		private int _discoveryPort;

		public HomeNetworkDiscovery(int port = 11070)
		{
			_discoveryPort = port;
		}

		public void StartDiscoveryServer()
		{
			Console.WriteLine("\n=== ПРАКТИЧЕСКИЙ ПРИМЕР: ДОМАШНИЙ DISCOVERY СЕРВЕР ===\n");

			_isRunning = true;
			_discoveryServer = new UdpClient(_discoveryPort);
			_discoveryServer.EnableBroadcast = true;

			Console.WriteLine($"Discovery сервер запущен на порту {_discoveryPort}");
			Console.WriteLine("Ожидание объявлений устройств...");

			Task.Run(() => DiscoveryServerWorker());

			// Также запускаем периодический broadcast для поиска устройств
			Task.Run(() => BroadcastDiscoveryRequests());
		}

		private async Task DiscoveryServerWorker()
		{
			while (_isRunning)
			{
				try
				{
					IPEndPoint senderEndpoint = new IPEndPoint(IPAddress.Any, 0);
					byte[] data = _discoveryServer.Receive(ref senderEndpoint);
					string message = Encoding.UTF8.GetString(data);

					// Обработка сообщений от устройств
					if (message.StartsWith("DEVICE_ANNOUNCE:"))
					{
						ProcessDeviceAnnouncement(message, senderEndpoint);
					}
					else if (message.StartsWith("DISCOVERY_REQUEST:"))
					{
						ProcessDiscoveryRequest(senderEndpoint);
					}
				}
				catch { }
			}
		}

		private void ProcessDeviceAnnouncement(string message, IPEndPoint endpoint)
		{
			// Формат: DEVICE_ANNOUNCE:DeviceName:DeviceType:Port
			var parts = message.Split(':');
			if (parts.Length >= 4)
			{
				var deviceInfo = new DeviceInfo
				{
					DeviceName = parts[1],
					DeviceType = parts[2],
					IPAddress = endpoint.Address,
					Port = int.Parse(parts[3]),
					LastSeen = DateTime.Now
				};

				string deviceKey = $"{deviceInfo.IPAddress}:{deviceInfo.Port}";
				_discoveredDevices[deviceKey] = deviceInfo;

				Console.WriteLine($"Обнаружено устройство: {deviceInfo}");

				// Отправляем подтверждение
				string ack = $"DEVICE_ACK:{deviceInfo.DeviceName}";
				byte[] ackData = Encoding.UTF8.GetBytes(ack);
				_discoveryServer.Send(ackData, ackData.Length, endpoint);
			}
		}

		private void ProcessDiscoveryRequest(IPEndPoint requester)
		{
			// Отправляем список известных устройств
			var deviceList = new List<string>();
			foreach (var device in _discoveredDevices.Values)
			{
				deviceList.Add($"{device.DeviceName}:{device.DeviceType}:{device.IPAddress}:{device.Port}");
			}

			string response = $"DISCOVERY_RESPONSE:" + string.Join(";", deviceList);
			byte[] responseData = Encoding.UTF8.GetBytes(response);

			_discoveryServer.Send(responseData, responseData.Length, requester);
		}

		private async Task BroadcastDiscoveryRequests()
		{
			using (var broadcastClient = new UdpClient())
			{
				broadcastClient.EnableBroadcast = true;

				while (_isRunning)
				{
					// Регулярный broadcast для поиска устройств
					string discoveryRequest = "DISCOVERY_REQUEST:SERVER";
					byte[] requestData = Encoding.UTF8.GetBytes(discoveryRequest);

					broadcastClient.Send(requestData, requestData.Length,
						new IPEndPoint(IPAddress.Broadcast, _discoveryPort));

					Console.WriteLine($"Отправлен broadcast запрос для поиска устройств");

					// Очистка устаревших устройств
					CleanupOldDevices();

					await Task.Delay(30000); // Каждые 30 секунд
				}
			}
		}

		private void CleanupOldDevices()
		{
			var now = DateTime.Now;
			var toRemove = new List<string>();

			foreach (var kvp in _discoveredDevices)
			{
				if ((now - kvp.Value.LastSeen).TotalMinutes > 5) // 5 минут
				{
					toRemove.Add(kvp.Key);
				}
			}

			foreach (var key in toRemove)
			{
				Console.WriteLine($"Устройство удалено (таймаут): {_discoveredDevices[key].DeviceName}");
				_discoveredDevices.Remove(key);
			}
		}

		public void Stop()
		{
			_isRunning = false;
			_discoveryServer?.Close();

			Console.WriteLine("\nDiscovery сервер остановлен");
			Console.WriteLine($"Всего обнаружено устройств: {_discoveredDevices.Count}");
		}
	}

	// Главная программа
	class Program
	{
		static async Task Main(string[] args)
		{
			Console.WriteLine("UDP BROADCAST: ШИРОКОВЕЩАТЕЛЬНАЯ ПЕРЕДАЧА ДАННЫХ");
			Console.WriteLine("================================================\n");

			using (var demo = new UdpBroadcastDemonstration())
			{
				await demo.DemonstrateBroadcast();
			}

			// Практический пример
			var homeDiscovery = new HomeNetworkDiscovery();
			homeDiscovery.StartDiscoveryServer();

			// Даём поработать discovery серверу
			Console.WriteLine("\nDiscovery сервер работает 15 секунд...");
			await Task.Delay(15000);

			homeDiscovery.Stop();
		}
	}
}
