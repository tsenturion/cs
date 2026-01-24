using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static CommunicationProtocols.ComplexProtocolServer;

namespace CommunicationProtocols
{
	// Базовый класс для демонстрации протоколов общения
	public class ProtocolDemonstration
	{
		public static async Task DemonstrateProtocols()
		{
			Console.WriteLine("=== ПРОТОКОЛЫ ОБЩЕНИЯ ПОВЕРХ TCP ===\n");

			// 1. Простейший текстовый протокол
			Console.WriteLine("1. ТЕКСТОВЫЙ ПРОТОКОЛ (разделитель строк):");
			await DemonstrateTextProtocol();

			// 2. Протокол с указанием длины сообщения
			Console.WriteLine("\n2. ПРОТОКОЛ С УКАЗАНИЕМ ДЛИНЫ:");
			await DemonstrateLengthPrefixProtocol();

			// 3. Бинарный протокол с фиксированной структурой
			Console.WriteLine("\n3. БИНАРНЫЙ ПРОТОКОЛ С ФИКСИРОВАННОЙ СТРУКТУРОЙ:");
			await DemonstrateBinaryProtocol();

			// 4. Stateful протокол (с состоянием)
			Console.WriteLine("\n4. STATEFUL ПРОТОКОЛ (с сессиями):");
			await DemonstrateStatefulProtocol();

			// 5. Обработка ошибок и устойчивость
			Console.WriteLine("\n5. ОБРАБОТКА ОШИБОК В ПРОТОКОЛЕ:");
			await DemonstrateErrorHandling();

			// 6. Реализация комплексного протокола
			Console.WriteLine("\n6. КОМПЛЕКСНЫЙ ПРОТОКОЛ В ДЕЙСТВИИ:");
			await DemonstrateComplexProtocol();
		}

		private static async Task DemonstrateTextProtocol()
		{
			Console.WriteLine("   Протокол: каждое сообщение заканчивается \\n");

			const int port = 11040;
			var server = new TextLineServer(port);
			var serverTask = server.StartAsync();

			await Task.Delay(500);

			using (var client = new TextLineClient(port))
			{
				await client.ConnectAsync();

				Console.WriteLine("\n   Тест 1: Простая отправка сообщений");
				await client.SendAsync("HELLO");
				await client.SendAsync("USER Alice");
				await client.SendAsync("QUIT");

				var responses = await server.GetResponsesAsync(3, 1000);
				Console.WriteLine($"   Сервер получил сообщения:");
				foreach (var response in responses)
				{
					Console.WriteLine($"     - '{response}'");
				}

				Console.WriteLine("\n   Тест 2: Проблема многострочных сообщений");
				await client.SendAsync("MESSAGE Hello\\nWorld\\nHow are you?");
				var multilineResponse = await server.GetLastResponseAsync(1000);
				Console.WriteLine($"   Сервер получил: '{multilineResponse}'");
				Console.WriteLine($"   Проблема: \\n в данных ломает протокол");

				Console.WriteLine("\n   Тест 3: Разные размеры сообщений");
				var longMessage = new string('A', 5000);
				await client.SendAsync(longMessage);
				var longResponse = await server.GetLastResponseAsync(1000);
				Console.WriteLine($"   Сервер получил {longResponse.Length} символов");
				Console.WriteLine($"   Превышение длины буфера может вызвать проблемы");
			}

			server.Stop();
			await serverTask;

			Console.WriteLine("\n   Выводы:");
			Console.WriteLine("   + Простота реализации и отладки");
			Console.WriteLine("   + Читаемость данных");
			Console.WriteLine("   - Зависимость от разделителя в данных");
			Console.WriteLine("   - Проблемы с бинарными данными");
			Console.WriteLine("   - Нет защиты от переполнения буфера");
		}

		private static async Task DemonstrateLengthPrefixProtocol()
		{
			Console.WriteLine("   Протокол: [4 байта длины][данные]");

			const int port = 11041;
			var server = new LengthPrefixServer(port);
			var serverTask = server.StartAsync();

			await Task.Delay(500);

			using (var client = new LengthPrefixClient(port))
			{
				await client.ConnectAsync();

				Console.WriteLine("\n   Тест 1: Текстовые сообщения разной длины");
				await client.SendAsync("Short");
				await client.SendAsync("Medium length message");
				await client.SendAsync("Очень длинное сообщение на русском языке с дополнительными символами");

				var messages = await server.GetMessagesAsync(3, 1000);
				Console.WriteLine($"   Сервер получил {messages.Count} сообщений:");
				foreach (var msg in messages)
				{
					Console.WriteLine($"     - '{msg}' ({msg.Length} байт)");
				}

				Console.WriteLine("\n   Тест 2: Бинарные данные");
				var binaryData = new byte[] { 0x00, 0x01, 0x02, 0xFF, 0xFE, 0xFD };
				await client.SendAsync(binaryData);
				var binaryResponse = await server.GetLastBinaryMessageAsync(1000);
				Console.WriteLine($"   Сервер получил бинарные данные: {binaryResponse.Length} байт");

				Console.WriteLine("\n   Тест 3: Большие сообщения");
				var largeData = new byte[10000];
				new Random().NextBytes(largeData);
				await client.SendAsync(largeData);
				var largeResponse = await server.GetLastBinaryMessageAsync(2000);
				Console.WriteLine($"   Сервер получил {largeResponse.Length} байт");
				Console.WriteLine($"   Сообщение передано корректно независимо от содержимого");
			}

			server.Stop();
			await serverTask;

			Console.WriteLine("\n   Выводы:");
			Console.WriteLine("   + Надёжное разделение сообщений");
			Console.WriteLine("   + Поддержка бинарных данных");
			Console.WriteLine("   + Нет проблем с разделителями в данных");
			Console.WriteLine("   - Сложнее в отладке (нужен hex-просмотрщик)");
			Console.WriteLine("   - Требует строгого соблюдения формата");
		}

