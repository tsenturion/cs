using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace Program
{
	internal class Server
	{
		Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
		IPEndPoint IPEnd = new(IPAddress.Parse("127.0.0.1"), 1024);
		Thread serverThread = new Thread(new ThreadStart(StartServer()));
		public Server()
		{
			
		}
		public IPEndPoint GetEndPoint()
		{
			return IPEnd;
		}
		void AcceptCallback(IAsyncResult iar)
		{
			Socket server = (Socket)iar.AsyncState;
			Socket client = server.EndAccept(iar);
			Console.WriteLine($"Подключение установлено: {client.RemoteEndPoint.ToString()}\n");
			byte[] buffer = new byte[1024];
			while (client.Connected)
			{
				buffer = Encoding.UTF8.GetBytes(DateTime.Now.TimeOfDay.ToString() + "\n");
				//client.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(SendCallback), client);
				Thread.Sleep(1000);
				client.SendAsync(buffer);
			}
			client.Shutdown(SocketShutdown.Both);
			client.Close();
		}
		void SendCallback(IAsyncResult iar)
		{
			Socket client = (Socket)iar.AsyncState;
			client.EndSend(iar);
			client.Shutdown(SocketShutdown.Both);
			client.Close();
		}
		public void StartServer()
		{
			socket.Bind(IPEnd);
			socket.Listen();

			socket.BeginAccept(new AsyncCallback(AcceptCallback), socket);
		}
	}
	
	internal class Client
	{
		[DllImport("kernel32.dll")]
		static extern bool AllocConsole();
		Socket client = null;
		public Socket ConnectToServer(Server server)
		{
			client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.IP);
			//temp.Connect(server.GetEndPoint());
			client.BeginConnect(server.GetEndPoint(), new AsyncCallback(ConnectCallback), client);
			//AsyncCallback cc = new AsyncCallback(ConnectCallback);
			//cc.BeginInvoke
			return client;
		}
		void ConnectCallback(IAsyncResult iar)
		{
			Socket server = (Socket)iar.AsyncState;
			server.EndConnect(iar);
			AllocConsole();
			byte[] buffer = new byte[1024];
			for(int i=0;i<10;i++)
			{
				server.ReceiveAsync(buffer);
				Console.WriteLine(Encoding.UTF8.GetString(buffer));
				Thread.Sleep(1000);
			}
			server.Shutdown(SocketShutdown.Both);
			server.Close();


			//server.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, null, server);
		}
	}
	class Program
	{
		static void Main()
		{
			Server server = new Server();
			//Thread serverThread = new Thread(new ThreadStart(server.StartServer()));
			server.StartServer();

			for(int i=0;i<1;i++)
			{
				Client temp = new Client();
				Console.WriteLine($"Клиент {i}: подключение...");
				temp.ConnectToServer(server);
			}
			

			Thread.Sleep(120000);
		}
	}
}