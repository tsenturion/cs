using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Чтобы лучше видеть заголовки и тело:
builder.Services.AddHttpLogging(_ => { }); // можно включить детальную настройку при желании
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseHttpLogging();

// ====== MIDDLEWARE: демонстрация принципов HTTP ======

// 1) Логирование "стартовой строки" (метод + путь + версия HTTP)
app.Use(async (ctx, next) =>
{
	// Пример стартовой строки: "GET /users?limit=10 HTTP/1.1"
	var startLine = $"{ctx.Request.Method} {ctx.Request.Path}{ctx.Request.QueryString} {ctx.Request.Protocol}";
	Console.WriteLine($"\n--- REQUEST START LINE: {startLine}");

	// Заголовки как метаданные
	Console.WriteLine($"Host: {ctx.Request.Host}");
	Console.WriteLine($"User-Agent: {ctx.Request.Headers.UserAgent}");
	Console.WriteLine($"Accept: {ctx.Request.Headers.Accept}");
	Console.WriteLine($"Content-Type: {ctx.Request.ContentType}");
	Console.WriteLine($"Authorization: {(ctx.Request.Headers.Authorization.ToString() is { Length: > 0 } ? "[present]" : "[missing]")}");
	Console.WriteLine($"X-Request-Id: {ctx.Request.Headers["X-Request-Id"].ToString()}");

	var sw = Stopwatch.StartNew();
	await next();
	sw.Stop();

	// Структура ответа: статус + заголовки + (возможное) тело
	Console.WriteLine($"--- RESPONSE: {ctx.Response.StatusCode} in {sw.ElapsedMilliseconds}ms");
	Console.WriteLine($"Response Content-Type: {ctx.Response.ContentType}");
});

// 2) "Stateless" демонстрация: сервер не хранит состояние пользователя между запросами.
// Но мы можем передавать состояние явно: через токен, cookie, заголовки, query и т.д.
// Ниже — имитация "токена" и "сессии" через заголовок Authorization.

// ====== "База данных" в памяти (для демонстрации ресурсов) ======
var users = new ConcurrentDictionary<int, User>();
users[1] = new User(1, "Alice", "alice@example.com", IsActive: true);
users[2] = new User(2, "Bob", "bob@example.com", IsActive: true);

int nextId = 3;

// ====== ROUTES: URI/URL и маршрутизация ======

// OPTIONS: показать поддерживаемые методы для ресурса
app.MapMethods("/users", new[] { "OPTIONS" }, (HttpContext ctx) =>
{
	ctx.Response.Headers["Allow"] = "GET,POST,HEAD,OPTIONS";
	// В реальности при CORS браузер делает preflight OPTIONS
	return Results.Ok(new
	{
		resource = "/users",
		allowedMethods = new[] { "GET", "POST", "HEAD", "OPTIONS" },
		note = "Это демонстрация OPTIONS: узнать возможности ресурса."
	});
});

// HEAD: как GET, но без тела
app.MapMethods("/users", new[] { "HEAD" }, (HttpContext ctx) =>
{
	// Возвращаем только заголовки (например, Count)
	ctx.Response.Headers["X-Users-Count"] = users.Count.ToString();
	// Тело отсутствует
	return Results.StatusCode(StatusCodes.Status200OK);
});

// GET коллекции + query-параметры фильтрации/пагинации
app.MapGet("/users", ([FromQuery] int? limit, [FromQuery] bool? activeOnly) =>
{
	IEnumerable<User> query = users.Values.OrderBy(u => u.Id);

	if (activeOnly == true)
		query = query.Where(u => u.IsActive);

	if (limit is > 0)
		query = query.Take(limit.Value);

	return Results.Ok(new
	{
		items = query.ToArray(),
		total = users.Count,
		hint = "GET безопасный и идемпотентный. Фильтры — в query."
	});
});

// GET конкретного ресурса + кэширование (ETag + If-None-Match)
app.MapGet("/users/{id:int}", (HttpContext ctx, int id) =>
{
	if (!users.TryGetValue(id, out var user))
		return Results.NotFound(new { error = "User not found", id });

	var json = JsonSerializer.Serialize(user);
	var etag = HttpHelpers.ComputeETag(json);

	ctx.Response.Headers.ETag = etag;
	ctx.Response.Headers.CacheControl = "public, max-age=30";

	var ifNoneMatch = ctx.Request.Headers.IfNoneMatch.ToString();
	if (!string.IsNullOrWhiteSpace(ifNoneMatch) && string.Equals(ifNoneMatch, etag, StringComparison.Ordinal))
	{
		// 304: тело не отдаём
		return Results.StatusCode(StatusCodes.Status304NotModified);
	}

	return Results.Text(json, "application/json; charset=utf-8");
});

