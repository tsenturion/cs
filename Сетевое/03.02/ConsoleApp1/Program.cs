using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Diagnostics;

namespace HttpServerDemo
{
	// Базовая модель для демонстрации
	public class User
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }

		public User() { }

		public User(int id, string name, string email)
		{
			Id = id;
			Name = name;
			Email = email;
		}
	}

	public class Product
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public decimal Price { get; set; }
		public int Stock { get; set; }
	}

	// Простой HTTP-сервер на сырых сокетах для понимания основ
	public class RawHttpServer : IDisposable
	{
		private HttpListener _listener;
		private Thread _serverThread;
		private bool _isRunning;
		private readonly string _prefix;
		private readonly Dictionary<string, Func<HttpListenerContext, Task>> _routes = new();

		public RawHttpServer(string prefix = "http://localhost:8080/")
		{
			_prefix = prefix;
			_listener = new HttpListener();
			_listener.Prefixes.Add(prefix);
		}

		public void Start()
		{
			Console.WriteLine($"=== ПРОСТОЙ HTTP-СЕРВЕР НА СЫРЫХ СОКЕТАХ ===\n");
			Console.WriteLine($"Сервер запускается на: {_prefix}");
			Console.WriteLine($"Сервер слушает HTTP-запросы...\n");

			_isRunning = true;
			_serverThread = new Thread(RunServer);
			_serverThread.Start();
		}

		// Регистрация маршрутов
		public void MapGet(string path, Func<HttpListenerContext, Task> handler)
		{
			_routes[$"GET {path}"] = handler;
		}

		public void MapPost(string path, Func<HttpListenerContext, Task> handler)
		{
			_routes[$"POST {path}"] = handler;
		}

		public void MapPut(string path, Func<HttpListenerContext, Task> handler)
		{
			_routes[$"PUT {path}"] = handler;
		}

		public void MapDelete(string path, Func<HttpListenerContext, Task> handler)
		{
			_routes[$"DELETE {path}"] = handler;
		}

		private async void RunServer()
		{
			_listener.Start();
			Console.WriteLine($"Сервер запущен. Ожидание запросов...");

			while (_isRunning)
			{
				try
				{
					// Блокирующее ожидание HTTP-запроса
					var context = await _listener.GetContextAsync();

					// Асинхронная обработка запроса
					_ = Task.Run(() => ProcessRequest(context));
				}
				catch (HttpListenerException) when (!_isRunning)
				{
					break;
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Ошибка при принятии запроса: {ex.Message}");
				}
			}
		}

		private async Task ProcessRequest(HttpListenerContext context)
		{
			var request = context.Request;
			var response = context.Response;

			Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {request.HttpMethod} {request.Url?.AbsolutePath}");

			try
			{
				// Маршрутизация
				string routeKey = $"{request.HttpMethod} {request.Url?.AbsolutePath}";

				if (_routes.TryGetValue(routeKey, out var handler))
				{
					await handler(context);
				}
				else
				{
					// 404 - маршрут не найден
					await SendResponse(response, 404, "text/plain", "Not Found");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Ошибка обработки запроса: {ex.Message}");
				await SendResponse(response, 500, "text/plain", $"Internal Server Error: {ex.Message}");
			}
			finally
			{
				response.Close();
			}
		}

		public async Task SendResponse(HttpListenerResponse response, int statusCode, string contentType, string content)
		{
			response.StatusCode = statusCode;
			response.ContentType = contentType;
			response.ContentEncoding = Encoding.UTF8;

			byte[] buffer = Encoding.UTF8.GetBytes(content);
			response.ContentLength64 = buffer.Length;

			await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
			response.OutputStream.Close();
		}

		public async Task SendJsonResponse<T>(HttpListenerResponse response, int statusCode, T data)
		{
			response.StatusCode = statusCode;
			response.ContentType = "application/json";
			response.ContentEncoding = Encoding.UTF8;

			var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				WriteIndented = true
			});

			byte[] buffer = Encoding.UTF8.GetBytes(json);
			response.ContentLength64 = buffer.Length;

			await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
			response.OutputStream.Close();
		}

		public void Stop()
		{
			_isRunning = false;
			_listener.Stop();
			_serverThread?.Join(3000);
			Console.WriteLine($"\nСервер остановлен");
		}

		public void Dispose()
		{
			Stop();
			_listener.Close();
		}
	}

	// HTTP-сервер на ASP.NET Core для сравнения
	public class AspNetHttpServerDemo
	{
		public static async Task RunDemo()
		{
			Console.WriteLine("\n\n=== HTTP-СЕРВЕР НА ASP.NET CORE ===\n");
			Console.WriteLine("Это демонстрация того, как бы выглядел код на ASP.NET Core.");
			Console.WriteLine("В реальности это отдельное приложение с использованием WebApplication.");
			Console.WriteLine("Ниже показаны основные концепции и паттерны.\n");

			// Демонстрация конфигурации
			Console.WriteLine("1. КОНФИГУРАЦИЯ HTTP-СЕРВЕРА:");
			Console.WriteLine("   var builder = WebApplication.CreateBuilder(args);");
			Console.WriteLine("   var app = builder.Build();\n");

			// Демонстрация маршрутизации
			Console.WriteLine("2. МАРШРУТИЗАЦИЯ:");
			Console.WriteLine("   app.MapGet(\"/\", () => \"Hello, World!\");");
			Console.WriteLine("   app.MapGet(\"/api/users/{id}\", (int id) => GetUser(id));");
			Console.WriteLine("   app.MapPost(\"/api/users\", (User user) => CreateUser(user));");
			Console.WriteLine("   app.MapPut(\"/api/users/{id}\", (int id, User user) => UpdateUser(id, user));");
			Console.WriteLine("   app.MapDelete(\"/api/users/{id}\", (int id) => DeleteUser(id));\n");

			// Демонстрация конвейера middleware
			Console.WriteLine("3. КОНВЕЙЕР ОБРАБОТКИ (MIDDLEWARE):");
			Console.WriteLine("   app.UseHttpsRedirection();");
			Console.WriteLine("   app.UseAuthentication();");
			Console.WriteLine("   app.UseAuthorization();");
			Console.WriteLine("   app.UseCors();");
			Console.WriteLine("   app.UseRateLimiting();");
			Console.WriteLine("   app.MapControllers();");
			Console.WriteLine("   app.UseExceptionHandler(\"/error\");\n");

			// Демонстрация запуска
			Console.WriteLine("4. ЗАПУСК СЕРВЕРА:");
			Console.WriteLine("   app.Run();");
			Console.WriteLine("   // Сервер начинает слушать порт и обрабатывать запросы\n");

			await Task.Delay(1000);
		}
	}

	// Демонстрация HTTP-сервера
	public class HttpServerDemonstration : IDisposable
	{
		private RawHttpServer _server;

		public async Task RunDemonstration()
		{
			Console.WriteLine("ПРАКТИЧЕСКАЯ ДЕМОНСТРАЦИЯ HTTP-СЕРВЕРА В C#");
			Console.WriteLine("=============================================\n");

			// Создаём сервер
			_server = new RawHttpServer();

			// Настраиваем маршруты
			ConfigureRoutes();

			// Запускаем сервер
			_server.Start();

			// Демонстрация запросов
			await DemonstrateHttpRequests();

			// Демонстрация ASP.NET Core
			await AspNetHttpServerDemo.RunDemo();

			Console.WriteLine("\nДемонстрация завершена.");
		}

		private void ConfigureRoutes()
		{
			// GET / - корневой маршрут
			_server.MapGet("/", async (context) =>
			{
				var response = @"
                    <html>
                        <head><title>HTTP Server Demo</title></head>
                        <body>
                            <h1>HTTP Server Demo</h1>
                            <p>Доступные эндпоинты:</p>
                            <ul>
                                <li>GET /hello - Простое приветствие</li>
                                <li>GET /api/users - Список пользователей</li>
                                <li>GET /api/users/{id} - Конкретный пользователь</li>
                                <li>POST /api/users - Создание пользователя</li>
                                <li>GET /status - Статус сервера</li>
                            </ul>
                        </body>
                    </html>";

				await _server.SendResponse(context.Response, 200, "text/html", response);
			});

			// GET /hello - простое приветствие
			_server.MapGet("/hello", async (context) =>
			{
				await _server.SendResponse(context.Response, 200, "text/plain", "Hello, World!");
			});

			// GET /api/users - список пользователей
			_server.MapGet("/api/users", async (context) =>
			{
				var users = new List<User>
				{
					new User(1, "Иван Иванов", "ivan@example.com"),
					new User(2, "Мария Петрова", "maria@example.com"),
					new User(3, "Алексей Сидоров", "alexey@example.com")
				};

				await _server.SendJsonResponse(context.Response, 200, users);
			});

			// GET /api/users/{id} - конкретный пользователь
			_server.MapGet("/api/users/{id}", async (context) =>
			{
				var idStr = context.Request.Url.Segments[^1];
				if (int.TryParse(idStr, out int id))
				{
					var user = id switch
					{
						1 => new User(1, "Иван Иванов", "ivan@example.com"),
						2 => new User(2, "Мария Петрова", "maria@example.com"),
						3 => new User(3, "Алексей Сидоров", "alexey@example.com"),
						_ => null
					};

					if (user != null)
					{
						await _server.SendJsonResponse(context.Response, 200, user);
					}
					else
					{
						await _server.SendResponse(context.Response, 404, "text/plain", "User not found");
					}
				}
				else
				{
					await _server.SendResponse(context.Response, 400, "text/plain", "Invalid user ID");
				}
			});

			// POST /api/users - создание пользователя
			_server.MapPost("/api/users", async (context) =>
			{
				try
				{
					// Чтение тела запроса
					using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
					string body = await reader.ReadToEndAsync();

					var user = JsonSerializer.Deserialize<User>(body);
					if (user == null || string.IsNullOrEmpty(user.Name) || string.IsNullOrEmpty(user.Email))
					{
						await _server.SendResponse(context.Response, 400, "text/plain", "Invalid user data");
						return;
					}

					// Имитация сохранения
					user.Id = new Random().Next(100, 1000);

					// 201 Created с Location header
					context.Response.Headers.Add("Location", $"/api/users/{user.Id}");
					await _server.SendJsonResponse(context.Response, 201, user);
				}
				catch (JsonException)
				{
					await _server.SendResponse(context.Response, 400, "text/plain", "Invalid JSON");
				}
			});

			// GET /status - статус сервера
			_server.MapGet("/status", async (context) =>
			{
				var status = new
				{
					Server = "RawHttpServer",
					Status = "Running",
					Timestamp = DateTime.UtcNow,
					ProcessId = Process.GetCurrentProcess().Id,
					ThreadCount = Process.GetCurrentProcess().Threads.Count
				};

				await _server.SendJsonResponse(context.Response, 200, status);
			});

			// GET /error - демонстрация ошибки
			_server.MapGet("/error", async (context) =>
			{
				throw new InvalidOperationException("Демонстрационная ошибка обработки запроса");
			});

			// Middleware для логирования всех запросов
			_server.MapGet("*", async (context) =>
			{
				// Этот маршрут будет перехватывать все GET-запросы к несуществующим маршрутам
				Console.WriteLine($"[404] Запрошен несуществующий маршрут: {context.Request.Url}");
				await _server.SendResponse(context.Response, 404, "text/plain", "Route not found");
			});
		}

		private async Task DemonstrateHttpRequests()
		{
			Console.WriteLine("ДЕМОНСТРАЦИЯ HTTP-ЗАПРОСОВ К СЕРВЕРУ:\n");

			// Даём серверу время запуститься
			await Task.Delay(1000);

			// Демонстрационные запросы
			var baseUrl = "http://localhost:8080";

			Console.WriteLine("1. GET /hello - Простое приветствие");
			await MakeRequest($"{baseUrl}/hello", "GET");

			Console.WriteLine("\n2. GET /api/users - Список пользователей");
			await MakeRequest($"{baseUrl}/api/users", "GET");

			Console.WriteLine("\n3. GET /api/users/1 - Конкретный пользователь");
			await MakeRequest($"{baseUrl}/api/users/1", "GET");

			Console.WriteLine("\n4. GET /api/users/999 - Несуществующий пользователь");
			await MakeRequest($"{baseUrl}/api/users/999", "GET");

			Console.WriteLine("\n5. POST /api/users - Создание пользователя");
			var newUser = new { Name = "Новый Пользователь", Email = "new@example.com" };
			await MakeRequest($"{baseUrl}/api/users", "POST", JsonSerializer.Serialize(newUser));

			Console.WriteLine("\n6. GET /status - Статус сервера");
			await MakeRequest($"{baseUrl}/status", "GET");

			Console.WriteLine("\n7. GET /notfound - Несуществующий маршрут");
			await MakeRequest($"{baseUrl}/notfound", "GET");

			await Task.Delay(500);
		}

		private async Task MakeRequest(string url, string method, string body = null)
		{
			try
			{
				var request = WebRequest.Create(url);
				request.Method = method;

				if (!string.IsNullOrEmpty(body) && method == "POST")
				{
					request.ContentType = "application/json";
					var data = Encoding.UTF8.GetBytes(body);
					request.ContentLength = data.Length;

					using (var requestStream = await request.GetRequestStreamAsync())
					{
						await requestStream.WriteAsync(data, 0, data.Length);
					}
				}

				using var response = (HttpWebResponse)await request.GetResponseAsync();
				using var responseStream = response.GetResponseStream();
				using var reader = new StreamReader(responseStream);
				string result = await reader.ReadToEndAsync();

				Console.WriteLine($"   → {method} {url}");
				Console.WriteLine($"   ← Status: {response.StatusCode} ({response.StatusDescription})");
				Console.WriteLine($"   ← Response: {result.Truncate(100)}...");
			}
			catch (WebException ex) when (ex.Response is HttpWebResponse response)
			{
				using var errorStream = response.GetResponseStream();
				using var reader = new StreamReader(errorStream);
				string result = await reader.ReadToEndAsync();

				Console.WriteLine($"   → {method} {url}");
				Console.WriteLine($"   ← Status: {response.StatusCode} ({response.StatusDescription})");
				Console.WriteLine($"   ← Response: {result.Truncate(100)}...");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"   → {method} {url}");
				Console.WriteLine($"   ← Error: {ex.Message}");
			}
		}

		public void Dispose()
		{
			_server?.Dispose();
		}
	}

	public static class StringExtensions
	{
		public static string Truncate(this string value, int maxLength)
		{
			if (string.IsNullOrEmpty(value)) return value;
			return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "...";
		}
	}

	// Главная программа
	class Program
	{
		static async Task Main(string[] args)
		{
			Console.WriteLine("HTTP-СЕРВЕР В C#");
			Console.WriteLine("===============\n");

			using (var demo = new HttpServerDemonstration())
			{
				await demo.RunDemonstration();
			}
		}
	}
}
