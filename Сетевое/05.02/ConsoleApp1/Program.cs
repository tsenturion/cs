using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace FtpProtocolDemo
{
	// Модель для хранения состояния FTP сессии
	public class FtpSessionState
	{
		public bool IsAuthenticated { get; set; }
		public string CurrentDirectory { get; set; } = "/";
		public string Username { get; set; }
		public TransferMode TransferMode { get; set; } = TransferMode.Binary;
		public DataConnectionMode DataConnectionMode { get; set; } = DataConnectionMode.Passive;
		public Encoding Encoding { get; set; } = Encoding.ASCII;
		public DateTime SessionStart { get; set; } = DateTime.Now;

		// Для пассивного режима
		public IPEndPoint PassiveEndpoint { get; set; }
		public TcpListener PassiveListener { get; set; }

		// Для активного режима
		public IPEndPoint ActiveEndpoint { get; set; }
	}

	public enum TransferMode
	{
		ASCII,
		Binary
	}

	public enum DataConnectionMode
	{
		Active,
		Passive
	}

	// Базовый FTP сервер для демонстрации протокола
	public class SimpleFtpServer : IDisposable
	{
		private TcpListener _controlListener;
		private bool _isRunning;
		private readonly int _port;
		private readonly string _rootDirectory;
		private readonly Dictionary<TcpClient, FtpSessionState> _sessions = new();
		private readonly Dictionary<string, string> _users; // Простая имитация базы пользователей

		// Статистика сервера
		public int ActiveSessions => _sessions.Count;
		public long TotalConnections { get; private set; }
		public long FilesTransferred { get; private set; }

		public SimpleFtpServer(int port = 21, string rootDirectory = null)
		{
			_port = port;
			_rootDirectory = rootDirectory ?? Path.Combine(Environment.CurrentDirectory, "ftp_root");

			// Создаём корневую директорию, если не существует
			if (!Directory.Exists(_rootDirectory))
			{
				Directory.CreateDirectory(_rootDirectory);
			}

			// Простая база пользователей для демонстрации
			_users = new Dictionary<string, string>
			{
				["anonymous"] = "", // Анонимный доступ
				["user1"] = "password1",
				["admin"] = "admin123"
			};
		}

		public void Start()
		{
			_isRunning = true;
			_controlListener = new TcpListener(IPAddress.Any, _port);
			_controlListener.Start();

			Console.WriteLine($"=== FTP СЕРВЕР ЗАПУЩЕН ===");
			Console.WriteLine($"Порт: {_port}");
			Console.WriteLine($"Корневая директория: {_rootDirectory}");
			Console.WriteLine($"Доступные пользователи: anonymous, user1, admin");
			Console.WriteLine($"================================\n");

			// Запуск асинхронного приёма подключений
			ThreadPool.QueueUserWorkItem(AcceptConnections);
		}

		private void AcceptConnections(object state)
		{
			while (_isRunning)
			{
				try
				{
					// Блокирующее ожидание подключения
					TcpClient client = _controlListener.AcceptTcpClient();
					TotalConnections++;

					Console.WriteLine($"[FTP] Новое подключение: {client.Client.RemoteEndPoint}");

					// Создаём состояние сессии для клиента
					var sessionState = new FtpSessionState();
					_sessions[client] = sessionState;

					// Запускаем обработку клиента в отдельном потоке
					ThreadPool.QueueUserWorkItem(HandleClient, client);
				}
				catch (SocketException) when (!_isRunning)
				{
					break;
				}
				catch (Exception ex)
				{
					Console.WriteLine($"[FTP] Ошибка при приёме подключения: {ex.Message}");
				}
			}
		}

		private void HandleClient(object state)
		{
			TcpClient client = (TcpClient)state;
			NetworkStream stream = client.GetStream();
			StreamReader reader = new StreamReader(stream, Encoding.ASCII);
			StreamWriter writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };

			FtpSessionState session = _sessions[client];
			string? remoteEndpoint = null;
			try
			{
				remoteEndpoint = client.Client?.RemoteEndPoint?.ToString();
			}
			catch
			{
				remoteEndpoint = null;
			}

			try
			{
				// Шаг 1: Отправка приветственного сообщения (код 220)
				SendResponse(writer, 220, "Simple FTP Server Ready");

				// Цикл обработки команд
				while (_isRunning && client.Connected)
				{
					string commandLine = reader.ReadLine();

					if (string.IsNullOrEmpty(commandLine))
					{
						Thread.Sleep(100);
						continue;
					}

					Console.WriteLine($"[{client.Client.RemoteEndPoint}] >> {commandLine}");

					// Разбор команды
					string[] parts = commandLine.Split(' ', 2);
					string command = parts[0].ToUpper();
					string arguments = parts.Length > 1 ? parts[1] : string.Empty;

					// Обработка команды
					ProcessCommand(client, session, command, arguments, writer);
				}
			}
			catch (IOException ex)
			{
				Console.WriteLine($"[{client.Client.RemoteEndPoint}] Соединение разорвано: {ex.Message}");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[{client.Client.RemoteEndPoint}] Ошибка: {ex.Message}");
			}
			finally
			{
				CleanupSession(session);
				_sessions.Remove(client);
				client.Close();

				string endpoint = remoteEndpoint ?? "<unknown>";
				Console.WriteLine($"[{endpoint}] Сессия завершена");
			}
		}

		private void ProcessCommand(TcpClient client, FtpSessionState session,
								   string command, string arguments, StreamWriter writer)
		{
			try
			{
				switch (command)
				{
					case "USER":
						HandleUser(client, session, arguments, writer);
						break;

					case "PASS":
						HandlePass(client, session, arguments, writer);
						break;

					case "SYST":
						SendResponse(writer, 215, "UNIX Type: L8");
						break;

					case "FEAT":
						SendResponse(writer, 211, "Features:");
						writer.WriteLine(" UTF8");
						writer.WriteLine(" PASV");
						writer.WriteLine(" SIZE");
						SendResponse(writer, 211, "End");
						break;

					case "PWD":
						HandlePwd(session, writer);
						break;

					case "CWD":
						HandleCwd(session, arguments, writer);
						break;

					case "LIST":
						HandleList(client, session, writer);
						break;

					case "PASV":
						HandlePasv(client, session, writer);
						break;

					case "PORT":
						HandlePort(session, arguments, writer);
						break;

					case "TYPE":
						HandleType(session, arguments, writer);
						break;

					case "RETR":
						HandleRetr(client, session, arguments, writer);
						break;

					case "STOR":
						HandleStor(client, session, arguments, writer);
						break;

					case "DELE":
						HandleDele(session, arguments, writer);
						break;

					case "MKD":
						HandleMkd(session, arguments, writer);
						break;

					case "RMD":
						HandleRmd(session, arguments, writer);
						break;

					case "SIZE":
						HandleSize(session, arguments, writer);
						break;

					case "QUIT":
						SendResponse(writer, 221, "Goodbye");
						client.Close();
						break;

					case "NOOP":
						SendResponse(writer, 200, "OK");
						break;

					default:
						SendResponse(writer, 502, "Command not implemented");
						break;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[Command Error] {command}: {ex.Message}");
				SendResponse(writer, 550, $"Error: {ex.Message}");
			}
		}

		#region Команды управления сессией

		private void HandleUser(TcpClient client, FtpSessionState session, string username, StreamWriter writer)
		{
			session.Username = username;

			if (username.ToLower() == "anonymous")
			{
				// Для анонимного пользователя сразу успех
				session.IsAuthenticated = true;
				SendResponse(writer, 230, "Anonymous login ok, send your email as password");
			}
			else if (_users.ContainsKey(username))
			{
				SendResponse(writer, 331, "Password required");
			}
			else
			{
				SendResponse(writer, 530, "User not found");
			}
		}

		private void HandlePass(TcpClient client, FtpSessionState session, string password, StreamWriter writer)
		{
			if (string.IsNullOrEmpty(session.Username))
			{
				SendResponse(writer, 503, "Login with USER first");
				return;
			}

			if (session.Username.ToLower() == "anonymous")
			{
				// Анонимный доступ - пароль игнорируется
				session.IsAuthenticated = true;
				SendResponse(writer, 230, "Anonymous login ok");
				return;
			}

			if (_users.TryGetValue(session.Username, out string storedPassword) &&
				storedPassword == password)
			{
				session.IsAuthenticated = true;
				SendResponse(writer, 230, "Login successful");
			}
			else
			{
				SendResponse(writer, 530, "Login incorrect");
			}
		}

		#endregion

		#region Команды навигации

		private void HandlePwd(FtpSessionState session, StreamWriter writer)
		{
			if (!CheckAuthentication(session, writer)) return;

			SendResponse(writer, 257, $"\"{session.CurrentDirectory}\" is current directory");
		}

		private void HandleCwd(FtpSessionState session, string directory, StreamWriter writer)
		{
			if (!CheckAuthentication(session, writer)) return;

			try
			{
				string newPath = NormalizePath(session.CurrentDirectory, directory);

				// Проверяем существование директории
				string physicalPath = MapToPhysicalPath(newPath);
				if (!Directory.Exists(physicalPath))
				{
					SendResponse(writer, 550, "Directory not found");
					return;
				}

				session.CurrentDirectory = newPath;
				SendResponse(writer, 250, "Directory changed to " + newPath);
			}
			catch
			{
				SendResponse(writer, 550, "Failed to change directory");
			}
		}

		#endregion

		#region Команды передачи данных

		private void HandlePasv(TcpClient client, FtpSessionState session, StreamWriter writer)
		{
			if (!CheckAuthentication(session, writer)) return;

			try
			{
				// Создаём пассивный слушатель на случайном порту
				session.PassiveListener?.Stop();
				session.PassiveListener = new TcpListener(IPAddress.Any, 0);
				session.PassiveListener.Start();

				// Получаем локальный IP и порт
				var endpoint = (IPEndPoint)session.PassiveListener.LocalEndpoint;
				session.PassiveEndpoint = endpoint;
				session.DataConnectionMode = DataConnectionMode.Passive;

				// Выбираем IP адрес, который действительно достижим клиентом
				IPAddress ipAddress = endpoint.Address;
				if (ipAddress.Equals(IPAddress.Any) || ipAddress.Equals(IPAddress.IPv6Any))
				{
					if (client.Client.LocalEndPoint is IPEndPoint localEndpoint)
					{
						ipAddress = localEndpoint.Address;
					}
				}

				if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
				{
					ipAddress = ipAddress.IsIPv4MappedToIPv6 ? ipAddress.MapToIPv4() : IPAddress.Loopback;
				}

				if (ipAddress.GetAddressBytes().Length != 4)
				{
					ipAddress = IPAddress.Loopback;
				}

				// Формируем ответ в формате FTP для пассивного режима
				byte[] ipBytes = ipAddress.GetAddressBytes();
				int port = endpoint.Port;
				int p1 = port / 256;
				int p2 = port % 256;

				string response = string.Format(
					"Entering Passive Mode ({0},{1},{2},{3},{4},{5})",
					ipBytes[0], ipBytes[1], ipBytes[2], ipBytes[3],
					p1, p2);

				SendResponse(writer, 227, response);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[PASV Error] {ex.Message}");
				SendResponse(writer, 550, "Failed to enter passive mode");
			}
		}

		private void HandlePort(FtpSessionState session, string arguments, StreamWriter writer)
		{
			if (!CheckAuthentication(session, writer)) return;

			try
			{
				// Разбираем параметры PORT: h1,h2,h3,h4,p1,p2
				string[] parts = arguments.Split(',');
				if (parts.Length != 6)
				{
					SendResponse(writer, 501, "Invalid PORT command");
					return;
				}

				byte[] ipBytes = new byte[4];
				for (int i = 0; i < 4; i++)
				{
					ipBytes[i] = byte.Parse(parts[i]);
				}

				int port = int.Parse(parts[4]) * 256 + int.Parse(parts[5]);
				var ipAddress = new IPAddress(ipBytes);

				session.ActiveEndpoint = new IPEndPoint(ipAddress, port);
				session.DataConnectionMode = DataConnectionMode.Active;

				SendResponse(writer, 200, "PORT command successful");
			}
			catch
			{
				SendResponse(writer, 501, "Invalid PORT command");
			}
		}

		private void HandleType(FtpSessionState session, string typeCode, StreamWriter writer)
		{
			if (!CheckAuthentication(session, writer)) return;

			switch (typeCode.ToUpper())
			{
				case "A":
					session.TransferMode = TransferMode.ASCII;
					SendResponse(writer, 200, "Type set to ASCII");
					break;

				case "I":
					session.TransferMode = TransferMode.Binary;
					SendResponse(writer, 200, "Type set to Binary");
					break;

				default:
					SendResponse(writer, 504, "Type not supported");
					break;
			}
		}

		private void HandleList(TcpClient client, FtpSessionState session, StreamWriter writer)
		{
			if (!CheckAuthentication(session, writer)) return;

			try
			{
				// Устанавливаем соединение для передачи данных
				using (var dataClient = EstablishDataConnection(session, writer))
				{
					if (dataClient == null) return;

					SendResponse(writer, 150, "Here comes the directory listing");

					string physicalPath = MapToPhysicalPath(session.CurrentDirectory);
					var files = Directory.GetFiles(physicalPath);
					var directories = Directory.GetDirectories(physicalPath);

					using (var dataStream = dataClient.GetStream())
					using (var dataWriter = new StreamWriter(dataStream, Encoding.ASCII))
					{
						// Формируем список в формате UNIX ls -l
						foreach (var dir in directories)
						{
							var dirInfo = new DirectoryInfo(dir);
							dataWriter.WriteLine($"drwxr-xr-x 1 owner group {GetDirectorySize(dir):D12} " +
											   $"{dirInfo.LastWriteTime:MMM dd HH:mm} {dirInfo.Name}");
						}

						foreach (var file in files)
						{
							var fileInfo = new FileInfo(file);
							dataWriter.WriteLine($"-rw-r--r-- 1 owner group {fileInfo.Length:D12} " +
											   $"{fileInfo.LastWriteTime:MMM dd HH:mm} {fileInfo.Name}");
						}
					}

					SendResponse(writer, 226, "Directory send OK");
					FilesTransferred++;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[LIST Error] {ex.Message}");
				SendResponse(writer, 550, "Failed to list directory");
			}
		}

		private void HandleRetr(TcpClient client, FtpSessionState session, string filename, StreamWriter writer)
		{
			if (!CheckAuthentication(session, writer)) return;

			try
			{
				string physicalPath = MapToPhysicalPath(session.CurrentDirectory, filename);

				if (!File.Exists(physicalPath))
				{
					SendResponse(writer, 550, "File not found");
					return;
				}

				// Устанавливаем соединение для передачи данных
				using (var dataClient = EstablishDataConnection(session, writer))
				{
					if (dataClient == null) return;

					SendResponse(writer, 150, $"Opening {session.TransferMode} mode data connection for {filename}");

					using (var dataStream = dataClient.GetStream())
					using (var fileStream = File.OpenRead(physicalPath))
					{
						fileStream.CopyTo(dataStream);
					}

					SendResponse(writer, 226, "Transfer complete");
					FilesTransferred++;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[RETR Error] {ex.Message}");
				SendResponse(writer, 550, "Failed to retrieve file");
			}
		}

		private void HandleStor(TcpClient client, FtpSessionState session, string filename, StreamWriter writer)
		{
			if (!CheckAuthentication(session, writer)) return;

			try
			{
				string physicalPath = MapToPhysicalPath(session.CurrentDirectory, filename);

				// Устанавливаем соединение для передачи данных
				using (var dataClient = EstablishDataConnection(session, writer))
				{
					if (dataClient == null) return;

					SendResponse(writer, 150, $"Opening {session.TransferMode} mode data connection for {filename}");

					using (var dataStream = dataClient.GetStream())
					using (var fileStream = File.Create(physicalPath))
					{
						dataStream.CopyTo(fileStream);
					}

					SendResponse(writer, 226, "Transfer complete");
					FilesTransferred++;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[STOR Error] {ex.Message}");
				SendResponse(writer, 550, "Failed to store file");
			}
		}

		#endregion

		#region Команды управления файлами

		private void HandleDele(FtpSessionState session, string filename, StreamWriter writer)
		{
			if (!CheckAuthentication(session, writer)) return;

			try
			{
				string physicalPath = MapToPhysicalPath(session.CurrentDirectory, filename);

				if (!File.Exists(physicalPath))
				{
					SendResponse(writer, 550, "File not found");
					return;
				}

				File.Delete(physicalPath);
				SendResponse(writer, 250, "File deleted successfully");
			}
			catch
			{
				SendResponse(writer, 550, "Failed to delete file");
			}
		}

		private void HandleMkd(FtpSessionState session, string directory, StreamWriter writer)
		{
			if (!CheckAuthentication(session, writer)) return;

			try
			{
				string physicalPath = MapToPhysicalPath(session.CurrentDirectory, directory);
				Directory.CreateDirectory(physicalPath);

				string fullPath = NormalizePath(session.CurrentDirectory, directory);
				SendResponse(writer, 257, $"\"{fullPath}\" directory created");
			}
			catch
			{
				SendResponse(writer, 550, "Failed to create directory");
			}
		}

		private void HandleRmd(FtpSessionState session, string directory, StreamWriter writer)
		{
			if (!CheckAuthentication(session, writer)) return;

			try
			{
				string physicalPath = MapToPhysicalPath(session.CurrentDirectory, directory);

				if (!Directory.Exists(physicalPath))
				{
					SendResponse(writer, 550, "Directory not found");
					return;
				}

				Directory.Delete(physicalPath);
				SendResponse(writer, 250, "Directory removed");
			}
			catch
			{
				SendResponse(writer, 550, "Failed to remove directory");
			}
		}

		private void HandleSize(FtpSessionState session, string filename, StreamWriter writer)
		{
			if (!CheckAuthentication(session, writer)) return;

			try
			{
				string physicalPath = MapToPhysicalPath(session.CurrentDirectory, filename);

				if (!File.Exists(physicalPath))
				{
					SendResponse(writer, 550, "File not found");
					return;
				}

				long size = new FileInfo(physicalPath).Length;
				SendResponse(writer, 213, size.ToString());
			}
			catch
			{
				SendResponse(writer, 550, "Could not get file size");
			}
		}

		#endregion

		#region Вспомогательные методы

		private bool CheckAuthentication(FtpSessionState session, StreamWriter writer)
		{
			if (!session.IsAuthenticated)
			{
				SendResponse(writer, 530, "Please login with USER and PASS");
				return false;
			}
			return true;
		}

		private void SendResponse(StreamWriter writer, int code, string message)
		{
			string response = $"{code} {message}";
			writer.WriteLine(response);
			writer.Flush();

			// Для демонстрации выводим в консоль
			var frame = new System.Diagnostics.StackTrace().GetFrame(1);
			var method = frame.GetMethod().Name;
			Console.WriteLine($"[FTP Response] {method}: {response}");
		}

		private string MapToPhysicalPath(string virtualPath, string filename = null)
		{
			string fullVirtualPath = string.IsNullOrEmpty(filename)
				? virtualPath
				: NormalizePath(virtualPath, filename);

			// Убираем начальный слеш и заменяем разделители
			string relativePath = fullVirtualPath.TrimStart('/');
			if (string.IsNullOrEmpty(relativePath))
				relativePath = ".";

			string physicalPath = Path.Combine(_rootDirectory, relativePath.Replace('/', Path.DirectorySeparatorChar));

			// Защита от выхода за пределы корневой директории
			physicalPath = Path.GetFullPath(physicalPath);
			if (!physicalPath.StartsWith(_rootDirectory, StringComparison.OrdinalIgnoreCase))
			{
				throw new UnauthorizedAccessException("Access denied");
			}

			return physicalPath;
		}

		private string NormalizePath(string current, string relative)
		{
			if (string.IsNullOrEmpty(relative))
				return current;

			if (relative.StartsWith("/"))
				return relative;

			// Простая нормализация пути
			if (relative == "..")
			{
				int lastSlash = current.LastIndexOf('/');
				return lastSlash > 0 ? current.Substring(0, lastSlash) : "/";
			}

			if (relative == ".")
				return current;

			return current == "/" ? $"/{relative}" : $"{current}/{relative}";
		}

		private TcpClient EstablishDataConnection(FtpSessionState session, StreamWriter writer)
		{
			try
			{
				if (session.DataConnectionMode == DataConnectionMode.Passive)
				{
					if (session.PassiveListener == null)
					{
						SendResponse(writer, 425, "Cannot open data connection");
						return null;
					}

					// Ожидаем подключение клиента в пассивном режиме
					var client = session.PassiveListener.AcceptTcpClient();

					// Останавливаем слушатель после принятия соединения
					session.PassiveListener.Stop();
					session.PassiveListener = null;

					return client;
				}
				else // Активный режим
				{
					if (session.ActiveEndpoint == null)
					{
						SendResponse(writer, 425, "Cannot open data connection");
						return null;
					}

					var client = new TcpClient();
					client.Connect(session.ActiveEndpoint);
					return client;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[Data Connection Error] {ex.Message}");
				SendResponse(writer, 425, "Cannot open data connection");
				return null;
			}
		}

		private long GetDirectorySize(string directory)
		{
			try
			{
				long size = 0;
				var files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);
				foreach (var file in files)
				{
					size += new FileInfo(file).Length;
				}
				return size;
			}
			catch
			{
				return 0;
			}
		}

		private void CleanupSession(FtpSessionState session)
		{
			try
			{
				session.PassiveListener?.Stop();
			}
			catch { }
		}

		#endregion

		public void Stop()
		{
			_isRunning = false;

			// Закрываем все активные сессии
			foreach (var session in _sessions)
			{
				CleanupSession(session.Value);
				session.Key.Close();
			}
			_sessions.Clear();

			_controlListener?.Stop();

			Console.WriteLine($"\n=== FTP СЕРВЕР ОСТАНОВЛЕН ===");
			Console.WriteLine($"Всего подключений: {TotalConnections}");
			Console.WriteLine($"Файлов передано: {FilesTransferred}");
		}

		public void Dispose()
		{
			Stop();
		}
	}

	// Простой FTP клиент для демонстрации взаимодействия
	public class SimpleFtpClient : IDisposable
	{
		private TcpClient _controlClient;
		private NetworkStream _controlStream;
		private StreamReader _reader;
		private StreamWriter _writer;
		private bool _isConnected;
		private string _currentDirectory = "/";
		private IPEndPoint? _passiveEndpoint;

		public bool IsConnected => _isConnected && _controlClient?.Connected == true;
		public string CurrentDirectory => _currentDirectory;

		public void Connect(string host, int port = 21)
		{
			try
			{
				_controlClient = new TcpClient(host, port);
				_controlStream = _controlClient.GetStream();
				_reader = new StreamReader(_controlStream, Encoding.ASCII);
				_writer = new StreamWriter(_controlStream, Encoding.ASCII) { AutoFlush = true };

				// Читаем приветственное сообщение сервера
				string welcome = _reader.ReadLine();
				Console.WriteLine($"[FTP Client] Подключено: {welcome}");

				_isConnected = true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[FTP Client] Ошибка подключения: {ex.Message}");
				throw;
			}
		}

		public bool Login(string username, string password)
		{
			if (!IsConnected) return false;

			try
			{
				SendCommand($"USER {username}");
				var userResponse = ReadResponse();

				SendCommand($"PASS {password}");
				var passResponse = ReadResponse();

				return passResponse.StartsWith("230");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[FTP Client] Ошибка аутентификации: {ex.Message}");
				return false;
			}
		}

		public void SetPassiveMode(bool passive = true)
		{
			if (!IsConnected) return;

			if (passive)
			{
				SendCommand("PASV");
				var response = ReadResponse();

				if (response.StartsWith("227") && TryParsePassiveEndpoint(response, out var endpoint))
				{
					_passiveEndpoint = endpoint;
					Console.WriteLine($"[FTP Client] Переключен в пассивный режим: {endpoint.Address}:{endpoint.Port}");
				}
				else
				{
					throw new InvalidOperationException("Не удалось перейти в пассивный режим");
				}
			}
			else
			{
				// Для активного режима нужен специальный обработчик
				Console.WriteLine($"[FTP Client] Активный режим не реализован в демо");
			}
		}

		public string[] ListDirectory()
		{
			if (!IsConnected) return Array.Empty<string>();

			try
			{
				// Устанавливаем тип передачи
				SendCommand("TYPE A");
				ReadResponse();

				// Переходим в пассивный режим
				SetPassiveMode(true);

				var entries = new List<string>();

				// Открываем соединение данных
				using (var dataClient = OpenDataConnection())
				{
					// Запрашиваем список
					SendCommand("LIST");
					var listResponse = ReadResponse();

					if (!listResponse.StartsWith("150"))
					{
						Console.WriteLine($"[FTP Client] Ошибка получения списка: {listResponse}");
						return Array.Empty<string>();
					}

					using (var dataStream = dataClient.GetStream())
					using (var dataReader = new StreamReader(dataStream, Encoding.ASCII))
					{
						string line;
						while ((line = dataReader.ReadLine()) != null)
						{
							entries.Add(line);
						}
					}
				}

				// Читаем завершающий ответ
				var finalResponse = ReadResponse();
				if (finalResponse.StartsWith("226"))
				{
					Console.WriteLine($"[FTP Client] Список получен успешно");
				}

				foreach (var entry in entries)
				{
					Console.WriteLine($"[FTP Client] LIST: {entry}");
				}

				return entries.ToArray();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[FTP Client] Ошибка получения списка: {ex.Message}");
				return Array.Empty<string>();
			}
		}

		public void ChangeDirectory(string directory)
		{
			if (!IsConnected) return;

			SendCommand($"CWD {directory}");
			var response = ReadResponse();

			if (response.StartsWith("250"))
			{
				_currentDirectory = directory;
				Console.WriteLine($"[FTP Client] Перешел в директорию: {directory}");
			}
		}

		public void PrintWorkingDirectory()
		{
			if (!IsConnected) return;

			SendCommand("PWD");
			var response = ReadResponse();
			Console.WriteLine($"[FTP Client] Текущая директория: {response}");
		}

		public void DownloadFile(string remoteFile, string localFile)
		{
			if (!IsConnected) return;

			Console.WriteLine($"[FTP Client] Начинаю загрузку {remoteFile} -> {localFile}");

			try
			{
				// Устанавливаем бинарный режим
				SendCommand("TYPE I");
				ReadResponse();

				// Переходим в пассивный режим
				SetPassiveMode(true);

				using (var dataClient = OpenDataConnection())
				{
					// Запрашиваем файл
					SendCommand($"RETR {remoteFile}");
					var retrResponse = ReadResponse();

					if (retrResponse.StartsWith("150"))
					{
						Console.WriteLine($"[FTP Client] Загрузка начата");

						string fullPath = Path.GetFullPath(localFile);
						string? dir = Path.GetDirectoryName(fullPath);
						if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
						{
							Directory.CreateDirectory(dir);
						}

						using (var dataStream = dataClient.GetStream())
						using (var fileStream = File.Create(fullPath))
						{
							dataStream.CopyTo(fileStream);
						}

						var finalResponse = ReadResponse();
						if (finalResponse.StartsWith("226"))
						{
							Console.WriteLine($"[FTP Client] Файл загружен: {localFile}");
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[FTP Client] Ошибка загрузки: {ex.Message}");
			}
		}

		public void UploadFile(string localFile, string remoteFile)
		{
			if (!IsConnected) return;

			Console.WriteLine($"[FTP Client] Начинаю выгрузку {localFile} -> {remoteFile}");

			try
			{
				// Устанавливаем бинарный режим
				SendCommand("TYPE I");
				ReadResponse();

				// Переходим в пассивный режим
				SetPassiveMode(true);

				string fullPath = Path.GetFullPath(localFile);
				if (!File.Exists(fullPath))
				{
					File.WriteAllText(fullPath, "Demo file content for FTP upload.\r\n");
				}

				using (var dataClient = OpenDataConnection())
				{
					// Отправляем файл
					SendCommand($"STOR {remoteFile}");
					var storResponse = ReadResponse();

					if (storResponse.StartsWith("150"))
					{
						Console.WriteLine($"[FTP Client] Выгрузка начата");

						using (var dataStream = dataClient.GetStream())
						using (var fileStream = File.OpenRead(fullPath))
						{
							fileStream.CopyTo(dataStream);
						}

						var finalResponse = ReadResponse();
						if (finalResponse.StartsWith("226"))
						{
							Console.WriteLine($"[FTP Client] Файл выгружен: {remoteFile}");
						}
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[FTP Client] Ошибка выгрузки: {ex.Message}");
			}
		}

		public void DeleteFile(string filename)
		{
			if (!IsConnected) return;

			SendCommand($"DELE {filename}");
			var response = ReadResponse();

			if (response.StartsWith("250"))
			{
				Console.WriteLine($"[FTP Client] Файл удален: {filename}");
			}
		}

		public void CreateDirectory(string directory)
		{
			if (!IsConnected) return;

			SendCommand($"MKD {directory}");
			var response = ReadResponse();

			if (response.StartsWith("257"))
			{
				Console.WriteLine($"[FTP Client] Директория создана: {directory}");
			}
		}

		public void Disconnect()
		{
			if (IsConnected)
			{
				SendCommand("QUIT");
				ReadResponse();
			}

			_isConnected = false;
			_controlClient?.Close();

			Console.WriteLine($"[FTP Client] Отключен");
		}

		private void SendCommand(string command)
		{
			if (!IsConnected) return;

			Console.WriteLine($"[FTP Client] >> {command}");
			_writer.WriteLine(command);
			_writer.Flush();
		}

		private TcpClient OpenDataConnection()
		{
			if (_passiveEndpoint == null)
			{
				throw new InvalidOperationException("PASV не выполнен");
			}

			var dataClient = new TcpClient();
			dataClient.Connect(_passiveEndpoint);
			return dataClient;
		}

		private bool TryParsePassiveEndpoint(string response, out IPEndPoint endpoint)
		{
			endpoint = default!;

			int start = response.IndexOf('(');
			int end = response.IndexOf(')');
			if (start < 0 || end <= start)
			{
				return false;
			}

			string data = response.Substring(start + 1, end - start - 1);
			string[] parts = data.Split(',');
			if (parts.Length != 6)
			{
				return false;
			}

			byte[] ipBytes = new byte[4];
			for (int i = 0; i < 4; i++)
			{
				if (!byte.TryParse(parts[i], out ipBytes[i]))
				{
					return false;
				}
			}

			if (!int.TryParse(parts[4], out int p1) || !int.TryParse(parts[5], out int p2))
			{
				return false;
			}

			int port = p1 * 256 + p2;
			IPAddress ipAddress = new IPAddress(ipBytes);
			if (ipAddress.Equals(IPAddress.Any) || ipAddress.Equals(IPAddress.None))
			{
				if (_controlClient?.Client?.RemoteEndPoint is IPEndPoint remote)
				{
					ipAddress = remote.Address;
				}
				else
				{
					ipAddress = IPAddress.Loopback;
				}
			}

			if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
			{
				ipAddress = ipAddress.IsIPv4MappedToIPv6 ? ipAddress.MapToIPv4() : IPAddress.Loopback;
			}

			endpoint = new IPEndPoint(ipAddress, port);
			return true;
		}

		private string ReadResponse()
		{
			if (!IsConnected) return string.Empty;

			string response = _reader.ReadLine();
			Console.WriteLine($"[FTP Client] << {response}");
			return response;
		}

		public void Dispose()
		{
			Disconnect();
		}
	}

	// Демонстрационная программа
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("=== ДЕМОНСТРАЦИЯ ПРОТОКОЛА FTP ===\n");

			// Запуск FTP сервера
			using (var server = new SimpleFtpServer(2121)) // Используем нестандартный порт
			{
				server.Start();

				Thread.Sleep(1000); // Даём время серверу запуститься

				// Демонстрация работы клиента
				using (var client = new SimpleFtpClient())
				{
					try
					{
						Console.WriteLine("\n=== ДЕМОНСТРАЦИЯ FTP КЛИЕНТА ===\n");

						// Подключение
						client.Connect("localhost", 2121);

						// Аутентификация
						bool loggedIn = client.Login("user1", "password1");
						Console.WriteLine($"Аутентификация: {(loggedIn ? "УСПЕШНО" : "ОШИБКА")}\n");

						if (loggedIn)
						{
							// Команды навигации
							client.PrintWorkingDirectory();
							client.ChangeDirectory("/");

							// Работа с файлами
							client.CreateDirectory("test_dir");
							client.ChangeDirectory("test_dir");

							// Демонстрация загрузки/выгрузки (заглушки)
							client.UploadFile("test.txt", "uploaded.txt");
							client.DownloadFile("uploaded.txt", "downloaded.txt");

							// Возврат в корень
							client.ChangeDirectory("/");
							client.DeleteFile("test_dir/uploaded.txt");

							// Получение списка
							client.ListDirectory();
						}

						// Отключение
						client.Disconnect();
					}
					catch (Exception ex)
					{
						Console.WriteLine($"Ошибка в демонстрации: {ex.Message}");
					}
				}

				// Даём поработать серверу
				Thread.Sleep(2000);

				Console.WriteLine($"\n=== СТАТИСТИКА FTP СЕРВЕРА ===");
				Console.WriteLine($"Активных сессий: {server.ActiveSessions}");
				Console.WriteLine($"Всего подключений: {server.TotalConnections}");
				Console.WriteLine($"Файлов передано: {server.FilesTransferred}");

				Console.WriteLine("\nНажмите любую клавишу для остановки сервера...");
				Console.ReadKey();

				server.Stop();
			}
		}
	}
}
