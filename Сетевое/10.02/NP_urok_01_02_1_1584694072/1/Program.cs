using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace Program
{
	//internal class Server
	//{
	//	Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
	//	IPEndPoint IPEnd = new(IPAddress.Parse("127.0.0.1"), 1024);

	//	//Thread serverThread = new Thread(new ThreadStart(StartServer()));
	//	public Server()
	//	{
	//		socket.Bind(IPEnd);
	//		socket.Listen();
	//	}
	//	public IPEndPoint GetEndPoint()
	//	{
	//		return IPEnd;
	//	}
	//	void AcceptCallback(IAsyncResult iar)
	//	{
	//		Socket server = (Socket)iar.AsyncState;
	//		Socket client = server.EndAccept(iar);
	//		Console.WriteLine($"Подключение установлено: {client.RemoteEndPoint.ToString()}\n");
	//		byte[] buffer = new byte[1024];
	//		while (client.Connected)
	//		{
	//			buffer = Encoding.UTF8.GetBytes(DateTime.Now.TimeOfDay.ToString() + "\n");
	//			//client.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(SendCallback), client);
	//			Thread.Sleep(1000);
	//			client.SendAsync(buffer);
	//		}
	//		client.Shutdown(SocketShutdown.Both);
	//		client.Close();
	//	}
	//	void SendCallback(IAsyncResult iar)
	//	{
	//		Socket client = (Socket)iar.AsyncState;
	//		client.EndSend(iar);
	//		client.Shutdown(SocketShutdown.Both);
	//		client.Close();
	//	}
	//	public void StartServer()
	//	{
	//		Task acceptConnection = new Task(new Action(socket.BeginAccept(new AsyncCallback(AcceptCallback), socket)));
	//		socket.BeginAccept(new AsyncCallback(AcceptCallback), socket);
	//	}
	//	static void BeginAccept(Socket server)
	//	{
	//		server.BeginAccept(new AsyncCallback(AcceptCallback), server);
	//	}
	//}
	
	//internal class Client
	//{
	//	[DllImport("kernel32.dll")]
	//	static extern bool AllocConsole();
	//	Socket client = null;
	//	public Socket ConnectToServer(Server server)
	//	{
	//		client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.IP);
	//		//temp.Connect(server.GetEndPoint());
	//		client.BeginConnect(server.GetEndPoint(), new AsyncCallback(ConnectCallback), client);
	//		//AsyncCallback cc = new AsyncCallback(ConnectCallback);
	//		//cc.BeginInvoke
	//		return client;
	//	}
	//	void ConnectCallback(IAsyncResult iar)
	//	{
	//		Socket server = (Socket)iar.AsyncState;
	//		server.EndConnect(iar);
	//		AllocConsole();
	//		byte[] buffer = new byte[1024];
	//		for(int i=0;i<10;i++)
	//		{
	//			server.ReceiveAsync(buffer);
	//			Console.WriteLine(Encoding.UTF8.GetString(buffer));
	//			Thread.Sleep(1000);
	//		}
	//		server.Shutdown(SocketShutdown.Both);
	//		server.Close();


	//		//server.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, null, server);
	//	}
	//}
	class Program
	{
		static void Main()
		{
			//пытался сделать асинхронный сервер, не получилось

			//Server server = new Server();
			//Thread serverThread = new Thread(new ThreadStart(server.StartServer()));
			//server.StartServer();

			//for(int i=0;i<1;i++)
			//{
			//	Client temp = new Client();
			//	Console.WriteLine($"Клиент {i}: подключение...");
			//	temp.ConnectToServer(server);
			//}

			//Thread.Sleep(1200000);

			Task udp_server = new Task(delegate
			{
				IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 1024);
				Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				serverSocket.Bind(localEndPoint);

				Console.WriteLine("Server: Сервер запущен и ожидает данных...");

				while (true)
				{
					byte[] receiveBuffer = new byte[1024];
					EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
					int bytesReceived = serverSocket.ReceiveFrom(receiveBuffer, ref remoteEndPoint);

					Console.WriteLine($"Server: Получены данные от {remoteEndPoint.ToString()}: {Encoding.ASCII.GetString(receiveBuffer, 0, bytesReceived)}");

					string currentTime = DateTime.Now.ToString();
					byte[] sendBuffer = Encoding.ASCII.GetBytes(currentTime);
					serverSocket.SendTo(sendBuffer, remoteEndPoint);

					Console.WriteLine($"Server: Отправлено текущее время: {currentTime}");
				}
			});

			Task udp_client = new Task(delegate
			{
				IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1024);
				Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

				string message = "Hola";
				byte[] sendBuffer = Encoding.ASCII.GetBytes(message);
				clientSocket.SendTo(sendBuffer, remoteEndPoint);

				byte[] receiveBuffer = new byte[1024];
				EndPoint serverEndPoint = new IPEndPoint(IPAddress.Any, 0);
				int bytesReceived = clientSocket.ReceiveFrom(receiveBuffer, ref serverEndPoint);

				string receivedMessage = Encoding.ASCII.GetString(receiveBuffer, 0, bytesReceived);
				Console.WriteLine($"\tClient: Получено от сервера: {receivedMessage}");

				clientSocket.Close();
			});

			udp_server.Start();
			Thread.Sleep(100);
			udp_client.Start();
			Thread.Sleep(120000);

			/*
				Сервер запущен и ожидает данных...
				Получены данные от 127.0.0.1:58020: Hello
				Отправлено текущее время: 17.02.2025 9:41:48
				Получено от сервера: 17.02.2025 9:41:48
			*/
		}
	}
}