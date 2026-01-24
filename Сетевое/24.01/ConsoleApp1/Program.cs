using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace TCPDataTransfer
{
	// Демонстрация передачи данных по TCP
	public class TCPDataTransferDemo
	{
		public static async Task DemonstrateDataTransfer()
		{
			Console.WriteLine("=== ПЕРЕДАЧА ДАННЫХ ПО TCP ===\n");

			// 1. Поток байтов - фундаментальное понятие
			Console.WriteLine("1. ТЕОРИЯ: TCP КАК ПОТОК БАЙТОВ:");
			DemonstrateByteStreamTheory();

			// 2. Коварные моменты: несоответствие отправки и чтения
			Console.WriteLine("\n2. КОВАРСТВО: НЕСООТВЕТСТВИЕ ОТПРАВКИ И ЧТЕНИЯ:");
			await DemonstrateSendReceiveMismatch();

			// 3. Кодировки и преобразование данных
			Console.WriteLine("\n3. КОДИРОВКИ: ПРЕОБРАЗОВАНИЕ ДАННЫХ:");
			await DemonstrateEncodings();

			// 4. Блокирующее поведение операций
			Console.WriteLine("\n4. БЛОКИРОВКИ: КОГДА ПРОГРАММА ЖДЁТ:");
			await DemonstrateBlockingBehavior();

			// 5. Определение границ сообщений
			Console.WriteLine("\n5. ГРАНИЦЫ: КАК ОТДЕЛИТЬ СООБЩЕНИЯ:");
			await DemonstrateMessageBoundaries();

			// 6. Обработка ошибок и разрывов
			Console.WriteLine("\n6. ОШИБКИ: КОГДА ВСЁ ИДЁТ НЕ ТАК:");
			await DemonstrateErrorHandling();

			// 7. Практические паттерны передачи
			Console.WriteLine("\n7. ПРАКТИКА: ПАТТЕРНЫ ПЕРЕДАЧИ:");
			await DemonstratePracticalPatterns();

			// 8. Сравнение локальной и реальной передачи
			Console.WriteLine("\n8. РЕАЛЬНОСТЬ: ЛОКАЛЬНЫЕ VS УДАЛЁННЫЕ ТЕСТЫ:");
			await DemonstrateLocalVsRemote();
		}

		private static void DemonstrateByteStreamTheory()
		{
			Console.WriteLine("   Понимание потока байтов:");

			Console.WriteLine($"\n   КЛЮЧЕВАЯ ИДЕЯ:");
			Console.WriteLine($"     TCP не знает о 'строках', 'сообщениях' или 'командах'");
			Console.WriteLine($"     TCP знает только о байтах, идущих в одном направлении");

			Console.WriteLine($"\n   ПРОЦЕСС ПЕРЕДАЧИ:");
			Console.WriteLine($"     1. Приложение → Буфер отправки: байты");
			Console.WriteLine($"     2. TCP → Сеть: сегменты (разбивка/объединение)");
			Console.WriteLine($"     3. Сеть → Буфер приёма: байты");
			Console.WriteLine($"     4. Буфер приёма → Приложение: по запросу");

			Console.WriteLine($"\n   РЕАЛЬНЫЕ ДАННЫЕ:");

			string text = "Hello TCP";
			Console.WriteLine($"\n   Исходный текст: '{text}'");

			// Преобразование в байты
			byte[] bytes = Encoding.UTF8.GetBytes(text);
			Console.WriteLine($"   Байтовое представление (UTF-8):");
			Console.WriteLine($"     Длина: {bytes.Length} байт");
			Console.WriteLine($"     Байты: {BitConverter.ToString(bytes)}");
			Console.WriteLine($"     Hex: {string.Join(" ", bytes.Select(b => b.ToString("X2")))}");

			// Разные кодировки дают разные байты
			Console.WriteLine($"\n   Разные кодировки - разные байты:");

			byte[] utf8Bytes = Encoding.UTF8.GetBytes(text);
			byte[] utf32Bytes = Encoding.UTF32.GetBytes(text);
			byte[] unicodeBytes = Encoding.Unicode.GetBytes(text);

			Console.WriteLine($"     UTF-8:    {utf8Bytes.Length} байт");
			Console.WriteLine($"     UTF-32:   {utf32Bytes.Length} байт");
			Console.WriteLine($"     Unicode:  {unicodeBytes.Length} байт");

			Console.WriteLine($"\n   ВЫВОД: TCP передаёт байты, приложение отвечает за смысл");
		}

		private static async Task DemonstrateSendReceiveMismatch()
		{
			Console.WriteLine("   Демонстрация несоответствия отправки и чтения:");

			const int port = 11040;

			// Тест 1: Одно отправление - много чтений
			Console.WriteLine($"\n   ТЕСТ 1: Одна отправка, много чтений");

			var server1 = new SimpleTCPServer(port);
			var serverTask1 = server1.StartAsync();
			await Task.Delay(500);

			using (var client1 = new SimpleTCPClient(port))
			{
				await client1.ConnectAsync();

				string longMessage = new string('A', 1000); // 1000 символов 'A'
				Console.WriteLine($"   Клиент отправляет: 1000 символов одной операцией");
				await client1.SendAsync(longMessage);

				// Сервер читает маленькими частями
				var chunks = await server1.ReceiveWithSmallBuffer(100, 2000);
				Console.WriteLine($"   Сервер получил: {chunks.Count} чанками");
				Console.WriteLine($"     Первый чанк: {chunks.First().Length} символов");
				Console.WriteLine($"     Последний чанк: {chunks.Last().Length} символов");

				// Проверяем, что данные не потерялись
				string reconstructed = string.Concat(chunks);
				bool correct = reconstructed.Length == 1000 && reconstructed.All(c => c == 'A');
				Console.WriteLine($"   Данные целы: {correct} (длина: {reconstructed.Length})");
			}

			server1.Stop();
			await serverTask1;

			// Тест 2: Много отправлений - одно чтение
			Console.WriteLine($"\n   ТЕСТ 2: Много отправлений, одно чтение");

			var server2 = new SimpleTCPServer(port + 1);
			var serverTask2 = server2.StartAsync();
			await Task.Delay(500);

			using (var client2 = new SimpleTCPClient(port + 1))
			{
				await client2.ConnectAsync();

				Console.WriteLine($"   Клиент отправляет 5 сообщений подряд:");
				string[] messages = { "Hello", " ", "World", "!", "\n" };

				foreach (var message in messages)
				{
					Console.WriteLine($"     Отправка: '{message}'");
					await client2.SendAsync(message);
					await Task.Delay(50); // Небольшая задержка между отправками
				}

				// Сервер читает одним вызовом
				string received = await server2.WaitForDataAsync(2000);
				Console.WriteLine($"   Сервер получил одним чтением: '{received}'");
				Console.WriteLine($"   Ожидалось: 'Hello World!\\n'");
				Console.WriteLine($"   Совпало: {received == "Hello World!\n"}");
			}

			server2.Stop();
			await serverTask2;

			// Тест 3: Частичные чтения
			Console.WriteLine($"\n   ТЕСТ 3: Частичные чтения (самый коварный случай)");

			var server3 = new SimpleTCPServer(port + 2);
			var serverTask3 = server3.StartAsync();
			await Task.Delay(500);

			using (var client3 = new SimpleTCPClient(port + 2))
			{
				await client3.ConnectAsync();

				// Клиент отправляет два сообщения без задержки
				await client3.SendAsync("Message1");
				await client3.SendAsync("Message2");

				// Сервер пытается прочитать с фиксированным буфером
				string received1 = await server3.ReceiveExactAsync(8, 1000); // "Message1"
				Console.WriteLine($"   Сервер прочитал 8 байт: '{received1}'");

				// Но мог получить больше!
				string remaining = await server3.ReceiveAnyAsync(1000);
				Console.WriteLine($"   Осталось в буфере: '{remaining}'");

				Console.WriteLine($"\n   ВЫВОД: Одна отправка ≠ одно чтение");
				Console.WriteLine($"   TCP может разбивать и объединять данные произвольно");
			}

			server3.Stop();
			await serverTask3;
		}

		private static async Task DemonstrateEncodings()
		{
			Console.WriteLine("   Демонстрация работы с кодировками:");

			const int port = 11045;

			// Тест 1: UTF-8 как стандартная кодировка
			Console.WriteLine($"\n   ТЕСТ 1: UTF-8 - универсальная кодировка");

			var server = new EncodingAwareServer(port);
			var serverTask = server.StartAsync();
			await Task.Delay(500);

			using (var client = new SimpleTCPClient(port))
			{
				await client.ConnectAsync();

				// Текст с разными символами
				string[] testMessages =
				{
			"Hello ASCII",           // ASCII символы
            "Привет мир",            // Кириллица
            "🎉🎊",                    // Эмодзи
            "©®™"                    // Спецсимволы
        };

				foreach (var message in testMessages)
				{
					Console.WriteLine($"\n   Отправка: '{message}'");

					byte[] bytes = Encoding.UTF8.GetBytes(message);
					Console.WriteLine($"     Байтов: {bytes.Length}");
					Console.WriteLine($"     Hex: {BitConverter.ToString(bytes)}");

					await client.SendAsync(message);

					string received = await server.WaitForMessageAsync(1000);
					Console.WriteLine($"   Сервер получил: '{received}'");
					Console.WriteLine($"     Совпало: {received == message}");
				}
			}

			server.Stop();
			await serverTask;

			// Тест 2: Проблемы с разными кодировками
			Console.WriteLine($"\n   ТЕСТ 2: Проблема разных кодировок");

			// Регистрируем провайдер кодировок
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

			string russianText = "Привет";

			byte[] utf8Bytes = Encoding.UTF8.GetBytes(russianText);

			// Используем EncodingHelper или прямую регистрацию
			Encoding win1251Encoding;
			try
			{
				// Пытаемся получить кодировку Windows-1251
				win1251Encoding = EncodingHelper.GetEncoding(1251);
			}
			catch
			{
				// Если не поддерживается, используем альтернативу или ASCII
				Console.WriteLine("   Windows-1251 не поддерживается, используем ASCII для демонстрации");
				win1251Encoding = Encoding.ASCII;
			}

			byte[] win1251Bytes = win1251Encoding.GetBytes(russianText);

			Console.WriteLine($"   Текст: '{russianText}'");
			Console.WriteLine($"   UTF-8 байты: {BitConverter.ToString(utf8Bytes)}");
			Console.WriteLine($"   Windows-1251 байты: {BitConverter.ToString(win1251Bytes)}");

			// Декодирование неправильной кодировкой
			string corrupted;
			try
			{
				corrupted = Encoding.UTF8.GetString(win1251Bytes);
				Console.WriteLine($"   Декодирование UTF-8 как Windows-1251: '{corrupted}'");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"   Ошибка декодирования: {ex.Message}");

				// Альтернативная демонстрация с ASCII
				Console.WriteLine($"\n   Альтернативная демонстрация с ASCII:");
				string asciiText = "Hello";
				byte[] asciiBytes = Encoding.ASCII.GetBytes(asciiText);
				byte[] fakeUtf8Bytes = Encoding.UTF8.GetBytes(asciiText);
				Console.WriteLine($"   ASCII текст: '{asciiText}'");
				Console.WriteLine($"   ASCII байты: {BitConverter.ToString(asciiBytes)}");
				Console.WriteLine($"   UTF-8 байты: {BitConverter.ToString(fakeUtf8Bytes)}");

				// Декодируем ASCII байты как UTF-8 (должно работать для ASCII символов)
				string decoded = Encoding.UTF8.GetString(asciiBytes);
				Console.WriteLine($"   Декодирование ASCII байтов как UTF-8: '{decoded}'");
			}

			Console.WriteLine($"\n   ВЫВОД: Клиент и сервер должны использовать одну кодировку");
		}

		private static async Task DemonstrateBlockingBehavior()
		{
			Console.WriteLine("   Демонстрация блокирующего поведения:");

			const int port = 11050;

			// Тест 1: Блокировка при чтении
			Console.WriteLine($"\n   ТЕСТ 1: Блокировка чтения");

			var server = new BlockingServer(port);
			var serverTask = server.StartAsync();
			await Task.Delay(500); // Даем серверу время запуститься

			using (var client = new SimpleTCPClient(port))
			{
				await client.ConnectAsync();

				Console.WriteLine($"   Клиент подключён, но не отправляет данные");
				Console.WriteLine($"   Сервер вызывает Receive() и блокируется...");

				// Даем серверу время заблокироваться на чтении
				await Task.Delay(1000);

				// Запускаем задачу чтения с таймаутом
				var readTask = server.WaitForBlockingRead();

				// Ждем немного, чтобы убедиться, что сервер заблокирован
				await Task.Delay(500);

				Console.WriteLine($"   Клиент отправляет данные...");
				await client.SendAsync("Data");

				try
				{
					string received = await readTask;
					Console.WriteLine($"   Сервер получил данные: '{received}'");
					Console.WriteLine($"   Блокировка снята успешно!");
				}
				catch (TimeoutException)
				{
					Console.WriteLine($"   ТАЙМ-АУТ: Сервер не получил данные за ожидаемое время");
					Console.WriteLine($"   Возможные причины:");
					Console.WriteLine($"     1. Сервер не успел заблокироваться на Receive()");
					Console.WriteLine($"     2. Данные не дошли до сервера");
					Console.WriteLine($"     3. Проблема с синхронизацией потоков");
				}
			}

			server.Stop();
			await serverTask;

			// Тест 2: Блокировка при записи (переполнение буфера)
			Console.WriteLine($"\n   ТЕСТ 2: Блокировка записи");

			var server2 = new SlowConsumerServer(port + 1);
			var serverTask2 = server2.StartAsync();
			await Task.Delay(500);

			using (var client2 = new SimpleTCPClient(port + 1))
			{
				await client2.ConnectAsync();

				Console.WriteLine($"   Сервер читает медленно (100 мс на 1 байт)");
				Console.WriteLine($"   Клиент отправляет 10000 байт быстро...");

				string data = new string('X', 10000);

				var sendTask = Task.Run(async () =>
				{
					try
					{
						// Добавим логирование процесса отправки
						Console.WriteLine($"   Начало отправки...");
						await client2.SendAsync(data);
						Console.WriteLine($"   Отправка завершена");
					}
					catch (Exception ex)
					{
						Console.WriteLine($"   Ошибка отправки: {ex.Message}");
					}
				});

				// Мониторинг прогресса
				var monitorTask = Task.Run(async () =>
				{
					for (int i = 0; i < 10; i++)
					{
						await Task.Delay(500);
						int received = server2.GetBytesReceived();
						Console.WriteLine($"   Сервер получил: {received} байт ({received * 100 / 10000}%)");
					}
				});

				await Task.WhenAny(sendTask, Task.Delay(3000));

				// Проверяем, завершилась ли отправка
				if (sendTask.IsCompleted)
				{
					Console.WriteLine($"   Отправка завершилась за 3 секунды");
				}
				else
				{
					Console.WriteLine($"   Отправка ВСЕ ЕЩЁ продолжается через 3 секунды!");
					Console.WriteLine($"   Это демонстрирует блокировку записи - клиент ждет,");
					Console.WriteLine($"   пока TCP освободит буферы для отправки данных");
				}

				await Task.Delay(1000); // Даем немного времени для завершения
			}

			server2.Stop();
			await serverTask2;

			Console.WriteLine($"\n   ВЫВОД: Чтение и запись могут блокировать выполнение");
			Console.WriteLine($"   - Чтение блокируется, когда нет данных");
			Console.WriteLine($"   - Запись блокируется, когда буферы TCP полны");
			Console.WriteLine($"   - Всегда используйте асинхронные методы или тайм-ауты");
		}

		private static async Task DemonstrateMessageBoundaries()
		{
			Console.WriteLine("   Демонстрация определения границ сообщений:");

			// Метод 1: Фиксированная длина
			Console.WriteLine($"\n   МЕТОД 1: Фиксированная длина сообщений");

			const int fixedPort = 11060;
			var fixedServer = new FixedLengthServer(fixedPort, 10); // Сообщения по 10 байт
			var fixedServerTask = fixedServer.StartAsync();
			await Task.Delay(500);

			using (var fixedClient = new FixedLengthClient(fixedPort, 10))
			{
				await fixedClient.ConnectAsync();

				// Отправка сообщений фиксированной длины
				string[] messages = { "Hello12345", "World67890", "Test123456" };

				foreach (var message in messages)
				{
					Console.WriteLine($"   Отправка: '{message}' (10 байт)");
					await fixedClient.SendFixedAsync(message);

					string received = await fixedServer.GetNextMessageAsync(1000);
					Console.WriteLine($"   Сервер получил: '{received}'");
				}
			}

			fixedServer.Stop();
			await fixedServerTask;

			// Метод 2: Разделители
			Console.WriteLine($"\n   МЕТОД 2: Разделители сообщений");

			const int delimiterPort = 11061;
			var delimiterServer = new DelimitedServer(delimiterPort, '\n');
			var delimiterServerTask = delimiterServer.StartAsync();
			await Task.Delay(500);

			using (var delimiterClient = new DelimitedClient(delimiterPort, '\n'))
			{
				await delimiterClient.ConnectAsync();

				string[] delimitedMessages = { "First line", "Second line", "Third line" };

				foreach (var message in delimitedMessages)
				{
					Console.WriteLine($"   Отправка: '{message}\\n'");
					await delimiterClient.SendDelimitedAsync(message);

					string received = await delimiterServer.GetNextMessageAsync(1000);
					Console.WriteLine($"   Сервер получил: '{received}'");
				}
			}

			delimiterServer.Stop();
			await delimiterServerTask;

			// Метод 3: Заголовок с длиной
			Console.WriteLine($"\n   МЕТОД 3: Заголовок с длиной сообщения");

			const int lengthPrefixPort = 11062;
			var lengthPrefixServer = new LengthPrefixServer(lengthPrefixPort);
			var lengthPrefixServerTask = lengthPrefixServer.StartAsync();
			await Task.Delay(500);

			using (var lengthPrefixClient = new LengthPrefixClient(lengthPrefixPort))
			{
				await lengthPrefixClient.ConnectAsync();

				string[] variableMessages =
				{
					"Short",
					"Medium length message",
					"Very long message with many characters to demonstrate variable length"
				};

				foreach (var message in variableMessages)
				{
					Console.WriteLine($"   Отправка: '{message}' ({message.Length} байт)");
					await lengthPrefixClient.SendWithLengthAsync(message);

					string received = await lengthPrefixServer.GetNextMessageAsync(1000);
					Console.WriteLine($"   Сервер получил: '{received}' (длина: {received.Length})");
				}
			}

			lengthPrefixServer.Stop();
			await lengthPrefixServerTask;

			// Метод 4: Самозавершающиеся форматы
			Console.WriteLine($"\n   МЕТОД 4: Самозавершающиеся форматы (JSON)");

			const int jsonPort = 11063;
			var jsonServer = new JSONServer(jsonPort);
			var jsonServerTask = jsonServer.StartAsync();
			await Task.Delay(500);

			using (var jsonClient = new JSONClient(jsonPort))
			{
				await jsonClient.ConnectAsync();

				// Отправка JSON сообщений
				var messages = new[]
				{
					"{\"type\":\"login\",\"user\":\"alice\"}",
					"{\"type\":\"message\",\"text\":\"Hello\",\"timestamp\":1234567890}",
					"{\"type\":\"logout\",\"reason\":\"timeout\"}"
				};

				foreach (var json in messages)
				{
					Console.WriteLine($"   Отправка JSON: {json}");
					await jsonClient.SendJSONAsync(json);

					string received = await jsonServer.GetNextJSONAsync(1000);
					Console.WriteLine($"   Сервер получил: {received}");
				}
			}

			jsonServer.Stop();
			await jsonServerTask;

			Console.WriteLine($"\n   ВЫВОД: Приложение должно само определять границы сообщений");
			Console.WriteLine($"   Выбор метода зависит от требований приложения");
		}

		private static async Task DemonstrateErrorHandling()
		{
			Console.WriteLine("   Демонстрация обработки ошибок:");

			const int port = 11070;

			// Тест 1: Разрыв соединения при чтении
			Console.WriteLine($"\n   ТЕСТ 1: Разрыв соединения во время чтения");

			var server = new ResilientServer(port);
			var serverTask = server.StartAsync();
			await Task.Delay(500);

			using (var client = new SimpleTCPClient(port))
			{
				await client.ConnectAsync();

				Console.WriteLine($"   Клиент подключён");
				Console.WriteLine($"   Клиент внезапно закрывает соединение...");

				// Резкое закрытие без Shutdown
				client.Dispose();

				// Сервер пытается читать
				try
				{
					string result = await server.TryReadWithTimeout(2000);
					Console.WriteLine($"   Сервер получил: '{result}'");
				}
				catch (SocketException ex)
				{
					Console.WriteLine($"   Сервер получил ошибку: {ex.SocketErrorCode}");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"   Другая ошибка: {ex.Message}");
				}
			}

			server.Stop();
			await serverTask;

			// Тест 2: Разрыв соединения при записи
			Console.WriteLine($"\n   ТЕСТ 2: Разрыв соединения во время записи");

			var server2 = new ResilientServer(port + 1);
			var serverTask2 = server2.StartAsync();
			await Task.Delay(500);

			using (var client2 = new SimpleTCPClient(port + 1))
			{
				await client2.ConnectAsync();

				Console.WriteLine($"   Клиент подключён");
				Console.WriteLine($"   Сервер внезапно останавливается...");

				// Останавливаем сервер
				server2.Stop();
				await Task.Delay(100);

				Console.WriteLine($"   Клиент пытается отправить данные...");

				try
				{
					await client2.SendAsync("Test message");
					Console.WriteLine($"   ОШИБКА: Отправка должна была завершиться с ошибкой");
				}
				catch (SocketException ex)
				{
					Console.WriteLine($"   Ожидаемая ошибка: {ex.SocketErrorCode}");
				}
			}

			await serverTask2;

			// Тест 3: Неполные чтения и тайм-ауты
			Console.WriteLine($"\n   ТЕСТ 3: Обработка неполных данных и тайм-аутов");

			var robustClient = new RobustTCPClient(); // Убрали параметр (port + 2)

			try
			{
				Console.WriteLine($"   Попытка подключения к несуществующему серверу...");
				await robustClient.ConnectWithTimeoutAsync("127.0.0.1", port + 999, 2000);
				Console.WriteLine($"   ОШИБКА: Подключение не должно было состояться");
			}
			catch (TimeoutException)
			{
				Console.WriteLine($"   Ожидаемый тайм-аут подключения");
			}
			catch (SocketException ex)
			{
				Console.WriteLine($"   Ошибка подключения: {ex.SocketErrorCode}");
			}

			Console.WriteLine($"\n   ВЫВОД: Ошибки - нормальная часть сетевого программирования");
			Console.WriteLine($"   Код должен быть готов к разрывам и тайм-аутам");
		}

		private static async Task DemonstratePracticalPatterns()
		{
			Console.WriteLine("   Демонстрация практических паттернов:");

			// Паттерн 1: Чтение до заполнения буфера
			Console.WriteLine($"\n   ПАТТЕРН 1: Чтение до заполнения буфера");

			const int port = 11080;

			var server = new BufferFillingServer(port);
			var serverTask = server.StartAsync();
			await Task.Delay(500);

			using (var client = new SimpleTCPClient(port))
			{
				await client.ConnectAsync();

				string message = "Hello, this is a test message for buffer filling pattern.";
				await client.SendAsync(message);

				// Сервер читает до заполнения 20-байтного буфера
				string result = await server.ReadBufferAsync(20, 1000);
				Console.WriteLine($"   Сервер прочитал 20 байт: '{result}'");
				Console.WriteLine($"   Осталось в потоке: {message.Length - 20} байт");
			}

			server.Stop();
			await serverTask;

			// Паттерн 2: Накопление данных до определённого условия
			Console.WriteLine($"\n   ПАТТЕРН 2: Накопление данных до условия");

			var accumulator = new DataAccumulator();

			// Симуляция поступления данных частями
			string[] chunks = { "Hel", "lo ", "Wor", "ld", "!" };

			Console.WriteLine($"   Поступление данных частями:");
			foreach (var chunk in chunks)
			{
				accumulator.Add(chunk);
				Console.WriteLine($"     Чанк: '{chunk}', накоплено: '{accumulator.GetData()}'");

				// Проверяем, есть ли полное сообщение
				if (accumulator.GetData().Contains("World"))
				{
					Console.WriteLine($"   Найдено полное сообщение: 'World'");
					accumulator.ClearProcessed(8); // Удаляем обработанные данные
				}
			}

			// Паттерн 3: Асинхронное чтение с CancellationToken
			Console.WriteLine($"\n   ПАТТЕРН 3: Асинхронное чтение с отменой");

			var cts = new CancellationTokenSource();

			var asyncServer = new AsyncServer(port + 1);
			var asyncServerTask = asyncServer.StartAsync(); // Убрали передачу CancellationToken
			await Task.Delay(500);

			using (var asyncClient = new SimpleTCPClient(port + 1))
			{
				await asyncClient.ConnectAsync();

				// Запускаем асинхронное чтение
				var readTask = asyncServer.ReadAsync(cts.Token);

				// Отправляем данные с задержкой
				await Task.Delay(1000);
				await asyncClient.SendAsync("Delayed message");

				string received = await readTask;
				Console.WriteLine($"   Сервер получил асинхронно: '{received}'");

				// Тест отмены
				cts.Cancel();
				try
				{
					await asyncServer.ReadAsync(cts.Token);
				}
				catch (OperationCanceledException)
				{
					Console.WriteLine($"   Ожидаемая отмена операции");
				}
			}

			cts.Cancel();
			await asyncServerTask;

			Console.WriteLine($"\n   ВЫВОД: Практические паттерны решают общие проблемы TCP");
		}

		private static async Task DemonstrateLocalVsRemote()
		{
			Console.WriteLine("   Сравнение локальной и реальной передачи:");

			// Локальный тест: всё идеально
			Console.WriteLine($"\n   ЛОКАЛЬНЫЙ ТЕСТ (на одном компьютере):");

			const int localPort = 11090;

			var localServer = new SimpleTCPServer(localPort);
			var localServerTask = localServer.StartAsync();
			await Task.Delay(500);

			using (var localClient = new SimpleTCPClient(localPort))
			{
				await localClient.ConnectAsync();

				// Множественные отправки
				for (int i = 0; i < 5; i++)
				{
					await localClient.SendAsync($"Message {i}");
				}

				// Чтение одним вызовом
				string allData = await localServer.WaitForDataAsync(1000);
				Console.WriteLine($"   Локальный сервер получил: '{allData}'");
				Console.WriteLine($"   Длина: {allData.Length}, ожидалось: 38");

				// В локальной сети часто всё приходит одним куском
				Console.WriteLine($"   В локальном тесте часто получается идеально");
			}

			localServer.Stop();
			await localServerTask;

			// Симуляция реальных проблем
			Console.WriteLine($"\n   СИМУЛЯЦИЯ РЕАЛЬНЫХ ПРОБЛЕМ:");

			const int simulatedPort = 11091;

			var problematicServer = new ProblemSimulatingServer(simulatedPort);
			var problematicServerTask = problematicServer.StartAsync();
			await Task.Delay(500);

			using (var client = new SimpleTCPClient(simulatedPort))
			{
				await client.ConnectAsync();

				Console.WriteLine($"   Реальные проблемы, которые скрывает локальная сеть:");
				Console.WriteLine($"     1. Задержки (latency)");
				Console.WriteLine($"     2. Потери пакетов");
				Console.WriteLine($"     3. Фрагментация данных");
				Console.WriteLine($"     4. Переполнение буферов");
				Console.WriteLine($"     5. Неожиданные разрывы");

				// Сервер симулирует проблемы
				await client.SendAsync("Test message");

				var result = await problematicServer.GetReceivedWithProblems(2000);
				Console.WriteLine($"\n   Результат с симулированными проблемами:");
				Console.WriteLine($"     Получено чанков: {result.Chunks.Count}");
				Console.WriteLine($"     Время получения: {result.ElapsedMs}мс");
				Console.WriteLine($"     Данные целы: {result.DataIntegrity}");
			}

			problematicServer.Stop();
			await problematicServerTask;

			Console.WriteLine($"\n   РЕКОМЕНДАЦИИ:");
			Console.WriteLine($"     1. Всегда тестируйте с разными размерами данных");
			Console.WriteLine($"     2. Симулируйте задержки и потери");
			Console.WriteLine($"     3. Проверяйте обработку неполных данных");
			Console.WriteLine($"     4. Тестируйте разрывы соединений");
			Console.WriteLine($"     5. Используйте тайм-ауты во всех операциях");

			Console.WriteLine($"\n   ВЫВОД: Локальные тесты ≠ реальная работа");
			Console.WriteLine($"   Пишите код, который работает правильно в любых условиях");
		}
	}

	// Вспомогательные классы для демонстрации
	public class SimpleTCPServer : IDisposable
	{
		protected Socket _serverSocket;
		protected Socket _clientSocket;
		protected Thread _serverThread;
		protected bool _isRunning;
		protected readonly int _port;
		protected readonly object _lock = new object();
		protected string _lastReceived = string.Empty;

		public SimpleTCPServer(int port)
		{
			_port = port;
		}

		public virtual async Task StartAsync()
		{
			_serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			_serverSocket.Bind(new IPEndPoint(IPAddress.Loopback, _port));
			_serverSocket.Listen(1);

			_isRunning = true;
			_serverThread = new Thread(RunServer);
			_serverThread.Start();

			await Task.Delay(100);
		}

		protected virtual void RunServer()
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
							lock (_lock)
							{
								_lastReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead);
							}
						}
					}
					Thread.Sleep(10);
				}
			}
			catch { }
		}

		public async Task<string> WaitForDataAsync(int timeoutMs)
		{
			var startTime = DateTime.Now;

			while ((DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
			{
				lock (_lock)
				{
					if (!string.IsNullOrEmpty(_lastReceived))
					{
						string result = _lastReceived;
						_lastReceived = string.Empty;
						return result;
					}
				}
				await Task.Delay(10);
			}

			return string.Empty;
		}

		public async Task<List<string>> ReceiveWithSmallBuffer(int chunkSize, int timeoutMs)
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

		public async Task<string> ReceiveExactAsync(int exactBytes, int timeoutMs)
		{
			var buffer = new byte[exactBytes];
			int totalRead = 0;
			var startTime = DateTime.Now;

			while (totalRead < exactBytes && (DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
			{
				if (_clientSocket.Available > 0)
				{
					int bytesRead = _clientSocket.Receive(buffer, totalRead, exactBytes - totalRead, SocketFlags.None);
					totalRead += bytesRead;
				}
				await Task.Delay(10);
			}

			return Encoding.UTF8.GetString(buffer, 0, totalRead);
		}

		public async Task<string> ReceiveAnyAsync(int timeoutMs)
		{
			var buffer = new byte[1024];
			var startTime = DateTime.Now;

			while ((DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
			{
				if (_clientSocket.Available > 0)
				{
					int bytesRead = _clientSocket.Receive(buffer);
					return Encoding.UTF8.GetString(buffer, 0, bytesRead);
				}
				await Task.Delay(10);
			}

			return string.Empty;
		}

		public void Stop()
		{
			_isRunning = false;
			_clientSocket?.Close();
			_serverSocket?.Close();
			_serverThread?.Join(1000);
		}

		public void Dispose() => Stop();
	}

	public class SimpleTCPClient : IDisposable
	{
		protected Socket _socket;
		protected readonly int _port;

		public SimpleTCPClient(int port)
		{
			_port = port;
		}

		public virtual async Task ConnectAsync()
		{
			_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			await Task.Run(() => _socket.Connect(new IPEndPoint(IPAddress.Loopback, _port)));
		}

		public virtual async Task SendAsync(string message)
		{
			byte[] data = Encoding.UTF8.GetBytes(message);
			await Task.Run(() => _socket.Send(data));
		}

		public void Dispose()
		{
			_socket?.Close();
			_socket?.Dispose();
		}
	}

	// Классы для демонстрации кодировок
	public class EncodingAwareServer : SimpleTCPServer
	{
		private Encoding _encoding;

		public EncodingAwareServer(int port, Encoding encoding = null) : base(port)
		{
			_encoding = encoding ?? Encoding.UTF8;
		}

		public async Task<string> WaitForMessageAsync(int timeoutMs)
		{
			return await WaitForDataAsync(timeoutMs);
		}
	}

	// Классы для демонстрации блокировок
	public class BlockingServer : SimpleTCPServer
	{
		private ManualResetEventSlim _dataReceived = new ManualResetEventSlim();
		private ManualResetEventSlim _serverReady = new ManualResetEventSlim();
		private string _blockingResult;
		private Exception _readException;

		public BlockingServer(int port) : base(port) { }

		protected override void RunServer()
		{
			try
			{
				_clientSocket = _serverSocket.Accept();
				Console.WriteLine($"   [Сервер] Клиент подключен, сервер готов к блокировке");
				_serverReady.Set(); // Сервер готов

				// Блокирующее чтение
				var buffer = new byte[1024];

				Console.WriteLine($"   [Сервер] Вызываю Receive() (блокировка)...");
				int bytesRead = _clientSocket.Receive(buffer); // Блокируется здесь
				Console.WriteLine($"   [Сервер] Receive() вернул управление, получено {bytesRead} байт");

				_blockingResult = Encoding.UTF8.GetString(buffer, 0, bytesRead);
				_dataReceived.Set();
			}
			catch (SocketException ex)
			{
				Console.WriteLine($"   [Сервер] Ошибка сокета: {ex.SocketErrorCode}");
				_readException = ex;
				_dataReceived.Set();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"   [Сервер] Другая ошибка: {ex.Message}");
				_readException = ex;
				_dataReceived.Set();
			}
		}

		public async Task<string> WaitForBlockingRead()
		{
			// Ждем, пока сервер будет готов (примет подключение)
			if (!_serverReady.Wait(2000))
			{
				throw new TimeoutException("Сервер не готов к блокировке");
			}

			Console.WriteLine($"   [Тест] Сервер готов, ждем блокировку и данные...");

			// Ждем завершения блокирующего чтения
			if (_dataReceived.Wait(5000))
			{
				if (_readException != null)
				{
					throw _readException;
				}
				return _blockingResult;
			}

			throw new TimeoutException("Блокирующее чтение не завершилось за 5 секунд");
		}
	}

	public class SlowConsumerServer : SimpleTCPServer
	{
		private int _bytesReceived;

		public SlowConsumerServer(int port) : base(port) { }

		protected override void RunServer()
		{
			try
			{
				_clientSocket = _serverSocket.Accept();

				var buffer = new byte[1]; // Читаем по 1 байту
				while (_isRunning && _clientSocket.Connected)
				{
					if (_clientSocket.Available > 0)
					{
						int bytesRead = _clientSocket.Receive(buffer);
						_bytesReceived += bytesRead;

						// Медленная обработка
						Thread.Sleep(100);
					}
					Thread.Sleep(10);
				}
			}
			catch { }
		}

		public int GetBytesReceived() => _bytesReceived;
	}

	// Классы для демонстрации границ сообщений
	public class FixedLengthServer : SimpleTCPServer
	{
		private readonly int _messageLength;
		private readonly Queue<string> _messages = new Queue<string>();

		public FixedLengthServer(int port, int messageLength) : base(port)
		{
			_messageLength = messageLength;
		}

		protected override void RunServer()
		{
			try
			{
				_clientSocket = _serverSocket.Accept();

				var buffer = new byte[_messageLength];
				while (_isRunning && _clientSocket.Connected)
				{
					int totalRead = 0;
					while (totalRead < _messageLength && _isRunning)
					{
						int bytesRead = _clientSocket.Receive(buffer, totalRead, _messageLength - totalRead, SocketFlags.None);
						if (bytesRead == 0) break;
						totalRead += bytesRead;
					}

					if (totalRead == _messageLength)
					{
						string message = Encoding.UTF8.GetString(buffer, 0, totalRead);
						lock (_messages)
						{
							_messages.Enqueue(message);
						}
					}
					Thread.Sleep(10);
				}
			}
			catch { }
		}

		public async Task<string> GetNextMessageAsync(int timeoutMs)
		{
			var startTime = DateTime.Now;

			while ((DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
			{
				lock (_messages)
				{
					if (_messages.Count > 0)
					{
						return _messages.Dequeue();
					}
				}
				await Task.Delay(10);
			}

			return string.Empty;
		}
	}

	public class FixedLengthClient : SimpleTCPClient
	{
		private readonly int _messageLength;

		public FixedLengthClient(int port, int messageLength) : base(port)
		{
			_messageLength = messageLength;
		}

		public async Task SendFixedAsync(string message)
		{
			if (message.Length != _messageLength)
			{
				message = message.PadRight(_messageLength).Substring(0, _messageLength);
			}

			await SendAsync(message);
		}
	}

	public class DelimitedServer : SimpleTCPServer
	{
		private readonly char _delimiter;
		private readonly Queue<string> _messages = new Queue<string>();
		private readonly StringBuilder _buffer = new StringBuilder();

		public DelimitedServer(int port, char delimiter) : base(port)
		{
			_delimiter = delimiter;
		}

		protected override void RunServer()
		{
			try
			{
				_clientSocket = _serverSocket.Accept();

				var byteBuffer = new byte[1024];
				while (_isRunning && _clientSocket.Connected)
				{
					if (_clientSocket.Available > 0)
					{
						int bytesRead = _clientSocket.Receive(byteBuffer);
						string chunk = Encoding.UTF8.GetString(byteBuffer, 0, bytesRead);
						_buffer.Append(chunk);

						// Проверяем наличие разделителя
						int delimiterIndex;
						while ((delimiterIndex = _buffer.ToString().IndexOf(_delimiter)) >= 0)
						{
							string message = _buffer.ToString(0, delimiterIndex);
							lock (_messages)
							{
								_messages.Enqueue(message);
							}
							_buffer.Remove(0, delimiterIndex + 1);
						}
					}
					Thread.Sleep(10);
				}
			}
			catch { }
		}

		public async Task<string> GetNextMessageAsync(int timeoutMs)
		{
			var startTime = DateTime.Now;

			while ((DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
			{
				lock (_messages)
				{
					if (_messages.Count > 0)
					{
						return _messages.Dequeue();
					}
				}
				await Task.Delay(10);
			}

			return string.Empty;
		}
	}

	public class DelimitedClient : SimpleTCPClient
	{
		private readonly char _delimiter;

		public DelimitedClient(int port, char delimiter) : base(port)
		{
			_delimiter = delimiter;
		}

		public async Task SendDelimitedAsync(string message)
		{
			await SendAsync(message + _delimiter);
		}
	}

	public class LengthPrefixServer : SimpleTCPServer
	{
		private readonly Queue<string> _messages = new Queue<string>();

		public LengthPrefixServer(int port) : base(port) { }

		protected override void RunServer()
		{
			try
			{
				_clientSocket = _serverSocket.Accept();

				while (_isRunning && _clientSocket.Connected)
				{
					// Читаем длину (4 байта)
					byte[] lengthBytes = new byte[4];
					int lengthBytesRead = 0;
					while (lengthBytesRead < 4 && _isRunning)
					{
						int bytesRead = _clientSocket.Receive(lengthBytes, lengthBytesRead, 4 - lengthBytesRead, SocketFlags.None);
						if (bytesRead == 0) break;
						lengthBytesRead += bytesRead;
					}

					if (lengthBytesRead == 4)
					{
						int messageLength = BitConverter.ToInt32(lengthBytes, 0);

						// Читаем сообщение
						byte[] messageBytes = new byte[messageLength];
						int messageBytesRead = 0;
						while (messageBytesRead < messageLength && _isRunning)
						{
							int bytesRead = _clientSocket.Receive(messageBytes, messageBytesRead, messageLength - messageBytesRead, SocketFlags.None);
							if (bytesRead == 0) break;
							messageBytesRead += bytesRead;
						}

						if (messageBytesRead == messageLength)
						{
							string message = Encoding.UTF8.GetString(messageBytes, 0, messageLength);
							lock (_messages)
							{
								_messages.Enqueue(message);
							}
						}
					}
					Thread.Sleep(10);
				}
			}
			catch { }
		}

		public async Task<string> GetNextMessageAsync(int timeoutMs)
		{
			var startTime = DateTime.Now;

			while ((DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
			{
				lock (_messages)
				{
					if (_messages.Count > 0)
					{
						return _messages.Dequeue();
					}
				}
				await Task.Delay(10);
			}

			return string.Empty;
		}
	}

	public static class EncodingHelper
	{
		private static bool _encodingsRegistered = false;

		public static void RegisterWindowsEncodings()
		{
			if (_encodingsRegistered) return;

			// Регистрируем провайдер кодировок для Windows
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			_encodingsRegistered = true;
		}

		public static Encoding GetEncoding(int codePage)
		{
			RegisterWindowsEncodings();
			return Encoding.GetEncoding(codePage);
		}
	}

	public class LengthPrefixClient : SimpleTCPClient
	{
		public LengthPrefixClient(int port) : base(port) { }

		public async Task SendWithLengthAsync(string message)
		{
			byte[] messageBytes = Encoding.UTF8.GetBytes(message);
			byte[] lengthBytes = BitConverter.GetBytes(messageBytes.Length);

			// Отправляем длину, затем данные
			await Task.Run(() =>
			{
				_socket.Send(lengthBytes);
				_socket.Send(messageBytes);
			});
		}
	}

	public class JSONServer : SimpleTCPServer
	{
		private readonly Queue<string> _messages = new Queue<string>();
		private readonly StringBuilder _buffer = new StringBuilder();

		public JSONServer(int port) : base(port) { }

		protected override void RunServer()
		{
			try
			{
				_clientSocket = _serverSocket.Accept();

				var byteBuffer = new byte[1024];
				while (_isRunning && _clientSocket.Connected)
				{
					if (_clientSocket.Available > 0)
					{
						int bytesRead = _clientSocket.Receive(byteBuffer);
						string chunk = Encoding.UTF8.GetString(byteBuffer, 0, bytesRead);
						_buffer.Append(chunk);

						// Пытаемся найти валидный JSON
						string bufferStr = _buffer.ToString();
						int braceCount = 0;
						int startIndex = -1;

						for (int i = 0; i < bufferStr.Length; i++)
						{
							if (bufferStr[i] == '{')
							{
								if (braceCount == 0) startIndex = i;
								braceCount++;
							}
							else if (bufferStr[i] == '}')
							{
								braceCount--;
								if (braceCount == 0 && startIndex != -1)
								{
									string json = bufferStr.Substring(startIndex, i - startIndex + 1);
									lock (_messages)
									{
										_messages.Enqueue(json);
									}
									_buffer.Remove(0, i + 1);
									break;
								}
							}
						}
					}
					Thread.Sleep(10);
				}
			}
			catch { }
		}

		public async Task<string> GetNextJSONAsync(int timeoutMs)
		{
			var startTime = DateTime.Now;

			while ((DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
			{
				lock (_messages)
				{
					if (_messages.Count > 0)
					{
						return _messages.Dequeue();
					}
				}
				await Task.Delay(10);
			}

			return string.Empty;
		}
	}

	public class JSONClient : SimpleTCPClient
	{
		public JSONClient(int port) : base(port) { }

		public async Task SendJSONAsync(string json)
		{
			await SendAsync(json);
		}
	}

	// Классы для обработки ошибок
	public class ResilientServer : SimpleTCPServer
	{
		public ResilientServer(int port) : base(port) { }

		public async Task<string> TryReadWithTimeout(int timeoutMs)
		{
			try
			{
				if (_clientSocket != null && _clientSocket.Connected)
				{
					var buffer = new byte[1024];
					_clientSocket.ReceiveTimeout = timeoutMs;
					int bytesRead = _clientSocket.Receive(buffer);
					return Encoding.UTF8.GetString(buffer, 0, bytesRead);
				}
			}
			catch (SocketException ex)
			{
				throw;
			}

			return string.Empty;
		}
	}

	public class RobustTCPClient : IDisposable
	{
		private Socket _socket;

		// Если нужен конструктор с портом, добавьте его:
		public RobustTCPClient() { } // Конструктор по умолчанию

		// ИЛИ если нужен конструктор с портом:
		// public RobustTCPClient(int port) { } // Но тогда нужно хранить порт

		public async Task ConnectWithTimeoutAsync(string host, int port, int timeoutMs)
		{
			_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

			var connectTask = Task.Run(() => _socket.Connect(host, port));
			var timeoutTask = Task.Delay(timeoutMs);

			var completedTask = await Task.WhenAny(connectTask, timeoutTask);

			if (completedTask == timeoutTask)
			{
				_socket.Close();
				throw new TimeoutException($"Подключение к {host}:{port} превысило тайм-аут {timeoutMs}мс");
			}

			// Проверяем, была ли ошибка подключения
			await connectTask;
		}

		public void Dispose()
		{
			_socket?.Close();
			_socket?.Dispose();
		}
	}

	// Классы для практических паттернов
	public class BufferFillingServer : SimpleTCPServer
	{
		public BufferFillingServer(int port) : base(port) { }

		public async Task<string> ReadBufferAsync(int bufferSize, int timeoutMs)
		{
			var buffer = new byte[bufferSize];
			int totalRead = 0;
			var startTime = DateTime.Now;

			while (totalRead < bufferSize && (DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
			{
				if (_clientSocket != null && _clientSocket.Connected && _clientSocket.Available > 0)
				{
					int bytesRead = _clientSocket.Receive(buffer, totalRead, bufferSize - totalRead, SocketFlags.None);
					totalRead += bytesRead;
				}
				await Task.Delay(10);
			}

			return Encoding.UTF8.GetString(buffer, 0, totalRead);
		}
	}

	public class AsyncServer : SimpleTCPServer
	{
		private readonly CancellationTokenSource _cts = new CancellationTokenSource();

		public AsyncServer(int port) : base(port) { }

		public async Task<string> ReadAsync(CancellationToken cancellationToken)
		{
			if (_clientSocket == null || !_clientSocket.Connected)
				return string.Empty;

			var buffer = new byte[1024];

			using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cts.Token))
			{
				try
				{
					// Асинхронное чтение
					int bytesRead = await Task.Run(() =>
					{
						try
						{
							return _clientSocket.Receive(buffer);
						}
						catch (SocketException) when (linkedCts.Token.IsCancellationRequested)
						{
							return 0;
						}
					}, linkedCts.Token);

					return Encoding.UTF8.GetString(buffer, 0, bytesRead);
				}
				catch (OperationCanceledException)
				{
					return string.Empty;
				}
			}
		}

		public new void Stop()
		{
			_cts.Cancel();
			base.Stop();
		}
	}

	public class ProblemSimulatingServer : SimpleTCPServer
	{
		public class ProblemSimulationResult
		{
			public List<string> Chunks { get; set; } = new List<string>();
			public long ElapsedMs { get; set; }
			public bool DataIntegrity { get; set; }
		}

		public ProblemSimulatingServer(int port) : base(port) { }

		public async Task<ProblemSimulationResult> GetReceivedWithProblems(int timeoutMs)
		{
			var result = new ProblemSimulationResult();
			var stopwatch = Stopwatch.StartNew();

			// Симулируем проблемы: читаем с задержками и разными размерами
			var random = new Random();
			var allData = new StringBuilder();

			while (stopwatch.ElapsedMilliseconds < timeoutMs)
			{
				if (_clientSocket != null && _clientSocket.Connected && _clientSocket.Available > 0)
				{
					// Случайный размер буфера (1-100 байт)
					int bufferSize = random.Next(1, 101);
					var buffer = new byte[bufferSize];

					int bytesRead = _clientSocket.Receive(buffer, 0, Math.Min(bufferSize, _clientSocket.Available), SocketFlags.None);
					if (bytesRead > 0)
					{
						string chunk = Encoding.UTF8.GetString(buffer, 0, bytesRead);
						result.Chunks.Add(chunk);
						allData.Append(chunk);

						// Случайная задержка (0-200 мс)
						await Task.Delay(random.Next(0, 201));
					}
				}
				else
				{
					await Task.Delay(50);
				}
			}

			stopwatch.Stop();
			result.ElapsedMs = stopwatch.ElapsedMilliseconds;

			// Проверяем целостность данных (все чанки состоят из 'X')
			string allReceived = allData.ToString();
			result.DataIntegrity = allReceived.Length > 0 && allReceived.All(c => c == 'X');

			return result;
		}
	}

	public class DataAccumulator
	{
		private readonly StringBuilder _buffer = new StringBuilder();

		public void Add(string data)
		{
			_buffer.Append(data);
		}

		public string GetData()
		{
			return _buffer.ToString();
		}

		public void ClearProcessed(int length)
		{
			if (length >= _buffer.Length)
			{
				_buffer.Clear();
			}
			else
			{
				_buffer.Remove(0, length);
			}
		}
	}

	// Главная программа
	class Program
	{
		static async Task Main(string[] args)
		{
			Console.WriteLine("ПЕРЕДАЧА ДАННЫХ ПО TCP: ОТ ТЕОРИИ К ПРАКТИКЕ");
			Console.WriteLine("============================================\n");

			await TCPDataTransferDemo.DemonstrateDataTransfer();

			Console.WriteLine("\n\nОСНОВНЫЕ ВЫВОДЫ:");
			Console.WriteLine("1. TCP передаёт ПОТОК БАЙТОВ, не сообщения");
			Console.WriteLine("2. Одна отправка ≠ одно чтение (данные могут фрагментироваться)");
			Console.WriteLine("3. Кодировка должна быть согласована между клиентом и сервером");
			Console.WriteLine("4. Чтение и запись могут БЛОКИРОВАТЬ выполнение программы");
			Console.WriteLine("5. Границы сообщений определяет ПРИЛОЖЕНИЕ (не TCP)");
			Console.WriteLine("6. Ошибки и разрывы - НОРМАЛЬНАЯ часть сетевого программирования");
			Console.WriteLine("7. Локальные тесты часто скрывают реальные проблемы");
		}
	}
}