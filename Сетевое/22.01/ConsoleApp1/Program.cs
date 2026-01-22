using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace SocketArchitectureDemo
{
	// Архитектура: Слой управления состояниями соединения
	public enum ConnectionState
	{
		Created,     // Сокет создан
		Bound,       // Привязан к конечной точке
		Listening,   // Ожидает подключений (сервер)
		Connecting,  // Устанавливает соединение (клиент)
		Connected,   // Соединение установлено
		Receiving,   // Получает данные
		Sending,     // Отправляет данные
		Closing,     // Закрывается
		Closed,      // Закрыт
		Error        // Ошибка
	}

	// Архитектура: Абстракция конечной точки с контекстом
	public class NetworkEndpoint
	{
		public IPAddress Address { get; }
		public int Port { get; }
		public string Identifier { get; }
		public DateTime CreatedAt { get; }

		public NetworkEndpoint(IPAddress address, int port, string identifier = null)
		{
			Address = address ?? throw new ArgumentNullException(nameof(address));
			Port = port;
			Identifier = identifier ?? $"{address}:{port}";
			CreatedAt = DateTime.UtcNow;
		}

		public override string ToString() => Identifier;

		public static NetworkEndpoint FromString(string endpoint, string identifier = null)
		{
			var parts = endpoint.Split(':');
			if (parts.Length != 2)
				throw new ArgumentException("Формат конечной точки: адрес:порт");

			return new NetworkEndpoint(
				IPAddress.Parse(parts[0]),
				int.Parse(parts[1]),
				identifier
			);
		}
	}

	// Архитектура: Базовый класс для сокетного взаимодействия (инкапсуляция слоя)
	public abstract class SocketInteractionLayer : IDisposable
	{
		protected Socket _socket;
		protected ConnectionState _state;
		protected readonly object _stateLock = new object();
		protected readonly NetworkEndpoint _endpoint;
		protected readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		// Архитектура: События для уведомления о изменениях состояния
		public event EventHandler<ConnectionState> StateChanged;
		public event EventHandler<string> ErrorOccurred;
		public event EventHandler<byte[]> DataReceived;
		public event EventHandler<int> DataSent;

		public ConnectionState CurrentState
		{
			get
			{
				lock (_stateLock) return _state;
			}
			protected set
			{
				lock (_stateLock)
				{
					var oldState = _state;
					_state = value;
					OnStateChanged(oldState, value);
				}
			}
		}

		public NetworkEndpoint Endpoint => _endpoint;
		public bool IsActive => CurrentState != ConnectionState.Closed &&
							   CurrentState != ConnectionState.Error;

		protected SocketInteractionLayer(NetworkEndpoint endpoint)
		{
			_endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
			_state = ConnectionState.Created;
		}

		// Архитектура: Абстрактные методы, которые должны реализовать конкретные роли
		protected abstract void InitializeSocket();
		protected abstract Task EstablishConnectionAsync(CancellationToken cancellationToken);

		// Архитектура: Общая логика создания сокета
		protected virtual Socket CreateSocket()
		{
			var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			// Настройка параметров ОС через сокет
			socket.LingerState = new LingerOption(true, 3);
			socket.NoDelay = true;
			socket.ReceiveBufferSize = 8192;
			socket.SendBufferSize = 8192;
			socket.ReceiveTimeout = 30000;
			socket.SendTimeout = 30000;

			return socket;
		}

		// Архитектура: Переходы между состояниями
		protected bool TransitionToState(ConnectionState newState, bool validateTransition = true)
		{
			lock (_stateLock)
			{
				var oldState = _state;

				// Валидация переходов
				if (validateTransition && !IsValidTransition(oldState, newState))
				{
					var error = $"Недопустимый переход состояния: {oldState} -> {newState}";
					Console.WriteLine($"[{_endpoint.Identifier}] {error}");

					// Вызываем OnErrorOccurred напрямую, а не через TransitionToState
					_state = ConnectionState.Error;
					OnErrorOccurred(error, false); // false - не валидируем переход
					return false;
				}

				_state = newState;
				OnStateChanged(oldState, newState);
				Console.WriteLine($"[{_endpoint.Identifier}] Переход состояния: {oldState} -> {newState}");
				return true;
			}
		}

		private bool IsValidTransition(ConnectionState from, ConnectionState to)
		{
			// Архитектура: Матрица допустимых переходов состояний
			var validTransitions = new Dictionary<ConnectionState, List<ConnectionState>>
			{
				[ConnectionState.Created] = new List<ConnectionState>
					{ ConnectionState.Bound, ConnectionState.Connecting, ConnectionState.Closing, ConnectionState.Error },
				[ConnectionState.Bound] = new List<ConnectionState>
					{ ConnectionState.Listening, ConnectionState.Closing, ConnectionState.Error },
				[ConnectionState.Listening] = new List<ConnectionState>
					{ ConnectionState.Connected, ConnectionState.Closing, ConnectionState.Error },
				[ConnectionState.Connecting] = new List<ConnectionState>
					{ ConnectionState.Connected, ConnectionState.Closing, ConnectionState.Error },
				[ConnectionState.Connected] = new List<ConnectionState>
					{ ConnectionState.Receiving, ConnectionState.Sending, ConnectionState.Closing, ConnectionState.Error },
				[ConnectionState.Receiving] = new List<ConnectionState>
					{ ConnectionState.Connected, ConnectionState.Closing, ConnectionState.Error },
				[ConnectionState.Sending] = new List<ConnectionState>
					{ ConnectionState.Connected, ConnectionState.Closing, ConnectionState.Error },
				[ConnectionState.Closing] = new List<ConnectionState>
					{ ConnectionState.Closed, ConnectionState.Error },
				[ConnectionState.Closed] = new List<ConnectionState> { },
				[ConnectionState.Error] = new List<ConnectionState> { ConnectionState.Closing, ConnectionState.Closed }
			};

			return validTransitions.ContainsKey(from) &&
				   validTransitions[from].Contains(to);
		}

		// Архитектура: Защищённые методы для отправки событий
		protected virtual void OnStateChanged(ConnectionState oldState, ConnectionState newState)
		{
			StateChanged?.Invoke(this, newState);
		}

		protected virtual void OnErrorOccurred(string error, bool validateTransition = true)
		{
			Console.WriteLine($"[{_endpoint.Identifier}] Ошибка: {error}");

			// Переход в состояние Error без валидации, чтобы избежать рекурсии
			lock (_stateLock)
			{
				var oldState = _state;
				_state = ConnectionState.Error;
				ErrorOccurred?.Invoke(this, error);
				OnStateChanged(oldState, ConnectionState.Error);
			}
		}

		protected virtual void OnDataReceived(byte[] data)
		{
			DataReceived?.Invoke(this, data);
		}

		protected virtual void OnDataSent(int bytesSent)
		{
			DataSent?.Invoke(this, bytesSent);
		}

		// Архитектура: Обработка сетевых ошибок
		protected void HandleSocketError(SocketException ex, string operation)
		{
			var error = $"Сетевая ошибка при {operation}: {ex.SocketErrorCode} - {ex.Message}";
			OnErrorOccurred(error);
		}

		// Архитектура: Безопасное освобождение ресурсов
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				_cancellationTokenSource.Cancel();
				_cancellationTokenSource.Dispose();

				if (_socket != null)
				{
					try
					{
						if (_socket.Connected)
						{
							TransitionToState(ConnectionState.Closing, false);
							_socket.Shutdown(SocketShutdown.Both);
						}
					}
					catch { }

					_socket.Close();
					_socket.Dispose();
					_socket = null;
				}

				TransitionToState(ConnectionState.Closed, false);
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~SocketInteractionLayer()
		{
			Dispose(false);
		}
	}

	// Архитектура: Сторона, ожидающая взаимодействия (серверная роль)
	public class SocketAcceptor : SocketInteractionLayer
	{
		private readonly ConcurrentDictionary<string, SocketConnection> _activeConnections = new();
		private Task _acceptLoopTask;

		public int ActiveConnectionsCount => _activeConnections.Count;

		public SocketAcceptor(NetworkEndpoint endpoint) : base(endpoint)
		{
		}

		// Архитектура: Инициализация сокета для ожидания подключений
		protected override void InitializeSocket()
		{
			try
			{
				_socket = CreateSocket();
				_socket.Bind(new IPEndPoint(_endpoint.Address, _endpoint.Port));
				TransitionToState(ConnectionState.Bound);

				_socket.Listen(100);
				TransitionToState(ConnectionState.Listening);

				Console.WriteLine($"[Acceptor {_endpoint}] Ожидает подключений на {_endpoint.Address}:{_endpoint.Port}");
			}
			catch (Exception ex)
			{
				OnErrorOccurred($"Ошибка при инициализации сокета: {ex.Message}");
				throw;
			}
		}

		protected override async Task EstablishConnectionAsync(CancellationToken cancellationToken)
		{
			// Архитектура: Асинхронный цикл приёма подключений
			_acceptLoopTask = Task.Run(async () =>
			{
				Console.WriteLine($"[Acceptor {_endpoint}] Запущен цикл приёма подключений");

				while (!cancellationToken.IsCancellationRequested &&
					   CurrentState == ConnectionState.Listening)
				{
					try
					{
						// Архитектура: Асинхронное ожидание подключения
						var clientSocket = await Task.Factory.FromAsync(
							_socket.BeginAccept(null, null),
							_socket.EndAccept);

						if (clientSocket != null)
						{
							var clientEndpoint = (IPEndPoint)clientSocket.RemoteEndPoint;
							var connectionId = $"{clientEndpoint.Address}:{clientEndpoint.Port}";

							Console.WriteLine($"[Acceptor {_endpoint}] Принято подключение от {connectionId}");

							// Архитектура: Создание отдельного объекта для управления соединением
							var connection = new SocketConnection(
								new NetworkEndpoint(clientEndpoint.Address, clientEndpoint.Port, connectionId),
								clientSocket);

							_activeConnections[connectionId] = connection;

							// Архитектура: Запуск обработки соединения в отдельном контексте
							_ = Task.Run(() => HandleConnectionAsync(connection, cancellationToken), cancellationToken);
						}
					}
					catch (OperationCanceledException)
					{
						Console.WriteLine($"[Acceptor {_endpoint}] Приём подключений прерван");
						break;
					}
					catch (SocketException ex)
					{
						HandleSocketError(ex, "приёме подключения");
						break;
					}
					catch (Exception ex)
					{
						OnErrorOccurred($"Ошибка при приёме подключения: {ex.Message}");
						await Task.Delay(1000, cancellationToken);
					}
				}

				Console.WriteLine($"[Acceptor {_endpoint}] Цикл приёма подключений завершён");
			}, cancellationToken);

			await _acceptLoopTask;
		}

		// Архитектура: Обработка отдельного соединения
		private async Task HandleConnectionAsync(SocketConnection connection, CancellationToken cancellationToken)
		{
			Console.WriteLine($"[Connection {connection.Endpoint}] Начало обработки");

			try
			{
				// Архитектура: Цикл взаимодействия
				while (!cancellationToken.IsCancellationRequested &&
					   connection.CurrentState == ConnectionState.Connected)
				{
					// Архитектура: Асинхронный приём данных
					var data = await connection.ReceiveAsync(cancellationToken);
					if (data == null || data.Length == 0)
					{
						Console.WriteLine($"[Connection {connection.Endpoint}] Соединение закрыто удалённой стороной");
						break;
					}

					// Архитектура: Обработка полученных данных
					Console.WriteLine($"[Connection {connection.Endpoint}] Получено {data.Length} байт");
					OnDataReceived(data);

					// Архитектура: Эхо-ответ (пример бизнес-логики)
					var response = Encoding.UTF8.GetBytes($"Эхо: {Encoding.UTF8.GetString(data)}");
					await connection.SendAsync(response, cancellationToken);

					Console.WriteLine($"[Connection {connection.Endpoint}] Отправлено {response.Length} байт");
				}
			}
			catch (OperationCanceledException)
			{
				Console.WriteLine($"[Connection {connection.Endpoint}] Обработка прервана");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[Connection {connection.Endpoint}] Ошибка обработки: {ex.Message}");
			}
			finally
			{
				// Архитектура: Корректное завершение соединения
				connection.Dispose();
				_activeConnections.TryRemove(connection.Endpoint.Identifier, out _);

				Console.WriteLine($"[Connection {connection.Endpoint}] Обработка завершена. Активных соединений: {_activeConnections.Count}");
			}
		}

		public async Task StartAsync(CancellationToken cancellationToken = default)
		{
			if (CurrentState != ConnectionState.Created)
				throw new InvalidOperationException("Acceptor уже запущен");

			InitializeSocket();
			await EstablishConnectionAsync(cancellationToken);
		}

		public void Stop()
		{
			Console.WriteLine($"[Acceptor {_endpoint}] Остановка...");

			// Архитектура: Остановка всех активных соединений
			foreach (var connection in _activeConnections.Values)
			{
				connection.Dispose();
			}
			_activeConnections.Clear();

			Dispose();
		}
	}

	// Архитектура: Сторона, инициирующая взаимодействие (клиентская роль)
	public class SocketConnector : SocketInteractionLayer
	{
		public SocketConnector(NetworkEndpoint endpoint) : base(endpoint)
		{
		}

		protected override void InitializeSocket()
		{
			try
			{
				_socket = CreateSocket();
				Console.WriteLine($"[Connector {_endpoint}] Сокет создан");
			}
			catch (Exception ex)
			{
				OnErrorOccurred($"Ошибка при создании сокета: {ex.Message}");
				throw;
			}
		}

		protected override async Task EstablishConnectionAsync(CancellationToken cancellationToken)
		{
			if (!TransitionToState(ConnectionState.Connecting))
			{
				throw new InvalidOperationException("Не удалось перейти в состояние Connecting");
			}

			Console.WriteLine($"[Connector {_endpoint}] Установка соединения с {_endpoint.Address}:{_endpoint.Port}...");

			try
			{
				// Архитектура: Асинхронное подключение
				await Task.Factory.FromAsync(
					_socket.BeginConnect(_endpoint.Address, _endpoint.Port, null, null),
					_socket.EndConnect);

				TransitionToState(ConnectionState.Connected);
				Console.WriteLine($"[Connector {_endpoint}] Соединение установлено");
			}
			catch (SocketException ex) when (ex.SocketErrorCode == SocketError.ConnectionRefused ||
											ex.SocketErrorCode == SocketError.TimedOut)
			{
				// Это ожидаемые ошибки при подключении к несуществующему серверу
				OnErrorOccurred($"Не удалось подключиться: {ex.SocketErrorCode}");
				throw;
			}
			catch (SocketException ex)
			{
				HandleSocketError(ex, "установке соединения");
				throw;
			}
			catch (Exception ex)
			{
				OnErrorOccurred($"Неизвестная ошибка при подключении: {ex.Message}");
				throw;
			}
		}

		// Архитектура: Отправка данных
		public async Task SendAsync(byte[] data, CancellationToken cancellationToken = default)
		{
			if (CurrentState != ConnectionState.Connected)
				throw new InvalidOperationException("Соединение не установлено");

			if (!TransitionToState(ConnectionState.Sending))
				return;

			try
			{
				int totalSent = 0;

				// Архитектура: Отправка данных по частям
				while (totalSent < data.Length && !cancellationToken.IsCancellationRequested)
				{
					int sent = await Task.Factory.FromAsync<int>(
						(callback, state) => _socket.BeginSend(
							data, totalSent, data.Length - totalSent,
							SocketFlags.None, callback, state),
						_socket.EndSend,
						null);

					if (sent == 0)
						throw new SocketException((int)SocketError.ConnectionReset);

					totalSent += sent;
					OnDataSent(sent);

					Console.WriteLine($"[Connector {_endpoint}] Отправлено {sent} байт, всего {totalSent}/{data.Length}");
				}

				TransitionToState(ConnectionState.Connected);
				Console.WriteLine($"[Connector {_endpoint}] Всего отправлено {totalSent} байт");
			}
			catch (SocketException ex)
			{
				HandleSocketError(ex, "отправке данных");
				throw;
			}
		}

		// Архитектура: Получение данных
		public async Task<byte[]> ReceiveAsync(CancellationToken cancellationToken = default)
		{
			if (CurrentState != ConnectionState.Connected)
				throw new InvalidOperationException("Соединение не установлено");

			if (!TransitionToState(ConnectionState.Receiving))
				return null;

			try
			{
				var buffer = new byte[4096];

				// Архитектура: Асинхронное получение
				int bytesRead = await Task.Factory.FromAsync<int>(
					(callback, state) => _socket.BeginReceive(
						buffer, 0, buffer.Length,
						SocketFlags.None, callback, state),
					_socket.EndReceive,
					null);

				TransitionToState(ConnectionState.Connected);

				if (bytesRead == 0)
				{
					Console.WriteLine($"[Connector {_endpoint}] Соединение закрыто удалённой стороной");
					return null;
				}

				var receivedData = new byte[bytesRead];
				Array.Copy(buffer, 0, receivedData, 0, bytesRead);

				OnDataReceived(receivedData);
				Console.WriteLine($"[Connector {_endpoint}] Получено {bytesRead} байт");

				return receivedData;
			}
			catch (SocketException ex)
			{
				HandleSocketError(ex, "получении данных");
				throw;
			}
		}

		public async Task ConnectAsync(CancellationToken cancellationToken = default)
		{
			if (CurrentState != ConnectionState.Created && CurrentState != ConnectionState.Error)
				throw new InvalidOperationException("Connector уже используется");

			InitializeSocket();
			await EstablishConnectionAsync(cancellationToken);
		}
	}

	// Архитектура: Управление активным соединением
	public class SocketConnection : SocketInteractionLayer
	{
		public SocketConnection(NetworkEndpoint endpoint, Socket socket) : base(endpoint)
		{
			_socket = socket ?? throw new ArgumentNullException(nameof(socket));
			TransitionToState(ConnectionState.Connected, false);
		}

		protected override void InitializeSocket()
		{
			// Уже инициализировано в конструкторе
		}

		protected override Task EstablishConnectionAsync(CancellationToken cancellationToken)
		{
			// Соединение уже установлено
			return Task.CompletedTask;
		}

		public async Task<byte[]> ReceiveAsync(CancellationToken cancellationToken)
		{
			if (CurrentState != ConnectionState.Connected)
				throw new InvalidOperationException("Соединение не установлено");

			if (!TransitionToState(ConnectionState.Receiving))
				return null;

			try
			{
				var buffer = new byte[4096];
				int bytesRead = await Task.Factory.FromAsync<int>(
					(callback, state) => _socket.BeginReceive(
						buffer, 0, buffer.Length,
						SocketFlags.None, callback, state),
					_socket.EndReceive,
					null);

				TransitionToState(ConnectionState.Connected);

				if (bytesRead == 0)
					return null;

				var data = new byte[bytesRead];
				Array.Copy(buffer, 0, data, 0, bytesRead);

				return data;
			}
			catch (SocketException ex)
			{
				HandleSocketError(ex, "получении данных");
				return null;
			}
		}

		public async Task SendAsync(byte[] data, CancellationToken cancellationToken)
		{
			if (CurrentState != ConnectionState.Connected)
				throw new InvalidOperationException("Соединение не установлено");

			if (!TransitionToState(ConnectionState.Sending))
				return;

			try
			{
				int totalSent = 0;
				while (totalSent < data.Length && !cancellationToken.IsCancellationRequested)
				{
					int sent = await Task.Factory.FromAsync<int>(
						(callback, state) => _socket.BeginSend(
							data, totalSent, data.Length - totalSent,
							SocketFlags.None, callback, state),
						_socket.EndSend,
						null);

					totalSent += sent;
				}

				TransitionToState(ConnectionState.Connected);
			}
			catch (SocketException ex)
			{
				HandleSocketError(ex, "отправке данных");
				throw;
			}
		}
	}

	// Архитектура: Демонстрационный модуль
	public static class ArchitectureDemonstration
	{
		public static async Task RunDemoAsync()
		{
			Console.WriteLine("=== АРХИТЕКТУРА ВЗАИМОДЕЙСТВИЯ ЧЕРЕЗ СОКЕТЫ ===\n");

			// Часть 1: Демонстрация состояний и переходов
			Console.WriteLine("1. ДЕМОНСТРАЦИЯ СОСТОЯНИЙ И ПЕРЕХОДОВ:");
			await DemonstrateStateTransitions();

			// Часть 2: Архитектура взаимодействия
			Console.WriteLine("\n\n2. АРХИТЕКТУРА ВЗАИМОДЕЙСТВИЯ:");
			await DemonstrateInteractionArchitecture();

			// Часть 3: Обработка ошибок и отказоустойчивость
			Console.WriteLine("\n\n3. ОТКАЗОУСТОЙЧИВОСТЬ:");
			await DemonstrateFaultTolerance();

			// Часть 4: Масштабирование
			Console.WriteLine("\n\n4. МАСШТАБИРУЕМОСТЬ:");
			await DemonstrateScalability();
		}

		private static async Task DemonstrateStateTransitions()
		{
			Console.WriteLine("   Создание и управление состояниями сокета:");

			var endpoint = new NetworkEndpoint(IPAddress.Loopback, 11001, "StateTest");

			using (var connector = new SocketConnector(endpoint))
			{
				connector.StateChanged += (sender, state) =>
				{
					Console.WriteLine($"     [State Change] {state}");
				};

				connector.ErrorOccurred += (sender, error) =>
				{
					Console.WriteLine($"     [Error] {error}");
				};

				try
				{
					Console.WriteLine($"\n   Исходное состояние: {connector.CurrentState}");
					Console.WriteLine($"   Попытка подключения к несуществующему серверу...");

					// Используем короткий таймаут для демонстрации
					var cts = new CancellationTokenSource(2000);
					await connector.ConnectAsync(cts.Token);
				}
				catch (SocketException ex)
				{
					Console.WriteLine($"   Ожидаемая ошибка подключения: {ex.SocketErrorCode}");
				}
				catch (OperationCanceledException)
				{
					Console.WriteLine($"   Подключение отменено по таймауту");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"   Другая ошибка: {ex.Message}");
				}

				Console.WriteLine($"\n   Финальное состояние: {connector.CurrentState}");
			}
		}

		private static async Task DemonstrateInteractionArchitecture()
		{
			Console.WriteLine("   Архитектура клиент-серверного взаимодействия:");

			var serverEndpoint = new NetworkEndpoint(IPAddress.Loopback, 11002, "Server");
			var clientEndpoint = new NetworkEndpoint(IPAddress.Loopback, 11002, "Client");

			var cts = new CancellationTokenSource();
			cts.CancelAfter(TimeSpan.FromSeconds(8));

			// Архитектура: Запуск сервера
			var server = new SocketAcceptor(serverEndpoint);

			server.StateChanged += (sender, state) =>
			{
				Console.WriteLine($"     [Server State] {state}");
			};

			// Запуск сервера в фоне
			var serverTask = Task.Run(async () =>
			{
				try
				{
					await server.StartAsync(cts.Token);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"     [Server Error] {ex.Message}");
				}
			}, cts.Token);

			await Task.Delay(1000); // Даём время серверу запуститься

			// Архитектура: Подключение клиента
			using (var client = new SocketConnector(clientEndpoint))
			{
				try
				{
					Console.WriteLine($"\n   Подключение клиента...");
					await client.ConnectAsync(cts.Token);

					Console.WriteLine($"   Отправка тестового сообщения...");
					var message = "Тестовое сообщение";
					var data = Encoding.UTF8.GetBytes(message);

					await client.SendAsync(data, cts.Token);
					Console.WriteLine($"     Отправлено: {message}");

					var response = await client.ReceiveAsync(cts.Token);
					if (response != null)
					{
						var responseText = Encoding.UTF8.GetString(response);
						Console.WriteLine($"     Получено: {responseText}");
					}

					Console.WriteLine($"   Отключение...");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"     [Client Error] {ex.Message}");
				}
			}

			// Остановка сервера
			server.Stop();
			await Task.Delay(500);
			cts.Cancel();

			try { await serverTask; } catch { }

			Console.WriteLine($"   Сервер обработал соединений: {server.ActiveConnectionsCount}");
		}

		private static async Task DemonstrateFaultTolerance()
		{
			Console.WriteLine("   Демонстрация отказоустойчивости:");

			var endpoint = new NetworkEndpoint(IPAddress.Loopback, 11003, "ResilienceTest");
			var cts = new CancellationTokenSource();
			cts.CancelAfter(TimeSpan.FromSeconds(5));

			// Архитектура: Сервер с обработкой ошибок
			var server = new SocketAcceptor(endpoint);

			var serverTask = Task.Run(async () =>
			{
				try
				{
					await server.StartAsync(cts.Token);
				}
				catch { }
			}, cts.Token);

			await Task.Delay(500);

			// Тесты отказоустойчивости
			Console.WriteLine($"\n   Тест 1: Множественные подключения");

			var clients = new List<SocketConnector>();
			for (int i = 0; i < 3; i++)
			{
				var client = new SocketConnector(endpoint);
				clients.Add(client);

				try
				{
					await client.ConnectAsync(cts.Token);
					Console.WriteLine($"     Клиент {i + 1}: Подключён");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"     Клиент {i + 1}: Ошибка - {ex.Message}");
				}
			}

			// Очистка
			foreach (var client in clients)
			{
				client.Dispose();
			}

			cts.Cancel();
			server.Stop();

			try { await serverTask; } catch { }

			Console.WriteLine($"\n   Тесты отказоустойчивости завершены");
		}

		private static async Task DemonstrateScalability()
		{
			Console.WriteLine("   Демонстрация масштабируемости:");

			var endpoint = new NetworkEndpoint(IPAddress.Loopback, 11004, "ScalabilityTest");
			var cts = new CancellationTokenSource();
			cts.CancelAfter(TimeSpan.FromSeconds(3));

			var server = new SocketAcceptor(endpoint);

			var serverTask = Task.Run(async () =>
			{
				try
				{
					await server.StartAsync(cts.Token);
				}
				catch { }
			}, cts.Token);

			await Task.Delay(500);

			// Множественные подключения
			var tasks = new List<Task>();

			Console.WriteLine($"\n   Создание 5 одновременных подключений...");

			for (int i = 0; i < 5; i++)
			{
				var task = Task.Run(async () =>
				{
					using (var client = new SocketConnector(endpoint))
					{
						try
						{
							await client.ConnectAsync(cts.Token);
							await client.SendAsync(Encoding.UTF8.GetBytes("Test"), cts.Token);
							await Task.Delay(100, cts.Token);
						}
						catch { }
					}
				}, cts.Token);

				tasks.Add(task);
			}

			try
			{
				await Task.WhenAll(tasks);
				await Task.Delay(500);
			}
			finally
			{
				cts.Cancel();
				server.Stop();
				try { await serverTask; } catch { }
			}

			Console.WriteLine($"   Демонстрация масштабируемости завершена");
		}
	}

	// Главная программа
	class Program
	{
		static async Task Main(string[] args)
		{
			try
			{
				await ArchitectureDemonstration.RunDemoAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"\nКритическая ошибка: {ex}");
			}
		}
	}
}