// POST создание ресурса (не идемпотентный)
app.MapPost("/users", async (HttpContext ctx) =>
{
	// Пример: stateless авторизация через заголовок Authorization
	if (!FakeAuth.IsAuthorized(ctx))
		return Results.Unauthorized();

	// Content-Type важен: ожидаем JSON
	if (!string.Equals(ctx.Request.ContentType?.Split(';')[0].Trim(), "application/json", StringComparison.OrdinalIgnoreCase))
		return Results.StatusCode(StatusCodes.Status415UnsupportedMediaType);

	var dto = await ctx.Request.ReadFromJsonAsync<CreateUserDto>();
	if (dto is null || string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Email))
		return Results.BadRequest(new { error = "Invalid body: name/email required" });

	var id = Interlocked.Increment(ref nextId) - 1;
	var user = new User(id, dto.Name.Trim(), dto.Email.Trim(), IsActive: true);
	users[id] = user;

	// 201 Created + Location
	return Results.Created($"/users/{id}", user);
});

// PUT полная замена ресурса (идемпотентный)
app.MapPut("/users/{id:int}", async (HttpContext ctx, int id) =>
{
	if (!FakeAuth.IsAuthorized(ctx))
		return Results.Unauthorized();

	if (!string.Equals(ctx.Request.ContentType?.Split(';')[0].Trim(), "application/json", StringComparison.OrdinalIgnoreCase))
		return Results.StatusCode(StatusCodes.Status415UnsupportedMediaType);

	var dto = await ctx.Request.ReadFromJsonAsync<ReplaceUserDto>();
	if (dto is null || string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Email))
		return Results.BadRequest(new { error = "Invalid body: name/email required" });

	// PUT: "сделай ресурс именно таким"
	var updated = new User(id, dto.Name.Trim(), dto.Email.Trim(), dto.IsActive);
	users[id] = updated;

	// Можно 200 OK с телом
	return Results.Ok(updated);
});

// PATCH частичное обновление (часто делают идемпотентным, если patch-операции описаны как "установить значение")
app.MapPatch("/users/{id:int}", async (HttpContext ctx, int id) =>
{
	if (!FakeAuth.IsAuthorized(ctx))
		return Results.Unauthorized();

	if (!users.TryGetValue(id, out var existing))
		return Results.NotFound(new { error = "User not found", id });

	if (!string.Equals(ctx.Request.ContentType?.Split(';')[0].Trim(), "application/json", StringComparison.OrdinalIgnoreCase))
		return Results.StatusCode(StatusCodes.Status415UnsupportedMediaType);

	var patch = await ctx.Request.ReadFromJsonAsync<PatchUserDto>();
	if (patch is null)
		return Results.BadRequest(new { error = "Invalid body" });

	// PATCH: изменяем только то, что пришло
	var newName = string.IsNullOrWhiteSpace(patch.Name) ? existing.Name : patch.Name.Trim();
	var newEmail = string.IsNullOrWhiteSpace(patch.Email) ? existing.Email : patch.Email.Trim();
	var newActive = patch.IsActive ?? existing.IsActive;

	var updated = existing with { Name = newName, Email = newEmail, IsActive = newActive };
	users[id] = updated;

	return Results.Ok(updated);
});

// DELETE удаление (идемпотентный)
app.MapDelete("/users/{id:int}", (HttpContext ctx, int id) =>
{
	if (!FakeAuth.IsAuthorized(ctx))
		return Results.Unauthorized();

	// Idempotent: если уже удалён — можно вернуть 204 или 404 (зависит от контракта)
	users.TryRemove(id, out _);
	return Results.NoContent();
});

// Демонстрация form-data (multipart) — загрузка "аватара" как файл
app.MapPost("/users/{id:int}/avatar", async (HttpContext ctx, int id) =>
{
	if (!FakeAuth.IsAuthorized(ctx))
		return Results.Unauthorized();

	if (!users.ContainsKey(id))
		return Results.NotFound(new { error = "User not found", id });

	if (!ctx.Request.HasFormContentType)
		return Results.StatusCode(StatusCodes.Status415UnsupportedMediaType);

	var form = await ctx.Request.ReadFormAsync();
	var file = form.Files.GetFile("avatar");
	if (file is null || file.Length == 0)
		return Results.BadRequest(new { error = "avatar file is required" });

	// Мы не сохраняем файл на диск — просто читаем и возвращаем метаданные
	return Results.Ok(new
	{
		userId = id,
		fileName = file.FileName,
		contentType = file.ContentType,
		length = file.Length,
		note = "multipart/form-data пример"
	});
});

