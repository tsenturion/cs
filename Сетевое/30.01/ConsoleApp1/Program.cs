using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static MulticastDemo.MulticastImplementation;

namespace MulticastDemo
{
	// Демонстрация multicast-передачи данных
	public class MulticastImplementation : IDisposable
	{
		// Диапазоны multicast-адресов
		private const string MulticastGroupIPv4 = "224.100.0.1";
		private const string MulticastGroupIPv6 = "FF02::1";
		private const int MulticastPort = 12000;

		private UdpClient _sender;
		private UdpClient _receiver;
		private Thread _receiverThread;
		private bool _isRunning;
		private int _packetCounter = 0;

		public MulticastImplementation()
		{
			Console.WriteLine("=== MULTICAST: ГРУППОВАЯ РАССЫЛКА ДАННЫХ ===\n");
		}

		public void DemonstrateMulticastBasics()
		{
			Console.WriteLine("1. ОСНОВНЫЕ ПРИНЦИПЫ MULTICAST:");
			ExplainMulticastPrinciples();

			Console.WriteLine("\n2. MULTICAST АДРЕСАЦИЯ:");
			DemonstrateMulticastAddressing();

			Console.WriteLine("\n3. ОТПРАВКА ДАННЫХ В ГРУППУ:");
			DemonstrateMulticastSending();

			Console.WriteLine("\n4. ПОДПИСКА НА MULTICAST-ГРУППУ:");
			DemonstrateMulticastReceiving();

			Console.WriteLine("\n5. ЭФФЕКТИВНОСТЬ И СЦЕНАРИИ ИСПОЛЬЗОВАНИЯ:");
			DemonstrateEfficiencyAndScenarios();

			Console.WriteLine("\n6. ОГРАНИЧЕНИЯ И БЕЗОПАСНОСТЬ:");
			DemonstrateLimitationsAndSecurity();
		}

		private void ExplainMulticastPrinciples()
		{
			Console.WriteLine("   Ключевые принципы multicast:");
			Console.WriteLine("   • Один отправитель, много получателей");
			Console.WriteLine("   • Получатели самостоятельно подписываются на группу");
			Console.WriteLine("   • Отправитель не знает получателей");
			Console.WriteLine("   • Работает поверх UDP (ненадёжная доставка)");
			Console.WriteLine("   • Требует поддержки сети и маршрутизаторов");
			Console.WriteLine("   • Эффективен для потоковых данных");

			Console.WriteLine("\n   Сравнение с другими моделями:");
			Console.WriteLine("   Unicast (один-к-одному):");
			Console.WriteLine("     • Контроль и надёжность");
			Console.WriteLine("     • Знание получателя");
			Console.WriteLine("     • Множество соединений при массовой рассылке");

			Console.WriteLine("\n   Broadcast (один-ко-всем):");
			Console.WriteLine("     • Простота обнаружения");
			Console.WriteLine("     • Навязывание сообщений");
			Console.WriteLine("     • Неэффективность в больших сетях");

			Console.WriteLine("\n   Multicast (один-ко-многим):");
			Console.WriteLine("     • Эффективность при массовой рассылке");
			Console.WriteLine("     • Добровольная подписка получателей");
			Console.WriteLine("     • Требует поддержки сети");
		}

		private void DemonstrateMulticastAddressing()
		{
			Console.WriteLine("   Multicast-адреса IPv4:");
			Console.WriteLine("   • Диапазон: 224.0.0.0 - 239.255.255.255");
			Console.WriteLine("   • 224.0.0.0 - 224.0.0.255: локальные сети");
			Console.WriteLine("   • 224.0.1.0 - 238.255.255.255: глобальные");
			Console.WriteLine("   • 239.0.0.0 - 239.255.255.255: ограниченные");

			Console.WriteLine("\n   Примеры multicast-адресов:");
			Console.WriteLine("   • 224.0.0.1: все хосты в сети");
			Console.WriteLine("   • 224.0.0.2: все маршрутизаторы");
			Console.WriteLine("   • 224.0.0.9: RIP версия 2");
			Console.WriteLine("   • 224.0.1.1: NTP (сетевое время)");

			Console.WriteLine($"\n   Используемый адрес в демонстрации: {MulticastGroupIPv4}");

			// Проверка валидности адреса
			try
			{
				IPAddress multicastAddress = IPAddress.Parse(MulticastGroupIPv4);
				Console.WriteLine($"   Адрес валиден, семейство: {multicastAddress.AddressFamily}");

				if (!IsMulticastAddress(multicastAddress))
				{
					Console.WriteLine($"   ⚠️  Внимание: {MulticastGroupIPv4} не входит в multicast-диапазон!");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"   Ошибка парсинга адреса: {ex.Message}");
			}
		}