		private static async Task DemonstrateBinaryProtocol()
		{
			Console.WriteLine("   Бинарный протокол с фиксированной структурой:");
			Console.WriteLine("   Заголовок: [CommandID: 1 байт][DataLength: 2 байта]");

			const int port = 11042;
			var server = new BinaryProtocolServer(port);
			var serverTask = server.StartAsync();

			await Task.Delay(500);

			using (var client = new BinaryProtocolClient(port))
			{
				await client.ConnectAsync();

				Console.WriteLine("\n   Тест 1: Разные команды");
				await client.SendCommandAsync(0x01, "Login:Alice");
				await client.SendCommandAsync(0x02, "GetData:123");
				await client.SendCommandAsync(0x03, new byte[] { 0xAA, 0xBB, 0xCC });

				var commands = await server.GetCommandsAsync(3, 1000);
				Console.WriteLine($"   Сервер обработал {commands.Count} команд:");
				foreach (var cmd in commands)
				{
					Console.WriteLine($"     - ID: {cmd.CommandId}, Данные: {BitConverter.ToString(cmd.Data)}");
				}

				Console.WriteLine("\n   Тест 2: Валидация команд");
				try
				{
					// Попытка отправить неизвестную команду
					await client.SendCommandAsync(0xFF, "Unknown");
					Console.WriteLine($"   Ошибка: сервер должен был отклонить команду");
				}
				catch (ProtocolException ex)
				{
					Console.WriteLine($"   Ожидаемая ошибка: {ex.Message}");
				}

				Console.WriteLine("\n   Тест 3: Ограничение размера данных");
				try
				{
					var hugeData = new byte[65536]; // Больше, чем позволяет 2 байта
					await client.SendCommandAsync(0x01, hugeData);
					Console.WriteLine($"   Ошибка: размер должен быть ограничен");
				}
				catch (ProtocolException ex)
				{
					Console.WriteLine($"   Ожидаемая ошибка: {ex.Message}");
				}
			}

			server.Stop();
			await serverTask;

			Console.WriteLine("\n   Выводы:");
			Console.WriteLine("   + Эффективное использование сети");
			Console.WriteLine("   + Легко расширять новыми командами");
			Console.WriteLine("   + Строгая типизация");
			Console.WriteLine("   - Сложность чтения данных без документации");
			Console.WriteLine("   - Сложнее отладка");
		}

		private static async Task DemonstrateStatefulProtocol()
		{
			Console.WriteLine("   Stateful протокол с аутентификацией:");
			Console.WriteLine("   Состояния: NotAuthenticated → Authenticated → Processing");

			const int port = 11043;
			var server = new StatefulServer(port);
			var serverTask = server.StartAsync();

			await Task.Delay(500);

			using (var client = new StatefulClient(port))
			{
				await client.ConnectAsync();

				Console.WriteLine("\n   Тест 1: Попытка выполнить команду без аутентификации");
				try
				{
					await client.SendCommandAsync("GET_DATA");
					Console.WriteLine($"   Ошибка: команда должна быть отклонена");
				}
				catch (ProtocolException ex)
				{
					Console.WriteLine($"   Ожидаемая ошибка: {ex.Message}");
				}

				Console.WriteLine("\n   Тест 2: Корректная аутентификация");
				var authResult = await client.AuthenticateAsync("admin", "password123");
				Console.WriteLine($"   Аутентификация: {authResult}");

				Console.WriteLine("\n   Тест 3: Выполнение команд после аутентификации");
				await client.SendCommandAsync("GET_DATA");
				await client.SendCommandAsync("SET_VALUE 100");
				await client.SendCommandAsync("GET_STATS");

				var executedCommands = await server.GetExecutedCommandsAsync(3, 1000);
				Console.WriteLine($"   Выполнено команд: {executedCommands.Count}");

				Console.WriteLine("\n   Тест 4: Смена состояния");
				await client.LogoutAsync();
				Console.WriteLine($"   Клиент вышел из системы");

				try
				{
					await client.SendCommandAsync("GET_DATA");
					Console.WriteLine($"   Ошибка: команда должна быть отклонена после выхода");
				}
				catch (ProtocolException ex)
				{
					Console.WriteLine($"   Ожидаемая ошибка: {ex.Message}");
				}
			}

			server.Stop();
			await serverTask;

			Console.WriteLine("\n   Выводы:");
			Console.WriteLine("   + Безопасность (требуется аутентификация)");
			Console.WriteLine("   + Контекстные команды");
			Console.WriteLine("   + Управление сессиями");
			Console.WriteLine("   - Сложнее реализация");
			Console.WriteLine("   - Требует хранения состояния");
			Console.WriteLine("   - Обработка таймаутов сессий");
		}

