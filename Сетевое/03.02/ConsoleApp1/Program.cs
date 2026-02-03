using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.Json;

namespace HttpClientDemo
{
	// Демонстрация работы с HttpClient
	public class HttpClientDemonstration : IDisposable
	{
		// Единый экземпляр HttpClient для всего приложения
		private readonly HttpClient _httpClient;
		private readonly HttpClient _customizedClient;
		private bool _disposed = false;

		// Демонстрационные URL
		private const string TestApiUrl = "https://jsonplaceholder.typicode.com";
		private const string LocalTestUrl = "http://localhost:5000";

		// Статистика использования
		private int _totalRequests = 0;
		private int _failedRequests = 0;
		private readonly object _statsLock = new object();

		public HttpClientDemonstration()
		{
			Console.WriteLine("=== HTTP-КЛИЕНТ В C#: ОСОЗНАННОЕ ИСПОЛЬЗОВАНИЕ ===\n");

			// 1. Создание базового HttpClient с настройками по умолчанию
			_httpClient = new HttpClient();

			// 2. Создание настроенного HttpClient для демонстрации
			_customizedClient = CreateConfiguredClient();

			Console.WriteLine("HttpClient создан. Управление ресурсами начато.\n");
		}

		private HttpClient CreateConfiguredClient()
		{
			// Создание HttpClientHandler с настройками
			var handler = new HttpClientHandler
			{
				UseCookies = true,
				CookieContainer = new System.Net.CookieContainer(),
				AllowAutoRedirect = true,
				MaxAutomaticRedirections = 5,
				UseDefaultCredentials = false
			};

			// Создание HttpClient с настроенным обработчиком
			var client = new HttpClient(handler);

			// Настройка базовых параметров
			client.Timeout = TimeSpan.FromSeconds(30);
			client.MaxResponseContentBufferSize = 10 * 1024 * 1024; // 10 MB

			// Установка общих заголовков для всех запросов
			client.DefaultRequestHeaders.UserAgent.ParseAdd("HttpClientDemo/1.0");
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
			client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));

			// Keep-Alive заголовок для повторного использования соединений
			client.DefaultRequestHeaders.Connection.Add("keep-alive");