		private bool IsMulticastAddress(IPAddress address)
		{
			if (address.AddressFamily == AddressFamily.InterNetwork) // IPv4
			{
				byte[] bytes = address.GetAddressBytes();
				// Первые 4 бита: 1110 для multicast
				return (bytes[0] & 0xF0) == 0xE0;
			}
			return false;
		}

		private void DemonstrateMulticastSending()
		{
			Console.WriteLine("   Отправка данных в multicast-группу:");

			try
			{
				// Создание отправителя
				_sender = new UdpClient();

				// Настройка TTL (Time To Live)
				_sender.Ttl = 32; // Пакет пройдёт через 32 маршрутизатора
				Console.WriteLine($"   TTL отправителя: {_sender.Ttl}");

				IPAddress multicastAddress = IPAddress.Parse(MulticastGroupIPv4);
				IPEndPoint multicastEndpoint = new IPEndPoint(multicastAddress, MulticastPort);

				// Отправка тестового сообщения
				Console.WriteLine($"\n   Отправка сообщения в группу {MulticastGroupIPv4}:{MulticastPort}");

				string message = "Multicast тестовое сообщение";
				byte[] data = Encoding.UTF8.GetBytes(message);

				int bytesSent = _sender.Send(data, data.Length, multicastEndpoint);
				Console.WriteLine($"   Отправлено {bytesSent} байт");
				Console.WriteLine($"   Отправитель не знает, сколько получателей");

				// Отправка нескольких сообщений
				Console.WriteLine($"\n   Отправка потока данных:");
				for (int i = 1; i <= 3; i++)
				{
					string streamMessage = $"Поток данных #{i} от {DateTime.Now:HH:mm:ss.fff}";
					byte[] streamData = Encoding.UTF8.GetBytes(streamMessage);

					_sender.Send(streamData, streamData.Length, multicastEndpoint);
					Console.WriteLine($"   Отправлено: {streamMessage}");

					Thread.Sleep(500);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"   Ошибка отправки: {ex.Message}");
				Console.WriteLine($"   Возможно, сеть не поддерживает multicast");
			}
		}

		private void DemonstrateMulticastReceiving()
		{
			Console.WriteLine("   Подписка на multicast-группу:");

			try
			{
				// Создание получателя
				_receiver = new UdpClient();

				// Привязка к порту
				_receiver.Client.SetSocketOption(
					SocketOptionLevel.Socket,
					SocketOptionName.ReuseAddress,
					true);

				_receiver.Client.Bind(new IPEndPoint(IPAddress.Any, MulticastPort));

				Console.WriteLine($"   Приёмник привязан к порту {MulticastPort}");

				// Вступление в multicast-группу
				IPAddress multicastAddress = IPAddress.Parse(MulticastGroupIPv4);
				_receiver.JoinMulticastGroup(multicastAddress);

				Console.WriteLine($"   Приложение подписано на группу {MulticastGroupIPv4}");
				Console.WriteLine($"   Теперь будут приниматься пакеты этой группы");

				// Дополнительные настройки multicast
				// TTL для исходящих пакетов (если получатель тоже будет отправлять)
				_receiver.Ttl = 1; // Только локальная сеть

				// Настройка интерфейса для multicast (опционально)
				try
				{
					_receiver.MulticastLoopback = true; // Получать свои же пакеты
					Console.WriteLine($"   MulticastLoopback включён: получаем свои пакеты");
				}
				catch { }

				// Запуск потока приёма
				_isRunning = true;
				_receiverThread = new Thread(ReceiveMulticastPackets);
				_receiverThread.IsBackground = true;
				_receiverThread.Start();

				Console.WriteLine($"\n   Поток приёма запущен. Ожидание multicast-пакетов...");

				// Даём время настроиться
				Thread.Sleep(2000);

				// Отправляем тестовый пакет, чтобы проверить подписку
				if (_sender != null)
				{
					string testMessage = "Тест подписки на multicast";
					byte[] testData = Encoding.UTF8.GetBytes(testMessage);
					IPEndPoint multicastEndpoint = new IPEndPoint(multicastAddress, MulticastPort);

					_sender.Send(testData, testData.Length, multicastEndpoint);
					Console.WriteLine($"   Отправлен тестовый пакет для проверки подписки");
				}

				// Ждём некоторое время для приёма пакетов
				Thread.Sleep(3000);

				// Выход из группы
				Console.WriteLine($"\n   Выход из multicast-группы...");
				_receiver.DropMulticastGroup(multicastAddress);
				Console.WriteLine($"   Приложение отписано от группы {MulticastGroupIPv4}");

				_isRunning = false;
				Thread.Sleep(1000);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"   Ошибка при подписке: {ex.Message}");
			}
		}