// Endpoint для демонстрации 500 (ошибка сервера)
app.MapGet("/boom", () =>
{
	// Возвращаем 500 без исключения, чтобы не падать в логах
	return Results.Problem("Demo server error", statusCode: StatusCodes.Status500InternalServerError);
});

// ====== SERVER START ======
var serverTask = app.RunAsync("http://localhost:5080");

// ====== CLIENT DEMO: HttpClient примеры запросов ======
await Task.Delay(250); // небольшая пауза, чтобы сервер успел стартовать

Console.WriteLine("\n================ CLIENT DEMO ================");
// Avoid system proxy so localhost calls reach this process directly.
using var http = new HttpClient(new HttpClientHandler { UseProxy = false })
{
	BaseAddress = new Uri("http://localhost:5080")
};

// User-Agent и кастомный заголовок — как метаданные запроса
http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("DemoClient", "1.0"));
http.DefaultRequestHeaders.Add("X-Request-Id", Guid.NewGuid().ToString());

// 1) OPTIONS /users
{
	var req = new HttpRequestMessage(HttpMethod.Options, "/users");
	var resp = await http.SendAsync(req);
	Console.WriteLine($"\nCLIENT: OPTIONS /users => {(int)resp.StatusCode} {resp.ReasonPhrase}");
	var allow = resp.Headers.TryGetValues("Allow", out var allowValues)
		? string.Join(", ", allowValues)
		: resp.Content.Headers.TryGetValues("Allow", out var allowValues2)
			? string.Join(", ", allowValues2)
			: "n/a";
	Console.WriteLine("Allow: " + allow);
	Console.WriteLine(await resp.Content.ReadAsStringAsync());
}

// 2) HEAD /users
{
	var req = new HttpRequestMessage(HttpMethod.Head, "/users");
	var resp = await http.SendAsync(req);
	Console.WriteLine($"\nCLIENT: HEAD /users => {(int)resp.StatusCode} {resp.ReasonPhrase}");
	Console.WriteLine("X-Users-Count: " + (resp.Headers.TryGetValues("X-Users-Count", out var values) ? values.FirstOrDefault() : "n/a"));
	Console.WriteLine("Body length: " + (await resp.Content.ReadAsByteArrayAsync()).Length);
}

// 3) GET /users?limit=10&activeOnly=true (query-параметры)
{
	var resp = await http.GetAsync("/users?limit=10&activeOnly=true");
	Console.WriteLine($"\nCLIENT: GET /users?... => {(int)resp.StatusCode} {resp.ReasonPhrase}");
	Console.WriteLine(await resp.Content.ReadAsStringAsync());
}

// 4) GET /users/1 c ETag (кэширование)
string? etag = null;
{
	var resp = await http.GetAsync("/users/1");
	etag = resp.Headers.ETag?.Tag;
	Console.WriteLine($"\nCLIENT: GET /users/1 => {(int)resp.StatusCode} {resp.ReasonPhrase}");
	Console.WriteLine("ETag: " + etag);
	Console.WriteLine(await resp.Content.ReadAsStringAsync());
}
{
	var req = new HttpRequestMessage(HttpMethod.Get, "/users/1");
	if (!string.IsNullOrWhiteSpace(etag))
		req.Headers.IfNoneMatch.Add(new EntityTagHeaderValue(etag));

	var resp = await http.SendAsync(req);
	Console.WriteLine($"\nCLIENT: GET /users/1 If-None-Match => {(int)resp.StatusCode} {resp.ReasonPhrase}");
	Console.WriteLine("Body length: " + (await resp.Content.ReadAsByteArrayAsync()).Length);
}

// 5) POST /users без Authorization => 401
{
	var body = JsonSerializer.Serialize(new { name = "Charlie", email = "charlie@example.com" });
	var resp = await http.PostAsync("/users", new StringContent(body, Encoding.UTF8, "application/json"));
	Console.WriteLine($"\nCLIENT: POST /users (no auth) => {(int)resp.StatusCode} {resp.ReasonPhrase}");
}

// 6) POST /users с Authorization + JSON => 201 Created
int createdId = -1;
{
	var req = new HttpRequestMessage(HttpMethod.Post, "/users");
	req.Headers.Authorization = AuthenticationHeaderValue.Parse(FakeAuth.DemoToken);
	req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

	var body = JsonSerializer.Serialize(new { name = "Charlie", email = "charlie@example.com" });
	req.Content = new StringContent(body, Encoding.UTF8, "application/json");

	var resp = await http.SendAsync(req);
	var text = await resp.Content.ReadAsStringAsync();
	Console.WriteLine($"\nCLIENT: POST /users (auth) => {(int)resp.StatusCode} {resp.ReasonPhrase}");
	Console.WriteLine("Location: " + resp.Headers.Location);
	Console.WriteLine(text);

	if (!resp.IsSuccessStatusCode)
	{
		Console.WriteLine("CLIENT: create failed; skipping id parse.");
	}
	else if (string.IsNullOrWhiteSpace(text))
	{
		Console.WriteLine("CLIENT: empty response body; skipping id parse.");
	}
	else
	{
		using var doc = JsonDocument.Parse(text);
		if (doc.RootElement.TryGetProperty("id", out var idProp) && idProp.TryGetInt32(out var idValue))
			createdId = idValue;
		else
			Console.WriteLine("CLIENT: response JSON has no numeric id.");
	}
}