			return client;
		}

		public async Task DemonstrateHttpClientBasics()
		{
			Console.WriteLine("1. ОСНОВНЫЕ ПРИНЦИПЫ РАБОТЫ С HttpClient:");

			// Демонстрация повторного использования HttpClient
			await DemonstrateClientReuse();

			// Демонстрация различных HTTP-методов
			await DemonstrateHttpMethods();

			// Демонстрация работы с заголовками
			await DemonstrateHeaders();

			// Демонстрация отправки данных
			await DemonstrateSendingData();

			// Демонстрация обработки ошибок и таймаутов
			await DemonstrateErrorHandling();

			// Демонстрация асинхронности и отмены
			await DemonstrateAsyncAndCancellation();

			// Демонстрация архитектурного подхода
			await DemonstrateArchitecturalApproach();

			PrintStatistics();
		}

		private async Task DemonstrateClientReuse()
		{
			Console.WriteLine("\n   1.1. ПОВТОРНОЕ ИСПОЛЬЗОВАНИЕ HttpClient:");

			// Неправильный подход: создание нового клиента для каждого запроса
			Console.WriteLine("   ❌ НЕПРАВИЛЬНО: Новый HttpClient для каждого запроса");
			var stopwatch = Stopwatch.StartNew();

			for (int i = 0; i < 3; i++)
			{
				using (var tempClient = new HttpClient())
				{
					try
					{
						var response = await tempClient.GetAsync($"{TestApiUrl}/posts/{i + 1}");
						Console.WriteLine($"     Запрос {i + 1}: {response.StatusCode}");
					}
					catch { }
				}
			}

			stopwatch.Stop();
			Console.WriteLine($"     Время с новыми клиентами: {stopwatch.ElapsedMilliseconds} мс");

			// Правильный подход: повторное использование одного клиента
			Console.WriteLine("\n   ✅ ПРАВИЛЬНО: Один HttpClient для всех запросов");
			stopwatch.Restart();

			for (int i = 0; i < 3; i++)
			{
				try
				{
					var response = await _httpClient.GetAsync($"{TestApiUrl}/posts/{i + 1}");
					Console.WriteLine($"     Запрос {i + 1}: {response.StatusCode}");
					UpdateStatistics(response.IsSuccessStatusCode);
				}
				catch
				{
					UpdateStatistics(false);
				}
			}

			stopwatch.Stop();
			Console.WriteLine($"     Время с одним клиентом: {stopwatch.ElapsedMilliseconds} мс");
			Console.WriteLine($"     Примечание: HttpClient использует пул соединений");
		}

		private async Task DemonstrateHttpMethods()
		{
			Console.WriteLine("\n   1.2. РАЗЛИЧНЫЕ HTTP-МЕТОДЫ:");

			try
			{
				// GET запрос
				Console.WriteLine("\n   GET запрос:");
				var getResponse = await _customizedClient.GetAsync($"{TestApiUrl}/posts/1");
				Console.WriteLine($"     Status: {getResponse.StatusCode}");
				Console.WriteLine($"     Headers: {getResponse.Headers.Count()}");

				if (getResponse.IsSuccessStatusCode)
				{
					var content = await getResponse.Content.ReadAsStringAsync();
					Console.WriteLine($"     Body length: {content.Length} chars");
				}
				UpdateStatistics(getResponse.IsSuccessStatusCode);

				// POST запрос
				Console.WriteLine("\n   POST запрос:");
				var postData = new { title = "foo", body = "bar", userId = 1 };
				var postContent = new StringContent(
					JsonSerializer.Serialize(postData),
					Encoding.UTF8,
					"application/json");

				var postResponse = await _customizedClient.PostAsync($"{TestApiUrl}/posts", postContent);
				Console.WriteLine($"     Status: {postResponse.StatusCode}");
				UpdateStatistics(postResponse.IsSuccessStatusCode);

				// PUT запрос
				Console.WriteLine("\n   PUT запрос:");
				var putData = new { id = 1, title = "updated", body = "updated body", userId = 1 };
				var putContent = new StringContent(
					JsonSerializer.Serialize(putData),
					Encoding.UTF8,
					"application/json");

				var putResponse = await _customizedClient.PutAsync($"{TestApiUrl}/posts/1", putContent);
				Console.WriteLine($"     Status: {putResponse.StatusCode}");
				UpdateStatistics(putResponse.IsSuccessStatusCode);

				// DELETE запрос
				Console.WriteLine("\n   DELETE запрос:");
				var deleteResponse = await _customizedClient.DeleteAsync($"{TestApiUrl}/posts/1");
				Console.WriteLine($"     Status: {deleteResponse.StatusCode}");
				UpdateStatistics(deleteResponse.IsSuccessStatusCode);
			}
			catch (HttpRequestException ex)
			{
				Console.WriteLine($"   Сетевая ошибка: {ex.Message}");
				UpdateStatistics(false);
			}
		}

		private async Task DemonstrateHeaders()
		{
			Console.WriteLine("\n   1.3. РАБОТА С ЗАГОЛОВКАМИ:");

			try
			{
				// Заголовки клиента (устанавливаются для всех запросов)
				Console.WriteLine("   Заголовки клиента (общие):");
				foreach (var header in _customizedClient.DefaultRequestHeaders)
				{
					Console.WriteLine($"     {header.Key}: {string.Join(", ", header.Value)}");
				}

				// Заголовки конкретного запроса
				Console.WriteLine("\n   Заголовки конкретного запроса:");
				using (var request = new HttpRequestMessage(HttpMethod.Get, $"{TestApiUrl}/posts/1"))
				{
					// Добавляем специфичные заголовки
					request.Headers.Add("X-Custom-Header", "CustomValue");
					request.Headers.Add("X-Request-ID", Guid.NewGuid().ToString());
					request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

					Console.WriteLine("   Добавленные заголовки запроса:");
					foreach (var header in request.Headers)
					{
						Console.WriteLine($"     {header.Key}: {string.Join(", ", header.Value)}");
					}

					var response = await _customizedClient.SendAsync(request);
					Console.WriteLine($"\n   Ответные заголовки:");

					// Важно: сначала проверяем статус-код
					Console.WriteLine($"     Status Code: {response.StatusCode} ({(int)response.StatusCode})");

					if (response.IsSuccessStatusCode)
					{
						Console.WriteLine($"     Content-Type: {response.Content.Headers.ContentType}");
						Console.WriteLine($"     Content-Length: {response.Content.Headers.ContentLength}");
						Console.WriteLine($"     Date: {response.Headers.Date}");

						// Пример чтения кастомных заголовков
						if (response.Headers.TryGetValues("X-Powered-By", out var poweredByValues))
						{
							Console.WriteLine($"     X-Powered-By: {string.Join(", ", poweredByValues)}");
						}
					}
					UpdateStatistics(response.IsSuccessStatusCode);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"   Ошибка: {ex.Message}");
				UpdateStatistics(false);
			}
		}

		private async Task DemonstrateSendingData()
		{
			Console.WriteLine("\n   1.4. ОТПРАВКА ДАННЫХ:");

			try
			{
				// 1. Отправка JSON данных
				Console.WriteLine("\n   Отправка JSON данных:");
				var user = new { name = "John Doe", email = "john@example.com" };
				var jsonContent = new StringContent(
					JsonSerializer.Serialize(user),
					Encoding.UTF8,
					"application/json"); // Важно: устанавливаем Content-Type!

				var jsonResponse = await _customizedClient.PostAsync($"{TestApiUrl}/posts", jsonContent);
				Console.WriteLine($"     Status: {jsonResponse.StatusCode}");
				UpdateStatistics(jsonResponse.IsSuccessStatusCode);

				// 2. Отправка form-data
				Console.WriteLine("\n   Отправка form-data:");
				var formData = new Dictionary<string, string>
				{
					["username"] = "johndoe",
					["password"] = "secret123"
				};

				var formContent = new FormUrlEncodedContent(formData);
				var formResponse = await _customizedClient.PostAsync($"{TestApiUrl}/posts", formContent);
				Console.WriteLine($"     Status: {formResponse.StatusCode}");
				UpdateStatistics(formResponse.IsSuccessStatusCode);

				// 3. Отправка multipart/form-data
				Console.WriteLine("\n   Отправка multipart/form-data:");
				using (var multipartContent = new MultipartFormDataContent())
				{
					multipartContent.Add(new StringContent("John"), "firstName");
					multipartContent.Add(new StringContent("Doe"), "lastName");

					// Симуляция загрузки файла
					var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes("File content"));
					fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
					multipartContent.Add(fileContent, "file", "document.txt");

					var multipartResponse = await _customizedClient.PostAsync($"{TestApiUrl}/posts", multipartContent);
					Console.WriteLine($"     Status: {multipartResponse.StatusCode}");
					UpdateStatistics(multipartResponse.IsSuccessStatusCode);
				}

				// 4. Чтение ответа с проверкой типа контента
				Console.WriteLine("\n   Чтение ответа с проверкой типа:");
				var testResponse = await _customizedClient.GetAsync($"{TestApiUrl}/posts/1");

				if (testResponse.IsSuccessStatusCode)
				{
					// Проверяем Content-Type перед чтением
					var contentType = testResponse.Content.Headers.ContentType?.MediaType;

					if (contentType == "application/json")
					{
						var jsonString = await testResponse.Content.ReadAsStringAsync();
						Console.WriteLine($"     Получен JSON: {jsonString.Length} символов");

						// Десериализация с проверкой
						try
						{
							var post = JsonSerializer.Deserialize<JsonElement>(jsonString);
							Console.WriteLine($"     Title: {post.GetProperty("title")}");
						}
						catch (JsonException ex)
						{
							Console.WriteLine($"     Ошибка десериализации: {ex.Message}");
						}
					}
					else
					{
						Console.WriteLine($"     Неожиданный Content-Type: {contentType}");
					}
				}
				UpdateStatistics(testResponse.IsSuccessStatusCode);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"   Ошибка отправки данных: {ex.Message}");
				UpdateStatistics(false);
			}
		}

		private async Task DemonstrateErrorHandling()
		{
			Console.WriteLine("\n   1.5. ОБРАБОТКА ОШИБОК:");

			// 1. Обработка HTTP ошибок (статус-коды 4xx, 5xx)
			Console.WriteLine("\n   Обработка HTTP ошибок:");
			try
			{
				var errorResponse = await _customizedClient.GetAsync($"{TestApiUrl}/posts/99999");

				// Всегда проверяем статус-код перед работой с ответом
				if (!errorResponse.IsSuccessStatusCode)
				{
					Console.WriteLine($"     HTTP ошибка: {errorResponse.StatusCode}");

					// Можно прочитать тело ошибки
					var errorBody = await errorResponse.Content.ReadAsStringAsync();
					Console.WriteLine($"     Тело ошибки: {errorBody.Substring(0, Math.Min(100, errorBody.Length))}...");
				}
				UpdateStatistics(errorResponse.IsSuccessStatusCode);
			}
			catch (HttpRequestException ex)
			{
				Console.WriteLine($"     Сетевая ошибка: {ex.Message}");
				UpdateStatistics(false);
			}

			// 2. Таймауты
			Console.WriteLine("\n   Обработка таймаутов:");
			try
			{
				var timeoutClient = new HttpClient { Timeout = TimeSpan.FromMilliseconds(100) };
				var timeoutTask = timeoutClient.GetAsync("http://httpbin.org/delay/2"); // Задержка 2 секунды

				if (await Task.WhenAny(timeoutTask, Task.Delay(500)) != timeoutTask)
				{
					Console.WriteLine("     Запрос превысил таймаут");
				}
				else
				{
					var response = await timeoutTask;
					Console.WriteLine($"     Ответ: {response.StatusCode}");
				}
			}
			catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
			{
				Console.WriteLine($"     Таймаут: {ex.Message}");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"     Ошибка: {ex.Message}");
			}

			// 3. Недоступность сервера
			Console.WriteLine("\n   Недоступность сервера:");
			try
			{
				var unavailableResponse = await _customizedClient.GetAsync($"{LocalTestUrl}/nonexistent");
				Console.WriteLine($"     Статус: {unavailableResponse.StatusCode}");
				UpdateStatistics(unavailableResponse.IsSuccessStatusCode);
			}
			catch (HttpRequestException ex)
			{
				Console.WriteLine($"     Сервер недоступен: {ex.Message}");
				Console.WriteLine($"     Внутренняя ошибка: {ex.InnerException?.Message}");
				UpdateStatistics(false);
			}
		}

		private async Task DemonstrateAsyncAndCancellation()
		{
			Console.WriteLine("\n   1.6. АСИНХРОННОСТЬ И ОТМЕНА:");

			var cts = new CancellationTokenSource();

			// 1. Базовая асинхронность
			Console.WriteLine("\n   Асинхронное выполнение запросов:");
			var tasks = new List<Task>();

			for (int i = 1; i <= 3; i++)
			{
				tasks.Add(Task.Run(async () =>
				{
					try
					{
						var response = await _customizedClient.GetAsync($"{TestApiUrl}/posts/{i}", cts.Token);
						Console.WriteLine($"     Запрос {i}: {response.StatusCode}");
						UpdateStatistics(response.IsSuccessStatusCode);
					}
					catch (OperationCanceledException)
					{
						Console.WriteLine($"     Запрос {i}: Отменён");
					}
					catch (Exception ex)
					{
						Console.WriteLine($"     Запрос {i}: Ошибка - {ex.Message}");
					}
				}));
			}

			await Task.WhenAll(tasks);

			// 2. Отмена запросов
			Console.WriteLine("\n   Отмена запроса:");
			cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));

			try
			{
				var longTask = _customizedClient.GetAsync("http://httpbin.org/delay/3", cts.Token);
				await longTask;
				Console.WriteLine("     Запрос завершён (не должен был произойти)");
			}
			catch (OperationCanceledException)
			{
				Console.WriteLine("     Запрос успешно отменён");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"     Другая ошибка: {ex.Message}");
			}

			// 3. Параллельные запросы с ограничением
			Console.WriteLine("\n   Параллельные запросы с ограничением:");
			var semaphore = new SemaphoreSlim(2); // Максимум 2 параллельных запроса

			var parallelTasks = new List<Task>();
			for (int i = 0; i < 5; i++)
			{
				parallelTasks.Add(Task.Run(async () =>
				{
					await semaphore.WaitAsync();
					try
					{
						Console.WriteLine($"     Начало запроса...");
						var response = await _customizedClient.GetAsync($"{TestApiUrl}/posts/1");
						Console.WriteLine($"     Запрос завершён: {response.StatusCode}");
						UpdateStatistics(response.IsSuccessStatusCode);
					}
					finally
					{
						semaphore.Release();
					}
				}));
			}

			await Task.WhenAll(parallelTasks);
		}

		private async Task DemonstrateArchitecturalApproach()
		{
			Console.WriteLine("\n   1.7. АРХИТЕКТУРНЫЙ ПОДХОД:");

			// Пример сервисного слоя для работы с HTTP
			var userService = new UserService(_customizedClient);

			try
			{
				Console.WriteLine("\n   Использование сервисного слоя:");

				// Получение пользователя
				var user = await userService.GetUserAsync(1);
				if (user != null)
				{
					Console.WriteLine($"     Получен пользователь: {user.Name}");
				}

				// Создание пользователя
				var newUser = new UserDto { Name = "Alice", Email = "alice@example.com" };
				var createdUser = await userService.CreateUserAsync(newUser);
				if (createdUser != null)
				{
					Console.WriteLine($"     Создан пользователь с ID: {createdUser.Id}");
				}
			}
			catch (ApiException ex)
			{
				Console.WriteLine($"     Ошибка API: {ex.StatusCode} - {ex.Message}");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"     Общая ошибка: {ex.Message}");
			}
		}

		private void UpdateStatistics(bool isSuccess)
		{
			lock (_statsLock)
			{
				_totalRequests++;
				if (!isSuccess) _failedRequests++;
			}
		}

		private void PrintStatistics()
		{
			Console.WriteLine("\n" + new string('=', 50));
			Console.WriteLine("СТАТИСТИКА ИСПОЛЬЗОВАНИЯ HttpClient:");
			Console.WriteLine($"   Всего запросов: {_totalRequests}");
			Console.WriteLine($"   Успешных: {_totalRequests - _failedRequests}");
			Console.WriteLine($"   Неудачных: {_failedRequests}");
			Console.WriteLine($"   Процент успеха: {((_totalRequests - _failedRequests) * 100.0 / _totalRequests):F1}%");
			Console.WriteLine(new string('=', 50));
		}

		// Пример архитектурного сервиса
		public class UserService
		{
			private readonly HttpClient _httpClient;
			private readonly string _baseUrl = "https://jsonplaceholder.typicode.com";

			public UserService(HttpClient httpClient)
			{
				_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
			}

			public async Task<UserDto?> GetUserAsync(int userId)
			{
				try
				{
					var response = await _httpClient.GetAsync($"{_baseUrl}/users/{userId}");

					if (!response.IsSuccessStatusCode)
					{
						throw new ApiException($"Не удалось получить пользователя {userId}", response.StatusCode);
					}

					var json = await response.Content.ReadAsStringAsync();
					var user = JsonSerializer.Deserialize<UserDto>(json);
					return user;
				}
				catch (HttpRequestException ex)
				{
					throw new ApiException($"Сетевая ошибка: {ex.Message}", System.Net.HttpStatusCode.ServiceUnavailable);
				}
			}

			public async Task<UserDto?> CreateUserAsync(UserDto user)
			{
				try
				{
					var json = JsonSerializer.Serialize(user);
					var content = new StringContent(json, Encoding.UTF8, "application/json");

					var response = await _httpClient.PostAsync($"{_baseUrl}/users", content);

					if (!response.IsSuccessStatusCode)
					{
						throw new ApiException($"Не удалось создать пользователя", response.StatusCode);
					}

					var responseJson = await response.Content.ReadAsStringAsync();
					var createdUser = JsonSerializer.Deserialize<UserDto>(responseJson);
					return createdUser;
				}
				catch (HttpRequestException ex)
				{
					throw new ApiException($"Сетевая ошибка: {ex.Message}", System.Net.HttpStatusCode.ServiceUnavailable);
				}
			}
		}

		public class UserDto
		{
			public int Id { get; set; }
			public string Name { get; set; } = string.Empty;
			public string Email { get; set; } = string.Empty;
		}

		public class ApiException : Exception
		{
			public System.Net.HttpStatusCode StatusCode { get; }

			public ApiException(string message, System.Net.HttpStatusCode statusCode) : base(message)
			{
				StatusCode = statusCode;
			}
		}

		public void Dispose()
		{
			if (!_disposed)
			{
				Console.WriteLine("\nОсвобождение ресурсов HttpClient...");

				// HttpClient реализует IDisposable
				_httpClient?.Dispose();
				_customizedClient?.Dispose();

				_disposed = true;
				Console.WriteLine("Ресурсы освобождены. HttpClient больше не должен использоваться.");
			}
		}
	}

	// Главная программа
	class Program
	{
		static async Task Main(string[] args)
		{
			Console.WriteLine("HTTP-КЛИЕНТ В C#: ПРАКТИЧЕСКОЕ ИСПОЛЬЗОВАНИЕ");
			Console.WriteLine("=============================================\n");

			using (var demo = new HttpClientDemonstration())
			{
				await demo.DemonstrateHttpClientBasics();
			}
		}
	}
}