		private void ReceiveMulticastPackets()
		{
			Console.WriteLine($"   [Multicast Receiver] Поток приёма запущен");

			while (_isRunning)
			{
				try
				{
					IPEndPoint remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);
					byte[] data = _receiver.Receive(ref remoteEndpoint);

					_packetCounter++;
					string message = Encoding.UTF8.GetString(data);

					Console.WriteLine($"   [Пакет #{_packetCounter}] Получено от {remoteEndpoint}: {message}");
				}
				catch (SocketException ex) when (ex.SocketErrorCode == SocketError.Interrupted)
				{
					break;
				}
				catch (Exception ex)
				{
					if (_isRunning)
					{
						Console.WriteLine($"   [Ошибка приёма] {ex.Message}");
					}
					break;
				}
			}

			Console.WriteLine($"   [Multicast Receiver] Поток приёма завершён");
		}

		private void DemonstrateEfficiencyAndScenarios()
		{
			Console.WriteLine("   Эффективность multicast:");

			Console.WriteLine("\n   Пример: 100 получателей, 1 KB данных:");
			long unicastBandwidth = 100 * 1024; // 100 KB
			long multicastBandwidth = 1 * 1024; // 1 KB

			Console.WriteLine($"   • Unicast: {unicastBandwidth} байт (100 отдельных отправок)");
			Console.WriteLine($"   • Multicast: {multicastBandwidth} байт (1 отправка в группу)");
			Console.WriteLine($"   • Экономия: {(unicastBandwidth - multicastBandwidth) / 1024} KB ({((unicastBandwidth - multicastBandwidth) * 100.0 / unicastBandwidth):F1}%)");

			Console.WriteLine("\n   Типичные сценарии использования:");
			Console.WriteLine("   1. Видео- и аудиотрансляции");
			Console.WriteLine("   2. Финансовые котировки в реальном времени");
			Console.WriteLine("   3. Игровые события и обновления состояния");
			Console.WriteLine("   4. Системы мониторинга и телеметрии");
			Console.WriteLine("   5. Обновления конфигурации в кластерах");
			Console.WriteLine("   6. Обнаружение сервисов в локальной сети");

			Console.WriteLine("\n   Демонстрация потоковой передачи:");
			DemonstrateStreamingExample();
		}

		private void DemonstrateStreamingExample()
		{
			Console.WriteLine($"\n   Имитация потоковой передачи:");

			try
			{
				IPAddress multicastAddress = IPAddress.Parse(MulticastGroupIPv4);
				IPEndPoint multicastEndpoint = new IPEndPoint(multicastAddress, MulticastPort);

				// Имитация видеопотока
				Console.WriteLine($"   Начало трансляции видеопотока...");

				for (int frame = 1; frame <= 5; frame++)
				{
					string videoFrame = $"VIDEO_FRAME_{frame}_{DateTime.Now:HH:mm:ss.fff}";
					byte[] frameData = Encoding.UTF8.GetBytes(videoFrame);

					if (_sender != null)
					{
						_sender.Send(frameData, frameData.Length, multicastEndpoint);
						Console.WriteLine($"   Отправлен кадр {frame}: {videoFrame}");
					}

					// Имитация частоты кадров (30 FPS)
					Thread.Sleep(33);
				}

				Console.WriteLine($"   Трансляция завершена");
				Console.WriteLine($"   Потеря кадра в multicast - нормальное явление");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"   Ошибка потоковой передачи: {ex.Message}");
			}
		}

		private void DemonstrateLimitationsAndSecurity()
		{
			Console.WriteLine("   Ограничения и безопасность multicast:");

			Console.WriteLine("\n   Технические ограничения:");
			Console.WriteLine("   • Требует поддержки сети и маршрутизаторов");
			Console.WriteLine("   • Часто блокируется в корпоративных сетях");
			Console.WriteLine("   • Нет гарантий доставки (UDP-based)");
			Console.WriteLine("   • Нет контроля получателей");
			Console.WriteLine("   • Ограниченный TTL может мешать доставке");

			Console.WriteLine("\n   Вопросы безопасности:");
			Console.WriteLine("   • Любой может подписаться на группу");
			Console.WriteLine("   • Нет шифрования по умолчанию");
			Console.WriteLine("   • Возможность DoS-атак через flood");
			Console.WriteLine("   • Утечка информации через подслушивание");

			Console.WriteLine("\n   Меры безопасности:");
			Console.WriteLine("   1. Использование приватных multicast-адресов");
			Console.WriteLine("   2. Шифрование данных на уровне приложения");
			Console.WriteLine("   3. Аутентификация отправителя");
			Console.WriteLine("   4. Ограничение TTL для локальной доставки");
			Console.WriteLine("   5. Мониторинг multicast-трафика");

			Console.WriteLine("\n   Практические рекомендации:");
			Console.WriteLine("   • Используйте для публичных или несекретных данных");
			Console.WriteLine("   • Реализуйте механизм повторной отправки критичных данных");
			Console.WriteLine("   • Проверяйте поддержку multicast в целевой среде");
			Console.WriteLine("   • Имейте fallback на unicast при проблемах");
		}

		// Пример реального использования multicast
		public class MulticastServiceDiscovery : IDisposable
		{
			private UdpClient _multicastClient;
			private Thread _discoveryThread;
			private bool _isRunning;
			private readonly string _serviceName;
			private readonly int _discoveryPort = 13000;
			private readonly string _multicastGroup = "224.0.0.100";

			public event EventHandler<IPEndPoint> ServiceDiscovered;

			public MulticastServiceDiscovery(string serviceName)
			{
				_serviceName = serviceName;
			}

			public void StartDiscovery()
			{
				_isRunning = true;

				// Запуск в режиме приёма объявлений
				_discoveryThread = new Thread(DiscoveryListener);
				_discoveryThread.IsBackground = true;
				_discoveryThread.Start();

				// Периодическая отправка своего присутствия
				Task.Run(() => AnnouncePresence());

				Console.WriteLine($"\n[Service Discovery] Запущено обнаружение службы '{_serviceName}'");
				Console.WriteLine($"  Multicast группа: {_multicastGroup}:{_discoveryPort}");
			}

			private void DiscoveryListener()
			{
				try
				{
					using (var listener = new UdpClient())
					{
						listener.Client.SetSocketOption(
							SocketOptionLevel.Socket,
							SocketOptionName.ReuseAddress,
							true);

						listener.Client.Bind(new IPEndPoint(IPAddress.Any, _discoveryPort));

						IPAddress multicastAddress = IPAddress.Parse(_multicastGroup);
						listener.JoinMulticastGroup(multicastAddress);

						Console.WriteLine($"[Discovery Listener] Подписан на multicast-группу");

						while (_isRunning)
						{
							try
							{
								IPEndPoint remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);
								byte[] data = listener.Receive(ref remoteEndpoint);
								string message = Encoding.UTF8.GetString(data);

								// Обработка сообщения об обнаружении
								if (message.StartsWith("SERVICE_ANNOUNCE:"))
								{
									string[] parts = message.Split(':');
									if (parts.Length >= 3 && parts[1] == _serviceName)
									{
										Console.WriteLine($"[Discovery] Найдена служба '{_serviceName}' на {remoteEndpoint}");
										ServiceDiscovered?.Invoke(this, remoteEndpoint);
									}
								}
							}
							catch (SocketException ex) when (ex.SocketErrorCode == SocketError.Interrupted)
							{
								break;
							}
						}

						listener.DropMulticastGroup(multicastAddress);
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"[Discovery Error] {ex.Message}");
				}
			}

			private async Task AnnouncePresence()
			{
				try
				{
					using (var announcer = new UdpClient())
					{
						announcer.Ttl = 1; // Только локальная сеть

						IPAddress multicastAddress = IPAddress.Parse(_multicastGroup);
						IPEndPoint multicastEndpoint = new IPEndPoint(multicastAddress, _discoveryPort);

						while (_isRunning)
						{
							string announceMessage = $"SERVICE_ANNOUNCE:{_serviceName}:{DateTime.Now:HH:mm:ss}";
							byte[] data = Encoding.UTF8.GetBytes(announceMessage);

							announcer.Send(data, data.Length, multicastEndpoint);

							await Task.Delay(5000); // Объявление каждые 5 секунд
						}
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"[Announce Error] {ex.Message}");
				}
			}

			public void Dispose()
			{
				_isRunning = false;
				_discoveryThread?.Join(1000);
				_multicastClient?.Close();
			}
		}

		public void Dispose()
		{
			_isRunning = false;

			_sender?.Close();
			_receiver?.Close();

			_receiverThread?.Join(1000);

			Console.WriteLine($"\n   Ресурсы multicast освобождены");
		}
	}

	// Главная программа
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("MULTICAST: ГРУППОВАЯ ПЕРЕДАЧА ДАННЫХ В СЕТЯХ");
			Console.WriteLine("==============================================\n");

			using (var multicastDemo = new MulticastImplementation())
			{
				multicastDemo.DemonstrateMulticastBasics();
			}

			// Пример использования multicast для обнаружения служб
			Console.WriteLine("\n\n=== ПРИМЕР: ОБНАРУЖЕНИЕ СЛУЖБ ЧЕРЕЗ MULTICAST ===");

			using (var serviceDiscovery = new MulticastServiceDiscovery("MyService"))
			{
				serviceDiscovery.ServiceDiscovered += (sender, endpoint) =>
				{
					Console.WriteLine($"  → Обнаружена служба на {endpoint}");
				};

				serviceDiscovery.StartDiscovery();

				// Даём время поработать
				Thread.Sleep(10000);
			}
		}
	}
}