		private static async Task DemonstrateErrorHandling()
		{
			Console.WriteLine("   Обработка ошибок в протоколе:");

			const int port = 11044;
			var server = new ErrorHandlingServer(port);
			var serverTask = server.StartAsync();

			await Task.Delay(500);

			using (var client = new ErrorHandlingClient(port))
			{
				await client.ConnectAsync();

				Console.WriteLine("\n   Тест 1: Неверный формат сообщения");
				var errorResponse = await client.SendRawAsync("INVALID_FORMAT");
				Console.WriteLine($"   Ответ сервера: {errorResponse}");

				Console.WriteLine("\n   Тест 2: Слишком длинное сообщение");
				var longMessage = new string('X', 10000);
				errorResponse = await client.SendRawAsync(longMessage);
				Console.WriteLine($"   Ответ сервера: {errorResponse}");

				Console.WriteLine("\n   Тест 3: Неизвестная команда");
				errorResponse = await client.SendRawAsync("UNKNOWN_COMMAND arg1 arg2");
				Console.WriteLine($"   Ответ сервера: {errorResponse}");

				Console.WriteLine("\n   Тест 4: Недостаточно параметров");
				errorResponse = await client.SendRawAsync("GET_DATA"); // Нужны параметры
				Console.WriteLine($"   Ответ сервера: {errorResponse}");

				Console.WriteLine("\n   Тест 5: Частичное сообщение (сбой в середине)");
				// Имитация отправки части сообщения
				await client.SendPartialAsync("GET_DATA param1");
				await Task.Delay(100);
				// Сервер не должен зависнуть в ожидании
				var status = await server.GetStatusAsync();
				Console.WriteLine($"   Статус сервера: {status}");

				Console.WriteLine("\n   Тест 6: Восстановление после ошибки");
				errorResponse = await client.SendRawAsync("VALID_COMMAND");
				Console.WriteLine($"   Нормальный ответ после ошибки: {errorResponse}");
			}

			server.Stop();
			await serverTask;

			Console.WriteLine("\n   Выводы:");
			Console.WriteLine("   + Устойчивость к невалидным данным");
			Console.WriteLine("   + Защита от DoS (ограничение размера)");
			Console.WriteLine("   + Информативные сообщения об ошибках");
			Console.WriteLine("   + Восстановление после ошибок");
			Console.WriteLine("   - Усложнение протокола");
			Console.WriteLine("   - Больше кода для обработки краевых случаев");
		}

		private static async Task DemonstrateComplexProtocol()
		{
			Console.WriteLine("   Комплексный протокол с расширяемостью:");
			Console.WriteLine("   Формат: [Version][Type][Length][Data]");
			Console.WriteLine("   Поддерживает текстовые и бинарные сообщения");

			const int port = 11045;
			var server = new ComplexProtocolServer(port);
			var serverTask = server.StartAsync();

			await Task.Delay(500);

			using (var client = new ComplexProtocolClient(port))
			{
				await client.ConnectAsync();

				Console.WriteLine("\n   Тест 1: Текстовые сообщения разных версий");
				await client.SendTextMessageAsync("Hello", 1);
				await client.SendTextMessageAsync("Привет, мир!", 2);

				var textMessages = await server.GetTextMessagesAsync(2, 1000);
				Console.WriteLine($"   Получено текстовых сообщений: {textMessages.Count}");

				Console.WriteLine("\n   Тест 2: Бинарные сообщения");
				var binaryData = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };
				await client.SendBinaryMessageAsync(binaryData, MessageType.Data);

				var binaryMessages = await server.GetBinaryMessagesAsync(1, 1000);
				Console.WriteLine($"   Получено бинарных сообщений: {binaryMessages.Count}");

				Console.WriteLine("\n   Тест 3: Сжатые данные");
				var uncompressed = new string('A', 1000);
				await client.SendCompressedTextAsync(uncompressed);

				var compressedInfo = await server.GetCompressionInfoAsync(1000);
				Console.WriteLine($"   Сжатие: {compressedInfo.OriginalSize} → {compressedInfo.CompressedSize}");

				Console.WriteLine("\n   Тест 4: Подтверждение получения");
				var ackMessages = new List<string>();
				for (int i = 1; i <= 3; i++)
				{
					var response = await client.SendWithAckAsync($"Message {i}");
					ackMessages.Add(response);
				}
				Console.WriteLine($"   Получено подтверждений: {ackMessages.Count}");

				Console.WriteLine("\n   Тест 5: Потоковая передача");
				var streamData = new List<string> { "Chunk1", "Chunk2", "Chunk3", "END" };
				foreach (var chunk in streamData)
				{
					await client.SendStreamChunkAsync(chunk);
					await Task.Delay(50);
				}

				var streamResult = await server.GetStreamResultAsync(2000);
				Console.WriteLine($"   Потоковая передача завершена: {streamResult}");

				Console.WriteLine("\n   Тест 6: Расширяемость");
				// Отправка сообщения нового типа (если бы протокол поддерживал)
				Console.WriteLine($"   Протокол позволяет добавлять новые типы сообщений");
				Console.WriteLine($"   без нарушения работы старых клиентов");
			}

			server.Stop();
			await serverTask;

