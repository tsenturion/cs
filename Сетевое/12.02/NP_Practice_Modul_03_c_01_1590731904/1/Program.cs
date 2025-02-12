using System.Diagnostics.Metrics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Program
{
	class Server
	{
		static void Main()
		{
			Random r = new();
			string[] quotes = {
				"\r\nUnderidoderidoderiododeridoo\r\n",
				"\r\nNever yield to force\r\n",
				"\r\nNever yield to the apparently overwhelming might of the enemy\r\n",
				"\r\nI love spaghetti\r\n"
			};

			Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
			IPEndPoint IPEnd = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1025);
			serverSocket.Bind(IPEnd);
			serverSocket.Listen(10);

			Socket clientSocket = serverSocket.Accept();
			Console.WriteLine($"New connection: {clientSocket.RemoteEndPoint}");
			clientSocket.Send(Encoding.ASCII.GetBytes("1 to get quote, 0 to close connection\r\n"));
			byte[] buffer = new byte[1024];
			bool flag = true;
			while(flag)
			{
				clientSocket.Receive(buffer);
				string query = Encoding.ASCII.GetString(buffer);
				switch(int.Parse(query))
				{
					case 1:
						{
							clientSocket.Send(Encoding.ASCII.GetBytes(quotes[r.Next(quotes.Length)]));
							break;
						}
					case 0:
						{
							clientSocket.Shutdown(SocketShutdown.Both);
							clientSocket.Close();
							flag = false;
							break;
						}
					default:
						{
							Console.WriteLine(query);
							break;
						}
				}
			}
			
		}
	}
}