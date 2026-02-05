using System;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace SmtpFundamentals
{
	// Низкоуровневая демонстрация SMTP протокола
	public class RawSmtpClient : IDisposable
	{
		private TcpClient _tcpClient;
		private NetworkStream _networkStream;
		private SslStream _sslStream;
		private StreamReader _reader;
		private StreamWriter _writer;
		private bool _useSsl;
		private string _smtpServer;
		private int _smtpPort;

		// Константы SMTP
		private const int DEFAULT_SMTP_PORT = 25;
		private const int SSL_SMTP_PORT = 465;
		private const int SUBMISSION_PORT = 587;

		// Коды ответов SMTP
		public class SmtpResponse
		{
			public int Code { get; set; }
			public string Message { get; set; }
			public bool IsSuccess => Code >= 200 && Code < 400;
			public bool IsError => Code >= 400;

			public override string ToString() => $"{Code} {Message}";
		}

		public RawSmtpClient(string smtpServer, int port = 587)
		{
			_smtpServer = smtpServer ?? throw new ArgumentNullException(nameof(smtpServer));
			_smtpPort = port;
			_useSsl = port == SSL_SMTP_PORT;
		}

		// Подключение к SMTP серверу
		public async Task<SmtpResponse> ConnectAsync()
		{
			Console.WriteLine($"[SMTP] Установка TCP-соединения с {_smtpServer}:{_smtpPort}");

			_tcpClient = new TcpClient();
			await _tcpClient.ConnectAsync(_smtpServer, _smtpPort);

			_networkStream = _tcpClient.GetStream();

			if (_useSsl)
			{
				Console.WriteLine($"[SMTP] Установка SSL-соединения");
				_sslStream = new SslStream(_networkStream, false, ValidateServerCertificate);
				await _sslStream.AuthenticateAsClientAsync(_smtpServer);

				_reader = new StreamReader(_sslStream);
				_writer = new StreamWriter(_sslStream) { AutoFlush = true };
			}
			else
			{
				_reader = new StreamReader(_networkStream);
				_writer = new StreamWriter(_networkStream) { AutoFlush = true };
			}

			// Чтение приветственного сообщения сервера
			var welcomeResponse = await ReadResponseAsync();
			Console.WriteLine($"[SMTP] Сервер ответил: {welcomeResponse}");

			if (welcomeResponse.Code != 220)
			{
				throw new SmtpException($"Сервер не готов: {welcomeResponse}");
			}

			return welcomeResponse;
		}

		private bool ValidateServerCertificate(object sender, X509Certificate certificate,
			X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			// В реальном приложении здесь была бы более строгая проверка
			Console.WriteLine($"[SSL] Сертификат: {certificate.Subject}");
			Console.WriteLine($"[SSL] Ошибки SSL: {sslPolicyErrors}");
			return sslPolicyErrors == SslPolicyErrors.None;
		}

		// Отправка команды и чтение ответа
		private async Task<SmtpResponse> SendCommandAsync(string command)
		{
			Console.WriteLine($"[SMTP →] {command}");
			await _writer.WriteLineAsync(command);

			return await ReadResponseAsync();
		}

		// Чтение ответа сервера
		private async Task<SmtpResponse> ReadResponseAsync()
		{
			StringBuilder responseBuilder = new StringBuilder();
			SmtpResponse finalResponse = null;

			while (true)
			{
				string line = await _reader.ReadLineAsync();

				if (string.IsNullOrEmpty(line))
				{
					Console.WriteLine($"[SMTP ←] (пустой ответ)");
					continue;
				}

				Console.WriteLine($"[SMTP ←] {line}");

				// SMTP ответы начинаются с 3-значного кода
				if (line.Length >= 4 && int.TryParse(line.Substring(0, 3), out int code))
				{
					responseBuilder.AppendLine(line.Substring(4));

					// Если 4-й символ не дефис, значит это последняя строка ответа
					if (line[3] != '-')
					{
						finalResponse = new SmtpResponse
						{
							Code = code,
							Message = responseBuilder.ToString().Trim()
						};
						break;
					}
					else
					{
						responseBuilder.AppendLine(line.Substring(4));
					}
				}
				else
				{
					responseBuilder.AppendLine(line);
				}
			}

			return finalResponse;
		}

		// Команда EHLO - идентификация клиента
		public async Task<SmtpResponse> EhloAsync(string clientName = "localhost")
		{
			var response = await SendCommandAsync($"EHLO {clientName}");

			if (response.Code == 250)
			{
				Console.WriteLine($"[SMTP] Сервер поддерживает расширения:");
				Console.WriteLine($"[SMTP] {response.Message}");
			}
			else if (response.Code == 502) // Если EHLO не поддерживается, пробуем HELO
			{
				Console.WriteLine($"[SMTP] EHLO не поддерживается, пробуем HELO");
				response = await SendCommandAsync($"HELO {clientName}");
			}

			return response;
		}

		// Команда STARTTLS - переход на шифрованное соединение
		public async Task<SmtpResponse> StartTlsAsync()
		{
			var response = await SendCommandAsync("STARTTLS");

			if (response.Code == 220)
			{
				Console.WriteLine($"[SMTP] Переход на TLS...");

				// Создаём SSL поток поверх существующего TCP-соединения
				_sslStream = new SslStream(_networkStream, false, ValidateServerCertificate);
				await _sslStream.AuthenticateAsClientAsync(_smtpServer);

				// Заменяем ридеры и врайтеры для работы с SSL
				_reader = new StreamReader(_sslStream);
				_writer = new StreamWriter(_sslStream) { AutoFlush = true };

				Console.WriteLine($"[SMTP] TLS соединение установлено");

				// После STARTTLS нужно снова отправить EHLO
				return await EhloAsync();
			}

			return response;
		}

		// Аутентификация PLAIN методом
		public async Task<SmtpResponse> AuthenticatePlainAsync(string username, string password)
		{
			// Кодируем логин и пароль в Base64
			string credentials = Convert.ToBase64String(
				Encoding.UTF8.GetBytes($"\0{username}\0{password}"));

			var response = await SendCommandAsync("AUTH PLAIN");

			if (response.Code == 334) // Сервер ожидает данные аутентификации
			{
				response = await SendCommandAsync(credentials);
			}
			else
			{
				// Если сервер не ожидает отдельной команды, отправляем сразу с данными
				response = await SendCommandAsync($"AUTH PLAIN {credentials}");
			}

			if (response.Code == 235)
			{
				Console.WriteLine($"[SMTP] Аутентификация успешна");
			}

			return response;
		}

		// Аутентификация LOGIN методом
		public async Task<SmtpResponse> AuthenticateLoginAsync(string username, string password)
		{
			var response = await SendCommandAsync("AUTH LOGIN");

			if (response.Code == 334 && response.Message.Contains("Username"))
			{
				string encodedUsername = Convert.ToBase64String(Encoding.UTF8.GetBytes(username));
				response = await SendCommandAsync(encodedUsername);
			}

			if (response.Code == 334 && response.Message.Contains("Password"))
			{
				string encodedPassword = Convert.ToBase64String(Encoding.UTF8.GetBytes(password));
				response = await SendCommandAsync(encodedPassword);
			}

			if (response.Code == 235)
			{
				Console.WriteLine($"[SMTP] Аутентификация успешна");
			}

			return response;
		}

		// Указание отправителя
		public async Task<SmtpResponse> MailFromAsync(string fromAddress)
		{
			return await SendCommandAsync($"MAIL FROM:<{fromAddress}>");
		}

		// Указание получателя
		public async Task<SmtpResponse> RcptToAsync(string toAddress)
		{
			return await SendCommandAsync($"RCPT TO:<{toAddress}>");
		}

		// Начало передачи данных письма
		public async Task<SmtpResponse> DataAsync()
		{
			return await SendCommandAsync("DATA");
		}

		// Отправка содержимого письма
		public async Task<SmtpResponse> SendEmailDataAsync(string emailContent)
		{
			Console.WriteLine($"[SMTP →] (начало данных письма)");

			// Добавляем точку в конце для обозначения конца данных
			await _writer.WriteLineAsync(emailContent);
			await _writer.WriteLineAsync(".");

			return await ReadResponseAsync();
		}

		// Сброс текущей транзакции
		public async Task<SmtpResponse> ResetAsync()
		{
			return await SendCommandAsync("RSET");
		}

		// Завершение сессии
		public async Task<SmtpResponse> QuitAsync()
		{
			return await SendCommandAsync("QUIT");
		}

		// Отправка письма целиком
		public async Task<SmtpResponse> SendEmailAsync(
			string from,
			string to,
			string subject,
			string body,
			string username = null,
			string password = null)
		{
			Console.WriteLine($"[SMTP] Отправка письма:");
			Console.WriteLine($"  От: {from}");
			Console.WriteLine($"  Кому: {to}");
			Console.WriteLine($"  Тема: {subject}");

			// 1. Аутентификация (если указаны логин/пароль)
			if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
			{
				var authResponse = await AuthenticatePlainAsync(username, password);
				if (!authResponse.IsSuccess)
				{
					authResponse = await AuthenticateLoginAsync(username, password);
				}

				if (authResponse.IsError)
				{
					return authResponse;
				}
			}

			// 2. Указание отправителя
			var mailFromResponse = await MailFromAsync(from);
			if (mailFromResponse.IsError)
				return mailFromResponse;

			// 3. Указание получателя
			var rcptToResponse = await RcptToAsync(to);
			if (rcptToResponse.IsError)
				return rcptToResponse;

			// 4. Начало передачи данных
			var dataResponse = await DataAsync();
			if (dataResponse.IsError)
				return dataResponse;

			// 5. Формирование письма
			string emailData = BuildEmailData(from, to, subject, body);

			// 6. Отправка данных письма
			var sendResponse = await SendEmailDataAsync(emailData);

			return sendResponse;
		}

		private string BuildEmailData(string from, string to, string subject, string body)
		{
			var emailBuilder = new StringBuilder();

			// Заголовки письма
			emailBuilder.AppendLine($"From: {from}");
			emailBuilder.AppendLine($"To: {to}");
			emailBuilder.AppendLine($"Subject: {subject}");
			emailBuilder.AppendLine($"Date: {DateTime.Now:R}");
			emailBuilder.AppendLine($"MIME-Version: 1.0");
			emailBuilder.AppendLine($"Content-Type: text/plain; charset=utf-8");
			emailBuilder.AppendLine($"Content-Transfer-Encoding: 7bit");
			emailBuilder.AppendLine(); // Пустая строка разделяет заголовки и тело

			// Тело письма
			emailBuilder.AppendLine(body);

			return emailBuilder.ToString();
		}

		// Демонстрация всего SMTP диалога
		public async Task DemonstrateSmtpDialogAsync(string username, string password)
		{
			try
			{
				Console.WriteLine($"\n=== ПОЛНЫЙ SMTP ДИАЛОГ ===\n");

				// 1. Подключение
				await ConnectAsync();

				// 2. Приветствие сервера и идентификация клиента
				await EhloAsync("smtp-client.demo");

				// 3. STARTTLS (если порт 587)
				if (_smtpPort == SUBMISSION_PORT)
				{
					await StartTlsAsync();
				}

				// 4. Аутентификация
				await AuthenticatePlainAsync(username, password);

				// 5. Отправка тестового письма
				string testFrom = username;
				string testTo = username; // Отправляем самому себе
				string testSubject = "Тестовое письмо через SMTP";
				string testBody = "Это тестовое письмо, отправленное через низкоуровневый SMTP клиент.\n\n" +
								"SMTP - это текстовый протокол, который работает по принципу диалога.";

				var sendResult = await SendEmailAsync(testFrom, testTo, testSubject, testBody);

				Console.WriteLine($"\nРезультат отправки: {sendResult}");

				// 6. Завершение сессии
				await QuitAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Ошибка SMTP: {ex.Message}");
			}
		}

		public void Dispose()
		{
			_writer?.Dispose();
			_reader?.Dispose();
			_sslStream?.Dispose();
			_networkStream?.Dispose();
			_tcpClient?.Dispose();
		}
	}

	// Высокоуровневая обёртка с использованием System.Net.Mail
	public class HighLevelSmtpDemo
	{
		public static void SendEmailWithSmtpClient(
			string smtpServer,
			int port,
			string username,
			string password,
			string from,
			string to,
			string subject,
			string body)
		{
			Console.WriteLine($"\n=== ОТПРАВКА ЧЕРЕЗ System.Net.Mail.SmtpClient ===\n");

			try
			{
				using (var smtpClient = new SmtpClient(smtpServer, port))
				{
					smtpClient.Credentials = new NetworkCredential(username, password);
					smtpClient.EnableSsl = true;
					smtpClient.Timeout = 10000;

					Console.WriteLine($"Параметры SMTP:");
					Console.WriteLine($"  Сервер: {smtpClient.Host}:{smtpClient.Port}");
					Console.WriteLine($"  SSL: {smtpClient.EnableSsl}");
					Console.WriteLine($"  Таймаут: {smtpClient.Timeout} мс");

					var mailMessage = new MailMessage(from, to, subject, body)
					{
						IsBodyHtml = false
					};

					Console.WriteLine($"\nОтправка письма...");

					// Асинхронная отправка
					smtpClient.Send(mailMessage);

					Console.WriteLine($"✓ Письмо отправлено успешно");
					Console.WriteLine($"  От: {from}");
					Console.WriteLine($"  Кому: {to}");
					Console.WriteLine($"  Тема: {subject}");
				}
			}
			catch (SmtpException ex)
			{
				Console.WriteLine($"✗ SMTP ошибка: {ex.StatusCode} - {ex.Message}");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"✗ Общая ошибка: {ex.Message}");
			}
		}
	}

	// Демонстрация DNS MX-записей
	public class DnsMxLookup
	{
		public static void DemonstrateMxLookup(string domain)
		{
			Console.WriteLine($"\n=== DNS MX-ЗАПИСИ ДЛЯ {domain} ===\n");

			try
			{
				var mxRecords = System.Net.Dns.GetHostAddresses(domain);

				Console.WriteLine($"DNS записи для {domain}:");
				foreach (var record in mxRecords)
				{
					Console.WriteLine($"  - {record}");
				}

				// В реальном приложении для MX-записей нужна отдельная библиотека
				// System.Net.Dns не имеет встроенной поддержки MX записей
				Console.WriteLine($"\nПримечание: System.Net.Dns.GetHostAddresses() возвращает A-записи.");
				Console.WriteLine($"Для MX-записей требуется использовать стороннюю библиотеку или DNS-клиент.");

				Console.WriteLine($"\nПроцесс маршрутизации SMTP:");
				Console.WriteLine($"  1. Клиент получает домен из email (после @)");
				Console.WriteLine($"  2. Делает DNS MX запрос для этого домена");
				Console.WriteLine($"  3. Получает приоритет и имя почтового сервера");
				Console.WriteLine($"  4. Подключается к серверу с наивысшим приоритетом");
				Console.WriteLine($"  5. Если сервер недоступен, пробует следующий по приоритету");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Ошибка DNS: {ex.Message}");
			}
		}
	}

	// Конфигурация из .env файла
	public class EmailConfiguration
	{
		public string Sender { get; set; }
		public string Password { get; set; }
		public string Recipient { get; set; }
		public string SmtpServer { get; set; }
		public int SmtpPort { get; set; }

		public static EmailConfiguration FromEnvironment()
		{
			// В реальном приложении здесь читался бы .env файл
			// Для демонстрации используем константы

			return new EmailConfiguration
			{
				Sender = "efimov.matvey23@yandex.ru",
				Password = "vkxfywedeecchzrv",
				Recipient = "pznb@yandex.ru",
				SmtpServer = "smtp.yandex.ru",
				SmtpPort = 587
			};
		}
	}

	// Главная программа
	class Program
	{
		static async Task Main(string[] args)
		{
			Console.WriteLine("SMTP - ПРОТОКОЛ ОТПРАВКИ ЭЛЕКТРОННОЙ ПОЧТЫ");
			Console.WriteLine("==========================================\n");

			// Получаем конфигурацию
			var config = EmailConfiguration.FromEnvironment();

			Console.WriteLine("Конфигурация из .env:");
			Console.WriteLine($"  Отправитель: {config.Sender}");
			Console.WriteLine($"  Получатель: {config.Recipient}");
			Console.WriteLine($"  SMTP сервер: {config.SmtpServer}");
			Console.WriteLine($"  SMTP порт: {config.SmtpPort}");
			Console.WriteLine($"  Пароль приложения: {new string('*', config.Password.Length)}");

			// Часть 1: Низкоуровневый SMTP диалог
			Console.WriteLine("\n\n1. НИЗКОУРОВНЕВЫЙ SMTP ДИАЛОГ:");
			using (var rawSmtp = new RawSmtpClient(config.SmtpServer, config.SmtpPort))
			{
				// Для демонстрации отправляем письмо самому себе
				await rawSmtp.DemonstrateSmtpDialogAsync(config.Sender, config.Password);
			}

			// Часть 2: Высокоуровневая отправка
			Console.WriteLine("\n\n2. ВЫСОКОУРОВНЕВАЯ ОТПРАВКА:");
			HighLevelSmtpDemo.SendEmailWithSmtpClient(
				config.SmtpServer,
				config.SmtpPort,
				config.Sender,
				config.Password,
				config.Sender,
				config.Recipient,
				"Тестовое письмо с курса",
				"Это письмо отправлено через высокоуровневый SmtpClient.\n\n" +
				"System.Net.Mail скрывает низкоуровневые детали SMTP протокола,\n" +
				"но внутри всё равно происходит тот же самый диалог с сервером."
			);

			// Часть 3: DNS и маршрутизация
			Console.WriteLine("\n\n3. DNS И МАРШРУТИЗАЦИЯ ПОЧТЫ:");
			string domain = config.Sender.Substring(config.Sender.IndexOf('@') + 1);
			DnsMxLookup.DemonstrateMxLookup(domain);

			// Часть 4: Теория SMTP протокола
			Console.WriteLine("\n\n4. КЛЮЧЕВЫЕ АСПЕКТЫ SMTP:");
			PrintSmtpTheory();
		}

		static void PrintSmtpTheory()
		{
			Console.WriteLine("Ключевые аспекты SMTP протокола:");
			Console.WriteLine();
			Console.WriteLine("1. ТЕКСТОВЫЙ ПРОТОКОЛ:");
			Console.WriteLine("   • Команды и ответы в читаемом формате");
			Console.WriteLine("   • Каждая команда начинается с 4-буквенного кода");
			Console.WriteLine("   • Ответы начинаются с 3-значного кода");
			Console.WriteLine();
			Console.WriteLine("2. ДИАЛОГОВЫЙ ФОРМАТ:");
			Console.WriteLine("   • Клиент отправляет команду");
			Console.WriteLine("   • Сервер отвечает кодом и сообщением");
			Console.WriteLine("   • Код определяет успешность операции");
			Console.WriteLine();
			Console.WriteLine("3. ПОСЛЕДОВАТЕЛЬНОСТЬ КОМАНД:");
			Console.WriteLine("   • EHLO/HELO - приветствие сервера");
			Console.WriteLine("   • STARTTLS - переход на шифрование (опционально)");
			Console.WriteLine("   • AUTH - аутентификация");
			Console.WriteLine("   • MAIL FROM - указание отправителя");
			Console.WriteLine("   • RCPT TO - указание получателя");
			Console.WriteLine("   • DATA - начало передачи письма");
			Console.WriteLine("   • . (точка) - конец письма");
			Console.WriteLine("   • QUIT - завершение сессии");
			Console.WriteLine();
			Console.WriteLine("4. КОДЫ ОТВЕТОВ:");
			Console.WriteLine("   • 2xx - успех");
			Console.WriteLine("   • 3xx - промежуточный успех");
			Console.WriteLine("   • 4xx - временная ошибка");
			Console.WriteLine("   • 5xx - постоянная ошибка");
			Console.WriteLine();
			Console.WriteLine("5. БЕЗОПАСНОСТЬ:");
			Console.WriteLine("   • Порт 25 - без шифрования (обычно блокируется)");
			Console.WriteLine("   • Порт 587 - STARTTLS (шифрование по запросу)");
			Console.WriteLine("   • Порт 465 - SSL/TLS (шифрование сразу)");
			Console.WriteLine();
			Console.WriteLine("6. DNS И MX-ЗАПИСИ:");
			Console.WriteLine("   • Для маршрутизации почты используются MX-записи");
			Console.WriteLine("   • MX-запись указывает на почтовый сервер домена");
			Console.WriteLine("   • Приоритет определяет порядок попыток доставки");
		}
	}
}