			Console.WriteLine("\n   Итоги комплексного протокола:");
			Console.WriteLine("   1. Поддержка разных версий для обратной совместимости");
			Console.WriteLine("   2. Разные типы сообщений (текст, бинарные, сжатые)");
			Console.WriteLine("   3. Надёжность (подтверждения, повторные отправки)");
			Console.WriteLine("   4. Эффективность (сжатие, потоковая передача)");
			Console.WriteLine("   5. Расширяемость (новые типы, новые версии)");
		}
	}

	// Реализации протоколов

	#region Текстовый протокол с разделителем строк
	public class TextLineServer : IDisposable
	{
		private TcpListener _listener;
		private Thread _serverThread;
		private bool _isRunning;
		private readonly Queue<string> _receivedMessages = new();
		private readonly object _lock = new();

		public TextLineServer(int port)
		{
			_listener = new TcpListener(IPAddress.Loopback, port);
		}

		public async Task StartAsync()
		{
			_listener.Start();
			_isRunning = true;

			_serverThread = new Thread(() =>
			{
				try
				{
					while (_isRunning)
					{
						var client = _listener.AcceptTcpClient();
						Task.Run(() => HandleClient(client));
					}
				}
				catch { }
			});

			_serverThread.Start();

			await Task.Delay(100);
		}

		private void HandleClient(TcpClient client)
		{
			using (client)
			using (var stream = client.GetStream())
			using (var reader = new StreamReader(stream, Encoding.UTF8))
			{
				try
				{
					while (_isRunning && client.Connected)
					{
						// Читаем до символа новой строки
						string line = reader.ReadLine();
						if (line == null) break;

						lock (_lock)
						{
							_receivedMessages.Enqueue(line);
						}

						// Простой ответ
						var response = $"OK: {line}";
						var responseBytes = Encoding.UTF8.GetBytes(response + "\n");
						stream.Write(responseBytes, 0, responseBytes.Length);
					}
				}
				catch { }
			}
		}

		public async Task<List<string>> GetResponsesAsync(int count, int timeoutMs)
		{
			var result = new List<string>();
			var startTime = DateTime.Now;

			while (result.Count < count && (DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
			{
				lock (_lock)
				{
					while (_receivedMessages.Count > 0 && result.Count < count)
					{
						result.Add(_receivedMessages.Dequeue());
					}
				}

				if (result.Count < count)
					await Task.Delay(10);
			}

			return result;
		}

		public async Task<string> GetLastResponseAsync(int timeoutMs)
		{
			var responses = await GetResponsesAsync(1, timeoutMs);
			return responses.Count > 0 ? responses[0] : string.Empty;
		}

		public void Stop()
		{
			_isRunning = false;
			_listener.Stop();
			_serverThread?.Join(1000);
		}

		public void Dispose() => Stop();
	}

	public class TextLineClient : IDisposable
	{
		private TcpClient _client;
		private NetworkStream _stream;
		private StreamReader _reader;
		private StreamWriter _writer;
		private readonly int _port;

		// Конструктор с портом
		public TextLineClient(int port)
		{
			_port = port;
		}

		// Конструктор без параметров (для демонстрации)
		public TextLineClient() : this(11040) { }

		public async Task ConnectAsync()
		{
			_client = new TcpClient();
			await _client.ConnectAsync(IPAddress.Loopback, _port);
			_stream = _client.GetStream();
			_reader = new StreamReader(_stream, Encoding.UTF8);
			_writer = new StreamWriter(_stream, Encoding.UTF8) { AutoFlush = true };
		}

		public async Task SendAsync(string message)
		{
			await _writer.WriteLineAsync(message);
		}

		public async Task<string> ReceiveAsync()
		{
			return await _reader.ReadLineAsync() ?? string.Empty;
		}

		public void Dispose()
		{
			_writer?.Dispose();
			_reader?.Dispose();
			_stream?.Dispose();
			_client?.Dispose();
		}
	}
	#endregion

	#region Протокол с указанием длины
	public class LengthPrefixServer : IDisposable
	{
		private TcpListener _listener;
		private Thread _serverThread;
		private bool _isRunning;
		private readonly Queue<byte[]> _receivedMessages = new();
		private readonly object _lock = new();

		public LengthPrefixServer(int port)
		{
			_listener = new TcpListener(IPAddress.Loopback, port);
		}

		public async Task StartAsync()
		{
			_listener.Start();
			_isRunning = true;

			_serverThread = new Thread(() =>
			{
				try
				{
					while (_isRunning)
					{
						var client = _listener.AcceptTcpClient();
						Task.Run(() => HandleClient(client));
					}
				}
				catch { }
			});

			_serverThread.Start();

			await Task.Delay(100);
		}

		private void HandleClient(TcpClient client)
		{
			using (client)
			using (var stream = client.GetStream())
			{
				try
				{
					while (_isRunning && client.Connected)
					{
						// Читаем длину сообщения (4 байта)
						byte[] lengthBytes = new byte[4];
						int lengthRead = stream.Read(lengthBytes, 0, 4);
						if (lengthRead != 4) break;

						int messageLength = BitConverter.ToInt32(lengthBytes, 0);

						// Читаем само сообщение
						byte[] messageBytes = new byte[messageLength];
						int totalRead = 0;

						while (totalRead < messageLength)
						{
							int read = stream.Read(messageBytes, totalRead, messageLength - totalRead);
							if (read == 0) break;
							totalRead += read;
						}

						if (totalRead == messageLength)
						{
							lock (_lock)
							{
								_receivedMessages.Enqueue(messageBytes);
							}
						}
					}
				}
				catch { }
			}
		}

		public async Task<List<string>> GetMessagesAsync(int count, int timeoutMs)
		{
			var result = new List<string>();
			var startTime = DateTime.Now;

			while (result.Count < count && (DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
			{
				lock (_lock)
				{
					while (_receivedMessages.Count > 0 && result.Count < count)
					{
						result.Add(Encoding.UTF8.GetString(_receivedMessages.Dequeue()));
					}
				}

				if (result.Count < count)
					await Task.Delay(10);
			}

			return result;
		}

		public async Task<byte[]> GetLastBinaryMessageAsync(int timeoutMs)
		{
			var startTime = DateTime.Now;

			while ((DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
			{
				lock (_lock)
				{
					if (_receivedMessages.Count > 0)
						return _receivedMessages.Dequeue();
				}

				await Task.Delay(10);
			}

			return Array.Empty<byte>();
		}

		public void Stop()
		{
			_isRunning = false;
			_listener.Stop();
			_serverThread?.Join(1000);
		}

		public void Dispose() => Stop();
	}

	public class LengthPrefixClient : IDisposable
	{
		private TcpClient _client;
		private NetworkStream _stream;
		private readonly int _port;

		public LengthPrefixClient(int port)
		{
			_port = port;
		}

		public LengthPrefixClient() : this(11041) { }

		public async Task ConnectAsync()
		{
			_client = new TcpClient();
			await _client.ConnectAsync(IPAddress.Loopback, _port);
			_stream = _client.GetStream();
		}

		public async Task SendAsync(string message)
		{
			byte[] data = Encoding.UTF8.GetBytes(message);
			await SendAsync(data);
		}

		public async Task SendAsync(byte[] data)
		{
			// Записываем длину сообщения
			byte[] lengthBytes = BitConverter.GetBytes(data.Length);
			await _stream.WriteAsync(lengthBytes, 0, lengthBytes.Length);

			// Записываем данные
			await _stream.WriteAsync(data, 0, data.Length);
		}

		public void Dispose()
		{
			_stream?.Dispose();
			_client?.Dispose();
		}
	}
	#endregion

	#region Бинарный протокол
	public class BinaryProtocolServer : IDisposable
	{
		private TcpListener _listener;
		private Thread _serverThread;
		private bool _isRunning;
		private readonly List<CommandInfo> _receivedCommands = new();
		private readonly object _lock = new();

		public record CommandInfo(byte CommandId, byte[] Data);

		public BinaryProtocolServer(int port)
		{
			_listener = new TcpListener(IPAddress.Loopback, port);
		}

		public async Task StartAsync()
		{
			_listener.Start();
			_isRunning = true;

			_serverThread = new Thread(() =>
			{
				try
				{
					while (_isRunning)
					{
						var client = _listener.AcceptTcpClient();
						Task.Run(() => HandleClient(client));
					}
				}
				catch { }
			});

			_serverThread.Start();

			await Task.Delay(100);
		}

		private void HandleClient(TcpClient client)
		{
			using (client)
			using (var stream = client.GetStream())
			{
				try
				{
					while (_isRunning && client.Connected)
					{
						// Читаем заголовок: [CommandID: 1 байт][DataLength: 2 байта]
						byte[] header = new byte[3];
						int headerRead = stream.Read(header, 0, 3);
						if (headerRead != 3) break;

						byte commandId = header[0];
						ushort dataLength = BitConverter.ToUInt16(header, 1);

						// Валидация команды
						if (commandId > 0x0F) // Допустимы только команды 0x00-0x0F
						{
							SendErrorResponse(stream, "Invalid command ID");
							continue;
						}

						if (dataLength > 1024) // Ограничение размера данных
						{
							SendErrorResponse(stream, "Data too large");
							continue;
						}

						// Читаем данные
						byte[] data = new byte[dataLength];
						int totalRead = 0;

						while (totalRead < dataLength)
						{
							int read = stream.Read(data, totalRead, dataLength - totalRead);
							if (read == 0) break;
							totalRead += read;
						}

						if (totalRead == dataLength)
						{
							lock (_lock)
							{
								_receivedCommands.Add(new CommandInfo(commandId, data));
							}

							SendSuccessResponse(stream);
						}
					}
				}
				catch { }
			}
		}

		private void SendSuccessResponse(NetworkStream stream)
		{
			byte[] response = new byte[] { 0x01 }; // 0x01 = успех
			stream.Write(response, 0, response.Length);
		}

		private void SendErrorResponse(NetworkStream stream, string message)
		{
			byte[] messageBytes = Encoding.UTF8.GetBytes(message);
			byte[] response = new byte[1 + messageBytes.Length];
			response[0] = 0x00; // 0x00 = ошибка
			Array.Copy(messageBytes, 0, response, 1, messageBytes.Length);
			stream.Write(response, 0, response.Length);
		}

		public async Task<List<CommandInfo>> GetCommandsAsync(int count, int timeoutMs)
		{
			var result = new List<CommandInfo>();
			var startTime = DateTime.Now;

			while (result.Count < count && (DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
			{
				lock (_lock)
				{
					while (_receivedCommands.Count > 0 && result.Count < count)
					{
						result.Add(_receivedCommands[0]);
						_receivedCommands.RemoveAt(0);
					}
				}

				if (result.Count < count)
					await Task.Delay(10);
			}

			return result;
		}

		public void Stop()
		{
			_isRunning = false;
			_listener.Stop();
			_serverThread?.Join(1000);
		}

		public void Dispose() => Stop();
	}

	public class BinaryProtocolClient : IDisposable
	{
		private TcpClient _client;
		private NetworkStream _stream;
		private readonly int _port;

		public BinaryProtocolClient(int port)
		{
			_port = port;
		}

		public BinaryProtocolClient() : this(11042) { }

		public async Task ConnectAsync()
		{
			_client = new TcpClient();
			await _client.ConnectAsync(IPAddress.Loopback, _port);
			_stream = _client.GetStream();
		}

		public async Task SendCommandAsync(byte commandId, string data)
		{
			byte[] dataBytes = Encoding.UTF8.GetBytes(data);
			await SendCommandAsync(commandId, dataBytes);
		}

		public async Task SendCommandAsync(byte commandId, byte[] data)
		{
			if (data.Length > 65535)
				throw new ProtocolException("Data too large");

			// Формируем заголовок
			byte[] header = new byte[3];
			header[0] = commandId;
			byte[] lengthBytes = BitConverter.GetBytes((ushort)data.Length);
			Array.Copy(lengthBytes, 0, header, 1, 2);

			// Отправляем заголовок
			await _stream.WriteAsync(header, 0, header.Length);

			// Отправляем данные
			await _stream.WriteAsync(data, 0, data.Length);

			// Читаем ответ
			byte[] response = new byte[1];
			int read = await _stream.ReadAsync(response, 0, 1);

			if (read == 1 && response[0] == 0x00)
			{
				// Читаем сообщение об ошибке
				byte[] errorBytes = new byte[1024];
				int errorRead = await _stream.ReadAsync(errorBytes, 0, errorBytes.Length);
				string errorMessage = Encoding.UTF8.GetString(errorBytes, 0, errorRead);
				throw new ProtocolException($"Server error: {errorMessage}");
			}
		}

		public void Dispose()
		{
			_stream?.Dispose();
			_client?.Dispose();
		}
	}

	public class ProtocolException : Exception
	{
		public ProtocolException(string message) : base(message) { }
	}
	#endregion

	#region Stateful протокол
	public class StatefulServer : IDisposable
	{
		private TcpListener _listener;
		private Thread _serverThread;
		private bool _isRunning;
		private readonly List<string> _executedCommands = new();
		private readonly object _lock = new();

		// Хранилище сессий (в реальности было бы сложнее)
		private readonly Dictionary<string, Session> _sessions = new();

		private class Session
		{
			public string Username { get; set; }
			public DateTime Created { get; } = DateTime.Now;
			public bool IsAuthenticated { get; set; }
		}

		public StatefulServer(int port)
		{
			_listener = new TcpListener(IPAddress.Loopback, port);
		}

		public async Task StartAsync()
		{
			_listener.Start();
			_isRunning = true;

			_serverThread = new Thread(() =>
			{
				try
				{
					while (_isRunning)
					{
						var client = _listener.AcceptTcpClient();
						Task.Run(() => HandleClient(client));
					}
				}
				catch { }
			});

			_serverThread.Start();

			await Task.Delay(100);
		}

		private void HandleClient(TcpClient client)
		{
			using (client)
			using (var stream = client.GetStream())
			using (var reader = new StreamReader(stream, Encoding.UTF8))
			using (var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
			{
				try
				{
					string sessionId = Guid.NewGuid().ToString();
					var session = new Session();
					_sessions[sessionId] = session;

					writer.WriteLine($"SESSION {sessionId}");

					while (_isRunning && client.Connected)
					{
						string line = reader.ReadLine();
						if (line == null) break;

						string[] parts = line.Split(' ', 2);
						string command = parts[0].ToUpper();
						string argument = parts.Length > 1 ? parts[1] : string.Empty;

						if (command == "AUTH")
						{
							// Формат: AUTH username password
							string[] authParts = argument.Split(' ');
							if (authParts.Length == 2)
							{
								if (authParts[0] == "admin" && authParts[1] == "password123")
								{
									session.IsAuthenticated = true;
									session.Username = authParts[0];
									writer.WriteLine("AUTH_SUCCESS");
								}
								else
								{
									writer.WriteLine("AUTH_FAILED");
								}
							}
						}
						else if (command == "LOGOUT")
						{
							session.IsAuthenticated = false;
							writer.WriteLine("LOGOUT_SUCCESS");
						}
						else if (!session.IsAuthenticated)
						{
							writer.WriteLine("ERROR Not authenticated");
						}
						else
						{
							lock (_lock)
							{
								_executedCommands.Add($"{command} {argument}");
							}
							writer.WriteLine($"OK {command} executed");
						}
					}
				}
				catch { }
				finally
				{
					// Очистка сессии
					_sessions.Clear();
				}
			}
		}

		public async Task<List<string>> GetExecutedCommandsAsync(int count, int timeoutMs)
		{
			var result = new List<string>();
			var startTime = DateTime.Now;

			while (result.Count < count && (DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
			{
				lock (_lock)
				{
					while (_executedCommands.Count > 0 && result.Count < count)
					{
						result.Add(_executedCommands[0]);
						_executedCommands.RemoveAt(0);
					}
				}

				if (result.Count < count)
					await Task.Delay(10);
			}

			return result;
		}

		public void Stop()
		{
			_isRunning = false;
			_listener.Stop();
			_serverThread?.Join(1000);
		}

		public void Dispose() => Stop();
	}

	public class StatefulClient : IDisposable
	{
		private TcpClient _client;
		private NetworkStream _stream;
		private StreamReader _reader;
		private StreamWriter _writer;
		private readonly int _port;

		public StatefulClient(int port)
		{
			_port = port;
		}

		public StatefulClient() : this(11043) { }

		public async Task ConnectAsync()
		{
			_client = new TcpClient();
			await _client.ConnectAsync(IPAddress.Loopback, _port);
			_stream = _client.GetStream();
			_reader = new StreamReader(_stream, Encoding.UTF8);
			_writer = new StreamWriter(_stream, Encoding.UTF8) { AutoFlush = true };

			// Читаем приветственное сообщение с session ID
			string welcome = await _reader.ReadLineAsync();
			if (welcome == null || !welcome.StartsWith("SESSION"))
				throw new ProtocolException("Invalid server response");
		}

		public async Task<bool> AuthenticateAsync(string username, string password)
		{
			await _writer.WriteLineAsync($"AUTH {username} {password}");
			string response = await _reader.ReadLineAsync();
			return response == "AUTH_SUCCESS";
		}

		public async Task<string> SendCommandAsync(string command)
		{
			await _writer.WriteLineAsync(command);
			return await _reader.ReadLineAsync();
		}

		public async Task<bool> LogoutAsync()
		{
			await _writer.WriteLineAsync("LOGOUT");
			string response = await _reader.ReadLineAsync();
			return response == "LOGOUT_SUCCESS";
		}

		public void Dispose()
		{
			_writer?.Dispose();
			_reader?.Dispose();
			_stream?.Dispose();
			_client?.Dispose();
		}
	}
	#endregion

	#region Обработка ошибок
	public class ErrorHandlingServer : IDisposable
	{
		private TcpListener _listener;
		private Thread _serverThread;
		private bool _isRunning;
		private string _status = "Stopped";

		public ErrorHandlingServer(int port)
		{
			_listener = new TcpListener(IPAddress.Loopback, port);
		}

		public async Task StartAsync()
		{
			_listener.Start();
			_isRunning = true;
			_status = "Running";

			_serverThread = new Thread(() =>
			{
				try
				{
					while (_isRunning)
					{
						var client = _listener.AcceptTcpClient();
						Task.Run(() => HandleClient(client));
					}
				}
				catch { }
			});

			_serverThread.Start();

			await Task.Delay(100);
		}

		private void HandleClient(TcpClient client)
		{
			using (client)
			using (var stream = client.GetStream())
			using (var reader = new StreamReader(stream, Encoding.UTF8))
			using (var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
			{
				try
				{
					// Устанавливаем таймаут для чтения
					client.ReceiveTimeout = 5000;

					char[] buffer = new char[4096];
					int totalRead = 0;

					while (_isRunning && client.Connected)
					{
						// Читаем порциями с проверкой максимального размера
						int read = reader.Read(buffer, totalRead, buffer.Length - totalRead);
						if (read == 0) break;

						totalRead += read;

						// Проверяем на наличие разделителя
						string received = new string(buffer, 0, totalRead);
						int delimiterIndex = received.IndexOf('\n');

						if (delimiterIndex >= 0)
						{
							// Нашли полное сообщение
							string message = received.Substring(0, delimiterIndex);
							ProcessMessage(message, writer);

							// Сдвигаем буфер
							int remaining = totalRead - delimiterIndex - 1;
							if (remaining > 0)
							{
								Array.Copy(buffer, delimiterIndex + 1, buffer, 0, remaining);
								totalRead = remaining;
							}
							else
							{
								totalRead = 0;
							}
						}
						else if (totalRead >= buffer.Length)
						{
							// Переполнение буфера
							writer.WriteLine("ERROR Message too long");
							totalRead = 0; // Сбрасываем буфер
						}
					}
				}
				catch (IOException) when (!_isRunning)
				{
					// Ожидаемое прерывание
				}
				catch
				{
					// Игнорируем ошибки клиента
				}
			}
		}

		private void ProcessMessage(string message, StreamWriter writer)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(message))
				{
					writer.WriteLine("ERROR Empty message");
					return;
				}

				if (message.Length > 1024)
				{
					writer.WriteLine("ERROR Message too large");
					return;
				}

				string[] parts = message.Split(' ', 2);
				string command = parts[0].ToUpper();
				string arguments = parts.Length > 1 ? parts[1] : string.Empty;

				switch (command)
				{
					case "HELLO":
						writer.WriteLine("OK Hello from server");
						break;

					case "GET_DATA":
						if (string.IsNullOrEmpty(arguments))
							writer.WriteLine("ERROR Missing parameters for GET_DATA");
						else
							writer.WriteLine($"OK Data: {arguments}");
						break;

					case "VALID_COMMAND":
						writer.WriteLine("OK Command executed successfully");
						break;

					default:
						writer.WriteLine($"ERROR Unknown command: {command}");
						break;
				}
			}
			catch (Exception ex)
			{
				writer.WriteLine($"ERROR Internal error: {ex.Message}");
			}
		}

		public async Task<string> GetStatusAsync()
		{
			await Task.Delay(100);
			return _status;
		}

		public void Stop()
		{
			_isRunning = false;
			_status = "Stopped";
			_listener.Stop();
			_serverThread?.Join(1000);
		}

		public void Dispose() => Stop();
	}

	public class ErrorHandlingClient : IDisposable
	{
		private TcpClient _client;
		private NetworkStream _stream;
		private StreamReader _reader;
		private StreamWriter _writer;
		private readonly int _port;

		public ErrorHandlingClient(int port)
		{
			_port = port;
		}

		public ErrorHandlingClient() : this(11044) { }

		public async Task ConnectAsync()
		{
			_client = new TcpClient();
			await _client.ConnectAsync(IPAddress.Loopback, _port);
			_stream = _client.GetStream();
			_reader = new StreamReader(_stream, Encoding.UTF8);
			_writer = new StreamWriter(_stream, Encoding.UTF8) { AutoFlush = true };
		}

		public async Task<string> SendRawAsync(string message)
		{
			await _writer.WriteLineAsync(message);
			return await _reader.ReadLineAsync() ?? "NO_RESPONSE";
		}

		public async Task SendPartialAsync(string message)
		{
			// Отправка без завершающего \n
			await _writer.WriteAsync(message);
			await _writer.FlushAsync();
		}

		public void Dispose()
		{
			_writer?.Dispose();
			_reader?.Dispose();
			_stream?.Dispose();
			_client?.Dispose();
		}
	}
	#endregion

	#region Комплексный протокол (упрощенная реализация для демонстрации)
	public class ComplexProtocolServer : IDisposable
	{
		private TcpListener _listener;
		private Thread _serverThread;
		private bool _isRunning;
		private readonly List<string> _textMessages = new();
		private readonly List<byte[]> _binaryMessages = new();
		private (int OriginalSize, int CompressedSize) _compressionInfo;
		private string _streamResult = string.Empty;

		public enum MessageType : byte
		{
			Text = 1,
			Binary = 2,
			Compressed = 3,
			Stream = 4,
			Data = 5
		}

		public ComplexProtocolServer(int port)
		{
			_listener = new TcpListener(IPAddress.Loopback, port);
		}

		public async Task StartAsync()
		{
			_listener.Start();
			_isRunning = true;

			_serverThread = new Thread(() =>
			{
				try
				{
					while (_isRunning)
					{
						var client = _listener.AcceptTcpClient();
						Task.Run(() => HandleClient(client));
					}
				}
				catch { }
			});

			_serverThread.Start();

			await Task.Delay(100);
		}

		private void HandleClient(TcpClient client)
		{
			using (client)
			using (var stream = client.GetStream())
			{
				try
				{
					while (_isRunning && client.Connected)
					{
						// Читаем заголовок: [Version][Type][Length]
						byte[] header = new byte[7]; // 1 + 1 + 4 + 1 (версия, тип, длина, флаг подтверждения)
						int headerRead = stream.Read(header, 0, header.Length);
						if (headerRead != header.Length) break;

						byte version = header[0];
						MessageType type = (MessageType)header[1];
						int length = BitConverter.ToInt32(header, 2);
						bool requiresAck = header[6] == 1;

						// Читаем данные
						byte[] data = new byte[length];
						int totalRead = 0;

						while (totalRead < length)
						{
							int read = stream.Read(data, totalRead, length - totalRead);
							if (read == 0) break;
							totalRead += read;
						}

						if (totalRead == length)
						{
							ProcessMessage(version, type, data);

							if (requiresAck)
							{
								// Отправляем подтверждение
								byte[] ack = new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 };
								stream.Write(ack, 0, ack.Length);
							}
						}
					}
				}
				catch { }
			}
		}

		private void ProcessMessage(byte version, MessageType type, byte[] data)
		{
			switch (type)
			{
				case MessageType.Text:
					string text = Encoding.UTF8.GetString(data);
					_textMessages.Add($"[v{version}] {text}");
					break;

				case MessageType.Binary:
					_binaryMessages.Add(data);
					break;

				case MessageType.Compressed:
					// В реальности было бы распаковка
					_compressionInfo = (data.Length, data.Length / 2); // Симуляция сжатия
					break;

				case MessageType.Stream:
					string chunk = Encoding.UTF8.GetString(data);
					if (chunk == "END")
						_streamResult = "Stream completed";
					break;
			}
		}

		public async Task<List<string>> GetTextMessagesAsync(int count, int timeoutMs)
		{
			var startTime = DateTime.Now;

			while (_textMessages.Count < count && (DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
				await Task.Delay(10);

			return _textMessages.GetRange(0, Math.Min(count, _textMessages.Count));
		}

		public async Task<List<byte[]>> GetBinaryMessagesAsync(int count, int timeoutMs)
		{
			var startTime = DateTime.Now;

			while (_binaryMessages.Count < count && (DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
				await Task.Delay(10);

			return _binaryMessages.GetRange(0, Math.Min(count, _binaryMessages.Count));
		}

		public async Task<(int OriginalSize, int CompressedSize)> GetCompressionInfoAsync(int timeoutMs)
		{
			await Task.Delay(100);
			return _compressionInfo;
		}

		public async Task<string> GetStreamResultAsync(int timeoutMs)
		{
			var startTime = DateTime.Now;

			while (string.IsNullOrEmpty(_streamResult) && (DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
				await Task.Delay(10);

			return _streamResult;
		}

		public void Stop()
		{
			_isRunning = false;
			_listener.Stop();
			_serverThread?.Join(1000);
		}

		public void Dispose() => Stop();
	}

	public class ComplexProtocolClient : IDisposable
	{
		private TcpClient _client;
		private NetworkStream _stream;
		private readonly int _port;

		public ComplexProtocolClient(int port)
		{
			_port = port;
		}

		public ComplexProtocolClient() : this(11045) { }

		public async Task ConnectAsync()
		{
			_client = new TcpClient();
			await _client.ConnectAsync(IPAddress.Loopback, _port);
			_stream = _client.GetStream();
		}

		private async Task SendMessageAsync(byte version, byte type, byte[] data, bool requireAck = false)
		{
			// Формируем заголовок
			byte[] header = new byte[7];
			header[0] = version;
			header[1] = type;
			byte[] lengthBytes = BitConverter.GetBytes(data.Length);
			Array.Copy(lengthBytes, 0, header, 2, 4);
			header[6] = requireAck ? (byte)1 : (byte)0;

			// Отправляем
			await _stream.WriteAsync(header, 0, header.Length);
			await _stream.WriteAsync(data, 0, data.Length);

			if (requireAck)
			{
				// Ждем подтверждения
				byte[] ack = new byte[7];
				await _stream.ReadAsync(ack, 0, ack.Length);
			}
		}

		public async Task SendTextMessageAsync(string text, byte version = 1)
		{
			byte[] data = Encoding.UTF8.GetBytes(text);
			await SendMessageAsync(version, (byte)MessageType.Text, data);
		}

		public async Task SendBinaryMessageAsync(byte[] data, MessageType type)
		{
			await SendMessageAsync(1, (byte)type, data);
		}

		public async Task SendCompressedTextAsync(string text)
		{
			// Симуляция сжатия
			byte[] data = Encoding.UTF8.GetBytes(text);
			await SendMessageAsync(1, (byte)MessageType.Compressed, data);
		}

		public async Task<string> SendWithAckAsync(string message)
		{
			byte[] data = Encoding.UTF8.GetBytes(message);
			await SendMessageAsync(1, (byte)MessageType.Text, data, true);
			return "ACK_RECEIVED";
		}

		public async Task SendStreamChunkAsync(string chunk)
		{
			byte[] data = Encoding.UTF8.GetBytes(chunk);
			await SendMessageAsync(1, (byte)MessageType.Stream, data);
		}

		public void Dispose()
		{
			_stream?.Dispose();
			_client?.Dispose();
		}
	}
	#endregion

	// Главная программа
	class Program
	{
		static async Task Main(string[] args)
		{
			Console.WriteLine("ПРОТОКОЛЫ ОБЩЕНИЯ ПОВЕРХ TCP В C#");
			Console.WriteLine("==================================\n");

			await ProtocolDemonstration.DemonstrateProtocols();

			Console.WriteLine("\n\nКлючевые выводы:");
			Console.WriteLine("1. TCP передаёт байты, протокол определяет их смысл");
			Console.WriteLine("2. Простейший протокол лучше, чем отсутствие протокола");
			Console.WriteLine("3. Текстовый протокол с разделителями прост, но ненадёжен");
			Console.WriteLine("4. Протокол с указанием длины надёжнее, но сложнее");
			Console.WriteLine("5. Бинарные протоколы эффективны, но требуют строгой типизации");
			Console.WriteLine("6. Stateful протоколы позволяют реализовать сессии и состояния");
			Console.WriteLine("7. Обработка ошибок должна быть частью протокола");
			Console.WriteLine("8. Хороший протокол предусматривает расширяемость");
			Console.WriteLine("9. Симметричность реализации клиента и сервера критически важна");
			Console.WriteLine("10. Протокол должен быть устойчив к невалидным данным");
		}
	}
}