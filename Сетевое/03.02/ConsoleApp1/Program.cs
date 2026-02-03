using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;

namespace HttpSerializationDemo
{
	// Базовые модели данных для демонстрации
	public class User
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }

		// Метод для демонстрации бизнес-логики, которая не сериализуется
		public bool IsValid()
		{
			return !string.IsNullOrEmpty(Name) &&
				   !string.IsNullOrEmpty(Email) &&
				   Email.Contains('@');
		}

		public override string ToString()
		{
			return $"User[Id={Id}, Name='{Name}', Email='{Email}']";
		}
	}

	// DTO (Data Transfer Object) для внешнего API
	public class UserDto
	{
		public int id { get; set; }
		public string full_name { get; set; }
		public string email_address { get; set; }
		public DateTime created_at { get; set; }
		public bool is_active { get; set; }

		public static UserDto FromUser(User user)
		{
			return new UserDto
			{
				id = user.Id,
				full_name = user.Name,
				email_address = user.Email,
				created_at = DateTime.UtcNow,
				is_active = true
			};
		}

		public User ToUser()
		{
			return new User
			{
				Id = id,
				Name = full_name,
				Email = email_address
			};
		}
	}

	// Сложная модель с вложенными объектами
	public class Order
	{
		public string OrderId { get; set; }
		public User Customer { get; set; }
		public List<OrderItem> Items { get; set; } = new List<OrderItem>();
		public decimal TotalAmount { get; set; }
		public OrderStatus Status { get; set; }
		public DateTime? CompletedAt { get; set; }

		public override string ToString()
		{
			return $"Order[Id='{OrderId}', Customer={Customer?.Name}, Items={Items.Count}, Total={TotalAmount:C}]";
		}
	}

	public class OrderItem
	{
		public string ProductId { get; set; }
		public string ProductName { get; set; }
		public int Quantity { get; set; }
		public decimal Price { get; set; }

		public decimal Subtotal => Quantity * Price;
	}

	public enum OrderStatus
	{
		Pending,
		Processing,
		Shipped,
		Delivered,
		Cancelled
	}

	// Модель с nullable значениями для демонстрации
	public class Product
	{
		public int? Id { get; set; }
		public string Name { get; set; }
		public string? Description { get; set; } // Nullable reference type
		public decimal? Price { get; set; }
		public int? StockQuantity { get; set; }
		public List<string>? Tags { get; set; }

		public override string ToString()
		{
			return $"Product[Id={Id?.ToString() ?? "null"}, Name='{Name}', Price={Price?.ToString("C") ?? "null"}]";
		}
	}

	// Демонстрация сериализации и десериализации
	public class SerializationDemonstration
	{
		public static void RunBasicDemo()
		{
			Console.WriteLine("=== СЕРИАЛИЗАЦИЯ И ДЕСЕРИАЛИЗАЦИЯ В HTTP-КЛИЕНТЕ ===\n");

			Console.WriteLine("1. БАЗОВАЯ СЕРИАЛИЗАЦИЯ ОБЪЕКТА В JSON:");
			DemonstrateBasicSerialization();

			Console.WriteLine("\n2. ДЕСЕРИАЛИЗАЦИЯ JSON В ОБЪЕКТ:");
			DemonstrateBasicDeserialization();

			Console.WriteLine("\n3. НАСТРОЙКИ СЕРИАЛИЗАЦИИ:");
			DemonstrateSerializationOptions();

			Console.WriteLine("\n4. NULLABLE ЗНАЧЕНИЯ И ОТСУТСТВУЮЩИЕ ПОЛЯ:");
			DemonstrateNullableValues();

			Console.WriteLine("\n5. СЛОЖНЫЕ ОБЪЕКТЫ И ВЛОЖЕННОСТЬ:");
			DemonstrateComplexObjects();

			Console.WriteLine("\n6. DTO И ИЗОЛЯЦИЯ КОНТРАКТОВ:");
			DemonstrateDtoIsolation();

			Console.WriteLine("\n7. ИНТЕГРАЦИЯ С HTTPCLIENT:");
			DemonstrateHttpClientIntegration();
		}

		private static void DemonstrateBasicSerialization()
		{
			// Создаём объект в памяти приложения
			var user = new User
			{
				Id = 1,
				Name = "Иван Петров",
				Email = "ivan@example.com"
			};

			Console.WriteLine($"   Исходный объект в C#: {user}");
			Console.WriteLine($"   Проверка бизнес-логики: IsValid = {user.IsValid()}");

			// Сериализация объекта в JSON
			string json = JsonSerializer.Serialize(user);

			Console.WriteLine($"\n   Результат сериализации:");
			Console.WriteLine($"     {json}");

			Console.WriteLine($"\n   Анализ JSON:");
			Console.WriteLine($"     Тип данных: {json.GetType().Name}");
			Console.WriteLine($"     Длина: {json.Length} символов");
			Console.WriteLine($"     Кодировка: UTF-8 (внутреннее представление)");

			// Демонстрация байтового представления
			byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
			Console.WriteLine($"     Байтовое представление: {jsonBytes.Length} байт");

			// Сериализация с форматированием для читаемости
			string formattedJson = JsonSerializer.Serialize(user, new JsonSerializerOptions
			{
				WriteIndented = true
			});

			Console.WriteLine($"\n   Форматированный JSON:");
			Console.WriteLine(formattedJson);
		}

		private static void DemonstrateBasicDeserialization()
		{
			// Исходный JSON (например, полученный от HTTP-сервера)
			string json = @"
            {
                ""id"": 42,
                ""name"": ""Анна Сидорова"",
                ""email"": ""anna@example.com""
            }";

			Console.WriteLine($"   Полученный JSON от HTTP-сервера:");
			Console.WriteLine($"     {json}");

			// Десериализация JSON в объект C#
			User user = JsonSerializer.Deserialize<User>(json);

			Console.WriteLine($"\n   Результат десериализации:");
			Console.WriteLine($"     Объект в памяти: {user}");
			Console.WriteLine($"     Тип: {user.GetType().FullName}");
			Console.WriteLine($"     Проверка бизнес-логики: IsValid = {user.IsValid()}");

			Console.WriteLine($"\n   Анализ преобразования:");
			Console.WriteLine($"     JSON 'id': 42 → C# user.Id: {user.Id} (тип: {user.Id.GetType().Name})");
			Console.WriteLine($"     JSON 'name': 'Анна Сидорова' → C# user.Name: '{user.Name}'");
			Console.WriteLine($"     JSON 'email': 'anna@example.com' → C# user.Email: '{user.Email}'");

			// Демонстрация ошибки десериализации
			Console.WriteLine($"\n   Тест ошибки десериализации:");
			string invalidJson = @"{ ""id"": ""not_a_number"", ""name"": ""Test"" }";

			try
			{
				User invalidUser = JsonSerializer.Deserialize<User>(invalidJson);
				Console.WriteLine($"     Ошибка: десериализация прошла некорректно");
			}
			catch (JsonException ex)
			{
				Console.WriteLine($"     Ожидаемая ошибка: {ex.Message}");
				Console.WriteLine($"     JSON не может быть преобразован в объект из-за несоответствия типов");
			}
		}

		private static void DemonstrateSerializationOptions()
		{
			var user = new User
			{
				Id = 100,
				Name = "Мария Иванова",
				Email = "maria@company.com"
			};

			Console.WriteLine($"   Объект для сериализации: {user}");

			// 1. Сериализация по умолчанию
			string defaultJson = JsonSerializer.Serialize(user);
			Console.WriteLine($"\n   Сериализация по умолчанию:");
			Console.WriteLine($"     {defaultJson}");
			Console.WriteLine($"     Свойства сохраняют оригинальные имена (PascalCase)");

			// 2. Сериализация с camelCase
			var camelCaseOptions = new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				WriteIndented = true
			};

			string camelCaseJson = JsonSerializer.Serialize(user, camelCaseOptions);
			Console.WriteLine($"\n   Сериализация с camelCase:");
			Console.WriteLine($"     {camelCaseJson}");
			Console.WriteLine($"     Id → id, Name → name, Email → email");

			// 3. Сериализация с игнорированием null значений
			var userWithNulls = new User
			{
				Id = 101,
				Name = null, // Будет проигнорировано
				Email = "test@example.com"
			};

			var ignoreNullOptions = new JsonSerializerOptions
			{
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			};

			string ignoreNullJson = JsonSerializer.Serialize(userWithNulls, ignoreNullOptions);
			Console.WriteLine($"\n   Сериализация с игнорированием null:");
			Console.WriteLine($"     {ignoreNullJson}");
			Console.WriteLine($"     Свойство Name=null отсутствует в JSON");

			// 4. Десериализация с учетом стиля именования
			Console.WriteLine($"\n   Десериализация camelCase JSON:");

			string camelCaseInput = @"{ ""id"": 200, ""name"": ""Тест"", ""email"": ""test@test.com"" }";
			User camelCaseUser = JsonSerializer.Deserialize<User>(camelCaseInput, camelCaseOptions);

			Console.WriteLine($"     Входной JSON: {camelCaseInput}");
			Console.WriteLine($"     Десериализованный объект: {camelCaseUser}");
			Console.WriteLine($"     Важно: при десериализации нужно использовать те же настройки!");

			// 5. Сериализация с конвертером для enum
			var order = new Order
			{
				OrderId = "ORD-001",
				Status = OrderStatus.Processing
			};

			var enumOptions = new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				WriteIndented = true,
				Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
			};

			string enumJson = JsonSerializer.Serialize(order, enumOptions);
			Console.WriteLine($"\n   Сериализация enum как строки:");
			Console.WriteLine($"     {enumJson}");
			Console.WriteLine($"     OrderStatus.Processing → 'processing'");
		}

		private static void DemonstrateNullableValues()
		{
			Console.WriteLine($"   Демонстрация работы с nullable значениями:");

			// JSON с отсутствующими полями
			string partialJson = @"
            {
                ""name"": ""Товар без цены"",
                ""stockQuantity"": 10
            }";

			Console.WriteLine($"\n   Входной JSON (неполный):");
			Console.WriteLine($"     {partialJson}");
			Console.WriteLine($"     Отсутствуют поля: id, description, price, tags");

			// Десериализация
			Product product = JsonSerializer.Deserialize<Product>(partialJson);

			Console.WriteLine($"\n   Результат десериализации:");
			Console.WriteLine($"     {product}");
			Console.WriteLine($"     Id: {product.Id?.ToString() ?? "null"} (не было в JSON → null)");
			Console.WriteLine($"     Name: '{product.Name}' (было в JSON → значение)");
			Console.WriteLine($"     Description: '{product.Description ?? "null"}' (не было в JSON → null)");
			Console.WriteLine($"     Price: {product.Price?.ToString("C") ?? "null"} (не было в JSON → null)");
			Console.WriteLine($"     StockQuantity: {product.StockQuantity} (было в JSON → значение)");
			Console.WriteLine($"     Tags: {(product.Tags == null ? "null" : $"List[{product.Tags.Count}]")}");

			// Обратная сериализация
			Console.WriteLine($"\n   Обратная сериализация (с null значениями):");
			string serializedBack = JsonSerializer.Serialize(product, new JsonSerializerOptions { WriteIndented = true });
			Console.WriteLine(serializedBack);

			Console.WriteLine($"\n   Обратная сериализация (без null значений):");
			var ignoreNullOptions = new JsonSerializerOptions
			{
				WriteIndented = true,
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
			};

			string serializedWithoutNulls = JsonSerializer.Serialize(product, ignoreNullOptions);
			Console.WriteLine(serializedWithoutNulls);
		}

		private static void DemonstrateComplexObjects()
		{
			Console.WriteLine($"   Демонстрация сложных объектов с вложенностью:");

			// Создаём сложный объект
			var order = new Order
			{
				OrderId = "ORDER-2024-001",
				Customer = new User
				{
					Id = 1,
					Name = "Петр Сергеев",
					Email = "peter@example.com"
				},
				Items = new List<OrderItem>
				{
					new OrderItem
					{
						ProductId = "P-001",
						ProductName = "Ноутбук",
						Quantity = 1,
						Price = 1500.00m
					},
					new OrderItem
					{
						ProductId = "P-002",
						ProductName = "Мышь",
						Quantity = 2,
						Price = 25.50m
					}
				},
				TotalAmount = 1551.00m,
				Status = OrderStatus.Processing,
				CompletedAt = null
			};

			Console.WriteLine($"\n   Сложный объект Order:");
			Console.WriteLine($"     {order}");
			Console.WriteLine($"     Вложенный Customer: {order.Customer}");
			Console.WriteLine($"     Количество Items: {order.Items.Count}");

			// Сериализация
			var options = new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				WriteIndented = true,
				Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
			};

			string orderJson = JsonSerializer.Serialize(order, options);

			Console.WriteLine($"\n   Сериализованный JSON:");
			Console.WriteLine(orderJson);

			Console.WriteLine($"\n   Анализ структуры JSON:");
			Console.WriteLine($"     Корневой объект: Order");
			Console.WriteLine($"     Вложенный объект: customer (тип User)");
			Console.WriteLine($"     Массив: items (список OrderItem)");
			Console.WriteLine($"     Enum: status (преобразовано в строку)");
			Console.WriteLine($"     Null значение: completedAt (отсутствует в JSON)");

			// Десериализация обратно
			Console.WriteLine($"\n   Десериализация обратно в объект:");
			Order deserializedOrder = JsonSerializer.Deserialize<Order>(orderJson, options);

			Console.WriteLine($"     OrderId совпадает: {order.OrderId == deserializedOrder.OrderId}");
			Console.WriteLine($"     Customer совпадает: {order.Customer.Name == deserializedOrder.Customer.Name}");
			Console.WriteLine($"     Items count совпадает: {order.Items.Count == deserializedOrder.Items.Count}");
			Console.WriteLine($"     TotalAmount совпадает: {order.TotalAmount == deserializedOrder.TotalAmount}");
		}

		private static void DemonstrateDtoIsolation()
		{
			Console.WriteLine($"   Демонстрация DTO и изоляции контрактов:");

			// Внутренняя бизнес-модель
			var internalUser = new User
			{
				Id = 500,
				Name = "Внутренний Пользователь",
				Email = "internal@company.com"
			};

			Console.WriteLine($"\n   Внутренняя бизнес-модель:");
			Console.WriteLine($"     {internalUser}");
			Console.WriteLine($"     Свойства: Id, Name, Email");
			Console.WriteLine($"     Методы: IsValid(), ToString()");

			// Преобразование в DTO для внешнего API
			UserDto dto = UserDto.FromUser(internalUser);

			Console.WriteLine($"\n   DTO для внешнего API:");
			Console.WriteLine($"     Свойства: id, full_name, email_address, created_at, is_active");
			Console.WriteLine($"     Конвертеры: FromUser(), ToUser()");

			// Сериализация DTO
			string dtoJson = JsonSerializer.Serialize(dto, new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				WriteIndented = true
			});

			Console.WriteLine($"\n   Сериализованный DTO (для отправки по HTTP):");
			Console.WriteLine(dtoJson);

			// Симуляция получения DTO от внешнего API
			string externalApiResponse = @"
            {
                ""id"": 600,
                ""full_name"": ""Внешний Пользователь"",
                ""email_address"": ""external@api.com"",
                ""created_at"": ""2024-01-15T10:30:00Z"",
                ""is_active"": true
            }";

			Console.WriteLine($"\n   Получен ответ от внешнего API:");
			Console.WriteLine(externalApiResponse);

			// Десериализация DTO
			UserDto receivedDto = JsonSerializer.Deserialize<UserDto>(externalApiResponse);

			// Преобразование DTO во внутреннюю модель
			User externalUser = receivedDto.ToUser();

			Console.WriteLine($"\n   Преобразование во внутреннюю модель:");
			Console.WriteLine($"     {externalUser}");

			Console.WriteLine($"\n   Преимущества DTO подхода:");
			Console.WriteLine($"     1. Изоляция: изменение API не затрагивает бизнес-логику");
			Console.WriteLine($"     2. Адаптация: разные форматы для разных API");
			Console.WriteLine($"     3. Безопасность: скрытие внутренней структуры");
			Console.WriteLine($"     4. Стабильность: внутренняя модель может эволюционировать независимо");
		}

		private static void DemonstrateHttpClientIntegration()
		{
			Console.WriteLine($"   Интеграция сериализации с HttpClient:");

			// Симуляция HTTP-запроса
			Console.WriteLine($"\n   1. ПОДГОТОВКА ДАННЫХ ДЛЯ ОТПРАВКИ:");

			var newUser = new User
			{
				Name = "Новый Пользователь",
				Email = "new@example.com"
			};

			Console.WriteLine($"     Создан объект: {newUser}");

			// Сериализация
			string requestJson = JsonSerializer.Serialize(newUser, new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			});

			Console.WriteLine($"     Сериализован в JSON: {requestJson}");

			// Создание HTTP-контента
			var content = new StringContent(
				requestJson,
				Encoding.UTF8,
				"application/json"
			);

			Console.WriteLine($"\n     HTTP Content создан:");
			Console.WriteLine($"       Тип: StringContent");
			Console.WriteLine($"       Кодировка: UTF-8");
			Console.WriteLine($"       Content-Type: application/json");
			Console.WriteLine($"       Длина: {requestJson.Length} символов");

			Console.WriteLine($"\n   2. ОТПРАВКА HTTP-ЗАПРОСА (симуляция):");
			Console.WriteLine($"     Метод: POST");
			Console.WriteLine($"     URL: https://api.example.com/users");
			Console.WriteLine($"     Тело запроса: application/json");
			Console.WriteLine($"     Данные переданы в сеть в виде байтов");

			Console.WriteLine($"\n   3. ОБРАБОТКА HTTP-ОТВЕТА (симуляция):");

			// Симуляция ответа от сервера
			string simulatedResponse = @"
            {
                ""id"": 999,
                ""name"": ""Новый Пользователь"",
                ""email"": ""new@example.com"",
                ""createdAt"": ""2024-01-15T12:00:00Z""
            }";

			Console.WriteLine($"     Получен ответ от сервера:");
			Console.WriteLine($"       Status Code: 201 Created");
			Console.WriteLine($"       Content-Type: application/json");
			Console.WriteLine($"       Длина тела: {simulatedResponse.Length} символов");
			Console.WriteLine($"       Тело ответа: {simulatedResponse}");

			// Чтение тела ответа
			Console.WriteLine($"\n     Чтение тела ответа как строки...");
			string responseJson = simulatedResponse; // В реальности: await response.Content.ReadAsStringAsync();

			// Десериализация
			Console.WriteLine($"\n   4. ДЕСЕРИАЛИЗАЦИЯ ОТВЕТА:");

			var responseOptions = new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			};

			User createdUser = JsonSerializer.Deserialize<User>(responseJson, responseOptions);

			Console.WriteLine($"     Десериализованный объект: {createdUser}");
			Console.WriteLine($"     Проверка бизнес-логики: IsValid = {createdUser.IsValid()}");

			Console.WriteLine($"\n   5. ОБРАБОТКА ОШИБОК СЕРИАЛИЗАЦИИ:");

			// Симуляция ошибки
			string errorResponse = @"Невалидный JSON";

			try
			{
				User errorUser = JsonSerializer.Deserialize<User>(errorResponse);
				Console.WriteLine($"     Ошибка: десериализация прошла некорректно");
			}
			catch (JsonException ex)
			{
				Console.WriteLine($"     Обработана ошибка десериализации:");
				Console.WriteLine($"       Тип: {ex.GetType().Name}");
				Console.WriteLine($"       Сообщение: {ex.Message}");
				Console.WriteLine($"       В реальном приложении: логирование и fallback");
			}

			Console.WriteLine($"\n   РЕКОМЕНДАЦИИ ПО ИНТЕГРАЦИИ:");
			Console.WriteLine($"     1. Всегда указывайте Content-Type: application/json");
			Console.WriteLine($"     2. Используйте одинаковые настройки сериализации на клиенте и сервере");
			Console.WriteLine($"     3. Обрабатывайте ошибки десериализации");
			Console.WriteLine($"     4. Логируйте отправляемые и получаемые данные для отладки");
			Console.WriteLine($"     5. Используйте DTO для изоляции контрактов API");
		}
	}

	// Демонстрационный HTTP-клиент с сериализацией
	public class JsonHttpClientDemo : IDisposable
	{
		private readonly HttpClient _httpClient;
		private readonly JsonSerializerOptions _jsonOptions;
		private bool _disposed;

		public JsonHttpClientDemo()
		{
			_httpClient = new HttpClient();
			_httpClient.BaseAddress = new Uri("https://jsonplaceholder.typicode.com/");

			_jsonOptions = new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				PropertyNameCaseInsensitive = true,
				WriteIndented = false
			};
		}

		public async Task DemonstrateRealHttpCalls()
		{
			Console.WriteLine("\n=== РЕАЛЬНЫЕ HTTP-ВЫЗОВЫ С СЕРИАЛИЗАЦИЕЙ ===\n");

			try
			{
				Console.WriteLine("1. GET-запрос с десериализацией:");
				await DemonstrateGetRequest();

				Console.WriteLine("\n2. POST-запрос с сериализацией:");
				await DemonstratePostRequest();

				Console.WriteLine("\n3. PUT-запрос с полным циклом:");
				await DemonstratePutRequest();
			}
			catch (HttpRequestException ex)
			{
				Console.WriteLine($"   Ошибка HTTP: {ex.Message}");
			}
			catch (JsonException ex)
			{
				Console.WriteLine($"   Ошибка сериализации: {ex.Message}");
			}
		}

		private async Task DemonstrateGetRequest()
		{
			Console.WriteLine($"   Отправка GET /posts/1");

			// Отправка запроса
			HttpResponseMessage response = await _httpClient.GetAsync("posts/1");
			response.EnsureSuccessStatusCode();

			Console.WriteLine($"   Ответ получен: {response.StatusCode}");

			// Чтение и десериализация
			string json = await response.Content.ReadAsStringAsync();
			Console.WriteLine($"   Тело ответа (сырой JSON): {json.Substring(0, Math.Min(100, json.Length))}...");

			// Десериализация в динамический объект для демонстрации
			var post = JsonSerializer.Deserialize<Dictionary<string, object>>(json, _jsonOptions);

			Console.WriteLine($"\n   Десериализованные данные:");
			Console.WriteLine($"     ID: {post["id"]}");
			Console.WriteLine($"     Title: {post["title"]}");
			Console.WriteLine($"     Body: {post["body"]?.ToString()?.Substring(0, 50)}...");
		}

		private async Task DemonstratePostRequest()
		{
			Console.WriteLine($"   Подготовка данных для POST /posts");

			// Создание объекта для отправки
			var newPost = new
			{
				title = "Новый пост",
				body = "Содержание нового поста",
				userId = 1
			};

			Console.WriteLine($"   Объект C#: {{ title: '{newPost.title}', userId: {newPost.userId} }}");

			// Сериализация
			string json = JsonSerializer.Serialize(newPost, _jsonOptions);
			Console.WriteLine($"   Сериализованный JSON: {json}");

			// Создание HTTP-контента
			var content = new StringContent(json, Encoding.UTF8, "application/json");

			// Отправка запроса
			Console.WriteLine($"\n   Отправка POST /posts");
			HttpResponseMessage response = await _httpClient.PostAsync("posts", content);
			response.EnsureSuccessStatusCode();

			Console.WriteLine($"   Ответ: {response.StatusCode}");

			// Обработка ответа
			string responseJson = await response.Content.ReadAsStringAsync();
			var createdPost = JsonSerializer.Deserialize<Dictionary<string, object>>(responseJson, _jsonOptions);

			Console.WriteLine($"\n   Созданный пост (с сервера):");
			Console.WriteLine($"     ID: {createdPost["id"]}");
			Console.WriteLine($"     Title: {createdPost["title"]}");
		}

		private async Task DemonstratePutRequest()
		{
			Console.WriteLine($"   Полный цикл: GET → модификация → PUT");

			// 1. Получение существующих данных
			HttpResponseMessage getResponse = await _httpClient.GetAsync("posts/1");
			string originalJson = await getResponse.Content.ReadAsStringAsync();
			var originalPost = JsonSerializer.Deserialize<Dictionary<string, object>>(originalJson, _jsonOptions);

			Console.WriteLine($"\n   Исходный пост:");
			Console.WriteLine($"     Title: {originalPost["title"]}");

			// 2. Модификация данных
			originalPost["title"] = "Обновленный заголовок";
			originalPost["body"] = "Обновленное содержание";

			// 3. Сериализация модифицированных данных
			string updatedJson = JsonSerializer.Serialize(originalPost, _jsonOptions);

			// 4. Отправка PUT-запроса
			var content = new StringContent(updatedJson, Encoding.UTF8, "application/json");
			HttpResponseMessage putResponse = await _httpClient.PutAsync("posts/1", content);

			Console.WriteLine($"\n   PUT-запрос отправлен: {putResponse.StatusCode}");

			// 5. Проверка результата
			string finalJson = await putResponse.Content.ReadAsStringAsync();
			var finalPost = JsonSerializer.Deserialize<Dictionary<string, object>>(finalJson, _jsonOptions);

			Console.WriteLine($"\n   Обновленный пост:");
			Console.WriteLine($"     Title: {finalPost["title"]}");
			Console.WriteLine($"     Проверка: заголовок обновлен = {finalPost["title"] == "Обновленный заголовок"}");
		}

		public void Dispose()
		{
			if (_disposed)
			{
				return;
			}

			_httpClient.Dispose();
			_disposed = true;
			GC.SuppressFinalize(this);
		}
	}

	// Главная программа
	class Program
	{
		static async Task Main(string[] args)
		{
			Console.WriteLine("СЕРИАЛИЗАЦИЯ И ДЕСЕРИАЛИЗАЦИЯ В HTTP-КЛИЕНТЕ");
			Console.WriteLine("=============================================\n");

			// Базовая демонстрация
			SerializationDemonstration.RunBasicDemo();

			// Демонстрация реальных HTTP-вызовов
			using (var httpDemo = new JsonHttpClientDemo())
			{
				await httpDemo.DemonstrateRealHttpCalls();
			}
		}
	}
}
