using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{ 
	class Server
	{
		enum MessageModes
		{
			Human_Human,
			Human_Server,
			Computer_Human,
			Computer_Server
		};
		static string[] answers = { "Hi", "How are you", "Helo word", "HH", "What a day to be alive", "Bye" };
		static Random r = new();
		static void Main()
		{
			Socket serverSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
			IPEndPoint IPEnd = new(IPAddress.Parse("127.0.0.1"), 1024);
			serverSocket.Bind(IPEnd);
			serverSocket.Listen();
			while(true)
			{
				Socket clientSocket = serverSocket.Accept();
				while(clientSocket.Connected)
				{
					string serverAnswer;
					byte[] buffer = new byte[1024];
					string clientAnswer;
					try
					{
						serverAnswer = answers[r.Next(answers.Length)];
						if(serverAnswer == "Bye")
						{
							Thread.Sleep(1000);
							clientSocket.Shutdown(SocketShutdown.Both);
							clientSocket.Close();
						}
						clientSocket.Send(Encoding.ASCII.GetBytes(serverAnswer + "\r\n"));
						
						clientSocket.Receive(buffer);
						clientAnswer = Encoding.ASCII.GetString(buffer);
						if(clientAnswer == "Bye")
						{
							clientSocket.Send(Encoding.ASCII.GetBytes("Bye bye!" + "\r\n"));
							clientSocket.Shutdown(SocketShutdown.Both);
							clientSocket.Close();
						}
					}
					catch(Exception e)
					{
						Console.WriteLine(e.Message);
					}
				}
			}
		}
	}
}