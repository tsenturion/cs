using System;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace HttpsClientImplementation
{
	public class HttpsClientDemonstration : IDisposable
	{
		private HttpClient _httpClient;
		private HttpClientHandler _httpClientHandler;

		public HttpsClientDemonstration()
		{
			InitializeHttpClient();
		}

		private void InitializeHttpClient()
		{
			Console.WriteLine("=== ИНИЦИАЛИЗАЦИЯ HTTPS-CLIENT ===\n");

			// Создание обработчика HTTP с настройками для HTTPS
			_httpClientHandler = new HttpClientHandler
			{
				// Автоматическая проверка сертификатов (по умолчанию включена)
				ServerCertificateCustomValidationCallback = null,

				// Настройки TLS
				SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,

				// Максимальное количество подключений
				MaxConnectionsPerServer = 10,

				// Использование Keep-Alive (важно для HTTPS)
				UseProxy = false,
				UseCookies = false,

				// Автоматическое перенаправление
				AllowAutoRedirect = true,
				MaxAutomaticRedirections = 5
			};

			Console.WriteLine($"  Создан HttpClientHandler:");
			Console.WriteLine($"    SslProtocols: {_httpClientHandler.SslProtocols}");
			Console.WriteLine($"    MaxConnectionsPerServer: {_httpClientHandler.MaxConnectionsPerServer}");
			Console.WriteLine($"    ServerCertificateCustomValidationCallback: {(_httpClientHandler.ServerCertificateCustomValidationCallback == null ? "стандартная" : "кастомная")}");

			// Создание HttpClient с настройками для HTTPS
			_httpClient = new HttpClient(_httpClientHandler)
			{
				Timeout = TimeSpan.FromSeconds(30),

				// Базовый адрес для переиспользования соединений
				BaseAddress = new Uri("https://httpbin.org/")
			};

			// Настройка заголовков по умолчанию
			_httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("MyHttpsClient/1.0");
			_httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/json");

			Console.WriteLine($"\n  Создан HttpClient:");
			Console.WriteLine($"    BaseAddress: {_httpClient.BaseAddress}");
			Console.WriteLine($"    Timeout: {_httpClient.Timeout.TotalSeconds} сек");
			Console.WriteLine($"    User-Agent: {_httpClient.DefaultRequestHeaders.UserAgent}");
		}

		public async Task DemonstrateHttpsRequest()
		{
			Console.WriteLine("\n=== ВЫПОЛНЕНИЕ HTTPS-ЗАПРОСА ===\n");

			string url = "https://httpbin.org/get";

			Console.WriteLine($"  Отправка GET запроса: {url}");
			Console.WriteLine($"  Используется схема: {new Uri(url).Scheme}");

			Stopwatch stopwatch = Stopwatch.StartNew();

			try
			{
				// Отправка HTTPS-запроса
				HttpResponseMessage response = await _httpClient.GetAsync(url);

				stopwatch.Stop();

				Console.WriteLine($"\n  Ответ получен за {stopwatch.ElapsedMilliseconds} мс");
				Console.WriteLine($"  Status Code: {(int)response.StatusCode} {response.StatusCode}");
				Console.WriteLine($"  IsSuccessStatusCode: {response.IsSuccessStatusCode}");

				// Проверка, был ли запрос отправлен через HTTPS
				Console.WriteLine($"  RequestMessage.RequestUri: {response.RequestMessage.RequestUri}");
				Console.WriteLine($"  RequestMessage.Version: {response.RequestMessage.Version}");

				// Проверка заголовков безопасности
				Console.WriteLine($"\n  Заголовки ответа:");
				foreach (var header in response.Headers)
				{
					if (header.Key.StartsWith("Strict-Transport-Security") ||
						header.Key.StartsWith("Content-Security") ||
						header.Key.StartsWith("X-Content-Type"))
					{
						Console.WriteLine($"    {header.Key}: {string.Join(", ", header.Value)}");
					}
				}

				// Чтение тела ответа
				if (response.IsSuccessStatusCode)
				{
					string responseBody = await response.Content.ReadAsStringAsync();
					Console.WriteLine($"\n  Тело ответа (первые 500 символов):");
					Console.WriteLine($"    {responseBody.Substring(0, Math.Min(500, responseBody.Length))}...");
				}

				// Анализ информации о подключении
				Console.WriteLine($"\n  Информация о подключении:");
				Console.WriteLine($"    Headers count: {response.Headers.Count()}");

				// Демонстрация редиректов
				if (response.Headers.Location != null)
				{
					Console.WriteLine($"    Location header: {response.Headers.Location}");
				}
			}
			catch (HttpRequestException ex)
			{
				stopwatch.Stop();
				Console.WriteLine($"\n  ОШИБКА HTTP-запроса:");
				Console.WriteLine($"    Тип: {ex.GetType().Name}");
				Console.WriteLine($"    Сообщение: {ex.Message}");
				Console.WriteLine($"    InnerException: {ex.InnerException?.GetType().Name} - {ex.InnerException?.Message}");

				// Анализ ошибки TLS
				if (ex.InnerException is AuthenticationException authEx)
				{
					Console.WriteLine($"    Это ошибка аутентификации TLS!");
				}
			}
			catch (TaskCanceledException ex)
			{
				Console.WriteLine($"\n  ОШИБКА: Таймаут запроса ({_httpClient.Timeout.TotalSeconds} сек)");
				Console.WriteLine($"    Сообщение: {ex.Message}");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"\n  НЕОЖИДАННАЯ ОШИБКА:");
				Console.WriteLine($"    Тип: {ex.GetType().Name}");
				Console.WriteLine($"    Сообщение: {ex.Message}");
			}
		}

		public void DemonstrateCertificateValidation()
		{
			Console.WriteLine("\n=== ПРОВЕРКА СЕРТИФИКАТОВ ===\n");

			// 1. Стандартная проверка сертификатов
			Console.WriteLine("1. СТАНДАРТНАЯ ПРОВЕРКА СЕРТИФИКАТОВ:");

			Console.WriteLine($"   HttpClient проверяет:");
			Console.WriteLine($"     • Цепочку доверия до корневого сертификата");
			Console.WriteLine($"     • Срок действия сертификата");
			Console.WriteLine($"     • Соответствие имени хоста (Subject Alternative Names)");
			Console.WriteLine($"     • Отзыв сертификата (CRL/OCSP)");
			Console.WriteLine($"     • Ключевую подпись и алгоритмы");

			// 2. Кастомная проверка сертификатов
			Console.WriteLine("\n2. КАСТОМНАЯ ПРОВЕРКА СЕРТИФИКАТОВ:");

			var customHandler = new HttpClientHandler
			{
				ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, chain, sslPolicyErrors) =>
				{
					Console.WriteLine($"\n   Кастомная проверка для: {httpRequestMessage.RequestUri.Host}");
					Console.WriteLine($"     SSL Policy Errors: {sslPolicyErrors}");

					if (cert != null)
					{
						Console.WriteLine($"     Сертификат субъекта: {cert.Subject}");
						Console.WriteLine($"     Действителен с: {cert.NotBefore} до: {cert.NotAfter}");
						Console.WriteLine($"     Издатель: {cert.Issuer}");
						Console.WriteLine($"     Отпечаток: {cert.Thumbprint}");
					}

					if (chain != null)
					{
						Console.WriteLine($"     Цепочка сертификатов: {chain.ChainElements.Count} элементов");

						for (int i = 0; i < chain.ChainElements.Count; i++)
						{
							var element = chain.ChainElements[i];
							Console.WriteLine($"       [{i}] {element.Certificate.Subject} ({element.Certificate.Issuer})");
						}
					}

					// Пример: разрешаем определённые самоподписанные сертификаты
					bool allowSelfSigned = cert?.Subject?.Contains("CN=localhost") == true;

					if (allowSelfSigned)
					{
						Console.WriteLine($"     ⚠️  Разрешаем самоподписанный сертификат localhost");
						return true;
					}

					// Возвращаем стандартную проверку
					return sslPolicyErrors == SslPolicyErrors.None;
				}
			};

			Console.WriteLine($"   Кастомный обработчик создан с валидацией");

			// 3. Опасный вариант: отключение проверки сертификатов
			Console.WriteLine("\n3. ОПАСНО: ОТКЛЮЧЕНИЕ ПРОВЕРКИ СЕРТИФИКАТОВ:");

			var dangerousHandler = new HttpClientHandler
			{
				ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
			};

			Console.WriteLine($"   ⚠️  ОПАСНОСТЬ: Все сертификаты принимаются!");
			Console.WriteLine($"   ❌ НИКОГДА не используйте в продакшене!");

			dangerousHandler.Dispose();
			customHandler.Dispose();
		}

		public void DemonstrateSslProtocols()
		{
			Console.WriteLine("\n=== НАСТРОЙКА SSL/TLS ПРОТОКОЛОВ ===\n");

			Console.WriteLine("  Поддерживаемые протоколы в системе:");

			foreach (SslProtocols protocol in Enum.GetValues(typeof(SslProtocols)))
			{
				if (protocol != SslProtocols.None && protocol != SslProtocols.Default)
				{
					Console.WriteLine($"    • {protocol}");
				}
			}

			// Тестирование подключения с разными протоколами
			Console.WriteLine("\n  Тестирование подключений с разными протоколами:");

			var testUrls = new[]
			{
				"https://httpbin.org/",
				"https://tls-v1-2.badssl.com:1012/",
				"https://tls-v1-0.badssl.com:1010/"
			};

			foreach (var url in testUrls)
			{
				try
				{
					using var testHandler = new HttpClientHandler
					{
						SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13
					};

					using var testClient = new HttpClient(testHandler);
					testClient.Timeout = TimeSpan.FromSeconds(5);

					var response = testClient.GetAsync(url).GetAwaiter().GetResult();
					Console.WriteLine($"    ✓ {url}: {response.StatusCode} (TLS 1.2+)");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"    ✗ {url}: {ex.GetType().Name}");
				}
			}
		}

		public async Task DemonstrateConnectionReuse()
		{
			Console.WriteLine("\n=== ПЕРЕИСПОЛЬЗОВАНИЕ СОЕДИНЕНИЙ ===\n");

			string baseUrl = "https://httpbin.org";

			Console.WriteLine($"  Тест переиспользования соединений к {baseUrl}");
			Console.WriteLine($"  MaxConnectionsPerServer: {_httpClientHandler.MaxConnectionsPerServer}");

			var tasks = new Task[3];
			var stopwatches = new Stopwatch[3];

			for (int i = 0; i < 3; i++)
			{
				int index = i;
				stopwatches[index] = Stopwatch.StartNew();

				tasks[index] = Task.Run(async () =>
				{
					try
					{
						string endpoint = i == 0 ? "/get" : i == 1 ? "/headers" : "/ip";
						var response = await _httpClient.GetAsync(endpoint);
						stopwatches[index].Stop();

						Console.WriteLine($"    Запрос {index + 1} ({endpoint}): {response.StatusCode} за {stopwatches[index].ElapsedMilliseconds} мс");
					}
					catch (Exception ex)
					{
						Console.WriteLine($"    Запрос {index + 1}: Ошибка - {ex.Message}");
					}
				});
			}

			await Task.WhenAll(tasks);

			Console.WriteLine($"\n  Анализ производительности:");
			long firstRequestTime = stopwatches[0].ElapsedMilliseconds;

			for (int i = 1; i < 3; i++)
			{
				if (stopwatches[i].ElapsedMilliseconds < firstRequestTime)
				{
					Console.WriteLine($"    Запрос {i + 1} быстрее первого на {firstRequestTime - stopwatches[i].ElapsedMilliseconds} мс (возможно, переиспользовано соединение)");
				}
			}
		}

		public async Task DemonstrateErrorHandling()
		{
			Console.WriteLine("\n=== ОБРАБОТКА ОШИБОК HTTPS ===\n");

			// Тестовые URL для демонстрации разных ошибок
			var testCases = new[]
			{
				new { Url = "https://expired.badssl.com/", Description = "Просроченный сертификат" },
				new { Url = "https://wrong.host.badssl.com/", Description = "Несоответствие имени хоста" },
				new { Url = "https://self-signed.badssl.com/", Description = "Самоподписанный сертификат" },
				new { Url = "https://untrusted-root.badssl.com/", Description = "Недоверенный корневой сертификат" },
				new { Url = "https://httpbin.org/status/500", Description = "HTTP ошибка 500" },
				new { Url = "https://httpbin.org/status/404", Description = "HTTP ошибка 404" }
			};

			foreach (var testCase in testCases)
			{
				Console.WriteLine($"\n  Тест: {testCase.Description}");
				Console.WriteLine($"    URL: {testCase.Url}");

				try
				{
					// Используем отдельный HttpClient для каждого теста
					using var testClient = new HttpClient(new HttpClientHandler());
					testClient.Timeout = TimeSpan.FromSeconds(10);

					var response = await testClient.GetAsync(testCase.Url);

					Console.WriteLine($"    Результат: HTTP {(int)response.StatusCode} {response.StatusCode}");
					Console.WriteLine($"    IsSuccessStatusCode: {response.IsSuccessStatusCode}");

					if (!response.IsSuccessStatusCode)
					{
						string errorContent = await response.Content.ReadAsStringAsync();
						Console.WriteLine($"    Тело ошибки: {errorContent.Substring(0, Math.Min(200, errorContent.Length))}...");
					}
				}
				catch (HttpRequestException ex)
				{
					Console.WriteLine($"    ОШИБКА: HttpRequestException");
					Console.WriteLine($"      Сообщение: {ex.Message}");

					// Анализ вложенных исключений
					Exception inner = ex.InnerException;
					int depth = 0;

					while (inner != null && depth < 3)
					{
						Console.WriteLine($"      InnerException[{depth}]: {inner.GetType().Name} - {inner.Message}");
						inner = inner.InnerException;
						depth++;
					}
				}
				catch (AuthenticationException ex)
				{
					Console.WriteLine($"    ОШИБКА: AuthenticationException (TLS)");
					Console.WriteLine($"      Сообщение: {ex.Message}");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"    ОШИБКА: {ex.GetType().Name}");
					Console.WriteLine($"      Сообщение: {ex.Message}");
				}
			}
		}

		public void DemonstrateClientConfiguration()
		{
			Console.WriteLine("\n=== КОНФИГУРАЦИЯ HTTPS-CLIENT ===\n");

			// Демонстрация разных конфигураций
			Console.WriteLine("1. БАЗОВАЯ КОНФИГУРАЦИЯ:");

			var basicConfig = new HttpClientHandler
			{
				AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
				UseCookies = true,
				CookieContainer = new CookieContainer(),
				AllowAutoRedirect = true,
				MaxAutomaticRedirections = 5,
				UseProxy = true,
				Proxy = null, // Использует системные настройки
				DefaultProxyCredentials = null
			};

			Console.WriteLine($"   AutomaticDecompression: {basicConfig.AutomaticDecompression}");
			Console.WriteLine($"   AllowAutoRedirect: {basicConfig.AllowAutoRedirect}");
			Console.WriteLine($"   MaxAutomaticRedirections: {basicConfig.MaxAutomaticRedirections}");
			Console.WriteLine($"   UseCookies: {basicConfig.UseCookies}");

			// 2. Конфигурация для высоконагруженных систем
			Console.WriteLine("\n2. КОНФИГУРАЦИЯ ДЛЯ ВЫСОКОНАГРУЖЕННЫХ СИСТЕМ:");

			var highPerfConfig = new HttpClientHandler
			{
				MaxConnectionsPerServer = 100,
				MaxResponseHeadersLength = 128, // KB
				UseProxy = false,
				AllowAutoRedirect = false, // Управление редиректами вручную
				AutomaticDecompression = DecompressionMethods.All,
				UseCookies = false // Отключаем куки если не нужны
			};

			Console.WriteLine($"   MaxConnectionsPerServer: {highPerfConfig.MaxConnectionsPerServer}");
			Console.WriteLine($"   MaxResponseHeadersLength: {highPerfConfig.MaxResponseHeadersLength} KB");

			// 3. Конфигурация для строгой безопасности
			Console.WriteLine("\n3. КОНФИГУРАЦИЯ СТРОГОЙ БЕЗОПАСНОСТИ:");

			var secureConfig = new HttpClientHandler
			{
				SslProtocols = SslProtocols.Tls13, // Только TLS 1.3
				CheckCertificateRevocationList = true,
				ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
				{
					// Дополнительные проверки
					if (errors != SslPolicyErrors.None)
						return false;

					// Проверка минимальной длины ключа
					if (cert?.GetRSAPublicKey()?.KeySize < 2048)
						return false;

					// Проверка алгоритма подписи
					if (!cert.SignatureAlgorithm.FriendlyName?.Contains("sha256") == true)
						return false;

					return true;
				}
			};

			Console.WriteLine($"   SslProtocols: {secureConfig.SslProtocols}");
			Console.WriteLine($"   CheckCertificateRevocationList: {secureConfig.CheckCertificateRevocationList}");

			basicConfig.Dispose();
			highPerfConfig.Dispose();
			secureConfig.Dispose();
		}

		public async Task DemonstrateRealWorldScenario()
		{
			Console.WriteLine("\n=== РЕАЛЬНЫЙ СЦЕНАРИЙ ИСПОЛЬЗОВАНИЯ ===\n");

			// Симуляция работы с API
			Console.WriteLine("  Симуляция работы с защищённым API:");

			var apiClient = new HttpClient(new HttpClientHandler
			{
				AutomaticDecompression = DecompressionMethods.All,
				MaxConnectionsPerServer = 20
			})
			{
				BaseAddress = new Uri("https://jsonplaceholder.typicode.com/"),
				Timeout = TimeSpan.FromSeconds(15)
			};

			apiClient.DefaultRequestHeaders.Add("Accept", "application/json");
			apiClient.DefaultRequestHeaders.Add("User-Agent", "MyApiClient/1.0");

			try
			{
				// 1. Получение данных
				Console.WriteLine("\n  1. Получение списка пользователей:");
				var usersResponse = await apiClient.GetAsync("users");

				if (usersResponse.IsSuccessStatusCode)
				{
					string usersJson = await usersResponse.Content.ReadAsStringAsync();
					Console.WriteLine($"    ✓ Получено {usersJson.Length} байт");
					Console.WriteLine($"    Status Code: {(int)usersResponse.StatusCode}");

					// Проверка безопасности
					if (usersResponse.Headers.TryGetValues("Strict-Transport-Security", out var hsts))
					{
						Console.WriteLine($"    HSTS: {string.Join(", ", hsts)}");
					}
				}

				// 2. Отправка данных
				Console.WriteLine("\n  2. Создание нового поста:");

				var postData = new
				{
					title = "HTTPS Client Test",
					body = "Testing HTTPS client implementation",
					userId = 1
				};

				var jsonContent = new StringContent(
					System.Text.Json.JsonSerializer.Serialize(postData),
					Encoding.UTF8,
					"application/json"
				);

				var postResponse = await apiClient.PostAsync("posts", jsonContent);

				Console.WriteLine($"    Status Code: {(int)postResponse.StatusCode}");

				if (postResponse.IsSuccessStatusCode)
				{
					string postResult = await postResponse.Content.ReadAsStringAsync();
					Console.WriteLine($"    ✓ Пост создан, ответ: {postResult.Length} байт");
				}

				// 3. Параллельные запросы
				Console.WriteLine("\n  3. Параллельные запросы:");

				var parallelTasks = new[]
				{
					apiClient.GetAsync("posts/1"),
					apiClient.GetAsync("comments/1"),
					apiClient.GetAsync("albums/1")
				};

				var results = await Task.WhenAll(parallelTasks);

				foreach (var response in results)
				{
					Console.WriteLine($"    {response.RequestMessage.RequestUri.PathAndQuery}: {(int)response.StatusCode}");
				}

			}
			catch (Exception ex)
			{
				Console.WriteLine($"  Ошибка: {ex.GetType().Name}: {ex.Message}");
			}
			finally
			{
				apiClient.Dispose();
			}
		}

		public void Dispose()
		{
			_httpClient?.Dispose();
			_httpClientHandler?.Dispose();

			Console.WriteLine($"\n  HttpClient и ресурсы освобождены");
			Console.WriteLine($"  Закрыты все HTTPS-соединения");
		}
	}

	// Главная программа
	class Program
	{
		static async Task Main(string[] args)
		{
			Console.WriteLine("HTTPS-CLIENT В C#: РЕАЛИЗАЦИЯ И ПРАКТИКА");
			Console.WriteLine("=========================================\n");

			using (var demo = new HttpsClientDemonstration())
			{
				// Демонстрация работы HTTPS-клиента
				await demo.DemonstrateHttpsRequest();

				// Демонстрация проверки сертификатов
				demo.DemonstrateCertificateValidation();

				// Демонстрация SSL/TLS протоколов
				demo.DemonstrateSslProtocols();

				// Демонстрация переиспользования соединений
				await demo.DemonstrateConnectionReuse();

				// Демонстрация обработки ошибок
				await demo.DemonstrateErrorHandling();

				// Демонстрация конфигурации
				demo.DemonstrateClientConfiguration();

				// Реальный сценарий использования
				await demo.DemonstrateRealWorldScenario();
			}
		}
	}
}
