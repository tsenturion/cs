using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace HttpAuthCookiesDemo
{
	// Модели данных для демонстрации
	public class UserCredentials
	{
		public string Username { get; set; }
		public string Password { get; set; }
	}

	public class UserProfile
	{
		public string Username { get; set; }
		public string Email { get; set; }
		public string[] Roles { get; set; }
		public DateTime CreatedAt { get; set; }
	}

	public class ApiResponse<T>
	{
		public bool Success { get; set; }
		public T Data { get; set; }
		public string Error { get; set; }
	}

	public class AuthToken
	{
		public string AccessToken { get; set; }
		public string RefreshToken { get; set; }
		public DateTime ExpiresAt { get; set; }
		public string TokenType { get; set; } = "Bearer";
	}

	// Демонстрация работы с cookies в C#
	public class CookieDemonstration
	{
		public static void DemonstrateCookieMechanism()
		{
			Console.WriteLine("=== COOKIES В HTTP ===\n");

			// 1. Создание HTTP клиента с поддержкой cookies
			Console.WriteLine("1. СОЗДАНИЕ HTTP КЛИЕНТА С COOKIES:");

			// CookieContainer - контейнер для хранения cookies на стороне клиента
			var cookieContainer = new CookieContainer();

			// HttpClientHandler с настроенным контейнером cookies
			var handler = new HttpClientHandler
			{
				CookieContainer = cookieContainer,
				UseCookies = true,
				AllowAutoRedirect = true
			};

			var httpClient = new HttpClient(handler);

			Console.WriteLine($"   HttpClient создан с поддержкой cookies");
			Console.WriteLine($"   CookieContainer: {cookieContainer.Count} cookies");
			Console.WriteLine($"   UseCookies: {handler.UseCookies}");

			// 2. Пример cookie, которую мог бы отправить сервер
			Console.WriteLine("\n2. СОЗДАНИЕ COOKIE (имитация серверного ответа):");

			var sessionCookie = new Cookie
			{
				Name = "session_id",
				Value = "abc123def456ghi789",
				Domain = "localhost",
				Path = "/",
				Expires = DateTime.Now.AddHours(2),
				HttpOnly = true,      // Защита от JavaScript
				Secure = false        // Только HTTPS (false для localhost)
			};

			Console.WriteLine($"   Cookie '{sessionCookie.Name}':");
			Console.WriteLine($"     Value: {sessionCookie.Value}");
			Console.WriteLine($"     Domain: {sessionCookie.Domain}");
			Console.WriteLine($"     Path: {sessionCookie.Path}");
			Console.WriteLine($"     Expires: {sessionCookie.Expires}");
			Console.WriteLine($"     HttpOnly: {sessionCookie.HttpOnly}");
			Console.WriteLine($"     Secure: {sessionCookie.Secure}");
			Console.WriteLine($"     SameSite: Lax (имитация, в System.Net.Cookie нет свойства)");

			// 3. Добавление cookie в контейнер (имитация получения от сервера)
			Console.WriteLine("\n3. ДОБАВЛЕНИЕ COOKIE В КОНТЕЙНЕР:");

			cookieContainer.Add(new Uri("http://localhost"), sessionCookie);
			Console.WriteLine($"   Cookie добавлена в контейнер");
			Console.WriteLine($"   Теперь в контейнере: {cookieContainer.Count} cookies");

			// 4. Получение cookies для конкретного URI
			Console.WriteLine("\n4. ПОЛУЧЕНИЕ COOKIES ДЛЯ URI:");

			var uri = new Uri("http://localhost/api");
			var cookiesForUri = cookieContainer.GetCookies(uri);

			Console.WriteLine($"   Cookies для {uri}:");
			foreach (Cookie cookie in cookiesForUri)
			{
				Console.WriteLine($"     - {cookie.Name}: {cookie.Value}");
			}

			// 5. Работа с несколькими cookies
			Console.WriteLine("\n5. РАБОТА С НЕСКОЛЬКИМИ COOKIES:");

			var themeCookie = new Cookie("theme", "dark", "/", "localhost");
			var languageCookie = new Cookie("lang", "ru", "/", "localhost");

			cookieContainer.Add(uri, themeCookie);
			cookieContainer.Add(uri, languageCookie);

			Console.WriteLine($"   Добавлены дополнительные cookies");
			Console.WriteLine($"   Всего cookies: {cookieContainer.Count}");

			// 6. Удаление cookie
			Console.WriteLine("\n6. УДАЛЕНИЕ COOKIE:");

			var cookieToDelete = cookieContainer.GetCookies(uri)["theme"];
			if (cookieToDelete != null)
			{
				cookieToDelete.Expired = true;
				Console.WriteLine($"   Cookie 'theme' помечена как удалённая");
			}

			Console.WriteLine($"   Активные cookies: {cookieContainer.GetCookies(uri).Count}");
		}

		public static async Task DemonstrateHttpClientWithCookies()
		{
			Console.WriteLine("\n\n=== HTTPCLIENT С COOKIES НА ПРАКТИКЕ ===\n");

			// Имитация сервера, который использует cookies
			var testServerUri = "http://localhost:5000";

			// Клиент с cookies
			var handler = new HttpClientHandler
			{
				CookieContainer = new CookieContainer(),
				UseCookies = true
			};

			var httpClient = new HttpClient(handler);

			try
			{
				Console.WriteLine("1. ПЕРВЫЙ ЗАПРОС (без cookies):");

				// Имитация запроса, который устанавливает cookie
				var firstRequest = new HttpRequestMessage(HttpMethod.Get, $"{testServerUri}/set-cookie");
				var firstResponse = await httpClient.SendAsync(firstRequest);

				Console.WriteLine($"   Статус: {firstResponse.StatusCode}");

				// Проверка полученных cookies
				var cookies = handler.CookieContainer.GetCookies(new Uri(testServerUri));
				Console.WriteLine($"   Получено cookies: {cookies.Count}");

				if (cookies.Count > 0)
				{
					foreach (Cookie cookie in cookies)
					{
						Console.WriteLine($"     - {cookie.Name}: {cookie.Value}");
					}
				}

				Console.WriteLine("\n2. ВТОРОЙ ЗАПРОС (с cookies):");

				// Второй запрос уже содержит cookies
				var secondRequest = new HttpRequestMessage(HttpMethod.Get, $"{testServerUri}/check-cookie");
				var secondResponse = await httpClient.SendAsync(secondRequest);

				Console.WriteLine($"   Статус: {secondResponse.StatusCode}");

				// Чтение ответа
				var responseContent = await secondResponse.Content.ReadAsStringAsync();
				Console.WriteLine($"   Ответ: {responseContent}");

			}
			catch (HttpRequestException ex)
			{
				Console.WriteLine($"   Ошибка: {ex.Message}");
				Console.WriteLine($"   (Имитация - сервер не запущен)");
			}
		}
	}

	// Демонстрация сессий (серверная сторона)
	public class SessionDemonstration
	{
		// Имитация хранилища сессий на сервере
		private static Dictionary<string, UserSession> _sessionStore = new Dictionary<string, UserSession>();

		public class UserSession
		{
			public string SessionId { get; set; }
			public string UserId { get; set; }
			public UserProfile UserProfile { get; set; }
			public DateTime CreatedAt { get; set; }
			public DateTime LastAccessed { get; set; }
			public Dictionary<string, object> SessionData { get; set; } = new Dictionary<string, object>();

			public bool IsExpired(int timeoutMinutes = 30)
			{
				return (DateTime.Now - LastAccessed).TotalMinutes > timeoutMinutes;
			}
		}

		public static void DemonstrateSessionManagement()
		{
			Console.WriteLine("\n\n=== СЕССИИ НА СЕРВЕРЕ ===\n");

			// 1. Создание сессии (имитация обработки входа пользователя)
			Console.WriteLine("1. СОЗДАНИЕ СЕССИИ:");

			var credentials = new UserCredentials
			{
				Username = "john_doe",
				Password = "secure_password"
			};

			// Проверка учётных данных (имитация)
			if (AuthenticateUser(credentials))
			{
				var sessionId = GenerateSessionId();
				var userProfile = GetUserProfile(credentials.Username);

				var session = new UserSession
				{
					SessionId = sessionId,
					UserId = "user_123",
					UserProfile = userProfile,
					CreatedAt = DateTime.Now,
					LastAccessed = DateTime.Now,
					SessionData = new Dictionary<string, object>
					{
						["cart_items"] = new List<string> { "item1", "item2" },
						["preferences"] = new { theme = "dark", language = "ru" }
					}
				};

				// Сохранение сессии в хранилище
				_sessionStore[sessionId] = session;

				Console.WriteLine($"   Сессия создана:");
				Console.WriteLine($"     SessionId: {session.SessionId}");
				Console.WriteLine($"     UserId: {session.UserId}");
				Console.WriteLine($"     Username: {session.UserProfile.Username}");
				Console.WriteLine($"     CreatedAt: {session.CreatedAt}");
				Console.WriteLine($"     SessionData: {session.SessionData.Count} элементов");

				// 2. Получение сессии по идентификатору
				Console.WriteLine("\n2. ПОЛУЧЕНИЕ СЕССИИ ПО ID:");

				if (_sessionStore.TryGetValue(sessionId, out var retrievedSession))
				{
					Console.WriteLine($"   Сессия найдена:");
					Console.WriteLine($"     User: {retrievedSession.UserProfile.Username}");
					Console.WriteLine($"     Email: {retrievedSession.UserProfile.Email}");
					Console.WriteLine($"     Roles: {string.Join(", ", retrievedSession.UserProfile.Roles)}");

					// Обновление времени доступа
					retrievedSession.LastAccessed = DateTime.Now;
					Console.WriteLine($"     LastAccessed обновлено: {retrievedSession.LastAccessed}");
				}

				// 3. Проверка истечения сессии
				Console.WriteLine("\n3. ПРОВЕРКА ИСТЕЧЕНИЯ СЕССИИ:");

				// Симуляция просроченной сессии
				var expiredSession = new UserSession
				{
					SessionId = "expired_session",
					UserId = "user_456",
					CreatedAt = DateTime.Now.AddHours(-2),
					LastAccessed = DateTime.Now.AddHours(-1)
				};

				Console.WriteLine($"   Новая сессия истекла: {expiredSession.IsExpired(30)}");
				Console.WriteLine($"   Активная сессия истекла: {session.IsExpired(30)}");

				// 4. Удаление сессии (имитация выхода)
				Console.WriteLine("\n4. УДАЛЕНИЕ СЕССИИ (ВЫХОД):");

				_sessionStore.Remove(sessionId);
				Console.WriteLine($"   Сессия удалена из хранилища");
				Console.WriteLine($"   Осталось сессий: {_sessionStore.Count}");

				// 5. Очистка просроченных сессий
				Console.WriteLine("\n5. ОЧИСТКА ПРОСРОЧЕННЫХ СЕССИЙ:");

				var expiredSessions = _sessionStore
					.Where(kv => kv.Value.IsExpired())
					.Select(kv => kv.Key)
					.ToList();

				foreach (var expiredId in expiredSessions)
				{
					_sessionStore.Remove(expiredId);
				}

				Console.WriteLine($"   Удалено просроченных сессий: {expiredSessions.Count}");
				Console.WriteLine($"   Всего активных сессий: {_sessionStore.Count}");
			}
		}

		private static bool AuthenticateUser(UserCredentials credentials)
		{
			// Имитация проверки учётных данных
			return credentials.Username == "john_doe" && credentials.Password == "secure_password";
		}

		private static string GenerateSessionId()
		{
			// Генерация уникального идентификатора сессии
			return Guid.NewGuid().ToString("N");
		}

		private static UserProfile GetUserProfile(string username)
		{
			return new UserProfile
			{
				Username = username,
				Email = "john@example.com",
				Roles = new[] { "user", "premium" },
				CreatedAt = DateTime.Now.AddMonths(-6)
			};
		}
	}

	// Демонстрация авторизации с токенами
	public class TokenAuthDemonstration
	{
		public static void DemonstrateTokenBasedAuth()
		{
			Console.WriteLine("\n\n=== АВТОРИЗАЦИЯ НА ОСНОВЕ ТОКЕНОВ ===\n");

			// 1. Процесс аутентификации и получения токена
			Console.WriteLine("1. АУТЕНТИФИКАЦИЯ И ПОЛУЧЕНИЕ ТОКЕНА:");

			var credentials = new UserCredentials
			{
				Username = "alice",
				Password = "secret123"
			};

			// Имитация запроса на получение токена
			var token = AuthenticateAndGetToken(credentials);

			if (token != null)
			{
				Console.WriteLine($"   Токен получен:");
				Console.WriteLine($"     AccessToken: {token.AccessToken.Substring(0, 20)}...");
				Console.WriteLine($"     TokenType: {token.TokenType}");
				Console.WriteLine($"     ExpiresAt: {token.ExpiresAt}");
				Console.WriteLine($"     RefreshToken: {token.RefreshToken.Substring(0, 20)}...");
			}

			// 2. Использование токена для доступа к защищённому ресурсу
			Console.WriteLine("\n2. ИСПОЛЬЗОВАНИЕ ТОКЕНА В ЗАПРОСАХ:");

			if (token != null)
			{
				Console.WriteLine($"   Формирование запроса с токеном:");
				Console.WriteLine($"     Заголовок: Authorization: {token.TokenType} {token.AccessToken}");

				// Имитация проверки токена на сервере
				var isValid = ValidateToken(token.AccessToken);
				Console.WriteLine($"     Токен валиден: {isValid}");

				if (isValid)
				{
					var userInfo = ExtractUserInfoFromToken(token.AccessToken);
					Console.WriteLine($"     Пользователь: {userInfo.Username}");
					Console.WriteLine($"     Роли: {string.Join(", ", userInfo.Roles)}");
				}
			}

			// 3. Обновление токена
			Console.WriteLine("\n3. ОБНОВЛЕНИЕ ТОКЕНА:");

			if (token != null && token.ExpiresAt < DateTime.Now.AddMinutes(5))
			{
				var newToken = RefreshToken(token.RefreshToken);
				Console.WriteLine($"   Токен обновлён:");
				Console.WriteLine($"     Новый AccessToken: {newToken.AccessToken.Substring(0, 20)}...");
				Console.WriteLine($"     Новый ExpiresAt: {newToken.ExpiresAt}");
			}

			// 4. Сравнение с cookie-based авторизацией
			Console.WriteLine("\n4. СРАВНЕНИЕ С COOKIE-BASED АВТОРИЗАЦИЕЙ:");

			Console.WriteLine("   Cookie-based:");
			Console.WriteLine($"     • Состояние хранится на сервере");
			Console.WriteLine($"     • Идентификатор передаётся в cookie");
			Console.WriteLine($"     • Автоматическая отправка браузером");
			Console.WriteLine($"     • Уязвимо к CSRF (требует защиты)");

			Console.WriteLine("\n   Token-based:");
			Console.WriteLine($"     • Состояние в токене (stateless сервер)");
			Console.WriteLine($"     • Токен передаётся в заголовке Authorization");
			Console.WriteLine($"     • Явная отправка клиентом");
			Console.WriteLine($"     • Защищено от CSRF");
			Console.WriteLine($"     • Легче масштабируется");
		}

		private static AuthToken AuthenticateAndGetToken(UserCredentials credentials)
		{
			// Имитация аутентификации и выдачи токена
			if (credentials.Username == "alice" && credentials.Password == "secret123")
			{
				return new AuthToken
				{
					AccessToken = GenerateJwtToken(credentials.Username),
					RefreshToken = Guid.NewGuid().ToString("N"),
					ExpiresAt = DateTime.Now.AddHours(1),
					TokenType = "Bearer"
				};
			}

			return null;
		}

		private static string GenerateJwtToken(string username)
		{
			// Имитация JWT токена
			var header = "{\"alg\":\"HS256\",\"typ\":\"JWT\"}";
			var payload = $"{{\"sub\":\"{username}\",\"roles\":[\"user\",\"admin\"],\"exp\":{DateTimeOffset.Now.AddHours(1).ToUnixTimeSeconds()}}}";

			var headerBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(header));
			var payloadBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));

			return $"{headerBase64}.{payloadBase64}.signature";
		}

		private static bool ValidateToken(string token)
		{
			// Имитация проверки токена
			return !string.IsNullOrEmpty(token) && token.Contains(".");
		}

		private static UserProfile ExtractUserInfoFromToken(string token)
		{
			// Имитация извлечения информации из токена
			return new UserProfile
			{
				Username = "alice",
				Email = "alice@example.com",
				Roles = new[] { "user", "admin" },
				CreatedAt = DateTime.Now.AddYears(-1)
			};
		}

		private static AuthToken RefreshToken(string refreshToken)
		{
			// Имитация обновления токена
			return new AuthToken
			{
				AccessToken = GenerateJwtToken("alice"),
				RefreshToken = Guid.NewGuid().ToString("N"),
				ExpiresAt = DateTime.Now.AddHours(1),
				TokenType = "Bearer"
			};
		}
	}

	// Практический пример: клиент с полным циклом авторизации
	public class AuthHttpClient : IDisposable
	{
		private readonly HttpClient _httpClient;
		private readonly CookieContainer _cookieContainer;
		private AuthToken _currentToken;
		private string _baseUrl;

		public bool IsAuthenticated => _currentToken != null && _currentToken.ExpiresAt > DateTime.Now;

		public AuthHttpClient(string baseUrl)
		{
			_baseUrl = baseUrl;
			_cookieContainer = new CookieContainer();

			var handler = new HttpClientHandler
			{
				CookieContainer = _cookieContainer,
				UseCookies = true
			};

			_httpClient = new HttpClient(handler);
			_httpClient.BaseAddress = new Uri(baseUrl);
			_httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("AuthDemoClient/1.0");
		}

		public async Task<bool> LoginAsync(string username, string password)
		{
			Console.WriteLine($"\n[AuthHttpClient] Попытка входа пользователя: {username}");

			// Вариант 1: Cookie-based аутентификация
			var loginData = new UserCredentials
			{
				Username = username,
				Password = password
			};

			var json = JsonSerializer.Serialize(loginData);
			var content = new StringContent(json, Encoding.UTF8, "application/json");

			try
			{
				// Запрос на вход
				var response = await _httpClient.PostAsync("/api/auth/login", content);

				if (response.IsSuccessStatusCode)
				{
					// Проверка полученных cookies
					var cookies = _cookieContainer.GetCookies(new Uri(_baseUrl));
					Console.WriteLine($"[AuthHttpClient] Получено cookies: {cookies.Count}");

					if (cookies.Count > 0)
					{
						Console.WriteLine($"[AuthHttpClient] Аутентификация через cookies успешна");
						return true;
					}
				}

				// Вариант 2: Token-based аутентификация
				var tokenResponse = await _httpClient.PostAsync("/api/auth/token", content);

				if (tokenResponse.IsSuccessStatusCode)
				{
					var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
					_currentToken = JsonSerializer.Deserialize<AuthToken>(tokenJson);

					// Установка токена в заголовки
					_httpClient.DefaultRequestHeaders.Authorization =
						new System.Net.Http.Headers.AuthenticationHeaderValue(
							_currentToken.TokenType,
							_currentToken.AccessToken);

					Console.WriteLine($"[AuthHttpClient] Токен получен, истекает: {_currentToken.ExpiresAt}");
					return true;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[AuthHttpClient] Ошибка входа: {ex.Message}");
			}

			return false;
		}

		public async Task<UserProfile> GetProfileAsync()
		{
			Console.WriteLine($"\n[AuthHttpClient] Запрос профиля пользователя");

			try
			{
				var response = await _httpClient.GetAsync("/api/user/profile");

				if (response.IsSuccessStatusCode)
				{
					var json = await response.Content.ReadAsStringAsync();
					var profile = JsonSerializer.Deserialize<UserProfile>(json);

					Console.WriteLine($"[AuthHttpClient] Профиль получен: {profile.Username}");
					return profile;
				}
				else if (response.StatusCode == HttpStatusCode.Unauthorized)
				{
					Console.WriteLine($"[AuthHttpClient] Доступ запрещён (401 Unauthorized)");

					// Попытка обновить токен, если он есть
					if (_currentToken != null && await RefreshTokenAsync())
					{
						// Повторный запрос после обновления токена
						return await GetProfileAsync();
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[AuthHttpClient] Ошибка получения профиля: {ex.Message}");
			}

			return null;
		}

		public async Task<bool> LogoutAsync()
		{
			Console.WriteLine($"\n[AuthHttpClient] Выход из системы");

			try
			{
				var response = await _httpClient.PostAsync("/api/auth/logout", null);

				if (response.IsSuccessStatusCode)
				{
					// Очистка локальных данных
					_currentToken = null;
					_httpClient.DefaultRequestHeaders.Authorization = null;
					_cookieContainer.GetCookies(new Uri(_baseUrl)).Clear();

					Console.WriteLine($"[AuthHttpClient] Выход выполнен успешно");
					return true;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[AuthHttpClient] Ошибка выхода: {ex.Message}");
			}

			return false;
		}

		private async Task<bool> RefreshTokenAsync()
		{
			if (_currentToken == null || string.IsNullOrEmpty(_currentToken.RefreshToken))
				return false;

			Console.WriteLine($"[AuthHttpClient] Попытка обновления токена");

			try
			{
				var refreshData = new { refreshToken = _currentToken.RefreshToken };
				var json = JsonSerializer.Serialize(refreshData);
				var content = new StringContent(json, Encoding.UTF8, "application/json");

				var response = await _httpClient.PostAsync("/api/auth/refresh", content);

				if (response.IsSuccessStatusCode)
				{
					var tokenJson = await response.Content.ReadAsStringAsync();
					_currentToken = JsonSerializer.Deserialize<AuthToken>(tokenJson);

					// Обновление заголовка авторизации
					_httpClient.DefaultRequestHeaders.Authorization =
						new System.Net.Http.Headers.AuthenticationHeaderValue(
							_currentToken.TokenType,
							_currentToken.AccessToken);

					Console.WriteLine($"[AuthHttpClient] Токен обновлён, новый срок: {_currentToken.ExpiresAt}");
					return true;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[AuthHttpClient] Ошибка обновления токена: {ex.Message}");
			}

			return false;
		}

		public void PrintAuthStatus()
		{
			Console.WriteLine($"\n[AuthHttpClient] Статус аутентификации:");
			Console.WriteLine($"   IsAuthenticated: {IsAuthenticated}");

			if (_currentToken != null)
			{
				Console.WriteLine($"   Token-based: Да");
				Console.WriteLine($"   Token expires: {_currentToken.ExpiresAt}");
				Console.WriteLine($"   Time to expire: {(_currentToken.ExpiresAt - DateTime.Now).TotalMinutes:F0} минут");
			}

			var cookies = _cookieContainer.GetCookies(new Uri(_baseUrl));
			Console.WriteLine($"   Cookies: {cookies.Count}");

			foreach (Cookie cookie in cookies)
			{
				Console.WriteLine($"     - {cookie.Name}: {cookie.Value} (HttpOnly: {cookie.HttpOnly})");
			}
		}

		public void Dispose()
		{
			_httpClient?.Dispose();
		}
	}

	// Главная программа
	class Program
	{
		static async Task Main(string[] args)
		{
			Console.WriteLine("COOKIES, СЕССИИ И АВТОРИЗАЦИЯ В HTTP");
			Console.WriteLine("=====================================\n");

			// 1. Демонстрация cookies
			CookieDemonstration.DemonstrateCookieMechanism();
			await CookieDemonstration.DemonstrateHttpClientWithCookies();

			// 2. Демонстрация сессий
			SessionDemonstration.DemonstrateSessionManagement();

			// 3. Демонстрация токенной авторизации
			TokenAuthDemonstration.DemonstrateTokenBasedAuth();

			// 4. Практический пример клиента
			Console.WriteLine("\n\n=== ПРАКТИЧЕСКИЙ ПРИМЕР HTTP КЛИЕНТА ===\n");

			using (var authClient = new AuthHttpClient("https://api.example.com"))
			{
				// Симуляция различных сценариев
				Console.WriteLine("Сценарий 1: Попытка доступа без аутентификации");
				var profile = await authClient.GetProfileAsync();
				authClient.PrintAuthStatus();

				Console.WriteLine("\nСценарий 2: Аутентификация пользователя");
				var loggedIn = await authClient.LoginAsync("john_doe", "password123");
				Console.WriteLine($"Login successful: {loggedIn}");
				authClient.PrintAuthStatus();

				if (loggedIn)
				{
					Console.WriteLine("\nСценарий 3: Доступ к защищённому ресурсу");
					profile = await authClient.GetProfileAsync();

					Console.WriteLine("\nСценарий 4: Выход из системы");
					await authClient.LogoutAsync();
					authClient.PrintAuthStatus();
				}
			}
		}
	}
}