// 7) PUT /users/{id} (полная замена) => идемпотентность (повторим дважды)
{
	var req = new HttpRequestMessage(HttpMethod.Put, $"/users/{createdId}");
	req.Headers.Authorization = AuthenticationHeaderValue.Parse(FakeAuth.DemoToken);

	var body = JsonSerializer.Serialize(new { name = "Charlie Updated", email = "charlie.updated@example.com", isActive = true });
	req.Content = new StringContent(body, Encoding.UTF8, "application/json");

	var resp1 = await http.SendAsync(req);
	Console.WriteLine($"\nCLIENT: PUT /users/{createdId} => {(int)resp1.StatusCode} {resp1.ReasonPhrase}");

	// Повторим тот же PUT (идемпотентность)
	var req2 = new HttpRequestMessage(HttpMethod.Put, $"/users/{createdId}");
	req2.Headers.Authorization = AuthenticationHeaderValue.Parse(FakeAuth.DemoToken);
	req2.Content = new StringContent(body, Encoding.UTF8, "application/json");

	var resp2 = await http.SendAsync(req2);
	Console.WriteLine($"CLIENT: PUT повтор => {(int)resp2.StatusCode} {resp2.ReasonPhrase}");
}

// 8) PATCH /users/{id} (частичное обновление)
{
	var req = new HttpRequestMessage(HttpMethod.Patch, $"/users/{createdId}");
	req.Headers.Authorization = AuthenticationHeaderValue.Parse(FakeAuth.DemoToken);

	var body = JsonSerializer.Serialize(new { isActive = false }); // меняем только одно поле
	req.Content = new StringContent(body, Encoding.UTF8, "application/json");

	var resp = await http.SendAsync(req);
	Console.WriteLine($"\nCLIENT: PATCH /users/{createdId} => {(int)resp.StatusCode} {resp.ReasonPhrase}");
	Console.WriteLine(await resp.Content.ReadAsStringAsync());
}

// 9) DELETE /users/{id} (идемпотентный: повторный DELETE тоже "ок" по контракту)
{
	var req = new HttpRequestMessage(HttpMethod.Delete, $"/users/{createdId}");
	req.Headers.Authorization = AuthenticationHeaderValue.Parse(FakeAuth.DemoToken);

	var resp1 = await http.SendAsync(req);
	Console.WriteLine($"\nCLIENT: DELETE /users/{createdId} => {(int)resp1.StatusCode} {resp1.ReasonPhrase}");

	var req2 = new HttpRequestMessage(HttpMethod.Delete, $"/users/{createdId}");
	req2.Headers.Authorization = AuthenticationHeaderValue.Parse(FakeAuth.DemoToken);

	var resp2 = await http.SendAsync(req2);
	Console.WriteLine($"CLIENT: DELETE повтор => {(int)resp2.StatusCode} {resp2.ReasonPhrase}");
}

// 10) Ошибка сервера 500
{
	var resp = await http.GetAsync("/boom");
	Console.WriteLine($"\nCLIENT: GET /boom => {(int)resp.StatusCode} {resp.ReasonPhrase}");
	Console.WriteLine(await resp.Content.ReadAsStringAsync());
}

// Остановим сервер, чтобы приложение завершилось
Console.WriteLine("\n================ END ================");
await app.StopAsync();
await serverTask;

// ====== Helpers ======
static class FakeAuth
{
	public const string DemoToken = "Bearer demo-token-123";

	public static bool IsAuthorized(HttpContext ctx) =>
		string.Equals(ctx.Request.Headers.Authorization.ToString(), DemoToken, StringComparison.Ordinal);
}

static class HttpHelpers
{
	public static string ComputeETag(string payload)
	{
		var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
		return "\"" + Convert.ToHexString(bytes) + "\"";
	}
}

// ====== Models ======
public record User(int Id, string Name, string Email, bool IsActive);

public record CreateUserDto(string Name, string Email);
public record ReplaceUserDto(string Name, string Email, bool IsActive);
public record PatchUserDto(string? Name, string? Email, bool? IsActive);
