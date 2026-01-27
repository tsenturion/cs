using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace UdpOsInteraction
{
	// Демонстрация взаимодействия UDP с операционной системой
	public class UdpOsInteractionDemo : IDisposable
	{
		private UdpClient _receiver;
		private UdpClient _sender;
		private Thread _receiverThread;
		private bool _isRunning;
		private int _port;
		private readonly ConcurrentQueue<(byte[] data, IPEndPoint endpoint)> _packetQueue = new();
		private long _droppedPackets = 0;
		private long _receivedPackets = 0;
		private object _statisticsLock = new object();

		// Статистика для анализа
		public class UdpStatistics
		{
			public long TotalReceived { get; set; }
			public long TotalDropped { get; set; }
			public int BufferSize { get; set; }
			public int AvailableInBuffer { get; set; }
			public TimeSpan AverageProcessingTime { get; set; }
		}

		public UdpStatistics CurrentStatistics { get; private set; } = new UdpStatistics();

		public UdpOsInteractionDemo(int port = 11050)
		{
			_port = port;
		}

		public void DemonstrateUdpBasics()
		{
			Console.WriteLine("=== UDP И ВЗАИМОДЕЙСТВИЕ С ОПЕРАЦИОННОЙ СИСТЕМОЙ ===\n");

			Console.WriteLine("1. РЕГИСТРАЦИЯ В ОС КАК ПОЛУЧАТЕЛЬ:");
			DemonstrateOsRegistration();

			Console.WriteLine("\n2. БУФЕРЫ ПРИЁМА И ОТПРАВКИ В ОС:");
			DemonstrateOsBuffers();

			Console.WriteLine("\n3. БЛОКИРУЮЩИЕ ВЫЗОВЫ И ОЖИДАНИЕ ОС:");
			DemonstrateBlockingCalls();

			Console.WriteLine("\n4. ПЕРЕПОЛНЕНИЕ БУФЕРА И ПОТЕРИ В ОС:");
			DemonstrateBufferOverflow();

			Console.WriteLine("\n5. ФРАГМЕНТАЦИЯ НА УРОВНЕ ОС:");
			DemonstrateFragmentation();

			Console.WriteLine("\n6. АСИНХРОННОСТЬ И УВЕДОМЛЕНИЯ ОТ ОС:");
			DemonstrateAsyncNotifications();

			Console.WriteLine("\n7. NAT И FIREWALL - ПРЕПЯТСТВИЯ В ОС:");
			DemonstrateNetworkObstacles();
		}

		private void DemonstrateOsRegistration()
		{
			Console.WriteLine("   Приложение регистрируется в ОС как получатель UDP:");

			try
			{
				// Создание UDP-сокета и привязка к порту
				_receiver = new UdpClient(_port);

				Console.WriteLine($"   UdpClient создан на порту {_port}");
				Console.WriteLine($"   ОС зарегистрировала порт {_port} для этого процесса");
				Console.WriteLine($"   PID процесса: {Process.GetCurrentProcess().Id}");

				// Получение информации о сокете из ОС
				Socket socket = _receiver.Client;
				Console.WriteLine($"\n   Информация из ОС о сокете:");
				Console.WriteLine($"     Handle (дескриптор ОС): {socket.Handle}");
				Console.WriteLine($"     Локальная конечная точка: {socket.LocalEndPoint}");
				Console.WriteLine($"     Протокол: {socket.ProtocolType}");
				Console.WriteLine($"     AddressFamily: {socket.AddressFamily}");

				// Проверка, что порт действительно занят в ОС
				Console.WriteLine($"\n   Проверка состояния порта в ОС:");
				Console.WriteLine($"     IsBound: {socket.IsBound}");
				Console.WriteLine($"     Connected: {socket.Connected}");
				Console.WriteLine($"     Available (в буфере ОС): {socket.Available}");

				// Что происходит, когда порт уже занят другим процессом
				Console.WriteLine($"\n   Попытка повторной регистрации порта:");
				try
				{
					var duplicate = new UdpClient(_port);
					Console.WriteLine($"     ОШИБКА: ОС разрешила дублирование порта!");
					duplicate.Close();
				}
				catch (SocketException ex)
				{
					Console.WriteLine($"     ОС отказала: {ex.SocketErrorCode} - {ex.Message}");
					Console.WriteLine($"     ОС защищает порт от повторного использования");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"   Ошибка: {ex.Message}");
			}
		}

		private void DemonstrateOsBuffers()
		{
			Console.WriteLine("   Управление буферами в операционной системе:");

			if (_receiver == null)
			{
				_receiver = new UdpClient(new IPEndPoint(IPAddress.Any, _port));
			}

			Socket socket = _receiver.Client;

			// Размеры буферов по умолчанию
			int defaultReceiveBuffer = socket.ReceiveBufferSize;
			int defaultSendBuffer = socket.SendBufferSize;

			Console.WriteLine($"\n   Размеры буферов по умолчанию:");
			Console.WriteLine($"     ReceiveBufferSize: {defaultReceiveBuffer} байт ({defaultReceiveBuffer / 1024} KB)");
			Console.WriteLine($"     SendBufferSize: {defaultSendBuffer} байт ({defaultSendBuffer / 1024} KB)");

			// Настройка буферов (интерфейс с ОС)
			Console.WriteLine($"\n   Настройка буферов через ОС:");

			// Увеличение буферов для уменьшения потерь
			socket.ReceiveBufferSize = 1024 * 1024; // 1 MB
			socket.SendBufferSize = 1024 * 1024;    // 1 MB

			Console.WriteLine($"     Новый ReceiveBufferSize: {socket.ReceiveBufferSize} байт");
			Console.WriteLine($"     Новый SendBufferSize: {socket.SendBufferSize} байт");

			// Проверка лимитов ОС
			Console.WriteLine($"\n   Проверка лимитов ОС:");

			try
			{
				// Попытка установить слишком большой буфер
				socket.ReceiveBufferSize = int.MaxValue;
				Console.WriteLine($"     Максимальный буфер: {socket.ReceiveBufferSize}");
			}
			catch (SocketException ex)
			{
				Console.WriteLine($"     ОС ограничила буфер: {ex.Message}");
			}

			// Дополнительные параметры сокета для взаимодействия с ОС
			Console.WriteLine($"\n   Дополнительные параметры сокета:");
			socket.ReceiveTimeout = 5000; // Таймаут приёма
			socket.SendTimeout = 5000;    // Таймаут отправки
			socket.DontFragment = true;   // Запрет фрагментации на уровне ОС

			Console.WriteLine($"     ReceiveTimeout: {socket.ReceiveTimeout} мс");
			Console.WriteLine($"     SendTimeout: {socket.SendTimeout} мс");
			Console.WriteLine($"     DontFragment: {socket.DontFragment}");
		}

		private void DemonstrateBlockingCalls()
		{
			Console.WriteLine("   Блокирующие вызовы и ожидание в ОС:");

			if (_receiver == null) return;

			// Запуск фонового потока для приёма
			_isRunning = true;
			_receiverThread = new Thread(ReceiverWorker);
			_receiverThread.IsBackground = true;
			_receiverThread.Start();

			// Создание отправителя
			_sender = new UdpClient();
			_sender.Connect(new IPEndPoint(IPAddress.Loopback, _port));

			Console.WriteLine($"\n   Демонстрация блокирующего Receive:");

			// Отправка тестового пакета
			byte[] testData = Encoding.UTF8.GetBytes("Тестовый пакет");
			_sender.Send(testData, testData.Length);
			Console.WriteLine($"     Пакет отправлен в ОС");

			// Даём время ОС обработать пакет
			Thread.Sleep(100);

			// Проверка состояния буфера в ОС
			Socket socket = _receiver.Client;
			Console.WriteLine($"\n   Состояние буфера в ОС:");
			Console.WriteLine($"     Available (готовых пакетов): {socket.Available}");

			if (socket.Available > 0)
			{
				Console.WriteLine($"     ОС поместила пакет в буфер сокета");

				// Блокирующий вызов Receive
				Console.WriteLine($"\n   Вызов блокирующего Receive:");
				Console.WriteLine($"     Поток будет заблокирован в ОС до получения данных");

				// В реальном приложении этот вызов был бы блокирующим
				// Для демонстрации используем асинхронный вариант
				var receiveTask = _receiver.ReceiveAsync();
				if (receiveTask.Wait(1000))
				{
					var result = receiveTask.Result;
					Console.WriteLine($"     ОС разблокировала поток, получено {result.Buffer.Length} байт");
				}
			}

			// Демонстрация таймаута
			Console.WriteLine($"\n   Демонстрация таймаута Receive:");

			// Временное увеличение таймаута
			socket.ReceiveTimeout = 2000;
			Console.WriteLine($"     Таймаут установлен: {socket.ReceiveTimeout} мс");

			try
			{
				// Попытка принять данные, которых нет
				Console.WriteLine($"     Вызов Receive (ждём данные, которых нет)...");
				var receiveTask = _receiver.ReceiveAsync();

				if (!receiveTask.Wait(2500))
				{
					Console.WriteLine($"     ОС не разблокировала поток (таймаут)");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"     Исключение: {ex.GetType().Name}");
			}
		}

		private void ReceiverWorker()
		{
			Console.WriteLine($"\n   [Рабочий поток] Запущен, ожидает данные от ОС...");

			while (_isRunning)
			{
				try
				{
					// Блокирующий вызов - поток "спит" в ОС
					IPEndPoint remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);
					byte[] data = _receiver.Receive(ref remoteEndpoint);

					// Убираем readonly и используем обычное поле
					_receivedPackets++;

					// Используем _packetQueue
					_packetQueue.Enqueue((data, remoteEndpoint));

					// Имитация обработки
					Thread.Sleep(50); // Искусственная задержка
				}
				catch (SocketException ex) when (ex.SocketErrorCode == SocketError.Interrupted)
				{
					break;
				}
				catch (Exception ex)
				{
					Console.WriteLine($"   [Рабочий поток] Ошибка: {ex.Message}");
				}
			}

			Console.WriteLine($"   [Рабочий поток] Завершён");
		}

		private void DemonstrateBufferOverflow()
		{
			Console.WriteLine("   Переполнение буфера и потери внутри ОС:");

			if (_receiver == null || _sender == null) return;

			// Уменьшаем буфер для демонстрации переполнения
			Socket socket = _receiver.Client;
			int originalBufferSize = socket.ReceiveBufferSize;
			socket.ReceiveBufferSize = 1024; // Маленький буфер

			Console.WriteLine($"\n   Настройка маленького буфера:");
			Console.WriteLine($"     ReceiveBufferSize: {socket.ReceiveBufferSize} байт");
			Console.WriteLine($"     Примерный лимит пакетов: ~{socket.ReceiveBufferSize / 100} средних пакетов");

			_droppedPackets = 0;
			_receivedPackets = 0;

			// Запуск быстрой отправки пакетов
			Console.WriteLine($"\n   Быстрая отправка пакетов для переполнения буфера:");

			var sendTask = Task.Run(() =>
			{
				byte[] smallPacket = Encoding.UTF8.GetBytes("X");
				Stopwatch sw = Stopwatch.StartNew();
				int sentCount = 0;

				while (sw.ElapsedMilliseconds < 1000 && sentCount < 1000)
				{
					try
					{
						_sender.Send(smallPacket, smallPacket.Length);
						sentCount++;

						// Минимальная задержка для имитации быстрой отправки
						Thread.Sleep(1);
					}
					catch { }
				}

				return sentCount;
			});

			// Мониторинг потерь
			var monitorTask = Task.Run(async () =>
			{
				long lastReceived = 0;

				while (!sendTask.IsCompleted)
				{
					long currentReceived = Interlocked.Read(ref _receivedPackets);
					long dropped = sendTask.Result - currentReceived;

					Interlocked.Exchange(ref _droppedPackets, dropped);

					lock (_statisticsLock)
					{
						CurrentStatistics = new UdpStatistics
						{
							TotalReceived = currentReceived,
							TotalDropped = dropped,
							BufferSize = socket.ReceiveBufferSize,
							AvailableInBuffer = socket.Available
						};
					}

					Console.WriteLine($"     Отправлено: {sendTask.Result}, " +
									$"Получено: {currentReceived}, " +
									$"Потеряно в ОС: {dropped}");

					await Task.Delay(200);
				}
			});

			Task.WaitAll(sendTask, monitorTask);

			// Восстановление размера буфера
			socket.ReceiveBufferSize = originalBufferSize;

			Console.WriteLine($"\n   Итоги переполнения буфера:");
			Console.WriteLine($"     Всего отправлено: {sendTask.Result}");
			Console.WriteLine($"     Принято приложением: {_receivedPackets}");
			Console.WriteLine($"     Потеряно в буфере ОС: {_droppedPackets}");
			Console.WriteLine($"     Процент потерь: {(_droppedPackets * 100.0 / sendTask.Result):F1}%");

			// Очистка буфера
			while (_receiver.Client.Available > 0)
			{
				try
				{
					IPEndPoint dummy = new IPEndPoint(IPAddress.Any, 0);
					_receiver.Receive(ref dummy);
				}
				catch { }
			}
		}

		private void DemonstrateFragmentation()
		{
			Console.WriteLine("   Фрагментация на уровне ОС:");

			if (_sender == null) return;

			// Размер MTU для локальной петли
			int mtu = 1500; // Стандартный Ethernet MTU
			int udpHeaderSize = 8;
			int ipHeaderSize = 20;
			int maxUdpPayload = mtu - udpHeaderSize - ipHeaderSize;

			Console.WriteLine($"\n   Рассчёт максимального размера UDP-пакета:");
			Console.WriteLine($"     Ethernet MTU: {mtu} байт");
			Console.WriteLine($"     IP заголовок: {ipHeaderSize} байт");
			Console.WriteLine($"     UDP заголовок: {udpHeaderSize} байт");
			Console.WriteLine($"     Максимальный payload: {maxUdpPayload} байт");

			// Тест отправки пакетов разного размера
			Console.WriteLine($"\n   Тест отправки пакетов разного размера:");

			var testSizes = new[] { 100, 500, 1000, 1500, 2000, 5000, 10000 };

			foreach (int size in testSizes)
			{
				try
				{
					byte[] data = new byte[size];
					new Random().NextBytes(data);

					Stopwatch sw = Stopwatch.StartNew();
					int bytesSent = _sender.Send(data, data.Length);
					sw.Stop();

					bool requiresFragmentation = size > maxUdpPayload;

					Console.WriteLine($"     Размер: {size} байт -> " +
									$"Отправлено: {bytesSent} байт, " +
									$"Время: {sw.ElapsedMilliseconds} мс, " +
									$"Фрагментация: {(requiresFragmentation ? "ДА" : "нет")}");

					if (requiresFragmentation)
					{
						Console.WriteLine($"       ⚠️  ОС разбила пакет на {(size + maxUdpPayload - 1) / maxUdpPayload} фрагментов");
					}

					Thread.Sleep(50);
				}
				catch (SocketException ex)
				{
					Console.WriteLine($"     Размер: {size} байт -> Ошибка: {ex.SocketErrorCode}");

					if (ex.SocketErrorCode == SocketError.MessageSize)
					{
						Console.WriteLine($"       ❌ ОС отказалась отправлять (превышен максимальный размер)");
					}
				}
			}

			// Запрет фрагментации
			Console.WriteLine($"\n   Запрет фрагментации через ОС:");

			if (_receiver != null)
			{
				_receiver.Client.DontFragment = true;
				Console.WriteLine($"     DontFragment = true");

				try
				{
					byte[] largeData = new byte[maxUdpPayload + 100]; // Больше MTU
					int sent = _sender.Send(largeData, largeData.Length);
					Console.WriteLine($"     Отправка {largeData.Length} байт: {sent} байт отправлено");
				}
				catch (SocketException ex) when (ex.SocketErrorCode == SocketError.MessageSize)
				{
					Console.WriteLine($"     ОС отказалась отправлять: {ex.Message}");
					Console.WriteLine($"     Пакет был бы фрагментирован, но DontFragment запрещает это");
				}
			}
		}

		private void DemonstrateAsyncNotifications()
		{
			Console.WriteLine("   Асинхронность и уведомления от ОС:");

			if (_receiver == null) return;

			Console.WriteLine($"\n   Асинхронный приём через BeginReceive/EndReceive:");

			// Традиционный асинхронный подход
			var asyncResult = _receiver.BeginReceive(null, null);

			// Отправка тестового пакета
			if (_sender != null)
			{
				byte[] testData = Encoding.UTF8.GetBytes("Асинхронный тест");
				_sender.Send(testData, testData.Length);
			}

			// Ожидание завершения операции (реально ждём уведомления от ОС)
			if (asyncResult.AsyncWaitHandle.WaitOne(2000))
			{
				IPEndPoint remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);
				byte[] data = _receiver.EndReceive(asyncResult, ref remoteEndpoint);

				Console.WriteLine($"     ОС уведомила о данных, получено: {data.Length} байт");
			}
			else
			{
				Console.WriteLine($"     ОС не отправила уведомление (таймаут)");
			}

			Console.WriteLine($"\n   Современный асинхронный подход (async/await):");

			// Запуск асинхронного приёма
			var receiveTask = _receiver.ReceiveAsync();

			if (_sender != null)
			{
				byte[] testData = Encoding.UTF8.GetBytes("Async/await тест");
				_sender.Send(testData, testData.Length);
			}

			if (receiveTask.Wait(2000))
			{
				Console.WriteLine($"     ОС уведомила, получен пакет от {receiveTask.Result.RemoteEndPoint}");
			}
			else
			{
				Console.WriteLine($"     ОС не уведомила о данных");
			}

			// Демонстрация множественных асинхронных операций
			Console.WriteLine($"\n   Множественные асинхронные операции:");

			var tasks = new List<Task>();
			for (int i = 0; i < 3; i++)
			{
				tasks.Add(Task.Run(async () =>
				{
					try
					{
						var result = await _receiver.ReceiveAsync();
						Console.WriteLine($"     Задача {Task.CurrentId} получила {result.Buffer.Length} байт");
					}
					catch (Exception ex)
					{
						Console.WriteLine($"     Задача {Task.CurrentId} ошибка: {ex.Message}");
					}
				}));
			}

			// Отправка нескольких пакетов
			if (_sender != null)
			{
				for (int i = 0; i < 3; i++)
				{
					byte[] data = Encoding.UTF8.GetBytes($"Пакет {i + 1}");
					_sender.Send(data, data.Length);
					Thread.Sleep(100);
				}
			}

			Task.WaitAll(tasks.ToArray(), 3000);
		}

		private void DemonstrateNetworkObstacles()
		{
			Console.WriteLine("   NAT и Firewall - препятствия на уровне ОС/сети:");

			Console.WriteLine($"\n   Firewall операционной системы:");
			Console.WriteLine($"     • Может блокировать входящие UDP-пакеты");
			Console.WriteLine($"     • Может требовать разрешения для приложения");
			Console.WriteLine($"     • Пакеты отбрасываются до попадания в сетевой стек ОС");

			// Попытка определить локальные IP-адреса
			Console.WriteLine($"\n   Определение сетевых интерфейсов:");
			try
			{
				string hostName = Dns.GetHostName();
				IPAddress[] addresses = Dns.GetHostAddresses(hostName);

				Console.WriteLine($"     Имя хоста: {hostName}");
				Console.WriteLine($"     IP-адреса:");
				foreach (var addr in addresses)
				{
					Console.WriteLine($"       - {addr} ({addr.AddressFamily})");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"     Ошибка получения адресов: {ex.Message}");
			}

			Console.WriteLine($"\n   Проблемы с NAT (Network Address Translation):");
			Console.WriteLine($"     • Внешние пакеты не доходят без активного сопоставления");
			Console.WriteLine($"     • NAT 'забывает' сопоставление через 30-300 секунд");
			Console.WriteLine($"     • Решение: регулярные keep-alive пакеты");

			// Демонстрация keep-alive для NAT
			Console.WriteLine($"\n   Keep-alive для поддержания NAT:");
			if (_sender != null)
			{
				Console.WriteLine($"     Отправка keep-alive каждые 30 секунд имитирует активность");

				// В реальном приложении здесь был бы таймер
				byte[] keepAlive = Encoding.UTF8.GetBytes("KEEPALIVE");
				_sender.Send(keepAlive, keepAlive.Length);
				Console.WriteLine($"     Keep-alive отправлен");
			}

			Console.WriteLine($"\n   Рекомендации для работы через NAT/Firewall:");
			Console.WriteLine($"     1. Использовать STUN/TURN серверы для определения внешнего адреса");
			Console.WriteLine($"     2. Регулярные keep-alive пакеты (каждые 20-30 секунд)");
			Console.WriteLine($"     3. Обработка симметричного NAT (самый строгий тип)");
			Console.WriteLine($"     4. Fallback на TCP/TLS если UDP недоступен");
		}

		public void Dispose()
		{
			_isRunning = false;

			_receiver?.Close();
			_sender?.Close();

			_receiverThread?.Join(1000);

			Console.WriteLine($"\n   Ресурсы освобождены, порт {_port} освобождён в ОС");
		}
	}

	// Пример реального приложения с учетом взаимодействия с ОС
	public class UdpServerWithOsOptimization : IDisposable
	{
		private UdpClient _udpClient;
		private Thread _receiverThread;
		private bool _isRunning;
		private int _port;
		private long _totalReceived = 0;
		private long _totalDropped = 0;

		public UdpServerWithOsOptimization(int port)
		{
			_port = port;

			// Оптимизация взаимодействия с ОС
			InitializeWithOsOptimizations();
		}

		private void InitializeWithOsOptimizations()
		{
			Console.WriteLine($"\n[Инициализация сервера с оптимизацией для ОС]");

			// 1. Создание сокета с привязкой к порту
			_udpClient = new UdpClient(_port);
			Socket socket = _udpClient.Client;

			// 2. Увеличение буферов ОС для уменьшения потерь
			socket.ReceiveBufferSize = 4 * 1024 * 1024; // 4 MB
			socket.SendBufferSize = 4 * 1024 * 1024;    // 4 MB

			Console.WriteLine($"   Буферы ОС увеличены до {socket.ReceiveBufferSize / 1024 / 1024} MB");

			// 3. Разрешение повторного использования порта
			socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

			// 4. Настройка TTL (Time To Live)
			socket.Ttl = 64;

			Console.WriteLine($"   Порт {_port} зарегистрирован в ОС");
			Console.WriteLine($"   PID: {Process.GetCurrentProcess().Id}");
		}

		public void Start()
		{
			_isRunning = true;
			_receiverThread = new Thread(ReceiveLoop);
			_receiverThread.IsBackground = true;
			_receiverThread.Start();

			Console.WriteLine($"\n[Сервер запущен]");
			Console.WriteLine($"   Поток приёма: {_receiverThread.ManagedThreadId}");
			Console.WriteLine($"   Порт: {_port}");
		}

		private void ReceiveLoop()
		{
			Console.WriteLine($"[Поток приёма] Начало работы, ожидание данных от ОС...");

			byte[] buffer = new byte[65507]; // Максимальный размер UDP-пакета

			while (_isRunning)
			{
				try
				{
					// Блокирующий вызов - поток ждёт в ОС
					// Используем Socket.ReceiveFrom с EndPoint
					EndPoint remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);
					int bytesRead = _udpClient.Client.ReceiveFrom(buffer, SocketFlags.None, ref remoteEndpoint);

					Interlocked.Increment(ref _totalReceived);

					// Преобразуем в IPEndPoint
					IPEndPoint ipEndpoint = remoteEndpoint as IPEndPoint;
					if (ipEndpoint != null)
					{
						// Быстрая обработка для предотвращения переполнения буфера
						ProcessPacket(buffer, bytesRead, ipEndpoint);
					}

					// Проверка уровня заполнения буфера ОС
					if (_udpClient.Client.Available > _udpClient.Client.ReceiveBufferSize * 0.8)
					{
						Console.WriteLine($"[ВНИМАНИЕ] Буфер ОС заполнен на {(_udpClient.Client.Available * 100.0 / _udpClient.Client.ReceiveBufferSize):F1}%");
					}
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
		}

		private void ProcessPacket(byte[] data, int length, IPEndPoint remoteEndpoint)
		{
			// Быстрая обработка пакета
			// В реальном приложении здесь была бы бизнес-логика

			// Пример: эхо-ответ
			try
			{
				_udpClient.Send(data, length, remoteEndpoint);
			}
			catch
			{
				Interlocked.Increment(ref _totalDropped);
			}
		}

		public void PrintStatistics()
		{
			Console.WriteLine($"\n[Статистика сервера]");
			Console.WriteLine($"   Принято пакетов: {_totalReceived}");
			Console.WriteLine($"   Потеряно пакетов: {_totalDropped}");
			Console.WriteLine($"   Доступно в буфере ОС: {_udpClient.Client.Available}");
			Console.WriteLine($"   Размер буфера ОС: {_udpClient.Client.ReceiveBufferSize}");
		}

		public void Dispose()
		{
			_isRunning = false;

			// Корректное освобождение ресурсов ОС
			if (_udpClient != null)
			{
				try
				{
					_udpClient.Client.Shutdown(SocketShutdown.Receive);
				}
				catch { }

				_udpClient.Close();
			}

			_receiverThread?.Join(1000);

			Console.WriteLine($"\n[Сервер остановлен] Ресурсы ОС освобождены");
		}
	}

	// Главная программа
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("UDP И ВЗАИМОДЕЙСТВИЕ С ОПЕРАЦИОННОЙ СИСТЕМОЙ");
			Console.WriteLine("=============================================\n");

			using (var demo = new UdpOsInteractionDemo())
			{
				demo.DemonstrateUdpBasics();
			}

			// Пример реального сервера с оптимизациями
			Console.WriteLine("\n\n=== ПРИМЕР РЕАЛЬНОГО СЕРВЕРА С ОПТИМИЗАЦИЯМИ ===");

			using (var server = new UdpServerWithOsOptimization(11051))
			{
				server.Start();

				// Даём поработать серверу
				Thread.Sleep(2000);

				server.PrintStatistics();
			}
		}
	}
}