using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
	class Client
	{
		static void Main()
		{
			//Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
			//IPEndPoint IPEnd = new(IPAddress.Parse("127.0.0.1"), 1024);
			//clientSocket.Bind(IPEnd);
			Random r = new();
			for (int i=0;i<10;i++)
			{
				ThreadPool.QueueUserWorkItem(StartClient);
				Thread.Sleep(r.Next(500));
			}

			
		}
		static void StartClient(object obj)
		{
			Random r = new();

			Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
			serverSocket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1025));
			//clientSocket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1025));
			while (!serverSocket.Connected) Thread.Sleep(50);
			while (serverSocket.Connected)
			{
				serverSocket.Send(Encoding.ASCII.GetBytes("1"));
				byte[] buffer = new byte[1024];
				serverSocket.Receive(buffer);
				Console.WriteLine(Encoding.ASCII.GetString(buffer));
				Thread.Sleep(r.Next(1000));
			}
		}
	}
}